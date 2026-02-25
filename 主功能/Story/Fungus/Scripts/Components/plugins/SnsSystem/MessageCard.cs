using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using YKO.Support.Expansion;
using Unity.VisualScripting;
using YKO.Support;
using UnityEngine.Localization.SmartFormat;
using YKO.Common;

namespace Fungus
{
    public class MessageCard : MonoBehaviour
    {

        [SerializeField] private Image AvatarImage;

       // [SerializeField] private GameObject ConversationCloud;

        [SerializeField] private Text PlayerName;

        [SerializeField] private Transform MessageContentListParent;

        [SerializeField] private GameObject MessagePrefab = null;


        /// <summary>
        /// 假的讀取狀態的coroutine(方便停止動畫
        /// </summary>
        private Coroutine fakeReceiveLoadCoroutine = null;

        private MessageSetting megSet;

        public MessageSetting MySetting { get { return megSet; } }
        /// <summary>
        /// 劇情sns
        /// </summary>
        /// <param name="sns"></param>
        /// <param name="firstInit"></param>
        /// <returns></returns>
        public IEnumerator Init(SnsMessage sns,bool firstInit=false)
        {
            if (firstInit)//必須先隱蔽,不然轉換多語的加載時間會出現類似閃頭像跟名字的問題
            {
                gameObject.SetCanvasGroup(0);
            }
            var talkMessage="";
            if (sns.useAssetText)
            {
                yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                sns.mMessageType._message,
                sns.mLanguage,
                str => talkMessage = str
                );
            }
            else
            {
                talkMessage = sns.mMessageType._message;
            }
            var charaName = "";
            if (sns.useAssetText)
            {
                yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                sns.mChara.mName,
                AssetTextType.CharaNumber,
                sns.mLanguage,
                str =>charaName = str
                );
            }
            else
            {
                charaName = sns.mChara.mName;
            }
            var _data = new MessageCardData(sns.mMessageType._snsType,  charaName, sns.mChara.mAvatar, talkMessage, sns.mMessageType._sprite)
            {
                mDirection = sns.mChara.mDirection,
                aFade = sns.aFade,
                waitDurTime = sns.mMessageType._dialogWaitTime,
                intervalX=-30,
            };

            if (firstInit)
            {
                megSet.loadFinish = false;
                SetGroupSetting(_data);
            }
            yield return new WaitUntil(() => megSet.loadFinish);
            yield return SendMessage(_data, firstInit);
        }
        /// <summary>
        /// 系統sns
        /// </summary>
        /// <param name="aData"></param>
        /// <param name="firstInit"></param>
        /// <returns></returns>
        public IEnumerator Init(MessageCardData aData, bool firstInit = false)
        {
            //Debug.Log("有加載嗎?=>"+aData.message);
            if (firstInit) {
                megSet.charaName=aData.charaName;
                megSet.loadFinish = false;
                gameObject.SetCanvasGroup(0);
                 SetGroupSetting(aData);
            }
            yield return new WaitUntil(() => megSet.loadFinish);
            yield return SendMessage(aData,firstInit);
        }

        /// <summary>
        /// 設置部分UI更新位置與效果(靠左或靠右?,間隔等等
        /// </summary>
        private void SetGroupSetting(MessageCardData aData)
        {

            string megBgResName = "message_bottom_{0}";
            var textRect = PlayerName.GetComponent<RectTransform>();

            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x + (aData.intervalX * 2), textRect.sizeDelta.y);

            switch (aData.mDirection)
            {//預設為左
                
                case Direction.Right://自己

                    //PlayerName.alignment = TextAnchor.MiddleRight;
                  //  AvatarImage.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(425 + aData.intervalX, -125);

                    PlayerName.gameObject.SetActive(false);
                    AvatarImage.transform.parent.gameObject.SetActive(false);
                    MessageContentListParent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperRight;
                    MessageContentListParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(450 + aData.intervalX, -20);

                 //   ConversationCloud.GetComponent<RectTransform>().anchoredPosition = new Vector2(300 + aData.intervalX, -120);
                 //   ConversationCloud.transform.eulerAngles = new Vector3(0, 180, 0);
                    megBgResName = megBgResName.FormatSmart("me");
                    megSet.textColor = Color.white;
                    break;

                case Direction.Left://對方
                    PlayerName.alignment = TextAnchor.MiddleLeft;
                    AvatarImage.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-425 - aData.intervalX, -125);
                    MessageContentListParent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
                    MessageContentListParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300 - aData.intervalX, -80);
                  //  ConversationCloud.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300 - aData.intervalX, -120);
                    megBgResName = megBgResName.FormatSmart("cha");
                    megSet.textColor = Color.black;
                    break;
            }

            

            StartCoroutine( LoadAssetManager.LoadAsset<Sprite>( FungusResourcesPath.SnsSettingSprite.FormatSmart(megBgResName), res => {
                if (res != null)
                {
                    megSet.BGStyle = res;
                    megSet.loadFinish = true;
                }
            }));

        }       
        /// <summary>
        /// 寄出訊息至該角色對話群組
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendMessage(MessageCardData aData,bool isFirst)
        {
            GameObject sp = Instantiate(MessagePrefab);

            sp.transform.SetParent(MessageContentListParent, false);

            if (isFirst)
            {
                //to do 設置頭像
               // Debug.Log("第一次加載=>"+aData.charaName);
                AvatarImage.sprite = aData.mAvatar;
                PlayerName.text = aData.charaName;
            }
            //RectTransform BgRect = sp.GetComponent<RectTransform>();
            Transform child = null;
            string displayStr = "";
            

            if (aData.snsType==SnsType.Image) {

                child = sp.transform.Find("ContentImage");
                child.gameObject.SetActive(true);
                sp.transform.Find("ContentText").gameObject.SetActive(false);
            }
            else
            {
                sp.GetComponent<Image>().sprite = megSet.BGStyle;
                child = sp.transform.Find("ContentText");
                displayStr = aData.message;
            }

            Transform OrigineTransform = null;
            OrigineTransform = gameObject.transform.parent;
            gameObject.transform.SetParent(OrigineTransform.parent);
            gameObject.transform.SetParent(OrigineTransform);

            sp.SetActive(true);

            if (aData.snsType == SnsType.Message || aData.snsType == SnsType.Reply|| aData.snsType == SnsType.OneClickReply)
            {
                Text displayText = child.GetComponent<Text>();
                displayText.color = megSet.textColor;


                if (aData.waitDurTime > 0&& aData.snsType == SnsType.Message)
                {
                    fakeReceiveLoadCoroutine = StartCoroutine(FakeReceiveReply(aData,displayText));
                }
                else
                {
                   // child.gameObject.SetCanvasGroup(0);
                    displayText.text = displayStr;
                    UpdateTextWidth(aData,child.GetComponent<RectTransform>());
                }

                if (aData.aFade)
                {//可以判斷是不是切換角色傳訊息了
                    if (isFirst)
                    {
                        yield return gameObject.eSetCanvasGroup(1);
                    }
                    else
                    {
                        yield return sp.eSetCanvasGroup(1);
                    }
                }

              //  cg.alpha = 1;

                if (aData.waitDurTime > 0)
                {
                    yield return DisplayMessage(aData,displayText, displayStr);
                }
                else //歷史回顧,如果對話完成直接顯示
                {
                    //Debug.Log("有加載嗎3?=>" + aData.message);
                    gameObject.SetCanvasGroup(1);
                }
               // aData.onMessageDisplayerCallBack?.Invoke();
            }
            //目前還是顯示加載中
        }//---sendMessage

        /// <summary>
        /// 顯示出訊息
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisplayMessage(MessageCardData aData,Text text,string displayStr)
        {
            if (aData.snsType == SnsType.Message)
            {
                var waitSecond = aData.waitDurTime > 0 ? aData.waitDurTime : 0.2f;
                yield return new WaitForSeconds(waitSecond);
                if (fakeReceiveLoadCoroutine != null)
                {
                    StopCoroutine(fakeReceiveLoadCoroutine);
                    fakeReceiveLoadCoroutine = null;
                }
                //假讀取要消失
                text.gameObject.SetCanvasGroup(0);
                text.text = displayStr;
                UpdateTextWidth(aData,text.GetComponent<RectTransform>());
                yield return text.gameObject.eSetCanvasGroup(1);
            }
            else
            {
                UpdateTextWidth(aData,text.GetComponent<RectTransform>());
            }

            //文字y軸變大淡入,假讀取消失
        }


        /// <summary>
        /// 假傳遞訊息中UI
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textStr"></param>
        /// <returns></returns>
        private IEnumerator FakeReceiveReply(MessageCardData aData,Text text)
        {
            //需要做多語言
            text.text = ".";
            UpdateTextWidth(aData,text.GetComponent<RectTransform>());

            if (aData.onMessageDisplayerCallBack!=null)
            {
                aData.onMessageDisplayerCallBack();
            }

            int count = 0;
            while (true)
            {
                //yield return null;
                yield return new WaitForSeconds(0.5f);
                if (count > 3)
                {
                    count = 0;
                    text.text = ".";

                }
                else
                {
                    count++;
                    text.text += ".";
                }
                UpdateTextWidth(aData,text.GetComponent<RectTransform>());
            }
            //顯示偵訊錫
        }
        /// <summary>
        /// 更新文字寬度
        /// </summary>  
        private void UpdateTextWidth(MessageCardData aData,RectTransform child)
        {
            RectTransform BgRect=child.parent.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            if (aData.snsType == SnsType.Image)
            {
                Image img = child.GetComponent<Image>();
                img.preserveAspect = true;

                img.sprite = aData.img;

            }
            else
            {
                if (child.sizeDelta.x > 550) //鎖住文字框寬度
                {
                    child.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    child.sizeDelta = new Vector2(550, child.sizeDelta.y);
                    //yield return new WaitForEndOfFrame();
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());//文字
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);//文字

            BgRect.sizeDelta = new Vector2(child.sizeDelta.x + 40, child.sizeDelta.y + 40);

            LayoutRebuilder.ForceRebuildLayoutImmediate(BgRect);//文字氣泡底框

            LayoutRebuilder.ForceRebuildLayoutImmediate(MessageContentListParent.GetComponent<RectTransform>());//陣列

            LayoutRebuilder.ForceRebuildLayoutImmediate(MessageContentListParent.parent.parent.GetComponent<RectTransform>());
            switch (aData.mDirection) 
            {
                case Direction.Right:
                    gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1050, MessageContentListParent.GetComponent<RectTransform>().sizeDelta.y + 40);
                    break;
                case Direction.Left:
                    gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1050, MessageContentListParent.GetComponent<RectTransform>().sizeDelta.y + 120);
                    break;
            }
           // LayoutRebuilder.ForceRebuildLayoutImmediate(MessageContentListParent.parent.parent.GetComponent<RectTransform>());

        }

        /// <summary>
        /// messageCard傳遞的參數
        /// </summary>
        public class MessageCardData
        {

            /// <summary>
            /// 傳遞的訊息模式
            /// </summary>
            public SnsType snsType = SnsType.None;
      
            /// <summary>
            /// 角色姓名
            /// </summary>
            public string charaName = "";
            /// <summary>
            /// 角色對話
            /// </summary>
            public string message = "";
            /// <summary>
            /// 等待回覆的時間(會看到訊息以...的方式呈現
            /// </summary>
            public float waitDurTime = 0;

            /// <summary>
            /// 傳遞的圖片
            /// </summary>
            public Sprite img=null;

            /// <summary>
            /// 角色方向
            /// </summary>  
            public Direction mDirection;
            /// <summary>
            /// 如果為空,則代表不顯示頭像
            /// </summary>
            public Sprite mAvatar = null;

            /// <summary>
            /// 淡入(歷史訊息不用讓玩家等待淡入的時間
            /// </summary>
            public bool aFade;


            #region UI Adjust
            /// <summary>
            /// 訊息的x軸間距
            /// </summary>
            public float intervalX=0;
            #endregion

            #region CallBackFunction
            /// <summary>
            /// 訊息生成的瞬間回調
            /// </summary>
            public Action onMessageDisplayerCallBack=null;

            #endregion


            /// <summary>
            /// sns系統的構造函數
            /// </summary>
            /// <param name="_snsType"></param>
            /// <param name="_language"></param>
            /// <param name="_charaName"></param>
            /// <param name="avatar"></param>
            /// <param name="_message"></param>
            /// <param name="_img"></param>
            public MessageCardData(SnsType _snsType,string _charaName,Sprite avatar,string _message="",Sprite _img = null, string charaID="")
            {
                snsType=_snsType;
                //mLanguage = _language;
                charaName=_charaName;
                message = _message;
                img = _img;
                mAvatar=avatar;
                if (charaID=="1")
                {
                    mDirection = Direction.Right;
                }
            }

        }

    }//--class

    /// <summary>
    /// 訊息的顯示設定
    /// </summary>
    public struct MessageSetting
    {
        /// <summary>
        /// 確認樣板資料(以下參數的資料)確實已加載完
        /// </summary>
        public bool loadFinish;

        public string charaName;

        /// <summary>
        /// 對話背景圖片的樣式(自己跟他人回答的背景顏色不同
        /// </summary>
        public Sprite BGStyle;
        /// <summary>
        /// 設置字體顏色
        /// </summary>
        public Color textColor;




    }

}//-namespace