using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace Fungus.EditorUtils
{

    [CustomEditor(typeof(StartSns))]
    public class StartSnsEditor : CommandEditor
    {
        private StartSns mTarget;
        private SerializedProperty TopicLabelProp;
        private SerializedProperty DialogNameProp;
        private SerializedProperty DialogCharaProp;
        private SerializedProperty HistorySnsProp;
       //  private ReorderableList _roleList;
        private Vector2 scrollVec2;

        public override void OnEnable()
        {
            mTarget = (StartSns)target;
            TopicLabelProp=serializedObject.FindProperty("TopicLabel");
            DialogNameProp = serializedObject.FindProperty("DialogWindowName");
            DialogCharaProp = serializedObject.FindProperty("DialogChara");
            HistorySnsProp = serializedObject.FindProperty("HistorySns");

            //  _roleList = new ReorderableList(serializedObject, HistorySnsProp
            //  , true, true, true, true);

        }

        public override void DrawCommandGUI()
        {
            EditorGUILayout.PropertyField(TopicLabelProp);
            EditorGUILayout.PropertyField(DialogNameProp);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.PropertyField(DialogCharaProp,GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));

            mTarget.SetCharaValueForAllHistory();//設置角色進 history sns

            EditorGUILayout.LabelField("歷史對話", GUILayout.Height(25));

            HistorySnsProp.arraySize=EditorGUILayout.IntField(HistorySnsProp.arraySize);

           EditorGUILayout.BeginVertical();
            scrollVec2=EditorGUILayout.BeginScrollView(scrollVec2, GUILayout.Width(400), GUILayout.Height(300));
            for (int i=0;i<HistorySnsProp.arraySize;i++) {
                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(("element------" + i));

                SerializedProperty hisProp = HistorySnsProp.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(hisProp.FindPropertyRelative("mChara"));

                List<string> strList = new List<string>();
                strList.Add("Message");
                strList.Add("Image");
                EditorGUI.BeginChangeCheck();

                EnumField<SnsType>(
                    hisProp.FindPropertyRelative("mMessageType").FindPropertyRelative("_snsType"),
                    new GUIContent("SnsType", "SetSnsType"),
                    strList
                    ) ;

                if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                }

                switch (hisProp.FindPropertyRelative("mMessageType").FindPropertyRelative("_snsType").enumValueIndex) {
                    case 1:
                        EditorGUILayout.PropertyField(hisProp.FindPropertyRelative("mMessageType").FindPropertyRelative("_message"));
                        break;

                    case 3:
                        EditorGUILayout.PropertyField(hisProp.FindPropertyRelative("mMessageType").FindPropertyRelative("_sprite"));
                        break;
                
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
         //    EditorGUILayout.PropertyField(HistorySnsProp);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            /*  EditorGUILayout.PropertyField(test1);
    mTarget.SetCharaValueForAllHistory();

    EditorGUILayout.PropertyField(test2);*/

        }

















    }
}
