// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(Command), true)]
    public class CommandEditor : Editor
    {
        #region statics
        public static Command selectedCommand;
        public static bool SelectedCommandDataStale { get; set; }


        public static CommandInfoAttribute GetCommandInfo(System.Type commandType)
        {
            CommandInfoAttribute retval = null;



            object[] attributes = commandType.GetCustomAttributes(typeof(CommandInfoAttribute), false);
            foreach (object obj in attributes)
            {
                CommandInfoAttribute commandInfoAttr = obj as CommandInfoAttribute;
                if (commandInfoAttr != null)
                {
                    if (retval == null)
                    {
                        retval = commandInfoAttr;
                    }
                    else if (retval.Priority < commandInfoAttr.Priority)
                    {
                        retval = commandInfoAttr;
                    }
                }
            }

            return retval;
        }

        #endregion statics

        private Dictionary<string, ReorderableList> reorderableLists;

        public static GUIStyle TitleStyle = new GUIStyle();

        public virtual void OnEnable()
        {
            TitleStyle.alignment = TextAnchor.MiddleCenter;
            TitleStyle.fontSize = 15;
            TitleStyle.normal.textColor = Color.yellow;
            if (NullTargetCheck()) // Check for an orphaned editor instance
                return;

            reorderableLists = new Dictionary<string, ReorderableList>();
        }


        public virtual void DrawCommandInspectorGUI() //cammand的Inspector詳細資訊
        {
            Command t = target as Command;
            if (t == null)
            {
                return;
            }

            var flowchart = (Flowchart)t.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            CommandInfoAttribute commandInfoAttr = CommandEditor.GetCommandInfo(t.GetType());
            if (commandInfoAttr == null)
            {
                return;
            }




            GUILayout.BeginVertical(GUI.skin.box);

            if (t.enabled)
            {
                if (flowchart.ColorCommands)
                {
                    GUI.backgroundColor = t.GetButtonColor();
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
            }
            else
            {
                GUI.backgroundColor = Color.grey;
            }
            GUILayout.BeginHorizontal(GUI.skin.button);

            string commandName = commandInfoAttr.CommandName;
            GUILayout.Label(commandName, GUILayout.MinWidth(80), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("(" + t.ItemId + ")"));

            GUILayout.Space(10);

            GUI.backgroundColor = Color.white;
            bool enabled = t.enabled;
            enabled = GUILayout.Toggle(enabled, new GUIContent());

            if (t.enabled != enabled)
            {
                Undo.RecordObject(t, "Set Enabled");
                t.enabled = enabled;
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            DrawCommandGUI();//command的詳細資訊
            if (EditorGUI.EndChangeCheck())
            {
                SelectedCommandDataStale = true;
            }

            EditorGUILayout.Separator();

            if (t.ErrorMessage.Length > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = new Color(1, 0, 0);
                EditorGUILayout.LabelField(new GUIContent("Error: " + t.ErrorMessage), style);
            }

            GUILayout.EndVertical();

            // Display help text
            CommandInfoAttribute infoAttr = CommandEditor.GetCommandInfo(t.GetType());
            if (infoAttr != null)
            {
                EditorGUILayout.HelpBox(infoAttr.HelpText, MessageType.Info, true);
            }
        }

        public virtual void DrawCommandGUI() //command的詳細資訊
        {
            Command t = target as Command;

            // Code below was copied from here
            // http://answers.unity3d.com/questions/550829/how-to-add-a-script-field-in-custom-inspector.html

            // Users should not be able to change the MonoScript for the command using the usual Script field.
            // Doing so could cause block.commandList to contain null entries.
            // To avoid this we manually display all properties, except for m_Script.
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.name == "m_Script")
                {
                    continue;
                }

                if (!t.IsPropertyVisible(iterator.name))
                {
                    continue;
                }

                if (iterator.isArray &&
                    t.IsReorderableArray(iterator.name))
                {
                    ReorderableList reordList = null;
                    reorderableLists.TryGetValue(iterator.displayName, out reordList);
                    if (reordList == null)
                    {
                        var locSerProp = iterator.Copy();
                        //create and insert
                        reordList = new ReorderableList(serializedObject, locSerProp, true, false, true, true)
                        {
                            drawHeaderCallback = (Rect rect) =>
                            {
                                EditorGUI.LabelField(rect, locSerProp.displayName);
                            },
                            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                EditorGUI.PropertyField(rect, locSerProp.GetArrayElementAtIndex(index));
                            },
                            elementHeightCallback = (int index) =>
                            {
                                return EditorGUI.GetPropertyHeight(locSerProp.GetArrayElementAtIndex(index), null, true);// + EditorGUIUtility.singleLineHeight;
                            }
                        };

                        reorderableLists.Add(iterator.displayName, reordList);
                    }

                    reordList.DoLayoutList();
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        //陣列型顯示方式
        public static void ObjectField<T>(SerializedProperty property, GUIContent label, GUIContent nullLabel, List<T> objectList) where T :UnityEngine.Object
        {
            if (property == null)
            {
                return;
            }

            List<GUIContent> objectNames = new List<GUIContent>();

            T selectedObject = property.objectReferenceValue as T;

            int selectedIndex = -1; // Invalid index

            // First option in list is <None>
            objectNames.Add(nullLabel);
            if (selectedObject == null)
            {
                selectedIndex = 0;
            }

            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null) continue;
                objectNames.Add(new GUIContent(objectList[i].name));

                if (selectedObject == objectList[i])
                {
                    selectedIndex = i + 1;
                }
            }

            T result;

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

            if (selectedIndex == -1)
            {
                // Currently selected object is not in list, but nothing else was selected so no change.
                return;
            }
            else if (selectedIndex == 0) 
            {
                result = null; // Null option
            }
            else
            {
                result = objectList[selectedIndex - 1];
            }

            property.objectReferenceValue = result;
        }



        public static void StringField(SerializedProperty property, GUIContent label, GUIContent nullLabel, List<string> objectList)
        {

            if (property == null)
            {
                return;
            }
            if (objectList==null||objectList.Count<=0) {
                return;
            }

            List<GUIContent> objectNames = new List<GUIContent>();

            string selectedObject = property.stringValue;

            int selectedIndex = -1; // Invalid index

            // First option in list is <None>
            objectNames.Add(nullLabel);


            if (string.IsNullOrEmpty(selectedObject))
            {
                selectedIndex = 0;
            }

            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null) continue;
                objectNames.Add(new GUIContent(objectList[i]));

                if (selectedObject == objectList[i])
                {
                    selectedIndex = i + 1;
                }
            }
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

            if (selectedIndex <= 0)
            {
                // Currently selected object is not in list, but nothing else was selected so no change.

                property.stringValue = null;
                // return;
            }
            else
            {

                property.stringValue = objectList[selectedIndex - 1];
            }



        }


        public static void EnumField<T>(SerializedProperty property, GUIContent label, List<string> objectList)where T:Enum//作用 因為前面的某個選擇 刪減某些enum的選項 
        {
            if (property == null)
            {
                return;
            }
            if (objectList == null || objectList.Count <= 0)
            {
                return;
            }


            string[] origineEnum = Enum.GetNames(typeof(T));

            string origineSelectedStr = origineEnum[property.enumValueIndex];

            List<GUIContent> objectNames = new List<GUIContent>();

            int selectedIndex = 0;

            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null) continue;
                objectNames.Add(new GUIContent(objectList[i]));
                if (objectList[i]==origineSelectedStr) {
                    selectedIndex = i;
                }

            }

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());


            string selectStr = objectList[selectedIndex];

            int origineEnumIndex = 0;
            foreach (string str in origineEnum) {
                if (str==selectStr) {
                    break;
                }
                origineEnumIndex++;
            }
           
                // property.enumValueIndex =(UnityEngine.Object)Enum.Parse(typeof(T), objectList[(selectedIndex)]);
            property.enumValueIndex = origineEnumIndex;
            



        }



        // When modifying custom editor code you can occasionally end up with orphaned editor instances.
        // When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        // Once this situation occurs, the only way to fix it is to restart the Unity editor.
        // 
        // As a workaround, this function detects if this command editor is an orphan and deletes it. 
        // To use it, just call this function at the top of the OnEnable() method in your custom editor.
        protected virtual bool NullTargetCheck()
        {
            try
            {
                // The serializedObject accessor create a new SerializedObject if needed.
                // However, this will fail with a null exception if the target object no longer exists.
#pragma warning disable 0219
                SerializedObject so = serializedObject;
            }
            catch (System.NullReferenceException)
            {
                DestroyImmediate(this);
                return true;
            }

            return false;
        }
    }
}
