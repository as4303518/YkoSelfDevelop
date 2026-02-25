using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(SpineCharaAni))]
    //flow command editor
    public class SpineCharaAniEditor : CommandEditor
    {

        public SpineCharaAni tar = null;


        private SerializedProperty aSkeletonGraphicProp;
        private SerializedProperty StageProp;
        private SerializedProperty FacingProp;
        private SerializedProperty aAnimationProp;
        private SerializedProperty aDefaultAnimationProp;
        private SerializedProperty FromPosProp;
        private SerializedProperty ToPosProp;
        private SerializedProperty clickPositionProp;
        private SerializedProperty LoopProp;
        private SerializedProperty DisplayProp;
        private SerializedProperty aClickButtonSizeProp;
        private SerializedProperty aClickButtonSizeSettingProp;
        private SerializedProperty aInitialSkinName;

        private SerializedProperty effectAndAnimationSimultaneouslyExecutePorp;

        /////////////////////////// condition bool /////////////////
        private SerializedProperty WaitFadeFinishProp;
        private SerializedProperty WaitAnimationFinishProp;
        
        //  private SerializedProperty WaitForClick;
        private SerializedProperty clickModleProp;
        private SerializedProperty orderSwitchProp;
        private SerializedProperty orderNameProp;
        private SerializedProperty aSpineOrderProp;
        private SerializedProperty OffestProp;
        private SerializedProperty FadeProp;
        private SerializedProperty moveProp;
        private SerializedProperty scaleAniProp;
        private SerializedProperty scaleVectorAniProp;
        private SerializedProperty easeTypeProp;

        private SerializedProperty tweenTime;

        public override void OnEnable()
        {
            tar = target as SpineCharaAni;
            aSkeletonGraphicProp = serializedObject.FindProperty("aTarget");
            DisplayProp = serializedObject.FindProperty("display");
            StageProp = serializedObject.FindProperty("stage");
            FacingProp = serializedObject.FindProperty("facing");

            aAnimationProp = serializedObject.FindProperty("aAnimation");
            aDefaultAnimationProp = serializedObject.FindProperty("aFinishDefaultAnimation");

            FromPosProp = serializedObject.FindProperty("fromPosition");
            ToPosProp = serializedObject.FindProperty("toPosition");
            LoopProp = serializedObject.FindProperty("loop");
            aInitialSkinName = serializedObject.FindProperty("aInitialSkinName");
            WaitFadeFinishProp = serializedObject.FindProperty("waitFadeFinish");
            WaitAnimationFinishProp = serializedObject.FindProperty("waitAnimationFinish");
            clickModleProp = serializedObject.FindProperty("clickMode");
            clickPositionProp = serializedObject.FindProperty("ClickPos");
            aClickButtonSizeProp = serializedObject.FindProperty("ClickButtonSize");
            aClickButtonSizeSettingProp = serializedObject.FindProperty("aClickButtonSizeSetting");
            effectAndAnimationSimultaneouslyExecutePorp = serializedObject.FindProperty("effectAndAnimationSimultaneouslyExecute");
            //aClickButtonSizeSetting
            // WaitForClick=serializedObject.FindProperty("waitForClick");
            OffestProp = serializedObject.FindProperty("offest");
            FadeProp = serializedObject.FindProperty("fade");
            tweenTime = serializedObject.FindProperty("aTween");
            orderSwitchProp = serializedObject.FindProperty("orderSwitch");
            orderNameProp = serializedObject.FindProperty("orderName");
            aSpineOrderProp = serializedObject.FindProperty("spineOrder");

            moveProp = serializedObject.FindProperty("move");
            scaleAniProp = serializedObject.FindProperty("scaleAni");
            scaleVectorAniProp = serializedObject.FindProperty("effectScale");
            easeTypeProp = serializedObject.FindProperty("easeType");
        }



        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            if (Stage.ActiveStages.Count > 1)//有複數才會顯示
            {
                CommandEditor.ObjectField<Stage>(StageProp,
                                        new GUIContent("Portrait Stage", "Stage to display the character portraits on"),
                                        new GUIContent("<Default>"),
                                        Stage.ActiveStages);
            }
            else
            {
                tar._Stage = null;

            }

            EditorGUI.BeginChangeCheck();//選取造型

            EditorGUILayout.PropertyField(aSkeletonGraphicProp, new GUIContent("SkeletonGraphic", "SkeletonGraphic representing character"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }


            if (aSkeletonGraphicProp.objectReferenceValue != null)
            {
                Character cs = aSkeletonGraphicProp.objectReferenceValue as Character;

                if (cs.aSkeletonGraphic == null)
                {
                    EditorGUILayout.HelpBox(new GUIContent("Can Not Have a Chara"));
                    return;
                }


                string[] displayLabels = StringFormatter.FormatEnumNames(tar.Display, "<None>");
                DisplayProp.enumValueIndex = EditorGUILayout.Popup("Display", (int)DisplayProp.enumValueIndex, displayLabels);

                EditorGUILayout.PropertyField(OffestProp);

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Order", GUILayout.Width(EditorGUIUtility.currentViewWidth / 3));
                //SetSpineOrder = EditorGUILayout.Toggle(SetSpineOrder);
              //  EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(orderSwitchProp);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                if (tar.orderSwitch)
                {
                        StringField(
                        orderNameProp,
                        new GUIContent("SortingLayer", "Set Sort Layer"),
                        new GUIContent("None"),
                        SortingLayer.layers.ToList().Select(la => la.name).ToList()
                        );
                    EditorGUILayout.PropertyField(aSpineOrderProp);
                }



                // Debug.Log("釋出==>"+aSkeletonGraphic.objectReferenceValue);

                // tar.aSkeletonGraphic=aSkeletonGraphic.objectReferenceValue as SkeletonGraphic;//將更改後的Value反應在script

                if (tar.Display != DisplayType.None)
                {
                    if (tar.Display == DisplayType.Show)
                    {
                        EditorGUILayout.PropertyField(effectAndAnimationSimultaneouslyExecutePorp);
                        EditorGUILayout.PropertyField(FadeProp, new GUIContent("FadeIn", "淡入"));
                    }
                    else if (tar.Display == DisplayType.Hide)
                    {
                        EditorGUILayout.PropertyField(FadeProp, new GUIContent("FadeOut", "淡出"));
                    }

                    SerializedProperty aFadeAni = tweenTime.FindPropertyRelative("aFadeAniDuration");
                    if (tar.Fade)
                    {
                        EditorGUILayout.PropertyField(aFadeAni);
                        EditorGUILayout.PropertyField(WaitFadeFinishProp);
                    }

                    CommandEditor.StringField(aInitialSkinName,
                                new GUIContent("Skin", "Change representing Skin"),
                                new GUIContent("<None>"),
                                tar.aTarget.aSkeletonGraphic.GetSkinStrings());

                    CommandEditor.StringField(aAnimationProp,
                                            new GUIContent("Animation", "Animation representing character"),
                                            new GUIContent("<None>"),
                                             tar.aTarget.aSkeletonGraphic.GetSkeletonStrings());

                    if (tar.Display == DisplayType.Show)
                    {
                        CommandEditor.StringField(aDefaultAnimationProp,
                                            new GUIContent("FinishDefaultAnimation", "Animation representing character"),
                                            new GUIContent("<None>"),
                                             tar.aTarget.aSkeletonGraphic.GetSkeletonStrings());
                    }

                    if (string.IsNullOrWhiteSpace(tar.FinishDefaultAnimation))
                    {
                        EditorGUILayout.PropertyField(LoopProp);
                    }


                    aFadeAni.floatValue = aFadeAni.floatValue <= 0 ? 0 : aFadeAni.floatValue;

                    if (DisplayProp.enumValueIndex > 0 && aAnimationProp.stringValue != null)
                    {
                        EditorGUILayout.LabelField("Animation  Judge", EditorStyles.boldLabel);

                        EditorGUILayout.PropertyField(clickModleProp);

                        EditorGUILayout.PropertyField(WaitAnimationFinishProp);

                    }

                    if (tar.mClickMode == ClickMode.ClickOnButton)
                    {
                        if (tar._Stage == null)
                        {

                            tar._Stage = GameObject.FindObjectOfType<Stage>();

                        }


                        if (tar._Stage != null)
                        {
                            CommandEditor.ObjectField<RectTransform>(clickPositionProp,
                                new GUIContent("ButtonCreatePos", "CreateTouchButton"),
                                new GUIContent("<Previous>"),
                                tar._Stage.Positions);
                        }

                        EditorGUILayout.PropertyField(aClickButtonSizeSettingProp);

                        string[] enumName = aClickButtonSizeSettingProp.enumNames;
                        string clickSetting = enumName[aClickButtonSizeSettingProp.enumValueIndex];

                        var enumStatus = (ClickAeraSetting)Enum.Parse(typeof(ClickAeraSetting), clickSetting);

                        switch (enumStatus)
                        {
                            case ClickAeraSetting.Default:

                                break;
                            case ClickAeraSetting.Customize:
                                EditorGUILayout.PropertyField(aClickButtonSizeProp);
                                break;
                        }
                        tar.StartDraw = true;
                    }
                    else
                    {
                        tar.StartDraw = false;
                    }


                    string[] facingArrows = new string[]
                        {
                            "<Previous>",
                            "<--",
                            "-->",
                        };

                    FacingProp.enumValueIndex = EditorGUILayout.Popup("Facing", FacingProp.enumValueIndex, facingArrows);

                    EffectChoose();

                }


            }




            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

        }

        public void EffectChoose()
        {

            GUILayout.BeginHorizontal();
            //GUILayout.Space(EditorGUIUtility.currentViewWidth/2);

            EditorGUILayout.LabelField("Chara Effect", GUILayout.Height(30));

            GUILayout.EndHorizontal();

            if (!moveProp.boolValue)
            {
                Stage s = tar.ParentBlock.GetFlowchart().mStage;
                CommandEditor.ObjectField<RectTransform>(ToPosProp,
                new GUIContent("To Position", "Move the portrait to this position"),
                new GUIContent("<Previous>"),
                s.Positions);
            }

            EditorGUILayout.PropertyField(moveProp);

            if (moveProp.boolValue)
            {
                SetMoveProperty();
            }

            EditorGUILayout.PropertyField(scaleAniProp);

            if (scaleAniProp.boolValue)
            {
                SetScaleAniProperty();
            }

            if (moveProp.boolValue || scaleAniProp.boolValue)
            {
                DisplayTweenTime();
            }


        }

        private void SetMoveProperty()
        {

            Stage stage = tar.ParentBlock.GetFlowchart().mStage;

            if (stage == null)        // If no default specified, try to get any portrait stage in the scene
            {
                EditorGUILayout.HelpBox("No portrait stage has been set.", MessageType.Error);
                return;
            }

            CommandEditor.ObjectField<RectTransform>(FromPosProp,
                            new GUIContent("From Position", "Move the portrait to this position"),
                            new GUIContent("<Previous>"),
                            stage.Positions);

            CommandEditor.ObjectField<RectTransform>(ToPosProp,
            new GUIContent("To Position", "Move the portrait to this position"),
            new GUIContent("<Previous>"),
            stage.Positions);
        }


        private void SetScaleAniProperty()
        {

            Stage s = tar.ParentBlock.GetFlowchart().mStage;

            if (s == null)        // If no default specified, try to get any portrait stage in the scene
            {
                EditorGUILayout.HelpBox("No portrait stage has been set.", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(scaleVectorAniProp);

        }

        private void DisplayTweenTime()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            SerializedProperty aMoveAni = tweenTime.FindPropertyRelative("aEffectAniDuration");
            EditorGUILayout.PropertyField(easeTypeProp);
            EditorGUILayout.PropertyField(aMoveAni);
            aMoveAni.floatValue = aMoveAni.floatValue <= 0 ? 0 : aMoveAni.floatValue;
            GUILayout.EndVertical();

        }


    }
}

