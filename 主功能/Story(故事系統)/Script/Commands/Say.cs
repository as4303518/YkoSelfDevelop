// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{

    /// <summary>
    /// Writes text in a dialog box.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Say", 
                 "Writes text in a dialog box.")]
    [AddComponentMenu("")]
    public class Say : Command, ILocalizable
    {
        // Removed this tooltip as users's reported it obscures the text box
        /// <summary>
        /// 顯示字句帶碼 ex:Main01_ch00_01_024
        /// </summary>
        [SerializeField] protected string storyText = "";
        [TextArea(5, 10)]
        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description = "";

        [Tooltip("Character that is speaking")]
        [SerializeField] protected Character character;


        [SerializeField]protected string aAnimation;

        [SerializeField] protected string aFinishDefaultAnimation;

        [SerializeField] protected string aSkin;

        [SerializeField] protected bool loop;

        

        [SerializeField] protected string nameText = "";

        [Tooltip("Voiceover audio to play when writing the text")]
        [SerializeField] protected AudioClip voiceOverClip;

        [Tooltip("Always show this Say text when the command is executed multiple times")]
        [SerializeField] protected bool showAlways = true;

        [Tooltip("Number of times to show this Say text when the command is executed multiple times")]
        [SerializeField] protected int showCount = 1;

        [Tooltip("Type this text in the previous dialog box.")]
        [SerializeField] protected bool extendPrevious = false;

        [Tooltip("Fade out the dialog box when writing has finished and not waiting for input.")]
        [SerializeField] protected bool fadeWhenDone = false;

        [Tooltip("Wait for player to click before continuing.")]
        [SerializeField] protected bool waitForClick = true;

        [Tooltip("Stop playing voiceover when text finishes writing.")]
        [SerializeField] protected bool stopVoiceover = true;

        [Tooltip("Wait for the Voice Over to complete before continuing")]
        [SerializeField] protected bool waitForVO = false;

        [SerializeField] protected MouthAnimation mouthState;


        public MouthAnimation MouthState { get { return mouthState; } set { mouthState = value; } }
        //add wait for vo that overrides stopvo

        [Tooltip("Sets the active Say dialog with a reference to a Say Dialog object in the scene. All story text will now display using this Say Dialog.")]
        [SerializeField] protected SayDialog setSayDialog;

        protected int executionCount;

        private GameObject targetObj=null;

        #region Public members

        /// <summary>
        /// Character that is speaking.
        /// </summary>
        public virtual Character Character { get { return character; } set { character = value; } }

        public AudioClip VoiceOverClip { get { return voiceOverClip; }set { voiceOverClip = value; } }

        public string StoryText { get { return storyText; } set { storyText = value; } }

        public string Skin { get { return aSkin; } set { aSkin = value; } }
        public string FinishDefaultAnimation { get { return aFinishDefaultAnimation; } set { aFinishDefaultAnimation = value; } }

        /// <summary>
        /// Type this text in the previous dialog box.
        /// </summary>
        public virtual bool ExtendPrevious { get { return extendPrevious; } }
        /// <summary>
        /// 執行say的coroutine
        /// </summary>
        private Coroutine playSayCoro = null;

        public override void OnEnter()
        {
            if (ParentBlock.GetFlowchart().mStoryControl.isSkipPlay)
            {
              SaySkip();
              //  StartCoroutine(SaySkip());
            }
            else
            {
                StartCoroutine(SetSayInfo());
            }

        }

        private IEnumerator SetSayInfo() {


            if (ParentBlock.GetFlowchart().mStoryControl.isSkipPlay) 
            {
                Continue();
                yield break;
            }

            if (!showAlways && executionCount >= showCount)
            {
                Continue();
                yield break;
            }

            executionCount++;

            var sayDialog = SetDisplayDialog();

           // var lastDialog = SayDialog.ActiveSayDialog;
          //  if (lastDialog!=null) Debug.Log("上個對話框名稱=>" + lastDialog.gameObject.name);
          //  if (sayDialog != null) Debug.Log("當前對話框名稱=>" + sayDialog.gameObject.name);


            sayDialog.StopAllCoroutines();


            if (sayDialog == null)
            {
                Continue();
                yield break;
            }
            var flowchart = GetFlowchart();
            Stage stage = flowchart.mStage;


            //多一個確認反映到頭像的功能

            SayDialogPortraitSetting opt = new SayDialogPortraitSetting(aAnimation, FinishDefaultAnimation, aSkin, loop);

            yield return sayDialog.SetCharacterInfo(character, stage, flowchart, nameText);//設置對話框角色

            if ( !string.IsNullOrEmpty(aAnimation) &&  character!=null &&character.aSkeletonGraphic != null)
            {
                if (string.IsNullOrEmpty(aSkin))
                {
                    aSkin = character.DefaultSkin;
                }
                targetObj = sayDialog.CreateCharacterOfPortrait(stage);//生成角色
                sayDialog.StopAllCoroutines();
                sayDialog.SetCharacterPortrait(targetObj, opt);//設置角色頭像資訊
            }
            else
            {
                aFinishDefaultAnimation = "";
                aAnimation = "";
                sayDialog.AdjustTextArea();
                sayDialog.InitPortraitChara();
            }


            string displayText = "";
            if (flowchart.useAssetText) {
                     TranslateOfCsv.GetSpecifyValueOfCsvFile(storyText, flowchart.mLanguage, text => {
                    displayText = text;
                });
            }
            else
            {
                displayText = storyText;
            }

            var activeCustomTags = CustomTag.activeCustomTags;
            for (int i = 0; i < activeCustomTags.Count; i++)
            {
                var ct = activeCustomTags[i];
                displayText = displayText.Replace(ct.TagStartSymbol, ct.ReplaceTagStartWith);
                if (ct.TagEndSymbol != "" && ct.ReplaceTagEndWith != "")
                {
                    displayText = displayText.Replace(ct.TagEndSymbol, ct.ReplaceTagEndWith);
                }
            }


            string content = flowchart.SubstituteVariables(displayText);

            //sayDialog.GetWriter().SetAutoPlay

            if (GetNextCommandType() == typeof(Say))
            {
                fadeWhenDone = false;
            }

            Sayinfo sayinfo = new Sayinfo();
            sayinfo.content = content;
            sayinfo.clearPrevious = !extendPrevious;
            sayinfo.waitForClick = waitForClick;
            sayinfo.fadeWhenDone = fadeWhenDone;
            sayinfo.stopVoiceover = stopVoiceover;
            sayinfo.waitForVO = waitForVO;
            sayinfo.voiceOverClip = voiceOverClip;
            sayinfo.mouthAniState = mouthState;
           // sayinfo.isAutoPlayFromDefaultSetting = flowchart.mStoryControl.isAutoPlay;
            if (ParentBlock != null)
            {
                sayinfo.flowchart = ParentBlock.GetFlowchart();
            }
            else
            {
                sayinfo.flowchart = null;
            }
            
            sayinfo.onComplete = () =>
            {
                StopSaySpeakerAnimation(stage);
                Continue();
            };
            sayinfo.StartSayAni = () =>
            {
                StartSaySpeakerAnimation(stage);

            };
            sayinfo.StopSayAni = () =>
            {
                StopSaySpeakerAnimation(stage);
            };
            
            sayinfo.setLogDialog = (transName, dialog )=> {
                StoryControl sc = ParentBlock.GetFlowchart().mStoryControl;

                Sprite sprite = null;
                Color color = Color.black;

                if (character != null)
                {
                    sprite = character.NameBGSprite;
                    color = character.NameBGColor;
                }
                sc.SaveDialogRecord(new DialogInfo(transName, dialog, sprite, color, voiceOverClip));
            };

            /*  if (lastDialog!=null) { 
                   lastDialog.SetActive(false);
               //lastDialog.GetComponent<CanvasGroup>().alpha = 0;
              }*/
            SayDialog.CloseDialogStatus(sayDialog.name);
            sayDialog.SetActive(true);
            if (!string.IsNullOrEmpty(aFinishDefaultAnimation)) 
            {
                sayDialog.SetPlayFinishAnimation(targetObj, opt);
            }
            yield return sayDialog.ReactionSayDialogAlpha(true);
            playSayCoro=StartCoroutine( sayDialog.Say(sayinfo));

        }

        //直接展示完整文字
        private void DirectlyShowText()
        {
            if (playSayCoro != null) StopCoroutine(playSayCoro);

        }

        private void SaySkip()
        {
             LoadRecordToLogPopupData();
            Continue();
        }
        /// <summary>
        /// 加載歷史對話(skip之後,為了讓畫面呈現較快,所以歷史對話單獨跑continue加載下來
        /// </summary>
        /// <returns></returns>
        private void LoadRecordToLogPopupData()
        {
            var sayDialog = SetDisplayDialog();
            var flowchart = ParentBlock.GetFlowchart();
            StoryControl sc = ParentBlock.GetFlowchart().mStoryControl;
            string displayText = "";
            string charaName = "";
            sc.RecordCount += 1;
            
            StartCoroutine(sayDialog.ReactionSayDialogAlpha(false, false));
            if (flowchart.useAssetText)
            {
                TranslateOfCsv.GetSpecifyValueOfCsvFile
                (
                    storyText,
                    flowchart.mLanguage,
                    text => {
                    displayText = text;
                });

                TranslateOfCsv.GetSpecifyValueOfCsvFile
                (
                           character.NameText,
                           AssetTextType.CharaNumber,
                           flowchart.mLanguage,
                           name => { charaName = name; }
                 );
                var stringSubstituter = new StringSubstituter();
                charaName = stringSubstituter.SubstituteStrings(charaName);
            }
            else
            {
                charaName = nameText;
                displayText = storyText;
            }
            string content = flowchart.SubstituteVariables(displayText);
            string tokenText = TextVariationHandler.SelectVariations(content);
            List<TextTagToken> tokens = TextTagParser.Tokenize(tokenText);//獲得相應的功能 tag

            try
            {
                Sprite sprite = null;
                Color color = Color.black;

                if (character != null)
                {
                    sprite = character.NameBGSprite;
                    color = character.NameBGColor;
                }
                var tarToken = tokens.First(v => v.type == TokenType.Words);
                sc.SaveDialogRecord(new DialogInfo(charaName, tarToken.paramList[0], sprite, color, voiceOverClip));
            }
            catch
            {
                Debug.Log("!!!!#can not find display text "+storyText);

            }

        }

        /// <summary>
        /// 獲得對話框 並初始化
        /// </summary>
        /// <returns></returns>
        private SayDialog SetDisplayDialog()
        {
            SayDialog displayDialog = null;

            if (character != null && character.SetSayDialog != null)
            {

                displayDialog = character.SetSayDialog;
            }

            if (setSayDialog != null)
            {
                displayDialog = setSayDialog;
            }

            var dialog= SayDialog.GetSayDialog(displayDialog);
            dialog.Init();
            dialog.GetWriter().AutoPlay = ParentBlock.GetFlowchart().mStoryControl.isAutoPlay;
            return dialog;

        }

        public override void OnExit()
        {
         //   SayDialog sd= SayDialog.GetSayDialog();
         //   StoryControl sc = ParentBlock.GetFlowchart().mStoryControl;
        //    sc.SaveDialogRecord(new DialogInfo(sd.NameText,InputUISupportScript.RemoveAllRichText( sd.StoryText),voiceOverClip));
        }

        private void StartSaySpeakerAnimation(Stage stage)
        {
            if (character!=null) {

                SkeletonGraphic speaker = stage.FindSkeletonGraphicInstanceByName( character.name)?.GetComponent<SkeletonGraphic>();


                //Debug.Log("開始對話找到的角色=>" + speaker);
                if (speaker!=null) {
                    speaker.AnimationState.SetAnimation(1, "talk_start", true);
                }
            }
        }

        private void StopSaySpeakerAnimation(Stage stage)
        {
            if ( character != null)
            {
                SkeletonGraphic speaker = stage.FindSkeletonGraphicInstanceByName(character.name)?.GetComponent<SkeletonGraphic>();

                // Debug.Log("結束對話找到的角色=>" + speaker);
                if (speaker != null)
                {
                    speaker.AnimationState.SetEmptyAnimation(1, 0.1f);
                }
            }

        }

        public override string GetSummary()
        {
            string namePrefix = "";
            if (character != null) 
            {
                namePrefix = character.NameText + ": ";
            }
            if (extendPrevious)
            {
                namePrefix = "EXTEND" + ": ";
            }
            return namePrefix + "\"" + storyText + "\"";
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override void OnReset()
        {
            executionCount = 0;
        }

        public override void OnStopExecuting()
        {
            var sayDialog = SayDialog.GetSayDialog();
            if (sayDialog == null)
            {
                return;
            }

            sayDialog.Stop();
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return storyText;
        }

        public virtual void SetStandardText(string standardText)
        {
            storyText = standardText;
        }

        public virtual string GetDescription()
        {
            return description;
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual string GetStringId()
        {
            // String id for Say commands is SAY.<Localization Id>.<Command id>.[Character Name]
            string stringId = "SAY." + GetFlowchartLocalizationId() + "." + itemId + ".";
            if (character != null)
            {
                stringId += character.NameText;
            }
            return stringId;
        }
        public enum MouthAnimation
        {
            DidNotPlay,
            WaitStoryTextComplete,
            WaitVoiceComplete
        }
        #endregion
    }
    public struct Sayinfo
    {
        public string content;
        public bool clearPrevious;
        public bool waitForClick;
        public bool fadeWhenDone;
        public bool stopVoiceover;
        public bool waitForVO;
       // public bool isAutoPlayFromDefaultSetting;
        public bool isSkip;
        public Say.MouthAnimation mouthAniState;
        public AudioClip voiceOverClip;
        public Action<string,string> setLogDialog;
        public Action onComplete;
        public Action StartSayAni;
        public Action StopSayAni;
        public Action whenDialogFinish;
        public Flowchart flowchart;

    }

    public struct SayDialogPortraitSetting
    {
        public string _animation;
        public string _finishDefaultAnimation;
        public string _skin;
        public bool _loop;
        public SayDialogPortraitSetting(string ani, string finish, string skin, bool loop)
        {
            _animation = ani;
            _finishDefaultAnimation = finish;
            _skin = skin;
            _loop = loop;

        }
    }

}