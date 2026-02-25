// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(Flowchart))]
    public class FlowchartEditor : Editor
    {
        protected SerializedProperty mLanguageProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty colorCommandsProp;
        protected SerializedProperty hideComponentsProp;
        protected SerializedProperty stepPauseProp;
        protected SerializedProperty saveSelectionProp;
        protected SerializedProperty localizationIdProp;
        protected SerializedProperty variablesProp;
        protected SerializedProperty showLineNumbersProp;
        protected SerializedProperty hideCommandsProp;
        protected SerializedProperty luaEnvironmentProp;
        protected SerializedProperty luaBindingNameProp;
        protected SerializedProperty _storyControlProp;
        protected SerializedProperty _storyEnableProp;
        protected SerializedProperty _flowchartOverrideTextFileToJson;
        protected SerializedProperty _flowchartOverrideTextFileToCsv;
        protected SerializedProperty _dataNameProp;
        protected SerializedProperty _StageProp;
        protected SerializedProperty shaderColorWheelProp;

        //test Prop
        protected SerializedProperty useAssetTextProp;
        protected SerializedProperty useSafeModeProp;
        protected Texture2D addTexture;

        protected VariableListAdaptor variableListAdaptor;

        protected GUIStyle Title = new GUIStyle();

        public static bool FlowchartDataStale { get; set; }

        protected virtual void OnEnable()
        {

            Title = new GUIStyle();
            Title.fontSize = 15;
            Title.alignment = TextAnchor.MiddleCenter;
            Title.normal.textColor = Color.yellow;
            if (NullTargetCheck()) // Check for an orphaned editor instance
                return;
            mLanguageProp=serializedObject.FindProperty("mLanguage");
            descriptionProp = serializedObject.FindProperty("description");
            colorCommandsProp = serializedObject.FindProperty("colorCommands");
            hideComponentsProp = serializedObject.FindProperty("hideComponents");
            stepPauseProp = serializedObject.FindProperty("stepPause");
            saveSelectionProp = serializedObject.FindProperty("saveSelection");

            _storyControlProp = serializedObject.FindProperty("mStoryControl");

            _storyEnableProp = serializedObject.FindProperty("_storyEnabled");
            localizationIdProp = serializedObject.FindProperty("localizationId");

            _flowchartOverrideTextFileToJson = serializedObject.FindProperty("flowchartOverrideTextFileToJson");
            _flowchartOverrideTextFileToCsv = serializedObject.FindProperty("flowchartOverrideTextFileToCsv");
            _dataNameProp = serializedObject.FindProperty("dataName");
            variablesProp = serializedObject.FindProperty("variables");
            showLineNumbersProp = serializedObject.FindProperty("showLineNumbers");
            hideCommandsProp = serializedObject.FindProperty("hideCommands");
            luaEnvironmentProp = serializedObject.FindProperty("luaEnvironment");
            luaBindingNameProp = serializedObject.FindProperty("luaBindingName");
            _StageProp = serializedObject.FindProperty("mStage");
            shaderColorWheelProp = serializedObject.FindProperty("shaderColorWheel");
            useAssetTextProp = serializedObject.FindProperty("useAssetText");
            useSafeModeProp = serializedObject.FindProperty("useSafeMode");


            addTexture = FungusEditorResources.AddSmall;

            variableListAdaptor = new VariableListAdaptor(variablesProp, target as Flowchart);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var flowchart = target as Flowchart;

            flowchart.UpdateHideFlags();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_dataNameProp);

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("sss"));

            EditorGUILayout.PropertyField(mLanguageProp);
            EditorGUILayout.PropertyField(useAssetTextProp);
            EditorGUILayout.PropertyField(useSafeModeProp);
            EditorGUILayout.PropertyField(_StageProp);
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(colorCommandsProp);
            EditorGUILayout.PropertyField(hideComponentsProp);
            EditorGUILayout.PropertyField(stepPauseProp);
            EditorGUILayout.PropertyField(saveSelectionProp);
            EditorGUILayout.PropertyField(localizationIdProp);
            EditorGUILayout.PropertyField(showLineNumbersProp);
            EditorGUILayout.PropertyField(hideCommandsProp, new GUIContent(hideCommandsProp.displayName, hideCommandsProp.tooltip), true);
            EditorGUILayout.PropertyField(luaEnvironmentProp);
            EditorGUILayout.PropertyField(luaBindingNameProp);
            EditorGUILayout.PropertyField(_storyControlProp);
            EditorGUILayout.PropertyField(_storyEnableProp);


            if (GUILayout.Button(new GUIContent("Open Flowchart Window", "Opens the Flowchart Window")))
            {
                EditorWindow.GetWindow(typeof(FlowchartWindow), false, "Flowchart");
            }

            GUILayout.Space(30);
            EditorGUILayout.LabelField("ExportJsonSetting", Title, GUILayout.Height(30));


            // Show list of commands to hide in Add Command menu    
            //ReorderableListGUI.Title(new GUIContent(hideCommandsProp.displayName, hideCommandsProp.tooltip));
            //ReorderableListGUI.ListField(hideCommandsProp);
            // EditorGUILayout.PropertyField(hideCommandsProp, new GUIContent(hideCommandsProp.displayName, hideCommandsProp.tooltip), true);



            if (EditorGUI.EndChangeCheck())
            {
                FlowchartDataStale = true;
            }

            #region Export Data to json

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_flowchartOverrideTextFileToJson);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (flowchart.flowchartOverrideTextFileToJson != null)
                {
                    flowchart.SetDataNameText();
                }
            }

           /* if (GUILayout.Button(new GUIContent("Export Data To Json ", "Save Data To TextAsset With Json")))
            {

                flowchart.ExportJsonToCsv();
                // flowchart.ClickCreateSaveData();

             //   flowchart.TestExportFlowchart();
            }


            if (flowchart.flowchartOverrideTextFileToJson != null)
            {
                if (GUILayout.Button(new GUIContent("Override Flowchart Info", "Override Flowchart All Info")))
                {
                    Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(flowchart.OverrideSaveData());
                    //按鈕 將flowchart  覆蓋文本的資訊
                }
            }*/

            #endregion
            #region Export data to csv
            EditorGUILayout.LabelField("ExportCsvSetting", Title, GUILayout.Height(30));

            EditorGUILayout.PropertyField(_flowchartOverrideTextFileToCsv);

            if (GUILayout.Button(new GUIContent("Export Data To Csv ", "Save Data To TextAsset With Csv")))
            {
                Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(flowchart.ExportFlowchartToCsv());

            }

            if (flowchart.flowchartOverrideTextFileToCsv != null)
            {
                //將csv內容覆蓋在flowchart上
                if (GUILayout.Button(new GUIContent("OverrideFlowchartOfCsv", "Override Flowchart apart command info")))
                {
                    // flowchart.test();
                    Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(flowchart.GetCsvDataOverrideFlowchart());

                }
                //將csv內容按照commandType生成在flowchart上
                if (GUILayout.Button(new GUIContent("CreateBlockOfCsvToFlowchart", "Create apart command info to Flowchart")))
                {
                    // flowchart.test();
                    IEnumerator ie() {
                        yield return flowchart.CreateBlockOfCsvToFlowchart();
                    }
                    

                    Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(flowchart.CreateBlockOfCsvToFlowchart());
                }

            }
            #endregion

            GUILayout.Space(20);
            EditorGUILayout.LabelField("ColorWheel", Title, GUILayout.Height(30));
            EditorGUILayout.PropertyField(shaderColorWheelProp);

            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("VariableSetting", Title, GUILayout.Height(30));
            //Show the variables in the flowchart inspector

            DrawVariablesGUI(false, Mathf.FloorToInt(EditorGUIUtility.currentViewWidth) - VariableListAdaptor.ReorderListSkirts);



            // Debug.Log("統計數字"+VariableListAdaptor.ReorderListSkirts);

            // DrawVariablesGUI(false, 10);

        }

        public virtual void DrawVariablesGUI(bool showVariableToggleButton, int rate)
        {

            var t = target as Flowchart;

            if (t == null)
            {
                return;
            }

            serializedObject.Update();


            if (t.Variables.Count == 0)

            {
                t.VariablesExpanded = true;
                //showVariableToggleButton = true;
            }

            if (showVariableToggleButton && !t.VariablesExpanded)
            {

                if (GUILayout.Button("Variables (" + t.Variables.Count + ")", GUILayout.Height(24)))
                {
                    t.VariablesExpanded = true;
                }

                // Draw disclosure triangle
                Rect lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 5;
                lastRect.y += 5;
                EditorGUI.Foldout(lastRect, false, "");
            }
            else
            {
                // Remove any null variables from the list
                // Can sometimes happen when upgrading to a new version of Fungus (if .meta GUID changes for a variable class)
                for (int i = t.Variables.Count - 1; i >= 0; i--)
                {
                    if (t.Variables[i] == null)
                    {
                        t.Variables.RemoveAt(i);
                    }
                }

                variableListAdaptor.DrawVarList(rate);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static List<System.Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<System.Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                    .Where(t =>
                           t != derivedType &&
                           derivedType.IsAssignableFrom(t)
                           ).ToList();

        }

        /// <summary>
        /// When modifying custom editor code you can occasionally end up with orphaned editor instances.
        /// When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        /// Once this situation occurs, the only way to fix it is to restart the Unity editor.
        /// As a workaround, this function detects if this editor is an orphan and deletes it. 
        /// </summary>
        protected virtual bool NullTargetCheck()
        {
            try
            {
                // The serializedObject accessor creates a new SerializedObject if needed.
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