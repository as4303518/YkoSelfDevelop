using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(WaitForClick))]

    public class WaitForClickEditor : CommandEditor
    {
        private WaitForClick tar;
        private SerializedProperty clickModeProp;
        private SerializedProperty toPositionProp;
        private SerializedProperty waitSceondProp;

        public override void OnEnable()
        {
            tar=target as WaitForClick;
            clickModeProp = serializedObject.FindProperty("clickMode");
            toPositionProp =serializedObject.FindProperty("toPosition");
            waitSceondProp = serializedObject.FindProperty("WaitSceond");
        }

        public override void DrawCommandGUI()
        {

            Stage stage = tar.ParentBlock.GetFlowchart().mStage;


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(clickModeProp);
            if(EditorGUI.EndChangeCheck() )
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.PropertyField(waitSceondProp);

            if ( tar.ClickMode==ClickMode.ClickOnButton ) {
                ObjectField<RectTransform>(toPositionProp,
                new GUIContent("To Position"),
                new GUIContent("None"),
                stage.Positions
                );

            }
            serializedObject.ApplyModifiedProperties();

        }
    }

}