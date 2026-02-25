// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using DG.DOTweenEditor.UI;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;

namespace Fungus.EditorUtils
{
    [CustomEditor (typeof(Say))]
    public class SayEditor : CommandEditor
    {
        public static bool showTagHelp;
        public Texture2D blackTex;
        
        public static void DrawTagHelpLabel()
        {
            string tagsText = TextTagParser.GetTagHelp();

            if (CustomTag.activeCustomTags.Count > 0)
            {
                tagsText += "\n\n\t-------- CUSTOM TAGS --------";
                List<Transform> activeCustomTagGroup = new List<Transform>();
                foreach (CustomTag ct in CustomTag.activeCustomTags)
                {
                    if(ct.transform.parent != null)
                    {
                        if (!activeCustomTagGroup.Contains(ct.transform.parent.transform))
                        {
                            activeCustomTagGroup.Add(ct.transform.parent.transform);
                        }
                    }
                    else
                    {
                        activeCustomTagGroup.Add(ct.transform);
                    }
                }
                foreach(Transform parent in activeCustomTagGroup)
                {
                    string tagName = parent.name;
                    string tagStartSymbol = "";
                    string tagEndSymbol = "";
                    CustomTag parentTag = parent.GetComponent<CustomTag>();
                    if (parentTag != null)
                    {
                        tagName = parentTag.name;
                        tagStartSymbol = parentTag.TagStartSymbol;
                        tagEndSymbol = parentTag.TagEndSymbol;
                    }
                    tagsText += "\n\n\t" + tagStartSymbol + " " + tagName + " " + tagEndSymbol;
                    foreach(Transform child in parent)
                    {
                        tagName = child.name;
                        tagStartSymbol = "";
                        tagEndSymbol = "";
                        CustomTag childTag = child.GetComponent<CustomTag>();
                        if (childTag != null)
                        {
                            tagName = childTag.name;
                            tagStartSymbol = childTag.TagStartSymbol;
                            tagEndSymbol = childTag.TagEndSymbol;
                        }
                            tagsText += "\n\t      " + tagStartSymbol + " " + tagName + " " + tagEndSymbol;
                    }
                }
            }
            tagsText += "\n";
            float pixelHeight = EditorStyles.miniLabel.CalcHeight(new GUIContent(tagsText), EditorGUIUtility.currentViewWidth);
            EditorGUILayout.SelectableLabel(tagsText, GUI.skin.GetStyle("HelpBox"), GUILayout.MinHeight(pixelHeight));
        }
        private Say tar;
        protected SerializedProperty characterProp;
        protected SerializedProperty storyTextProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty voiceOverClipProp;
        protected SerializedProperty showAlwaysProp;
        protected SerializedProperty showCountProp;
        protected SerializedProperty extendPreviousProp;
        protected SerializedProperty fadeWhenDoneProp;
        protected SerializedProperty waitForClickProp;
        protected SerializedProperty stopVoiceoverProp;
        protected SerializedProperty setSayDialogProp;
        protected SerializedProperty waitForVOProp;
        protected SerializedProperty portraitAniProp;
        protected SerializedProperty portraitFinishAniProp;
        protected SerializedProperty portraitSkinProp;
        protected SerializedProperty portraitAniLoopProp;
        protected SerializedProperty nameTextProp;
        protected SerializedProperty mouthStateProp;

        //private Regex storyTextFormat= new Regex("^\\D+\\d*_\\D+\\d{2}_\\d{2}_\\D+\\d{3}$");
        private Regex storyTextFormat = new Regex("^\\D*\\d*_\\D*\\d*_\\D*\\d*_\\D*\\d*$");

        public override void OnEnable()
        {
            base.OnEnable();
           tar =target as Say;
            characterProp = serializedObject.FindProperty("character");
            storyTextProp = serializedObject.FindProperty("storyText");
            descriptionProp = serializedObject.FindProperty("description");
            voiceOverClipProp = serializedObject.FindProperty("voiceOverClip");
            showAlwaysProp = serializedObject.FindProperty("showAlways");
            showCountProp = serializedObject.FindProperty("showCount");
            extendPreviousProp = serializedObject.FindProperty("extendPrevious");
            fadeWhenDoneProp = serializedObject.FindProperty("fadeWhenDone");
            waitForClickProp = serializedObject.FindProperty("waitForClick");
            stopVoiceoverProp = serializedObject.FindProperty("stopVoiceover");
            setSayDialogProp = serializedObject.FindProperty("setSayDialog");
            waitForVOProp = serializedObject.FindProperty("waitForVO");
            portraitAniProp = serializedObject.FindProperty("aAnimation");
            portraitSkinProp = serializedObject.FindProperty("aSkin");
            portraitAniLoopProp = serializedObject.FindProperty("loop");
            portraitFinishAniProp= serializedObject.FindProperty("aFinishDefaultAnimation");
            nameTextProp = serializedObject.FindProperty("nameText");
            mouthStateProp = serializedObject.FindProperty("mouthState");
            
            if (blackTex == null)
            {
                blackTex = CustomGUI.CreateBlackTexture();
            }
        }
        
        protected virtual void OnDisable()
        {
            DestroyImmediate(blackTex);
        }

        public override void DrawCommandGUI() 
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Chara"); 
            characterProp.objectReferenceValue = (Character) EditorGUILayout.ObjectField(characterProp.objectReferenceValue, typeof(Character), true);
            EditorGUILayout.EndHorizontal();

            Say t = target as Say;

            // Only show portrait selection if...  // Character is selected
            if (t.Character != null && t.Character.aSkeletonGraphic != null )  
            {
                
                EditorGUILayout.LabelField("PortraitSetting", TitleStyle, GUILayout.Height(30));
                CommandEditor.StringField(portraitAniProp,
                     new GUIContent("PortraitAnimation", "Portrait representing speaking character"),
                     new GUIContent("<None>"),
                     t.Character.aSkeletonGraphic.GetAllAnimationsStringName()
                    ) ;

                  CommandEditor.StringField(portraitSkinProp,
                  new GUIContent("PortraitSkin", "Portrait representing speaking character"),
                  new GUIContent("<None>"),
                     t.Character.aSkeletonGraphic.GetSkinStrings()
                     );

                CommandEditor.StringField(portraitFinishAniProp,
              new GUIContent("PortraitFinishDefaultAnimation", "Portrait representing speaking character"),
              new GUIContent("<None>"),
              t.Character.aSkeletonGraphic.GetAllAnimationsStringName()
                );

                if (string.IsNullOrEmpty( t.FinishDefaultAnimation)) {
                    EditorGUILayout.PropertyField(portraitAniLoopProp);
                }

            }

            EditorGUILayout.PropertyField(nameTextProp);


            EditorGUILayout.LabelField("ConversationSetting", TitleStyle, GUILayout.Height(30));

            EditorGUILayout.PropertyField(storyTextProp);

            storyTextFormat = new Regex("^\\D*\\d*_\\D*\\d*_\\D*\\d*_\\D*\\d*$");

            if (storyTextFormat.IsMatch(storyTextProp.stringValue))
             {
                    EditorCoroutineUtility.StartCoroutineOwnerless(TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                        storyTextProp.stringValue,
                        (target as Say).ParentBlock.GetFlowchart().mLanguage,
                        str => {
                            
                            tar.Description = str;
                            //serializedObject.ApplyModifiedProperties();
                        }
                  ));
            }

            EditorGUILayout.PropertyField(descriptionProp);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(extendPreviousProp);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Tag Help", "View available tags"), new GUIStyle(EditorStyles.miniButton)))
            {
                showTagHelp = !showTagHelp;
            }
            EditorGUILayout.EndHorizontal();
            
            if (showTagHelp)
            {
                DrawTagHelpLabel();
            }
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(voiceOverClipProp, 
            new GUIContent("Voice Over Clip", "Voice over audio to play when the text is displayed"));

            EditorGUILayout.PropertyField(mouthStateProp,new GUIContent("Mouth Animation Over State"));

            EditorGUILayout.PropertyField(showAlwaysProp);
            
            if (showAlwaysProp.boolValue == false)
            {
                EditorGUILayout.PropertyField(showCountProp);
            }

            GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
            GUIStyle leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButton.fontSize = 10;
            leftButton.font = EditorStyles.toolbarButton.font;
            GUIStyle rightButton = new GUIStyle(EditorStyles.miniButtonRight);
            rightButton.fontSize = 10;
            rightButton.font = EditorStyles.toolbarButton.font;

            EditorGUILayout.PropertyField(fadeWhenDoneProp);
            EditorGUILayout.PropertyField(waitForClickProp);
            EditorGUILayout.PropertyField(stopVoiceoverProp);
            EditorGUILayout.PropertyField(waitForVOProp);
            EditorGUILayout.PropertyField(setSayDialogProp);
            
           /* if (showPortraits && t.Portrait != null)
            {
                Texture2D characterTexture = t.Portrait.texture;
                float aspect = (float)characterTexture.width / (float)characterTexture.height;
                Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), GUILayout.ExpandWidth(true));
                if (characterTexture != null)
                {
                    GUI.DrawTexture(previewRect,characterTexture,ScaleMode.ScaleToFit,true,aspect);
                }
            }*/
            
            serializedObject.ApplyModifiedProperties();
        }
    }    

    public class asd : FlowchartEditor
    {

        public asd()
        {

            

        }
    }



}