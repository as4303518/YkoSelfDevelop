// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using YKO.Mypage;
using Unity.Properties;
using static YKO.Common.Sound.SoundController;

namespace Fungus
{
    /// <summary>
    /// Display story text in a visual novel style dialog box.
    /// </summary>
    /// 



    public class SayDialog : MonoBehaviour//對話框本身的腳本
    {
        [Tooltip("Duration to fade dialogue in/out")]
        [SerializeField] protected float fadeDuration = 0.25f;

        [Tooltip("The continue button UI object")]
        [SerializeField] protected Button continueButton;

        [Tooltip("The canvas UI object")]
        [SerializeField] protected Canvas dialogCanvas;

        [Tooltip("The name text UI object")]
        [SerializeField] protected Text nameText;
      /*  /// <summary>
        /// 標註名字的左側圖
        /// </summary>
        [SerializeField] protected Image nameImage;*/
        /// <summary>
        /// 名稱底部背景
        /// </summary>
        [SerializeField] protected GameObject sentenceNameBG;

        [SerializeField] protected Transform AvatarParent;
        [Tooltip("TextAdapter will search for appropriate output on this GameObject if nameText is null")]
        [SerializeField] protected GameObject nameTextGO;
        protected TextAdapter nameTextAdapter = new TextAdapter();
        public virtual string NameText
        {
            
            get
            {
                return nameTextAdapter.Text;
            }
            set
            {
                nameTextAdapter.Text = value;
            }
        }

        [Tooltip("The story text UI object")]
        [SerializeField] protected Text storyText;
        [Tooltip("TextAdapter will search for appropriate output on this GameObject if storyText is null")]
        [SerializeField] protected GameObject storyTextGO;

        protected TextAdapter storyTextAdapter = new TextAdapter();
        public virtual string StoryText
        {
            get
            {
                return storyTextAdapter.Text;
            }
            set
            {
                storyTextAdapter.Text = value;
            }
        }
        public virtual RectTransform StoryTextRectTrans
        {
            get
            {
                return storyText != null ? storyText.rectTransform : storyTextGO.GetComponent<RectTransform>();
            }
        }




        [SerializeField] protected SkeletonGraphic PortraitChara;


        [Tooltip("Adjust width of story text when Character Image is displayed (to avoid overlapping)")]
        [SerializeField] protected bool fitTextWithImage = true;

        [Tooltip("Close any other open Say Dialogs when this one is active")]
        [SerializeField] protected bool closeOtherDialogs;

        protected float startStoryTextWidth;
        protected float startStoryTextInset;

        protected WriterAudio writerAudio;
        protected Writer writer;
        [SerializeField] protected CanvasGroup canvasGroup;

        protected bool fadeWhenDone = true;
        protected float targetAlpha = 0f;
        protected float fadeCoolDownTimer = 0f;

        // Most recent speaking character
        protected static Character speakingCharacter;

        protected StringSubstituter stringSubstituter = new StringSubstituter();

        // Cache active Say Dialogs to avoid expensive scene search
        protected static List<SayDialog> activeSayDialogs = new List<SayDialog>();

        #region Record StoryText UI Recttransfom
        /// <summary>
        /// 紀錄對話框的上下左右(anchor變化時顯現
        /// </summary>
        private Vector2 storyTextOffsetMax;
        /// <summary>
        /// 紀錄對話框的上下左右(anchor變化時顯現
        /// </summary>
        private Vector2 storyTextOffsetMin;
        #endregion

        protected virtual void Awake()
        {
            
            if (activeSayDialogs != null)
            {
                if (!activeSayDialogs.Contains(this))
                {
                    activeSayDialogs.Add(this);
                }
            }
            else
            {
                activeSayDialogs = new List<SayDialog>();
            }

            if (GetComponent<Canvas>().worldCamera == null)
            {
                GetComponent<Canvas>().worldCamera = Camera.main;
            }
            RecordOrigineStoryTextRectTransformInfo();
            nameTextAdapter.InitFromGameObject(nameText != null ? nameText.gameObject : nameTextGO);
            storyTextAdapter.InitFromGameObject(storyText != null ? storyText.gameObject : storyTextGO);

        }

        protected virtual void OnDestroy()
        {
            activeSayDialogs.Remove(this);
        }

        public virtual Writer GetWriter()
        {
            if (writer != null)
            {
                return writer;
            }

            writer = GetComponent<Writer>();
            if (writer == null)
            {
                writer = gameObject.AddComponent<Writer>();
            }

            return writer;
        }

        protected virtual CanvasGroup GetCanvasGroup()
        {
            if (canvasGroup != null)
            {
                return canvasGroup;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }

        protected virtual WriterAudio GetWriterAudio()
        {
            if (writerAudio != null)
            {
                return writerAudio;
            }

            writerAudio = GetComponent<WriterAudio>();
            if (writerAudio == null)
            {
                writerAudio = gameObject.AddComponent<WriterAudio>();
            }

            return writerAudio;
        }



        public void Init()
        {

            storyText.text = "";
            // Add a raycaster if none already exists so we can handle dialog input
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // It's possible that SetCharacterImage() has already been called from the
            // Start method of another component, so check that no image has been set yet.
            // Same for nameText.

            if (NameText == "")
            {
                nameTextAdapter.SetTextColor(UnityEngine.Color.white);
            }

          //  GetWriter().SetDialogStatusAction = ReactionSayDialogAlpha;


        }



        /// <summary>
        /// 對話框的透明度(淡入淡出)
        /// </summary>
        /// <param name="isDisplay"></param>
        /// <param name="canTween"></param>
        /// <returns></returns>
        public IEnumerator ReactionSayDialogAlpha(bool isDisplay,bool canTween=true)
        {
                if (isDisplay)//正在寫
                {
                    targetAlpha = 1f;
                    continueButton.gameObject.SetActive(false);
                    
                }
                else
                {
                targetAlpha = 0f;
                }

            CanvasGroup canvasGroup = GetCanvasGroup();

            if (targetAlpha==canvasGroup.alpha)
            {
                //對話框以符合當前透明度需求,不需要另外去淡入淡出
                yield break;
            }


            if (!canTween)//沒有過渡下，直接設置透明度
            {
                canvasGroup.alpha = targetAlpha;
                if (targetAlpha<=0) 
                {
                    GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
                else
                {
                    GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
            }
            else
            {
                if (canvasGroup.alpha < targetAlpha)
                {
                    yield return LeanTweenManager.FadeIn(gameObject, () =>
                    {
                        GetComponent<CanvasGroup>().blocksRaycasts = true;
                    });
                }
                else if (canvasGroup.alpha > targetAlpha )
                {
                    yield return LeanTweenManager.FadeOut(gameObject, () =>
                    {
                        GetComponent<CanvasGroup>().blocksRaycasts = false;
                       // gameObject.SetActive(false);
                    });
                }

            }

        }

        protected virtual void ClearStoryText()
        {
            StoryText = "";
        }

        #region Public members

        /// <summary>
        /// Currently active Say Dialog used to display Say text
        /// </summary>
        public static SayDialog ActiveSayDialog { get; set; }

        /// <summary>
        /// Returns a SayDialog by searching for one in the scene or creating one if none exists.
        /// </summary>
        public static SayDialog GetSayDialog(SayDialog sayDialog = null)
        {
            ActiveSayDialog = null;
            if (sayDialog == null)
            {
                SayDialog sd = null;
                // Use first active Say Dialog in the scene (if any)
                if (activeSayDialogs != null)
                {
                    if (activeSayDialogs.Count > 0)
                    {
                        foreach (var saydia in activeSayDialogs)
                        {
                            if (saydia.name == "SayDialog")
                            {//
                                sd = saydia;
                                ActiveSayDialog = sd;
                                break;
                            }

                          /*  if (saydia.gameObject.activeInHierarchy== true)
                            {//
                                sd = saydia;
                                ActiveSayDialog = sd;
                                break;
                            }*/
                        }
                    }
                }

                if (ActiveSayDialog == null)
                {
                    // Auto spawn a say dialog object from the prefab
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/SayDialog");
                    if (prefab != null)
                    {
                        GameObject spDialog = Instantiate(prefab) ;
                        spDialog.SetActive(false);
                        spDialog.name = prefab.name;
                        spDialog.GetComponent<CanvasGroup>().alpha = 0;

                        ActiveSayDialog = spDialog.GetComponent<SayDialog>();
                    }
                }
            }
            else
            {
                SayDialog displayDialog = null;
                if (activeSayDialogs != null)
                {
                    foreach (var dialog in activeSayDialogs)
                    {
                        if (dialog.name == sayDialog.name)
                        {
                            displayDialog = dialog;
                        }
                    }
                }

                if (displayDialog == null)
                {
                    GameObject spDialog = Instantiate(sayDialog.gameObject);
                    spDialog.SetActive(false);
                    spDialog.name = sayDialog.name;
                    spDialog.GetComponent<CanvasGroup>().alpha = 0;
                    displayDialog = spDialog.GetComponent<SayDialog>();
                }
                Debug.Log("尋找的dialog=>" + displayDialog.gameObject.name);
                ActiveSayDialog = displayDialog;
            }
            //ActiveSayDialog.Init();
            return ActiveSayDialog;
        }
        /// <summary>
        /// 統一設定所有對話框(不同角色可能不同對話框)的自動狀態
        /// 必須這樣設置
        /// </summary>
        /// <param name="_switch"></param>
        public static void SwitchSayDialogAutoPlayOfStatus(bool _switch) 
        {
            foreach (var Dialog in activeSayDialogs)
            {
            Dialog.GetWriter().AutoPlay=_switch;
            }
        }

        /// <summary>
        /// 關閉所有對話框的活動狀態
        /// </summary>
        /// <param name="exception">例外不關閉的對話框名稱</param>
        public static void CloseDialogStatus(string exception=null)
        {
            foreach (var Dialog in activeSayDialogs)
            {
                if (exception!=null&& Dialog.name==exception) continue;
                
                Dialog.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Stops all active portrait tweens.
        /// </summary>
     /*   public static void StopPortraitTweens()
        {
            // Stop all tweening portraits
            var activeCharacters = Character.ActiveCharacters;
            for (int i = 0; i < activeCharacters.Count; i++)
            {
                var c = activeCharacters[i];
                if (c.State.portraitImage != null)
                {
                    if (LeanTween.isTweening(c.State.portraitImage.gameObject))
                    {
                        LeanTween.cancel(c.State.portraitImage.gameObject, true);
                        PortraitController.SetRectTransform(c.State.portraitImage.rectTransform, c.State.position);
                        if (c.State.dimmed == true)
                        {
                            c.State.portraitImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        }
                        else
                        {
                            c.State.portraitImage.color = Color.white;
                        }
                    }
                }
            }
        }*/



        /// <summary>
        /// Sets the active state of the Say Dialog gameobject.
        /// </summary>
        public virtual void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }

        public bool NowAlphaStatus()
        {

            if (canvasGroup.alpha > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Sets the active speaking character.設置角色的名字,底框,在對話框上的對話角色明暗 等資訊  
        /// </summary>
        /// <param name="character">The active speaking character.</param>
        public virtual IEnumerator SetCharacterInfo(Character character, Stage stage, Flowchart flowchart, string defaultName = "")
        {
            
            if (character == null)
            {
                
                PortraitChara = null;
                AvatarParent.gameObject.SetActive(false);

                yield return SetCharacterName(defaultName, Color.white, flowchart);
                speakingCharacter = null;
            }
            else
            {
                speakingCharacter = character;

                stage.JudgeCharaSetDimmed(speakingCharacter.name);//設定角色明暗

                string characterName = null;

                if (string.IsNullOrEmpty(defaultName))
                {
                    characterName = character.NameText;
                }
                else
                {
                    characterName = defaultName;
                }

                if (string.IsNullOrEmpty(characterName))
                {
                    // Use game object name as default
                    characterName = character.GetObjectName();
                }

                yield return SetCharacterName(characterName, character.NameColor, flowchart);
                //SetCharaterNameImage(character);

            }


        }

        public GameObject CreateCharacterOfPortrait(Stage stage)//必須偵測是否有其他角色在佔用
        {

            if (PortraitChara != null)
            {
                PortraitChara.gameObject.SetActive(false);
            }
            bool isHave = false;

            for (int i = 0; i < stage.SpineSkeletonGraphicOnStageList.Count; i++)
            {
                var child = stage.SpineSkeletonGraphicOnStageList[i];

                if (speakingCharacter.name == child.name)
                {
                    isHave = true;

                   // PortraitChara = child.GetComponent<CharaRenderer>();
                    PortraitChara.gameObject.SetActive(true);
                }
            }

            if (!isHave&& speakingCharacter.aSkeletonGraphic!=null)
            {

                    GameObject sp = Instantiate(speakingCharacter.aSkeletonGraphic.gameObject);
                    sp.transform.SetParent(AvatarParent, false);
                     sp.transform.localScale = Vector3.one;
                    PortraitChara = sp.GetComponent<SkeletonGraphic>();

                    RectTransform rect = PortraitChara.GetComponent<RectTransform>();

                    rect.anchoredPosition3D = new Vector3(0, -(rect.sizeDelta.y * rect.localScale.y), 0);

                    sp.name = speakingCharacter.name;

                    PortraitChara.gameObject.SetActive(true);

                 stage.SpineSkeletonGraphicOnStageList.Add(PortraitChara);
            }
            return PortraitChara.gameObject;
        }

        /// <summary>
        /// 設置對話
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="opt"></param>
        public void SetCharacterPortrait(GameObject Obj, SayDialogPortraitSetting opt)
        {
            AvatarParent.gameObject.SetActive(true);
            SkeletonGraphic skeleG = Obj.GetComponent<SkeletonGraphic>();
            skeleG.startingLoop = opt._loop;
            skeleG.startingAnimation = opt._animation;
            skeleG.initialSkinName = opt._skin;
            skeleG.SetSkinToChara();
            skeleG.SetPlayAnimation();

            // Adjust story text box to not overlap image rect
            if (fitTextWithImage &&
                StoryText != null && PortraitChara != null)
            {
                if (Mathf.Approximately(startStoryTextWidth, 0f))
                {
                    startStoryTextWidth = StoryTextRectTrans.rect.width;
                    startStoryTextInset = StoryTextRectTrans.offsetMin.x;
                }
                RectTransform portParentRect = AvatarParent.GetComponent<RectTransform>();

                // Clamp story text to left or right depending on relative position of the character image
                if (StoryTextRectTrans.position.x < portParentRect.position.x)
                {
                    StoryTextRectTrans.SetInsetAndSizeFromParentEdge(
                        RectTransform.Edge.Left,
                        startStoryTextInset,
                        startStoryTextWidth - portParentRect.rect.width);
                }
                else
                {
                    StoryTextRectTrans.SetInsetAndSizeFromParentEdge(
                        RectTransform.Edge.Right,
                        startStoryTextInset,
                        startStoryTextWidth - portParentRect.rect.width);
                }
            }

            /* if (!string.IsNullOrEmpty(opt._finishDefaultAnimation))
             {
                 yield return new WaitForSeconds(dur);

                 skeleG.startingAnimation = opt._finishDefaultAnimation;
                 skeleG.startingLoop = true;
                 skeleG.SetPlayAnimation();
             }*/
        }

        public void SetPlayFinishAnimation(GameObject obj, SayDialogPortraitSetting opt)
        {
           StartCoroutine(eSetPlayFinishAnimation(obj,opt));
        }

        /// <summary>
        /// 設置頭像動畫完成後的循環動畫,因為資料尚未設置齊全前 saydialog 的active狀態是false  所以必須分開設置
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public IEnumerator eSetPlayFinishAnimation(GameObject obj,SayDialogPortraitSetting opt)
        {
            var skeleG = obj.GetComponent<SkeletonGraphic>();
            float dur = skeleG.GetAnimationTime(opt._animation);

            if (!string.IsNullOrEmpty(opt._finishDefaultAnimation))
            {
                yield return new WaitForSeconds(dur);

                skeleG.startingAnimation = opt._finishDefaultAnimation;
                skeleG.startingLoop = true;
                skeleG.SetPlayAnimation();
            }
        }

        public void AdjustTextArea()
        {

            StoryTextRectTrans.anchorMin = new Vector2(0, 0.5f);
            StoryTextRectTrans.anchorMax = new Vector2(1, 0.5f);
            // StoryTextRectTrans.offsetMax = new Vector2(-25, StoryTextRectTrans.offsetMax.y);
            //  StoryTextRectTrans.offsetMin = new Vector2(25, StoryTextRectTrans.offsetMin.y);
            StoryTextRectTrans.offsetMax = storyTextOffsetMax;
            StoryTextRectTrans.offsetMin = storyTextOffsetMin;

            //  StoryTextRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);

            // LayoutRebuilder.ForceRebuildLayoutImmediate(StoryTextRectTrans);

            // StoryTextRectTrans.sizeDelta = new Vector2(-50, 150);

        }
        /// <summary>
        /// 記錄對話文字框的Rect詳細資訊
        /// </summary>
        private void RecordOrigineStoryTextRectTransformInfo()
        {
            storyTextOffsetMax = StoryTextRectTrans.offsetMax;
            storyTextOffsetMin= StoryTextRectTrans.offsetMin;
        }

        public void InitPortraitChara()
        {
            if (PortraitChara != null)
            {
                PortraitChara.gameObject.SetActive(false);
            }
        }




        /// <summary>
        /// Sets the character name to display on the Say Dialog.
        /// Supports variable substitution e.g. John {$surname}
        /// </summary>
        public virtual IEnumerator SetCharacterName(string mName, Color color, Flowchart flowchart)
        {
            if (NameText != null)
            {
                var subbedName = "";
                if (flowchart.useAssetText)
                {
                    yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile
                    (
                            mName,
                            AssetTextType.CharaNumber,
                            flowchart.mLanguage,
                            name => { subbedName = name; }
                    );
                    subbedName = stringSubstituter.SubstituteStrings(subbedName);
                }
                else
                {
                    subbedName = stringSubstituter.SubstituteStrings(mName);
                }
                
                if (string.IsNullOrWhiteSpace(subbedName)) 
                {
                    sentenceNameBG.gameObject.SetActive(false);
                    NameText = "";
                }
                else
                {
                    if (subbedName== "Narrator"|| subbedName == "SpNarrator")
                    {
                        sentenceNameBG.gameObject.SetActive(false);
                    }
                    else
                    {
                        sentenceNameBG.gameObject.SetActive(true);
                    }
                    NameText = subbedName;
                    nameTextAdapter.SetTextColor(color);
                }
            }
        }

     /*   /// <summary>
        /// 設置角色名稱底圖與特效
        /// </summary>
        /// <param name="chara"></param>
        public virtual void SetCharaterNameImage(Character chara)
        {

            if (nameImage == null)
            {
                GameObject sp = new GameObject("nameImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                sp.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100);
                sp.transform.SetParent(gameObject.transform.Find("Panel"));
                if (nameText.gameObject == null)
                {
                    nameText = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                    nameText.GetComponent<RectTransform>().anchorMax = Vector2.one;
                    nameText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                    nameText.fontSize = 20;
                    nameText.resizeTextForBestFit = true;
                    nameText.resizeTextMinSize = 20;
                    nameText.resizeTextMaxSize = 40;
                    nameText.alignment = TextAnchor.MiddleCenter;
                    nameText.fontStyle = FontStyle.Bold;
                }
                nameText.gameObject.transform.SetParent(sp.transform);
            }

            nameImage.enabled = true;
            if (chara.NameBGColor != null)
            {
                nameImage.color = chara.NameBGColor;
            }
            if (chara.NameBGSprite != null)
            {
                nameImage.sprite = chara.NameBGSprite;
            }
            else
            {
                nameImage.sprite = null;
            }
            nameImage.preserveAspect = true;
        }*/

        /// <summary>
        /// Write a line of story text to the Say Dialog. Starts coroutine automatically.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="clearPrevious">Clear any previous text in the Say Dialog.</param>
        /// <param name="waitForInput">Wait for player input before continuing once text is written.</param>
        /// <param name="fadeWhenDone">Fade out the Say Dialog when writing and player input has finished.</param>
        /// <param name="stopVoiceover">Stop any existing voiceover audio before writing starts.</param>
        /// <param name="voiceOverClip">Voice over audio clip to play.</param>
        /// <param name="onComplete">Callback to execute when writing and player input have finished.</param>
        public virtual IEnumerator Say(Sayinfo sayinfo)
        {
            sayinfo.whenDialogFinish = OpenContinueButton;
            yield return DoSay(sayinfo);
        }

        /// <summary>
        /// Write a line of story text to the Say Dialog. Must be started as a coroutine.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="clearPrevious">Clear any previous text in the Say Dialog.</param>
        /// <param name="waitForInput">Wait for player input before continuing once text is written.</param>
        /// <param name="fadeWhenDone">Fade out the Say Dialog when writing and player input has finished.</param>
        /// <param name="stopVoiceover">Stop any existing voiceover audio before writing starts.</param>
        /// <param name="voiceOverClip">Voice over audio clip to play.</param>
        /// <param name="onComplete">Callback to execute when writing and player input have finished.</param>
        public virtual IEnumerator DoSay(Sayinfo sayInfo)
        {

            var writer = GetWriter();

            if (writer.IsWriting || writer.IsWaitingForInput)
            {
                writer.Stop();
                while (writer.IsWriting || writer.IsWaitingForInput)
                {
                    yield return null;
                }
            }

            if (closeOtherDialogs)
            {
                for (int i = 0; i < activeSayDialogs.Count; i++)
                {
                    var sd = activeSayDialogs[i];
                    if (sd.gameObject != gameObject)
                    {
                        sd.SetActive(false);
                    }
                }
            }
            gameObject.SetActive(true);



            this.fadeWhenDone = sayInfo.fadeWhenDone;

            // Voice over clip takes precedence over a character sound effect if provided  

            AudioClip soundEffectClip = null;

            if (sayInfo.mouthAniState != Fungus.Say.MouthAnimation.DidNotPlay)//如果不是  不加載嘴部動畫
            {
                sayInfo.StartSayAni();
            }

            if (sayInfo.voiceOverClip != null)
            {
                WriterAudio writerAudio = GetWriterAudio();
                writerAudio.SetWriterAudioVolume(PlayerPrefs.GetFloat(Enum.GetName(typeof(SoundType), SoundType.VOICE), 1.0f));
                writerAudio.OnVoiceover(sayInfo.voiceOverClip);


                if (sayInfo.mouthAniState == Fungus.Say.MouthAnimation.WaitVoiceComplete)
                {

                    IEnumerator stopSayAniCountDown(float time)
                    {
                        Debug.Log(time + "秒後停止對話");
                        yield return new WaitForSeconds(time);
                        sayInfo.StopSayAni();
                    }
                    StartCoroutine(stopSayAniCountDown(sayInfo.voiceOverClip.length));
                }


            }

            else if (speakingCharacter != null)
            {
                soundEffectClip = speakingCharacter.SoundEffect;
            }

            writer.AttachedWriterAudio = writerAudio;

            yield return StartCoroutine(writer.Write(sayInfo));

            yield return ReactionSayDialogAlpha(!fadeWhenDone, fadeWhenDone);
            

            if (sayInfo.onComplete != null)
            {
                sayInfo.onComplete();
            }

            //        StartCoroutine(ReactionAlpha(fadeWhenDone));
        }
        /// <summary>
        /// 激活繼續下一段對話提示按鈕
        /// </summary>
        private void OpenContinueButton()
        {
            continueButton.gameObject.SetActive(true);
        }
        /// <summary>
        /// Tell the Say Dialog to fade out once writing and player input have finished.
        /// </summary>
        public virtual bool FadeWhenDone { get { return fadeWhenDone; } set { fadeWhenDone = value; } }

        /// <summary>
        /// Stop the Say Dialog while its writing text.
        /// </summary>
        public virtual void Stop()
        {
            fadeWhenDone = true;
            GetWriter().Stop();
        }

        /// <summary>
        /// Stops writing text and clears the Say Dialog.
        /// </summary>
        public virtual void Clear()
        {
            ClearStoryText();

            // Kill any active write coroutine
            StopAllCoroutines();
        }

        #endregion
    }
}
