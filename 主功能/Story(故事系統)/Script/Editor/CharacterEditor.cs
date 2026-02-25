// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEditor;
using UnityEngine;
using Spine.Unity;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(Character))]
    public class CharacterEditor : Editor
    {
        protected SerializedProperty nameTextProp;
        protected SerializedProperty nameColorProp;
        protected SerializedProperty SayDialogNameSpriteProp;
        protected SerializedProperty SayDialogNameColorProp;

        protected SerializedProperty soundEffectProp;
        protected SerializedProperty portraitsProp;
        protected SerializedProperty portraitsFaceProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty setSayDialogProp;
        protected SerializedProperty setCharaAvatarProp;

        public SerializedProperty SpineSettingPro, SkeletonGraphicPro;//骨架陣列 
        public SerializedProperty SkeletonSkin, SkeletonAni;

        protected virtual void OnEnable()
        {
            nameTextProp = serializedObject.FindProperty("nameText");
            nameColorProp = serializedObject.FindProperty("nameColor");
            soundEffectProp = serializedObject.FindProperty("soundEffect");
            portraitsProp = serializedObject.FindProperty("portraits");
            portraitsFaceProp = serializedObject.FindProperty("portraitsFace");
            descriptionProp = serializedObject.FindProperty("description");
            setSayDialogProp = serializedObject.FindProperty("setSayDialog");
            SayDialogNameColorProp = serializedObject.FindProperty("SayDialogNameColor");
            SayDialogNameSpriteProp = serializedObject.FindProperty("SayDialogNameBgSprite");
            setCharaAvatarProp = serializedObject.FindProperty("charaAvatar");
            SpineSettingPro = serializedObject.FindProperty("mSet");
            SkeletonGraphicPro = serializedObject.FindProperty("mSkeletonGraphic");
            SkeletonSkin = serializedObject.FindProperty("DefaultSkin");
            SkeletonAni = serializedObject.FindProperty("DefaultAni");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Character t = target as Character;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("2D角色設定",GUILayout.Height(50));

            EditorGUILayout.PropertyField(nameTextProp, new GUIContent("Name Text", "Name of the character display in the dialog"));
            EditorGUILayout.PropertyField(nameColorProp, new GUIContent("Name Color", "Color of name text display in the dialog"));
            EditorGUILayout.PropertyField(SayDialogNameSpriteProp, new GUIContent("NameBGSprite", "Adjust Name Sprite in the dialog"));

            EditorGUILayout.PropertyField(SayDialogNameColorProp, new GUIContent("NameBGColor", "Adjust Name Background Color "));

            EditorGUILayout.PropertyField(soundEffectProp, new GUIContent("Sound Effect", "Sound to play when the character is talking. Overrides the setting in the Dialog."));
            EditorGUILayout.PropertyField(setSayDialogProp);
            EditorGUILayout.PropertyField(setCharaAvatarProp);
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description", "Notes about this story character (personality, attibutes, etc.)"));



            if (t.Portraits != null && t.Portraits.Count > 0)
            {
                t.ProfileSprite = t.Portraits[0];
            }
            else
            {
                t.ProfileSprite = null;
            }

            if (t.ProfileSprite != null)
            {
                Texture2D characterTexture = t.ProfileSprite.texture;

                 float aspect = (float)characterTexture.width / (float)characterTexture.height;//比例


                Rect previewRect = GUILayoutUtility.GetAspectRect(aspect);

                if (characterTexture != null)
                {
                    //                   value    外框比例            角色圖片                                                                  //圖片比例
                    GUI.DrawTexture(previewRect, characterTexture, ScaleMode.ScaleToFit, true, aspect);
                    
                    
                    // EditorGUI.DrawTextureTransparent(previewRect, characterTexture,ScaleMode.ScaleToFit,aspect);
                }

            }

            EditorGUILayout.PropertyField(portraitsProp, new GUIContent("Portraits", "Character image sprites to display in the dialog"), true);

            EditorGUILayout.HelpBox("All portrait images should use the exact same resolution to avoid positioning and tiling issues.", MessageType.Info);

            EditorGUILayout.Separator();

            string[] facingArrows = new string[]
            {
                "FRONT",
                "<--",
                "-->",
            };
            portraitsFaceProp.enumValueIndex = EditorGUILayout.Popup("Portraits Face", (int)portraitsFaceProp.enumValueIndex, facingArrows);

            EditorGUILayout.Separator();


            DisplayCharaInfo();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(t);

            serializedObject.ApplyModifiedProperties();
        }



        public void DisplayCharaInfo()
        {
            EditorGUILayout.LabelField("Spine角色設定", GUILayout.Height(50));
            EditorGUILayout.PropertyField(SkeletonGraphicPro);

            if (SkeletonGraphicPro.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("Can Not Have a SkeletonGraphic"));
                serializedObject.ApplyModifiedProperties();
                return;
            }

            SkeletonGraphic aSkele = SkeletonGraphicPro.objectReferenceValue as SkeletonGraphic;


            if (aSkele.skeletonDataAsset == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("Can Not Have a skeletonDataAsset"));
                serializedObject.ApplyModifiedProperties();
                return;
            }
            else
            {


                EditorGUILayout.PropertyField(SpineSettingPro);



                CommandEditor.StringField(SkeletonSkin,
                                        new GUIContent("Skin", "Change representing Skin"),
                                        new GUIContent("<None>"),
                                        aSkele.GetSkinStrings());

                CommandEditor.StringField(SkeletonAni,
                                        new GUIContent("Ani", "Change representing Ani"),
                                        new GUIContent("<None>"),
                                        aSkele.GetSkeletonStrings());
            }




            serializedObject.ApplyModifiedProperties();


        }

    }
}