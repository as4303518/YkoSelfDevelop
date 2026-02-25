using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using YKO.Support;
using DG.Tweening;
using UniRx;
using YKO.Support.Expansion;
using System.Net;
using YKO.Common;

namespace Fungus
{

    public class SnsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        //-------------------------------Parent--------------------

        public class LocalizeKey
        {
            public static readonly string ClickPointSetBlankSpace = "點擊任一空白處關閉訊息視窗";

            public static readonly string ClickInputMessageWithThis = "點此輸入訊息";
        }

        /// <summary>
        /// 選取的訊息完整文字
        /// </summary>
        [SerializeField] private Text detailOfChooseMessageText;

        /// <summary>
        /// 寄送按鈕
        /// </summary>
        [SerializeField] private Button SendMessageButton;
        

        /// <summary>
        /// 主訊息滾軸view
        /// </summary>
        [SerializeField] private RectTransform MessageScrollView;
        /// <summary>
        /// 主訊息滾軸Content
        /// </summary>
        [SerializeField] private RectTransform MessageContentParent;
        /// <summary>
        /// 選項view
        /// </summary>
        [SerializeField] private RectTransform optionScrollView;
        /// <summary>
        /// 選項content
        /// </summary>
        [SerializeField] private RectTransform optionContent;

        /// <summary>
        /// 關閉視窗母物件
        /// </summary>
        [SerializeField] private RectTransform filterParent;


        //-------------------------------List--------------------
        //  private bool AutoSendMessage = false;

        [SerializeField] private Text DialogObjNameText;
        /// <summary>
        /// 對話標題
        /// </summary>
        [SerializeField] private Text MessageTitleInfoText;


        public  List<CharaSnsSetting> mCharaSetting = new List<CharaSnsSetting>();

        public List<SnsMessage> SnsMessages = new List<SnsMessage>();//之後會逐一顯示的對話

        public List<SnsMessage> HistorySnsMessages = new List<SnsMessage>();//歷史對話

        private List<SnsMessage> DisplaySnsMessages = new List<SnsMessage>();//已經顯示出來的陣列

        private List<MessageCard> mMessageCards = new List<MessageCard>();


        //-------------------------------params--------------------

        private int curDisplaySnsCount = 0;//玩家觸碰後+1

        private bool CanAddMessage = true;
        /// <summary>
        /// 被選取的對話選項
        /// </summary>
        private SnsMessage chooseSnsMeg=null;



        /// <summary>
        /// 已生成的選項物件
        /// </summary>
        private List<SnsAnswerCell> optionObj = new List<SnsAnswerCell>();


        [SerializeField] private GameObject MessageCardPrefabs = null;

        [SerializeField] private GameObject ReplyOptionsPrefabs = null;

        [SerializeField] private GameObject SeparateLinePrefabs = null;

        private SnsManagerFunc aData;

        public  Action CloseWindowCallBack = null;//接續fungus劇情用


        //--------------------instance-------------------
        /*private static SnsManager instance=null;

        public static SnsManager GetInstance()
        {
            if (instance==null) {
                instance = GetSnsManager();
            }
            return instance;
        }
        private static SnsManager GetSnsManager()
        {
            return GameObject.FindFirstObjectByType<SnsManager>();
        }*/

        /// <summary>
        /// 有一個腳本要負責銜接block 分配點選哪些選項,就分配給哪些snsManager
        /// </summary>
        /// <param name="_Data"></param>
        /// <param name="isEffect"></param>
        /// <returns></returns>
        public IEnumerator Init(SnsManagerFunc _Data, bool isEffect = true)
        {
            if (isEffect)
            {
                LeanTweenManager.SetCanvasGroupAlpha(gameObject, 0);
            }
            aData = _Data;
            //MessageTitleInfoText.text = _Data.TopicLabel;
            if (aData.flowchart.useAssetText) {
                yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                    aData.DialogName,
                    AssetTextType.CharaNumber,
                    aData.flowchart.mLanguage,
                    str => { DialogObjNameText.text = str; }
                 );
            }
            else
            {
                DialogObjNameText.text = aData.DialogName;
            }

          //  DialogObjNameText.text = aData.DialogName;
            mCharaSetting = aData.DialogChara;
            HistorySnsMessages = aData.HistorySns;

            yield return SetOrigineMessage();

            if (isEffect) {
                yield return TweenIn();
            }
            aData.OnComplete();
            Register();

           // yield return AutoStartDialogue();

        }
        /// <summary>
        /// 註冊按鈕
        /// </summary>
        private void Register()
        {
            SendMessageButton.OnClickAsObservable().Subscribe(_ => {
                if (chooseSnsMeg!=null)
                {
                    detailOfChooseMessageText.text = LocalizeKey.ClickInputMessageWithThis;
                    StartCoroutine(SetMessage(chooseSnsMeg));
                    SwitchSendButtonStatus(ButtonStatus.Enable);
                    chooseSnsMeg = null;
                }
            });


            this.ObserveEveryValueChanged(x => x.aData.flowchart.mStoryControl.isSkipPlay)
             .Subscribe(skipStatus =>
             {
                 if (skipStatus&&!CanAddMessage) 
                 {
                     if (chooseSnsMeg!=null) 
                     {
                         SwitchSendButtonStatus(ButtonStatus.Enable);
                         //  chooseSnsMeg = null;必須留著讓顯示完劇情的OnComplete執行 targetblock
                     }
                     else
                     {
                         if (optionObj.Count>0)
                         {
                             chooseSnsMeg = SnsMessages[SnsMessages.Count - 1];
                             chooseSnsMeg.curTargetBlock = chooseSnsMeg.mMessageType._replyMessage[0].targetBlock;
                         }
                         else
                         {
                             Debug.LogError("選項為空,SnsReply尚未設置,無法獲取預設的值");
                         }
                     }
                     CanAddMessage = true;
                     detailOfChooseMessageText.text = LocalizeKey.ClickInputMessageWithThis;
                 }

             });

            this.ObserveEveryValueChanged(x => x.aData.flowchart.mStoryControl.isAutoPlay)
             .Subscribe(autoStatus =>
             {
                 if (optionObj.Count==1) 
                 {
                     chooseSnsMeg = null;
                     SwitchSendButtonStatus(ButtonStatus.Enable);
                     var sns = SnsMessages[curDisplaySnsCount];
                     sns.mMessageType._message = sns.mMessageType._replyMessage[0].message;
                     detailOfChooseMessageText.text = LocalizeKey.ClickInputMessageWithThis;
                     StartCoroutine( SetMessage(sns));
                 }
             });

            this.ObserveEveryValueChanged(script => script.MessageContentParent.sizeDelta.y)
              .Where(v=>MessageContentParent.pivot.y>0)
             .Subscribe(v => {

                 if (v>= MessageScrollView.sizeDelta.y)
                 {
                     MessageContentParent.pivot = new Vector2(0.5f, 0);
                     MessageScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
                 }
             });


        }

        /// <summary>
        /// 設置歷史對話
        /// </summary>
        /// <param name="isEffect">可選擇不需要sns對話框的特效,如果已經開啟,則可跳過</param>
        /// <returns></returns>
        private IEnumerator SetOrigineMessage()
        {
            int finishCount = 0;
            foreach (SnsMessage sns in HistorySnsMessages)
            {
                yield return SetMessageSetting(sns);
                yield return SetMessage(sns, () => { finishCount++; },false);
            }
            DisplaySnsMessages.Clear();//歷史對話完成後,重新計算顯示順序與連在一起的頭像訊息
            Debug.Log("歷史對話完成");
        }



     /*   private IEnumerator AutoStartDialogue()//自動對話點擊並進行(正常情況執行)
        {
            while (curDisplaySnsCount < SnsMessages.Count)
            {
                yield return SnsDialogue();
            }
                StartCoroutine(EndSnsWindow());

        }*/

        //onComolete for fungus func continue to setting
        /// <summary>
        /// fungus因應臨時對話系統(根據不同選項,給出不同回答)
        /// </summary>
        /// <param name="addSns"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public IEnumerator SetDialogue(SnsMessage addSns, Action onComplete = null)
        {
            SnsMessages.Add(addSns);
            while (!CanAddMessage)
            {
                yield return null;
            }

            yield return SnsDialogue(onComplete);

        }
        /// <summary>
        /// sns對話
        /// </summary>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        private IEnumerator SnsDialogue(Action onComplete=null)
        {
            SnsMessage sns = SnsMessages[curDisplaySnsCount];
            if (CanAddMessage)
                {
                    sns.aFade = true;
                   yield return SetMessageSetting(sns);

                    CanAddMessage = false;
                    switch (sns.mMessageType._snsType)
                    {
                        case SnsType.Message:
                         if (SnsMessages.Count > 0)
                         {
                             yield return AutoDialogForMessage(sns);
                        }
                        else
                        {
                            Debug.LogError("沒設置Sns對話");
                        }
                            break;
                        case SnsType.Reply :

                            yield return CreateReplyArea(sns);
                        
 
                        break;
                    case SnsType.OneClickReply:

                            yield return CreateOneClickReply(sns);

                        break;
                        case SnsType.Image:
                           yield return AutoDialogForMessage(sns);
                        break;
                    }
                    curDisplaySnsCount++;
                }
                yield return new WaitUntil(() => CanAddMessage);
            if (onComplete != null)
            {
                onComplete();
            }

 
            //對話完 關閉視窗
        }

        private IEnumerator AutoDialogForMessage(SnsMessage sns)
        {

        //    yield return new WaitForSeconds(sns.mMessageType._dialogWaitTime);

            StartCoroutine(SetMessage(sns));

            yield return new WaitUntil(() => CanAddMessage);

        }
        /*
        /// <summary>
        /// 每次點擊出現下一段對話
        /// </summary>
        /// <param name="sns"></param>
        /// <returns></returns>
        private IEnumerator CreateDialogArea(SnsMessage sns)
        {

             InputCallBack.InputOptions opt = new InputCallBack.InputOptions();
             opt.parentPos = OptionPanelParent;
             opt.touchSize = new Vector2(1080,350);

             yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(
                 ClickMode.ClickOnButton,
                 () => { 
                     StartCoroutine(SetMessage(sns)); 
                 },
                 opt);

            StartCoroutine(SetMessage(sns));

            yield return new WaitUntil(() => CanAddMessage);
        }*/
     
        
        /// <summary>
        /// 生成回覆選項
        /// </summary>
        /// <param name="sns"></param>
        /// <returns></returns>
        private IEnumerator CreateReplyArea(SnsMessage sns)
        {

            //關閉其他回復按鈕被選擇的狀態
            void CloseReplyBtnStatus()
            {
                foreach (var btn in optionObj)
                { 
                   // btn.GetComponent<SnsAnswerCell>().SwitchAreaBtn(false);
                    btn.CancelChoose();
                }
            }

            if (sns.mMessageType._replyMessage.Length>2)
            {
                optionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            }
            else
            {
                optionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            }
            //避免多選項任何數值所上的保險
            if (sns.mMessageType._replyMessage.Length <= 0)
            {
                GameObject sp = Instantiate(ReplyOptionsPrefabs, optionContent);
                //LeanTweenManager.SetCanvasGroupAlpha(sp, 0);

                var displayStr = "";
                if (sns.useAssetText) 
                {
                        yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                        sns.mMessageType._message,
                        sns.mLanguage,
                        str => {
                            displayStr = str;
                        }
                        );
                }
                else
                {
                    displayStr = sns.mMessageType._message;
                }
                var snsCell = sp.GetComponent<SnsAnswerCell>();
                snsCell.Init(
                    new SnsAnswerCell.SnsAnswerCellData(displayStr, () => {
                        chooseSnsMeg = sns;
                        detailOfChooseMessageText.text = displayStr;
                        //chooseSnsMeg.curTargetBlock=chooseSnsMeg.mMessageType
                        CloseReplyBtnStatus();
                        SwitchSendButtonStatus(ButtonStatus.Choose);
                    }
                    ));
                optionObj.Add(snsCell);
            }
            else//多選項
            {
                foreach (var option in sns.mMessageType._replyMessage)
                {
                    GameObject sp = Instantiate(ReplyOptionsPrefabs, optionContent);
                    var displaySimpleStr = "";//
                    var displayStr = "";
                    if (sns.useAssetText)
                    {
                        yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(option.introduction, sns.mLanguage, str =>
                        {
                            displaySimpleStr= str;
                        });
                        yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(option.message, sns.mLanguage, str =>
                        {
                            displayStr = str;
                        }); 
                    }
                    else
                    {
                        displaySimpleStr = option.introduction;
                    }

                    var snsCell=sp.GetComponent<SnsAnswerCell>();
                    snsCell.Init(
                        new SnsAnswerCell.SnsAnswerCellData(displaySimpleStr, () => {
                            chooseSnsMeg = sns;
                            detailOfChooseMessageText.text = displayStr;
                            sns.mMessageType._message = option.message;
                            sns.curTargetBlock = option.targetBlock;
                            CloseReplyBtnStatus();
                            SwitchSendButtonStatus(ButtonStatus.Choose);
                        }));

                    optionObj.Add(snsCell);

                }
            }

            if (aData.flowchart.mStoryControl.isAutoPlay&&optionObj.Count==1)
            {
                sns.mMessageType._message = sns.mMessageType._replyMessage[0].message;
                sns.curTargetBlock = sns.mMessageType._replyMessage[0].targetBlock;
                StartCoroutine(SetMessage(sns));
            }
            else
            {
                yield return ReplyScrollViewPanelSwitch(true);
                yield return new WaitUntil(() => CanAddMessage);
                yield return ReplyScrollViewPanelSwitch(false);
            }

            foreach (var obj in optionObj)
            {
                Destroy(obj.gameObject);
            }
            optionObj.Clear();

        }
        /// <summary>
        /// 生成點擊一次即發送的選項回復
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateOneClickReply(SnsMessage sns)
        {

         /*   void CloseReplyBtnStatus()
            {
                foreach (var btn in optionObj)
                {
                    btn.GetComponent<SnsAnswerCell>().SwitchAreaBtn(false);
                }
            }*/

            if (sns.mMessageType._replyMessage.Length > 2)
            {
                optionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            }
            else
            {
                optionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            }

            //避免多選項任何數值所上的保險
            if (sns.mMessageType._replyMessage.Length <= 0)
            {
                GameObject sp = Instantiate(ReplyOptionsPrefabs, optionContent);
                //LeanTweenManager.SetCanvasGroupAlpha(sp, 0);

                var displayStr = "";
                if (sns.useAssetText)
                {
                    yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                    sns.mMessageType._message,
                    sns.mLanguage,
                    str => {
                        displayStr = str;
                    }
                    );
                }
                else
                {
                    displayStr = sns.mMessageType._message;
                }
                var snsCell = sp.GetComponent<SnsAnswerCell>();
                snsCell.Init(
                    new SnsAnswerCell.SnsAnswerCellData(displayStr, () => {
                        //chooseSnsMeg = sns;
                      //  detailOfChooseMessageText.text = displayStr;
                        // CloseReplyBtnStatus();
                        StartCoroutine(SetMessage(sns));
                    }
                    ));
                optionObj.Add(snsCell);
            }
            else//多選項
            {
                foreach (var option in sns.mMessageType._replyMessage)
                {
                    GameObject sp = Instantiate(ReplyOptionsPrefabs, optionContent);
                    var displaySimpleStr = "";//
                    var displayStr = "";
                    if (sns.useAssetText)
                    {
                        yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(option.introduction, sns.mLanguage, str =>
                        {
                            displaySimpleStr = str;
                        });
                        yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(option.message, sns.mLanguage, str =>
                        {
                            displayStr = str;
                        });
                    }
                    else
                    {
                        displaySimpleStr = option.introduction;
                    }

                    var snsCell = sp.GetComponent<SnsAnswerCell>();
                    snsCell.Init(
                        new SnsAnswerCell.SnsAnswerCellData(displaySimpleStr, () => {
                            Debug.Log("自動設定=>"+option.message);
                            sns.mMessageType._message = option.message;
                            sns.curTargetBlock = option.targetBlock;
                            //CloseReplyBtnStatus();
                            StartCoroutine(SetMessage(sns));
                        }
                        ));

                    optionObj.Add(snsCell);

                }
            }

            if (aData.flowchart.mStoryControl.isAutoPlay && optionObj.Count == 1)
            {
                sns.mMessageType._message = sns.mMessageType._replyMessage[0].message;
                sns.curTargetBlock = sns.mMessageType._replyMessage[0].targetBlock;
                StartCoroutine(SetMessage(sns));
            }
            else
            {
                yield return ReplyScrollViewPanelSwitch(true);
                yield return new WaitUntil(() => CanAddMessage);
                yield return ReplyScrollViewPanelSwitch(false);
            }

            foreach (var obj in optionObj)
            {
                Destroy(obj.gameObject);
            }
            optionObj.Clear();


        }


        /// <summary>
        /// 設置回覆
        /// </summary>
        /// <param name="sns"></param>
        /// <param name="onComplete"></param>
        /// <param name="needSeparateLine"></param>
        /// <returns></returns>
        private IEnumerator SetMessage(SnsMessage sns,Action onComplete=null,bool needSeparateLine=true)
        {
            DisplaySnsMessages.Add(sns);//必須先抓對話 因為生成messageCard需要時間 如果晚加入,會讓後面的邏輯誤判

            if (needSeparateLine && curDisplaySnsCount == 0 && HistorySnsMessages.Count > 0)  //訊息分隔線
            {
                CreateSeparateLine(aData.TopicLabel);
            }
            
            if (IsLastOneOfSameCharacter(sns.mChara.mName))
            {
                MessageCard mc = mMessageCards[(mMessageCards.Count - 1)];
                sns.mChara.mAvatar= null;
                
                yield return mc.Init(sns);
            }
            else
            {
                sns.NotHaveAvatar = false;//文字段落連貫
                MessageCard mc = Instantiate(MessageCardPrefabs, MessageContentParent).GetComponent<MessageCard>();
                Debug.Log("生成card");
                mc.gameObject.SetCanvasGroup(0);
                mMessageCards.Add(mc);
             //   mc.transform.SetParent(MessageContentParent, false);
                yield return mc.Init(sns,true);
            }

          /*  if (MessageContentParent.sizeDelta.y>=1200) 
            {
                MessageContentParent.pivot = new Vector2(0.5f, 0);

                MessageScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
            }*/


            if (onComplete!=null) 
            {
                onComplete();
            }
            CanAddMessage = true;

        }
        /// <summary>
        /// 回覆對話框開啟與關閉狀態
        /// </summary>
        /// <returns></returns>
        public IEnumerator ReplyScrollViewPanelSwitch(bool _switch)
        {
            optionScrollView.DOKill();
            if (_switch)
            {
                MessageScrollView.DOSizeDelta(new Vector2(970, 820), 0.5f)
                .OnUpdate(() => {MessageScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;});
                yield return optionScrollView.DOSizeDelta(new Vector2(optionScrollView.sizeDelta.x,350),0.5f ).WaitForCompletion();
            }
            else
            {
                MessageScrollView.DOSizeDelta(new Vector2(970, 1170), 0.5f);
                yield return optionScrollView.DOSizeDelta(new Vector2(optionScrollView.sizeDelta.x, 0), 0.5f).WaitForCompletion(); 
            }
        }


        /// <summary>
        /// 生成分隔線
        /// </summary>
        /// <param name="content"></param>
        private void CreateSeparateLine(string content)
        {
            GameObject sepLine = Instantiate(SeparateLinePrefabs);
            sepLine.transform.SetParent(MessageContentParent, false);
            sepLine.transform.Find("TextHorizontalMask").Find("TopicTitle").GetComponent<Text>().text=content;
        }


        private IEnumerator SetMessageSetting( SnsMessage sns) {

            foreach (var chara in mCharaSetting)
            {//幫角色設置方向數值

                if (chara.mFungusChara.NameText == sns.mChara.mName)
                {
                    sns.mChara.mDirection = chara.mDirection;

                    switch (chara.mCharaRole)
                    {
                        case CharaRole.self :
                            if (sns.mMessageType._snsType == SnsType.None)
                            {
                                sns.mMessageType._snsType = SnsType.Reply;
                            }
                            break;
                        case CharaRole.otherSide:
                            if (sns.mMessageType._snsType == SnsType.None)
                            {
                                sns.mMessageType._snsType = SnsType.Message;
                            }
                            break;
                    }

                    if (chara.mFungusChara.charaAvatar == null)
                    {
                        yield return LoadAssetManager.LoadAsset<Sprite>(
                           string.Format( FungusResourcesPath.SnsAvatarSprite, ConvertInfoScript.GetCharaNumString(  chara.mFungusChara.NameText)) ,
                        res => {
                            chara.mFungusChara.charaAvatar = res;
                        }) ;
                    }
                    sns.mChara.mAvatar = chara.mFungusChara.charaAvatar;
                }

            }//---foreach

        }

        /// <summary>
        /// 是否與上一個回答的角色相同
        /// </summary>
        /// <param name="judgeName"></param>
        /// <returns></returns>
        private bool IsLastOneOfSameCharacter(string judgeName)
        {

            if (DisplaySnsMessages.Count < 2)
            {
                return false;
            }

            if (DisplaySnsMessages[DisplaySnsMessages.Count - 2].mChara.mName == judgeName)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public IEnumerator EndSnsWindow(Action endCB=null)
        {
            // yield return CreateCloseWindowTipText();

            CreateSeparateLine(LocalizeKey.ClickPointSetBlankSpace);

            InputCallBack.InputOptions opt = new InputCallBack.InputOptions();


            var rect = gameObject.transform.parent.GetComponent<RectTransform>();

            opt.parentPos = filterParent;
            opt.touchSize = new Vector2(rect.sizeDelta.x,rect.sizeDelta.y);

         /*   while (aData.flowchart.mStoryControl.isSkipPlay)
            {
            }*/

                yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(
                    ClickMode.ClickOnButton,
                    () => {
                        StartCoroutine(CloseSnsWindow(endCB));
                    },
                    opt);
            
        }
        /// <summary>
        /// 檢查寄出按鈕是否在閃爍狀態(減少玩家每次點選項,寄出按鈕動畫都要重跑得設置感
        /// </summary>
        private bool isFlashAni = false;
        /// <summary>
        /// 設定寄出按鈕的狀態
        /// </summary>
        /// <param name="status"></param>
        private void SwitchSendButtonStatus(ButtonStatus status)
        {

            switch (status) {
                case  ButtonStatus.Enable:
                    SendMessageButton.GetComponent<Image>().DOKill();
                    SendMessageButton.interactable = true;
                    SendMessageButton.GetComponent<Image>().DOColor(Color.white, 0.2f);
                    isFlashAni = false;
                    break;
                case ButtonStatus.Choose:
                    SendMessageButton.interactable = true;
                   // SendMessageButton.GetComponent<Image>().DOColor(new Color(0.85f, 0.17f, 0.21f), 0.2f);
                    var tarColor = new Color(0.85f, 0.17f, 0.21f);
                    if (!isFlashAni) 
                    {
                        SendMessageButton.GetComponent<Image>().DOKill();
                        isFlashAni = true;
                        SendMessageButton.GetComponent<Image>().color = Color.white;
                        SendMessageButton.GetComponent<Image>().DOColor(tarColor, 0.7f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                    }
                    break;
                case ButtonStatus.UnEnable:
                    SendMessageButton.GetComponent<Image>().DOKill();
                    SendMessageButton.GetComponent<Image>().color = Color.white;
                    SendMessageButton.interactable = false;
                    isFlashAni = false;
                    break;
            }

        }


      /*  private IEnumerator CreateCloseWindowTipText()
        {
            ResourceRequest resqu = Resources.LoadAsync<Font>(FungusResourcesPath.Font + "Amaranth-Bold");
            yield return new WaitUntil(() => resqu.isDone);

            GameObject BGimage = new GameObject("Background", typeof(RectTransform), typeof(Image));
            BGimage.transform.SetParent(transform, false);
            BGimage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            RectTransform imgRect = BGimage.GetComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(800, 250);

            imgRect.anchoredPosition = new Vector2(0, -750);
            Image img = BGimage.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);

            img.raycastTarget = false;
            GameObject textObj = new GameObject("Tip Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(BGimage.transform, false);
            Text text = textObj.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            text.fontSize = 70;
            text.raycastTarget = false;

            yield return TranslateOfCsv.GetSpecifyValueOfCsvFile(
                "CloseSnsWindowFormStory", 
                AssetTextType.UINumber,
                aData.flowchart.mLanguage, 
                str => { text.text = str; });

           // text.text = "Click Here To Close Window";
            //需要賦予字體
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(800, 250);
            text.font = resqu.asset as Font;

           StartCoroutine(LeanTweenManager.FadeIn(BGimage, () => { TweenManager.UIFlash(textObj, 1); }));
           StartCoroutine( LeanTweenManager.RectTransScale(BGimage, Vector3.one));
           // TweenManager.UIFlash(textObj,1);

        }*/
        private IEnumerator TweenIn()
        {

                StartCoroutine(LeanTweenManager.FadeIn(gameObject, 0.2f));
                yield return LeanTweenManager.RectTransScale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.15f);
                yield return LeanTweenManager.RectTransScale(gameObject, Vector3.one, 0.05f);
            
        }
        private IEnumerator CloseSnsWindow(Action endCB = null)
        {

            StartCoroutine(LeanTweenManager.FadeOut(gameObject, 0.2f));

            yield return LeanTweenManager.RectTransScale(gameObject, new Vector3(1.1f,1.1f,1.1f),0.05f);

            yield return LeanTweenManager.RectTransScale(gameObject, Vector3.zero,0.15f);

            if (endCB!=null) {
                endCB();
            }
            Destroy(gameObject);
        }

        [ExecuteAlways]
        public  List<string> GetCharacterArray()
        {

            List<string> list = new List<string>();

            if (mCharaSetting.Count < 1)
            {
                return list;
            }

            foreach (var name in mCharaSetting) {
                if (name.mFungusChara==null) {
                    break;
                }
                if (name.mFungusChara.NameText!=null&& name.mFungusChara.NameText!="") {
                    list.Add(name.mFungusChara.NameText);
                }
            
            }

            return list;

        }

        
        
    }
    /// <summary>
    /// 設定按鈕狀態
    /// </summary>
    public enum ButtonStatus
    {
        /// <summary>
        /// 平常狀態
        /// </summary>
        Enable,
        Choose,
        UnEnable
    }

    [Serializable]  
    public class SnsMessage
    {
        /// <summary>
        /// 是否擁有頭像
        /// </summary>
        [HideInInspector] public bool NotHaveAvatar = false;
        /// <summary>
        /// 淡入(顯示歷史訊息不用等待淡入的時間,所以必須新增此參數
        /// </summary>
        [HideInInspector] public bool aFade = false;
        /// <summary>
        /// 多語言
        /// </summary>
        public SettingLanguage mLanguage;
        /// <summary>
        /// 是否啟用多語言
        /// </summary>
        public bool useAssetText = false;

        /// <summary>
        /// 選擇選項後進入的目標block
        /// </summary>
        public Block curTargetBlock = null;
        /// <summary>
        /// 角色資訊
        /// </summary>
        [CharaDropOptions]
        public SnsChara mChara;
        /// <summary>
        /// 角色語言
        /// </summary>
        [SnsMessageProp]
        public SnsMessageType mMessageType;


    }

    /// <summary>
    /// 角色資訊
    /// </summary>
    [Serializable]
    public class SnsChara
    {

        [HideInInspector] public Sprite mAvatar = null;

        public string mName = "";

        [HideInInspector] public List<CharaSnsSetting> Charas = null;

        [HideInInspector] public Direction mDirection = Direction.Left;
    }

    [Serializable]
    public class SnsMessageType
    {
        [HideInInspector] public SnsType _snsType = SnsType.None;
        /// <summary>
        /// 傳遞的訊息(單選項
        /// </summary>
        public string _message;
        /// <summary>
        /// 複選項的傳遞訊息(通常為玩家自己選擇對話
        /// </summary>
        public ReplyAnswer[] _replyMessage = new ReplyAnswer[0];//不同回答,不同答案
        /// <summary>
        /// 對話傳送的圖片(
        /// </summary>
        public Sprite _sprite=null;
        /// <summary>
        /// 等待傳遞訊息的時間
        /// </summary>
        public float _dialogWaitTime = 0;
    }
    [Serializable]
    public class ReplyAnswer
    {
        [BlockList]
        public Block targetBlock = null;
        /// <summary>
        /// 簡介
        /// </summary>
        public string introduction;
        /// <summary>
        /// 實際句子
        /// </summary>
        public string message;
    }
    [Serializable]
    public class CharaSnsSetting
    {
        public Character mFungusChara;

        public CharaRole mCharaRole = CharaRole.otherSide;

        public Direction mDirection = Direction.Left;

    }
    /// <summary>
    /// 在SNS中出現的方位
    /// </summary>
    public enum Direction
    {
        Left,
        Right
    }
    /// <summary>
    /// 在SNS中扮演他人或自己
    /// </summary>
    public enum CharaRole
    {
        otherSide,
        self,

    }
    /// <summary>
    /// 在SNS中回覆訊息的格式(EX:圖片,Message等
    /// </summary>
    public enum SnsType // 
    {
        None,
        /// <summary>
        /// 自動回覆
        /// </summary>
        Message,
        /// <summary>
        /// 玩家選擇,多單選回覆(須選擇對話,並按下發送
        /// </summary>
        Reply,
        /// <summary>
        /// 玩家選擇,多單選回覆(點擊一次
        /// </summary>
        OneClickReply,
        Image,


    }

    public class SnsManagerFunc
    {
        /// <summary>
        /// 話題名稱(尚未多語言翻譯
        /// </summary>
        public string TopicLabel;
        /// <summary>
        /// Sns話題登場角色
        /// </summary>
        public List<CharaSnsSetting> DialogChara;
        /// <summary>
        /// 歷史訊息
        /// </summary>
        public List<SnsMessage> HistorySns;
        /// <summary>
        /// 對話角色
        /// </summary>
        public string DialogName = "";
        /// <summary>
        /// 在fungus裡是用來執行接下來動作的func
        /// </summary>
        public Action OnComplete = null;
        /// <summary>
        /// 主要執行的劇情
        /// </summary>
        public Flowchart flowchart=null;
        public SnsManagerFunc(Flowchart _flowchart,string topicLabel,string _dialogName, List<CharaSnsSetting> _dialogChara, List<SnsMessage> _historySns, Action _initOnComplete)
        {
            TopicLabel=topicLabel;
            DialogName = _dialogName;
            DialogChara = _dialogChara;
            HistorySns = _historySns;
            OnComplete = _initOnComplete;
            flowchart = _flowchart;
        }
    }



    public class CharaDropOptions:PropertyAttribute
    {
    }

    public class SnsMessagePropAttribute : PropertyAttribute
    {
    }



}
