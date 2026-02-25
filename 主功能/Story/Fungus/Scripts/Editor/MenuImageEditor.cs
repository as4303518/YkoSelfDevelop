using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fungus.EditorUtils;
using Spine.Unity;
using Fungus;
using System;


[CustomEditor(typeof(MenuImage))]


public class MenuImageEditor :CommandEditor
    {
    private MenuImage curTarget;
    private SerializedProperty optionListPro;
    private SerializedProperty clickAeraSettingProp;




    private void OnEnable()
    {
        curTarget = (MenuImage)target;
        optionListPro = serializedObject.FindProperty("Options");
        clickAeraSettingProp = serializedObject.FindProperty("clickAeraSetting");
    }

     public override void DrawCommandGUI()
     {
        var flowchart = FlowchartWindow.GetFlowchart();

        if (flowchart == null)
        {
            return;
        }

        serializedObject.Update();
        SerializedProperty tarBlockPro = optionListPro.FindPropertyRelative("targetBlock");
        SerializedProperty imgPro= optionListPro.FindPropertyRelative("_image");
        SerializedProperty imgSizePro = optionListPro.FindPropertyRelative("imageSize");
        SerializedProperty toPosPro = optionListPro.FindPropertyRelative("_options").FindPropertyRelative("parentPos");
        SerializedProperty touchSizePro = optionListPro.FindPropertyRelative("_options").FindPropertyRelative("touchSize");

        EditorGUILayout.PropertyField(imgPro);
        EditorGUILayout.PropertyField(imgSizePro);


        BlockEditor.BlockField(tarBlockPro,
                                   new GUIContent("Target Block", "Block to call when option is selected"),
                                   new GUIContent("<None>"));

        /*if (tarBlockPro.objectReferenceValue == null && GUILayout.Button("+", GUILayout.MaxWidth(17)))
        {
            var fw = EditorWindow.GetWindow<FlowchartWindow>();
            var t = (MenuImage)target;
            var activeFlowchart = t.GetFlowchart();
            var newBlock = fw.CreateBlockSuppressSelect(activeFlowchart, t.ParentBlock._NodeRect.position - Vector2.down * 60);
            tarBlockPro.objectReferenceValue = newBlock;
            activeFlowchart.SelectedBlock = t.ParentBlock;
        }*/

        Stage stage=GameObject.FindObjectOfType<Stage>();

        EditorGUI.BeginChangeCheck();
         if (stage != null)
         {
             CommandEditor.ObjectField<RectTransform>(toPosPro,
                 new GUIContent("ButtonCreatePos", "CreateTouchButton"),
                 new GUIContent("<Previous>"),
                stage.Positions);
         }
         else
         {
             Debug.LogError("Not Have Stage");
         }

        if (EditorGUI.EndChangeCheck()) {
        serializedObject.ApplyModifiedProperties();
        
        }

        if (!curTarget.Options._options.parentPos) {

            EditorGUILayout.HelpBox("Not Have a ParentPos", MessageType.Error);
        }
        else
        {
            EditorGUILayout.PropertyField(clickAeraSettingProp);

            if (curTarget.clickAeraSetting==ClickAeraSetting.Customize) {
                EditorGUILayout.PropertyField(touchSizePro);
            }



        }



        // EditorGUILayout.PropertyField(optionListPro);

        serializedObject.ApplyModifiedProperties();


       /* CommandEditor.ObjectField<RectTransform>(clickPositionPro,
                           new GUIContent("ButtonCreatePos", "CreateTouchButton"),
                           new GUIContent("<Previous>"),
                           tar._Stage.Positions);*/

    }





    }

/*[CustomPropertyDrawer(typeof(ImageOption))]
public class ImageOptionAttribute : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("_image"));
        //EditorGUILayout.PropertyField(property.FindPropertyRelative("_pos"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("_size"));
    }


}*/



