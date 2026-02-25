using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Fungus;
using YKO.Support;
using DG.Tweening;
using UniRx;
using System.Linq;
using YKO.Support.Expansion;
using YKO.DataModel;
using YKO.Main;
using YKO.Common.UI;
using Newtonsoft.Json;
using YKO.Common;
using static YKO.Main.SNSRedDotManager;

namespace YKO.SnsSetting
{
    /// <summary>
    /// sns聊天頁面
    /// </summary>
    public class SnsChatRoomControl : MonoBehaviour
    {
        //只要負責顯示故事內容就好
        //虛線的地方中間文字需要加上話題主題
        //會有回覆中的提示

        private class LocalizeKey
        {
            public static readonly string ClickPlayButtonStartTopic = "按下播放鍵,開始話題";
            /// <summary>
            /// 回覆對話的預設文字(需要在初始化時多語言
            /// </summary>
            public static readonly string defaultAnswerDisplayerStr = "想說點什麼...";
        }


        #region Panel UI 

        /// <summary>
        /// 訊息群對話母物件
        /// </summary>
        [SerializeField] private RectTransform messageContentParent;

        /// <summary>
        /// 訊息的滑動
        /// </summary>
        [SerializeField] private RectTransform messageScrollViewParent;

        /// <summary>
        /// 暫存生成的message的位置(為優化而生成
        /// </summary>
        [SerializeField] private RectTransform messageTempSaveParent;

        /// <summary>
        /// 返回角色清單按鈕
        /// </summary>
        [SerializeField] private Button charaListButton;

        /// <summary>
        /// 回顧按鈕
        /// </summary>
        [SerializeField] private Button topicHistoryButton;

        /// <summary>
        /// 聯絡人名稱
        /// </summary>
        [SerializeField] private Text contactPersonNameText;

        /// <summary>
        /// 紅點
        /// </summary>
        [SerializeField] private RedDotObj inputRedDotObj;


        #region Bottom Popup
        /// <summary>
        /// 點開話題按鈕(下方日常,可重複,非歷史訊息的話題
        /// </summary>
        [SerializeField] private Button OpenBottomAreaButton;



        /// <summary>
        /// 寄送訊息的按鈕
        /// </summary>
        [SerializeField] private Button SendMessageButton;



        /// <summary>
        /// 當前選擇的回答選項文字
        /// </summary>
        [SerializeField] private Text answerOptionText;
        /// <summary>
        /// 顯示話題與選項的彈窗
        /// </summary>
        [SerializeField] private RectTransform BottomPopup;

        /// <summary>
        /// 話題選擇 話題選項陣列母物件
        /// </summary>
        [SerializeField] private Transform TopicListContentParent;
        /// <summary>
        /// 話題選擇視窗
        /// </summary>
        [SerializeField] private Transform TopicListView;

        /// <summary>
        /// 選項母物件
        /// </summary>
        [SerializeField] private Transform OptionListViewParent;


        [Header("Button UI")]
        /// <summary>
        /// 篩選按鈕(全部顯示
        /// </summary>
        [SerializeField] private Button AllFilterButton;

        /// <summary>
        /// 篩選按鈕(話題類別日常
        /// </summary>
        [SerializeField] private Button DailyFilterButton;

        /// <summary>
        /// 篩選按鈕(話題類別支線
        /// </summary>
        [SerializeField] private Button SideStoryFilterButton;

        /// <summary>
        /// 篩選按鈕(話題類別特別
        /// </summary>
        [SerializeField] private Button SpecialFilterButton;

        #endregion



        #endregion

        #region Prefab
        [Header("Prefab")]

        /// <summary>
        /// 訊息群prefab(因應同一個角色連續說話不會持續出現頭像
        /// </summary>
        [SerializeField] private GameObject messageCardGroupPrefab;
        /// <summary>
        /// 分隔線
        /// </summary>
        [SerializeField] private GameObject SeparateLinePrefab;
        /// <summary>
        /// 玩家回答的字卡
        /// </summary>
        [SerializeField] private GameObject answerCellPrefab;

        /// <summary>
        /// 顯示下方話題的UI
        /// </summary>
        [SerializeField] private GameObject topicCellPrefab;

        #endregion

        #region Param

        //話題卡陣列

        private Color inputTextDefaultColor = new Color(0.75f,0.75f,0.75f);

        private Color inputTextConversationColor = new Color(0.4f, 0.4f, 0.4f);

        //當前對話選項陣列
        /// <summary>
        /// 聊天室的當前狀態
        /// </summary>
        private ChatRoomState state = ChatRoomState.None;
        /// <summary>
        /// 當前篩選的話題型
        /// </summary>
        private TopicType chatTopicListType = TopicType.None;

        /// <summary>
        /// 切換篩選器的樣式
        /// </summary>
        private TopicType ChatTopicListType
        {
            get { return chatTopicListType; }
            set
            {
                StartCoroutine(AlterTopicFilterColor(chatTopicListType, false));
                StartCoroutine(AlterTopicFilterColor(value, true));
                chatTopicListType = value;
            }
        }

        /// <summary>
        /// 訊息群陣列
        /// </summary>
        private List<MessageCard> messageCardList = new List<MessageCard>();
        /// <summary>
        /// 當前選擇的回覆
        /// </summary>
        private string chooseAnswer = "";

        /// <summary>
        /// 當前選擇的回覆
        /// </summary>
        private SnsData.Topic_Info chooseTopic = null;
        /// <summary>
        /// 回覆的字卡
        /// </summary>
        private List<SnsAnswerCell> answerCellList = new List<SnsAnswerCell>();

        /// <summary>
        /// 話題選擇的字卡
        /// </summary>
        private List<TopicCell> TopicCellList = new List<TopicCell>();

        /// <summary>
        /// 主要控制腳本
        /// </summary>
        private SnsSettingController control;
        /// <summary>
        /// 下方對話是否彈起
        /// </summary>
        private bool BottomPopupState = false;
        /// <summary>
        /// 當前顯示的角色資料
        /// </summary>
        private CharaSnsRecord record = null;
        /// <summary>
        /// 是否為最新話題(如果是,則必須無視掉同一個人發話,框會在一起的狀況
        /// </summary>
        private bool isNewTopic = false;

        #endregion



        /// <summary>
        /// 初始化  顯示歷史對話
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public IEnumerator Init(SnsSettingController _control)
        {
            control = _control;
            record = control.DisplayCharaRecord;
            InitUISetting();
            InitData();
            DetectCurTopicDisplayUI();
            SetButtonSetting();
            //需要顯示當下執行的話題
            yield return DisplayHistoryConversation();
        }
        /// <summary>
        /// 重新設置UI狀態為初始狀態
        /// </summary>
        private void InitUISetting()
        {
            BottomPopupState = false;
            BottomPopup.anchoredPosition = Vector2.zero;
            messageScrollViewParent.sizeDelta = new Vector2(960, 1175);
            messageScrollViewParent.anchoredPosition = new Vector2(0, 0);
            ClearTopicListContentParent();
            contactPersonNameText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, record.charaName);

            StartCoroutine(AlterTopicFilterColor(TopicType.None, true, false));
            StartCoroutine(AlterTopicFilterColor(TopicType.Special, false, false));
            StartCoroutine(AlterTopicFilterColor(TopicType.Daily, false, false));
            StartCoroutine(AlterTopicFilterColor(TopicType.SideStory, false, false));

            CloseSendButton();

            StartCoroutine(AutoSetInputBackgroundHeight(true));
            StartCoroutine(SetBottomPopupPositionY(false));

        }
        /// <summary>
        /// 重製數據
        /// </summary>
        private void InitData()
        {
            chooseAnswer = "";
            answerOptionText.text =LocalizeKey.defaultAnswerDisplayerStr;
            answerOptionText.color = inputTextDefaultColor;
            state = ChatRoomState.None;
            ChatTopicListType = TopicType.None;
            chooseTopic = null;
            foreach (var card in messageCardList)
            {
                Destroy(card.gameObject);
            }
            messageCardList.Clear();
            foreach (var card in answerCellList)
            {
                Destroy(card.gameObject);
            }
            answerCellList.Clear();

            for (int i = 0; i < messageContentParent.childCount; i++)
            {
                var child = messageContentParent.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 設置按鈕
        /// </summary>
        private void SetButtonSetting()
        {
            topicHistoryButton.onClick.RemoveAllListeners();
            charaListButton.onClick.RemoveAllListeners();
            OpenBottomAreaButton.onClick.RemoveAllListeners();
            AllFilterButton.onClick.RemoveAllListeners();
            DailyFilterButton.onClick.RemoveAllListeners();
            SideStoryFilterButton.onClick.RemoveAllListeners();
            SpecialFilterButton.onClick.RemoveAllListeners();
            SendMessageButton.onClick.RemoveAllListeners();

            topicHistoryButton.OnClickAsObservable().Where(_ => control.canTween).Subscribe(_ => StartCoroutine(ClickTopicHistoryButton()));
            charaListButton.OnClickAsObservable().Where(_ => control.canTween).Subscribe(_ => StartCoroutine(ClickCharaListButton()));

            OpenBottomAreaButton.OnClickAsObservable().Where(_ => control.canTween)
            .Subscribe(_ => StartCoroutine(ClickBottomTopicOptionButton(!BottomPopupState)));

            SendMessageButton.OnClickAsObservable().Subscribe(_ => ClickSendMessageButton());

            AllFilterButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickTopicFilter(TopicType.None)));
            DailyFilterButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickTopicFilter(TopicType.Daily)));
            SideStoryFilterButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickTopicFilter(TopicType.SideStory)));
            SpecialFilterButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickTopicFilter(TopicType.Special)));

            var redDotDataArray = new[]
            {
                new RedDotRegistParam<object>(
                    RedDotTypes.SNS_NEW_TOPICS,
                    new SNSRedDotManager.SNSRedDotData(uint.Parse(record.charaID), 0))
            };

            inputRedDotObj.StartRegistEvent(dict =>
            {
                if (dict.TryGetValue(RedDotTypes.SNS_TOPICS_RECORD, out var data))
                {
                    var other = data.Other as SNSCharaRecordTopicStatus;
                    return other.have_new_topic == 1&&state== ChatRoomState.WaitOpenTopic;
                }
                return false;
            }).RegistRedDot(redDotDataArray);




        }

        /// <summary>
        ///根據話題狀態  顯示對應的功能 lock unlock notRead等
        /// </summary>
        /// <returns></returns>
        private void DetectCurTopicDisplayUI()
        {
            var topic = record.GetCurLastNewTopic();

            if (topic != null)
            {
                switch (topic.state)
                {

                    case ContactPersonState.NotLoading:
                        OpenBottomAreaButton.gameObject.SetActive(false);
                        //繼續執行劇情(已讀 ,照理說對方會回覆到等待玩家回覆
                        //繼續執行故事
                        break;
                    case ContactPersonState.NotReply:
                        //繼續執行劇情(玩家回覆
                        //繼續執行故事
                        OpenBottomAreaButton.gameObject.SetActive(false);
                        //CreateAnswerOption(topic.GetCurReply());
                        break;
                    case ContactPersonState.Finish:
                        OpenBottomAreaButton.gameObject.SetActive(true);
                        //彈出話題視窗?
                        break;
                    default:
                        Debug.Log("Not Have Any Topic In Current");
                        break;
                }
            }
            else
            {
                Debug.Log("尚未開始話題");
            }
        }
        #region HistoryTopic

        /// <summary>
        /// 顯示過去對話
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisplayHistoryConversation()
        {
            //生成對話
            //根據最新對話的state 選擇顯示狀態
            //   control.DisplayCharaRecord.GetCurLastNewTopic().state
            if (record.historytopicLists.Count <= 0)
            {
                OpenBottomAreaButton.gameObject.SetActive(true);
                CreateTopicCellOption();
            }
            else
            {
                record.SortTopicList();
                uint topicNum = 0;
                Dictionary<uint, List<GameObject>> historyList = new Dictionary<uint, List<GameObject>>();
                foreach (var topic in record.historytopicLists)
                {
                    var num = topicNum;
                    if (topic.topicSort > 0 && topic.curID > 0 && topic.IsPlotOver() && topic != record.GetCurLastNewTopic())
                    {
                        Debug.Log("#@#需要回顧歷史話題");
                        StartCoroutine(StartHistoryTopic(topic, list => historyList.Add(num, list)));
                        topicNum++;
                    }
                }
                yield return new WaitUntil(() => historyList.Count >= topicNum);//等所有話題都加載完
                yield return null;

                for (uint i = 0; i < historyList.Count; i++)
                {
                    historyList[i].ToList().ForEach(v =>
                    {
                        v.transform.SetParent(messageContentParent, false);
                    });
                }


                yield return StartTopic(record.GetCurLastNewTopic());
            }
            //如果當前話題是已經結束,則直接開啟下方彈窗,若不是則繼續該話題
        }


        /// <summary>
        /// 開始一個話題(執行index的次數結束後,需要判斷是否已經播放完
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IEnumerator StartHistoryTopic(TopicData topic, Action<List<GameObject>> receiveCB)
        {
            //生成話題提示?-----xxx------
            List<MessageCard> messageCardLIst = new List<MessageCard>();
            List<GameObject> ObjList = new List<GameObject>();
            if (topic == null)
            {
                yield break;
            }
            OpenBottomAreaButton.gameObject.SetActive(false);

            long playID = 1;

            var lineObj = CreateSeparateLine(LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, topic.topicLabel));
            lineObj.transform.SetParent(messageTempSaveParent, false);
            //  Debug.Log("歷史當前id=>" + topic.curID);
            //目標劇情有撥放
            //正在撥放的不是當前的目標撥放
            //  不包含對應key代表劇情結束
            //     Debug.Log("歷史資訊=>" + playID);
            while (topic.curID > 0)
            {
                //已經獲取下一個要撥放的id跟最新話題的目標id是同一個,則進入撥放模式
                //為甚麼不能用目標0做判斷? 因為0代表結束,但玩家歷史對話有可能沒回覆完
                if (playID != topic.GetCurReply().GetTargetID() && topic.ReplyDic.ContainsKey(playID))
                {
                    var reply = topic[playID];
                    switch (reply._snsType)
                    {
                        case SnsType.Message:
                            //     Debug.Log("meg歷史資訊=>" + playID);
                            //yield return CreateMessage(reply, true);
                            yield return CreateHistoryMessage(reply, messageCardLIst);
                            //StartCoroutine(CreateMessage(reply, true));
                            break;
                        case SnsType.Reply:
                            //    Debug.Log("reply歷史資訊=>" + playID);
                            yield return CreateHistoryMessage(reply, messageCardLIst);
                            //StartCoroutine(CreateMessage(reply, true));
                            break;
                        case SnsType.Image:
                            //產生圖片
                            break;
                    }
                    playID = reply.GetTargetID();
                }
                else  //如果是已經全部執行完 跑這一段(可以生成最後的話題
                {
                    if (topic.state == ContactPersonState.Finish && topic.GetCurReply().GetTargetID() != 0)
                    {
                        var tarReply = topic[topic.GetCurReply().GetTargetID()];
                        Debug.Log("生成話題2=>" + tarReply.GetCurMessage());
                        yield return CreateHistoryMessage(tarReply, messageCardLIst);
                    }

                    ObjList.Add(lineObj);
                    ObjList.AddRange(messageCardLIst.Select(v => v.gameObject));
                    receiveCB(ObjList);
                    StartCoroutine(MoveMessageContentToBottom(0));
                    yield break;
                }
                //     Debug.Log("playID的狀況=>" + playID);
            }
            //以下是沒跑while時會執行
            // Debug.Log("未撥放過的話題=>" + topic.topicID);
            receiveCB(ObjList);
            StartCoroutine(MoveMessageContentToBottom(0));
        }


        /// <summary>
        /// 生成歷史訊息(先不把message加到 messageContentParent裡面
        /// </summary>
        /// <param name="data"></param>
        /// /// <returns></returns>
        private IEnumerator CreateHistoryMessage(ReplySns reply, List<MessageCard> megCardList)
        {
            // MessageCard.MessageCardData data = CreateNormalMessageCardData(reply);
            MessageCard.MessageCardData data = null;
            yield return eCreateNormalMessageCardData(
               reply, res =>
               {
                   data = res;
               });
            data.aFade = true;
            data.onMessageDisplayerCallBack = () =>
            {
                StartCoroutine(MoveMessageContentToBottom());
            };

            data.waitDurTime = 0;
            data.aFade = false;

            ///現在問題是 平行加載的話,無法抓到上一個對話的角色
            MessageCard curMessage = null;
            if (megCardList.Count > 0) curMessage = megCardList[megCardList.Count - 1];

            if (!isNewTopic && IsLastMessageCardSameChara(data.charaName, curMessage))
            {
                var card = megCardList[megCardList.Count - 1];

                data.mAvatar = null;
                StartCoroutine(card.Init(data));

            }
            else
            {
                isNewTopic = false;
                GameObject sp = Instantiate(messageCardGroupPrefab);
                MessageCard card = sp.GetComponent<MessageCard>();
                megCardList.Add(card);
                //messageCardList.Add(card);
                sp.transform.SetParent(messageTempSaveParent, false);
                StartCoroutine(card.Init(data, true));
            }

        }
        #endregion

        /// <summary>
        /// 開始一個話題(執行index的次數結束後,需要判斷是否已經播放完
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IEnumerator StartTopic(TopicData topic)
        {
            //生成話題提示?-----xxx------
            if (topic == null)
            {
                yield break;
            }
            OpenBottomAreaButton.gameObject.SetActive(false);

            long playID = 1;

            CreateSeparateLine(LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, topic.topicLabel));

            //開始執行sns系統
            void startExecuteTopic()
            {
                StartCoroutine(MoveMessageContentToBottom(0));

                if (topic.IsPlotOver())
                {
                    OpenBottomAreaButton.gameObject.SetActive(true);
                    CreateTopicCellOption();
                }
                else
                {
                    StartCoroutine(ExecuteTopic(topic));
                }

            };

            Debug.Log("當前id=>" + topic.curID);

            //目標劇情有撥放
            //正在撥放的不是當前的目標撥放
            //  不包含對應key代表劇情結束
            //     Debug.Log("歷史資訊=>" + playID);
            while (topic.curID > 0)
            {
                //已經獲取下一個要撥放的id跟最新話題的目標id是同一個,則進入撥放模式
                //為甚麼不能用目標0做判斷? 因為0代表結束,但玩家歷史對話有可能沒回覆完
                if (playID != topic.GetCurReply().GetTargetID() && topic.ReplyDic.ContainsKey(playID))
                //  if (playID != 0 && topic.ReplyDic.ContainsKey(playID))
                {
                    var reply = topic[playID];
                    switch (reply._snsType)
                    {
                        case SnsType.Message:
                            Debug.Log("meg歷史資訊=>" + playID);
                            //yield return CreateMessage(reply, true);
                            yield return CreateMessage(reply, true);
                            //StartCoroutine(CreateMessage(reply, true));
                            break;
                        case SnsType.Reply:
                            Debug.Log("reply歷史資訊=>" + playID);
                            yield return CreateMessage(reply, true);
                            //StartCoroutine(CreateMessage(reply, true));
                            break;
                        case SnsType.Image:
                            //產生圖片
                            break;
                    }
                    Debug.Log("#5測試3");
                    playID = reply.GetTargetID();
                }
                else  //如果是已經全部執行完 跑這一段(可以生成最後的話題
                {
                    Debug.Log("#5測試1");
                    if (topic.GetCurReply().monoUserDialog)
                    {
                        Debug.Log("#5測試2");
                        control.SendProto30204Request(topic.topicID, (uint)topic.GetCurReply().mID, 0);
                    }
                    if (topic.state == ContactPersonState.Finish && topic.GetCurReply().GetTargetID() != 0)
                    {
                        var tarReply = topic[topic.GetCurReply().GetTargetID()];
                        yield return CreateMessage(tarReply, true);
                    }
                    startExecuteTopic();
                    yield break;
                }
            }
            Debug.Log("未撥放過的話題=>" + topic.topicID);
            startExecuteTopic();

        }
        /// <summary>
        /// 執行話題(開始與玩家互動
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        private IEnumerator ExecuteTopic(TopicData topic)
        {
            //如果故事仍然繼續進行,下個執行目標不會低於0,


            while (topic.curID == 0 || topic.GetCurReply().GetTargetID() > 0)
            {
                ReplySns reply = null;
                yield return topic.WaitAddReply();
                if (topic.curID == 0)
                {
                    reply = topic.ReplyDic[1];
                }
                else
                {
                    reply = topic.GetTarReply();
                }

                //  Debug.Log("###執行=>" + reply.mID);
                switch (reply._snsType)
                {
                    case SnsType.Message:
                        state = ChatRoomState.WaitOtherSideSendMessage;
                        yield return CreateMessage(reply);
                        break;

                    case SnsType.Reply:
                        CreateAnswerOption(topic, reply);
                        yield return new WaitUntil(() => state == ChatRoomState.None);
                        break;
                    case SnsType.Image:
                        //產生圖片
                        break;
                }
                control.SendProto30205Request(topic.topicID);
                yield return control.WaitHandleProto("30205");
                if (topic.curID <= 0)//故事剛開始
                {
                    topic.curID = 1;
                }
                else
                {
                    topic.curID = topic.GetCurReply().GetTargetID();
                }
                if (reply.monoUserDialog)
                {
                    chooseAnswer = reply.messageList[0].message;
                    yield return SendReply(false);
                }

                //等待server回報
                //將資料序列化成topic的reply後,加入並再執行
            }
            Debug.Log("話題結束");
            if (topic.GetCurReply()._snsType == SnsType.Message) //最後一句需要紀錄給server 因為玩家回覆在選擇時就已經記錄,而玩家回覆則沒紀錄
            {
                control.SendProto30204Request(topic.topicID, (uint)topic.curID, 0);
                yield return control.WaitHandleProto("30204");
            }
            CreateSeparateLine(string.Format("[{0}]END", LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, topic.topicLabel)));
            yield return MoveMessageContentToBottom();
            OpenBottomAreaButton.gameObject.SetActive(true);
            CreateTopicCellOption();
            topic.state = ContactPersonState.Finish;
        }

        /// <summary>
        /// 完成對話(清空對話框的選項,設置對話框高
        /// </summary>
        private void ConversationOfResult()
        {
            if (answerCellList.Count > 0)
            {

                var coro = StartCoroutine(AutoSetInputBackgroundHeight());

                foreach (var cell in answerCellList)
                {
                    var rect = cell.GetComponent<RectTransform>();
                    rect.DOSizeDelta(new Vector2(rect.sizeDelta.x, 0), 0.2f);
                    StartCoroutine(cell.gameObject.eSetCanvasGroup(0, () =>
                    {
                        Destroy(cell.gameObject); StopCoroutine(coro);
                        state = ChatRoomState.None;
                    }));
                }
                answerCellList.Clear();
            }
        }


        /// <summary>
        /// 生成訊息
        /// </summary>
        /// <param name="data"></param>
        /// /// <returns></returns>
        private IEnumerator CreateMessage(ReplySns reply, bool isHaveWait = false)
        {
            // MessageCard.MessageCardData data = CreateNormalMessageCardData(reply);
            MessageCard.MessageCardData data = null;
            yield return eCreateNormalMessageCardData(
               reply, res =>
               {
                   data = res;
               });


            data.aFade = true;
            data.onMessageDisplayerCallBack = () =>
            {
                StartCoroutine(MoveMessageContentToBottom());
            };

            if (isHaveWait)
            {
                data.waitDurTime = 0;
                data.aFade = false;
            }

            ///現在問題是 平行加載的話,無法抓到上一個對話的角色
            if (IsLastMessageCardSameChara(data.charaName) && !isNewTopic)
            {
                var card = GetCurLastestMessageCard();

                data.mAvatar = null;

                yield return card.Init(data);
            }
            else
            {
                isNewTopic = false;
                GameObject sp = Instantiate(messageCardGroupPrefab);
                MessageCard card = sp.GetComponent<MessageCard>();
                messageCardList.Add(card);

                sp.transform.SetParent(messageContentParent, false);
                yield return card.Init(data, true);
            }
        }



        /// <summary>
        /// 生成話題分隔線
        /// </summary>
        /// <param name="content"></param>
        private GameObject CreateSeparateLine(string content)
        {
            isNewTopic = true;
            GameObject sepLine = Instantiate(SeparateLinePrefab);
            sepLine.transform.SetParent(messageContentParent, false);
            sepLine.transform.Find("TextHorizontalMask").Find("TopicTitle").GetComponent<Text>().text = content;
            return sepLine;
        }
        /// <summary>
        /// 生成回覆多選項
        /// </summary>
        /// <returns></returns>
        private void CreateAnswerOption(TopicData topic, ReplySns reply)
        {
            state = ChatRoomState.WaitReply;
            Coroutine coro = StartCoroutine(AutoSetInputBackgroundHeight());
            List<int> displayReplyNum = new List<int>();
            var topicInfo = record.topicInfoLists.Find(info => info.infoData.id == topic.topicID);


            if (reply.messageList.Count <= 1)
            {
                if (topicInfo.isRead)
                {
                    displayReplyNum.Add(0);
                }
            }
            else
            {
                if (topicInfo.choiceHistory.ContainsKey(reply.mID))
                {
                    displayReplyNum = topicInfo.choiceHistory[reply.mID];
                }
            }

            //  displayReplyNum = GetDisplayerSentence(topic, reply.mID);
            StartCoroutine(SetBottomPopupPositionY(true));
            Debug.Log("生成選項回覆數量=>" + reply.messageList.Count);
            for (int i = 0; i < reply.messageList.Count; i++)
            {
                var meg = reply.messageList[i];

                GameObject sp = Instantiate(answerCellPrefab);
                sp.transform.SetParent(OptionListViewParent, false);

                SnsAnswerCell cell = sp.GetComponent<SnsAnswerCell>();
                RectTransform rect = sp.GetComponent<RectTransform>();
                cell.AreaButton.interactable = false;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0);

                answerCellList.Add(cell);

                string displayCellStr = !string.IsNullOrWhiteSpace(meg.synopsisOfMessage) ? meg.synopsisOfMessage : meg.message;
                displayCellStr = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, displayCellStr);
                /*   Debug.Log("測試1=>" + reply.GetTargetID());
                   Debug.Log("測試2=>" + reply.mID);
                   Debug.Log("聊天室meg synopsis=>"+ meg.synopsisOfMessage);
                   Debug.Log("聊天室meg=>" + meg.message);*/

                cell.Init(
                    new SnsAnswerCell.SnsAnswerCellData(
                    displayCellStr,
                    () =>
                    {
                        chooseAnswer = meg.message;
                        answerOptionText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, meg.message);
                        answerOptionText.color = inputTextConversationColor;
                        AdjustAnswerCellListToCancelState(cell);

                        StartCoroutine(AutoSetInputBackgroundHeight(true));
                        OpenSendButton();
                    })
                    {
                        chooseInPast = displayReplyNum.Contains(i)
                    }

                    );

                rect.DOSizeDelta(new Vector2(rect.sizeDelta.x, 100), 0.2f).OnComplete(() =>
                {
                    cell.AreaButton.interactable = true;
                    StopCoroutine(coro);
                });
            }

            StartCoroutine(SetBottomPopupPositionY(true));

        }

        /// <summary>
        /// 玩家可選擇新話題後,生成話題卡
        /// </summary>
        private void CreateTopicCellOption()
        {
            foreach (var topicInfo in record.topicInfoLists)
            {
                Debug.Log("有哪些話題資料=>" + topicInfo.infoData.name);
                //話題不可重複    且曾經已出現該對話話題
                if (record.JudgeTopicHasBeenPlayed((uint)topicInfo.infoData.id))
                {
                    continue;
                }

                if (!topicInfo.isLock || topicInfo.topicType == TopicType.Daily)
                {
                    GameObject sp = Instantiate(topicCellPrefab);
                    sp.transform.SetParent(TopicListContentParent, false);
                    TopicCellList.Add(sp.GetComponent<TopicCell>());
                    sp.GetComponent<TopicCell>().Init(
                        topicInfo,
                        _topic =>
                        {
                            if (chooseTopic != null && chooseTopic.id != _topic.infoData.id)
                            {
                                AdjustTopicCellCancel(chooseTopic);
                            }
                            else if (chooseTopic == null)
                            {
                                answerOptionText.text = LocalizeKey.ClickPlayButtonStartTopic;
                                answerOptionText.color = inputTextConversationColor;
                                OpenSendButton();
                            }
                            chooseTopic = _topic.infoData;

                        },
                        SnsSettingController.SnsControlPage.SnsChatRoom
                      );

                }
            }
        }

        /// <summary>
        /// 按下寄送按鈕
        /// </summary>
        private void ClickSendMessageButton()
        {
            if (state == ChatRoomState.WaitReply)
            {
                Debug.Log("###回覆話題");
                if (!string.IsNullOrWhiteSpace(chooseAnswer))
                {

                    StartCoroutine(SendReply());
                }
                else
                {
                    Debug.Log("給予的回覆還是空的");
                }
            }
            else
            if (state == ChatRoomState.WaitOpenTopic)
            {
                Debug.Log("開始新的話題");
                //偵測是否有選中的話題
                if (chooseTopic != null)
                {
                    IEnumerator startTopic()
                    {
                        ClearTopicListContentParent();
                        //0的話就是重新執行  結束的話是看目標
                        control.SendProto30202Request((uint)chooseTopic.id);
                        ChatTopicListType = TopicType.None;
                        yield return control.WaitHandleProto("30202");
                        yield return new WaitUntil(() => !record.GetCurLastNewTopic().IsPlotOver());
                        OpenBottomAreaButton.gameObject.SetActive(false);
                        yield return ClickBottomTopicOptionButton(false);
                        yield return StartTopic(record.GetCurLastNewTopic());
                    }
                    StartCoroutine(startTopic());
                }
            }
        }
        /// <summary>
        /// 寄出回覆
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendReply(bool createMeg = true)
        {
            Debug.Log("寄出回覆");
            yield return SetBottomPopupPositionY(false);
            // CloseSendButton();
            ReplySns reply = null;

            bool tempDefault = false;//暫時設定數值時會是true

            if (record.GetCurLastNewTopic().curID <= 0)
            {
                record.GetCurLastNewTopic().curID = 1;
                tempDefault = true;
            }
            if (record.GetCurLastNewTopic().GetCurReply().monoUserDialog)
            {
                reply = record.GetCurLastNewTopic().GetCurReply();
            }
            else
            {
                reply = record.GetCurLastNewTopic().GetTarReply();
            }

            for (int i = 0; i < reply.messageList.Count; i++)
            {
                var meg = reply.messageList[i];
                if (meg.message == chooseAnswer)
                {
                    reply.curIndex = i;
                    break;
                }
            }
            long topicIndex = 0;
            int requestIndex = 0;//回報的選項,因為機制關係,所以選項回覆跟單選回覆回傳的不同

            var snsFixed = SnsFixedData.GetTopicData(record.GetCurLastNewTopic().topicID.ToString());

            // Debug.Log("監測mono2=>"+ record.GetCurLastNewTopic().GetCurReply().mID+ "布林結果=>"+ record.GetCurLastNewTopic().GetCurReply().monoUserDialog);

            if (record.GetCurLastNewTopic().GetCurReply().monoUserDialog)//在該index卻沒完成
            {
                Debug.Log("###有mono=>" + topicIndex);
                topicIndex = record.GetCurLastNewTopic().curID;
                requestIndex = reply.curIndex;
            }
            else if
             (snsFixed.ContainsKey(reply.mID.ToString()) && snsFixed[reply.mID.ToString()].chara_id > 0)//在該index的玩家單選回覆
            {
                topicIndex = reply.mID;
                requestIndex = reply.curIndex;
                Debug.Log("###沒有mono,但玩家回覆=>" + topicIndex);
            }
            else//在該index的多選項回覆
            {
                topicIndex = record.GetCurLastNewTopic().curID;
                requestIndex = (reply.curIndex + 1);
                record.topicInfoLists.Find(info => info.infoData.id == record.GetCurLastNewTopic().topicID)
                .AddHistoryRecordInChoice(record.GetCurLastNewTopic().GetCurReply().GetTargetID(), reply.curIndex);
                Debug.Log("###沒有mono=>" + topicIndex);
            }

            control.SendProto30204Request(
                record.GetCurLastNewTopic().topicID,
                (uint)topicIndex,
                (uint)requestIndex
                );

            yield return control.WaitHandleProto("30204",
                result =>
                {
                    if (result)
                    {
                        //  Debug.Log("發送成功");
                    }
                    else
                    {
                        //   Debug.Log("發送失敗");
                    }
                });

            answerOptionText.text =LocalizeKey.defaultAnswerDisplayerStr;
            answerOptionText.color = inputTextDefaultColor;
            chooseAnswer = "";

            if (tempDefault) //將暫時預設的數值恢復(主要是因為為了傳送choose proto給server所以才改數值,但後面因為劇情撥放會有錯誤判斷,所以func跑完再調整回來
            {
                record.GetCurLastNewTopic().curID = 0;
            }

            if (createMeg)
            {
                StartCoroutine(CreateMessage(reply));
                ConversationOfResult();
            }
        }

        /// <summary>
        /// 前往角色列表
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickCharaListButton()
        {
            yield return control.GoCharaListPage();
            StopAllCoroutines();
        }

        /// <summary>
        /// 前往話題列表
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickTopicHistoryButton()
        {
            Debug.Log("前往歷史列表");
            yield return control.GoTopicHistoryPage();
            StopAllCoroutines();
        }


        /// <summary>
        /// 彈出下方的話題列表按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickBottomTopicOptionButton(bool isPopup = true)
        {
            control.canTween = false;
            state = ChatRoomState.WaitOpenTopic;
            yield return SetBottomPopupPositionY(isPopup);
            control.canTween = true;
            //陳列話題與開啟對應的按鈕觸碰
        }

        /// <summary>
        /// 篩選話題 (多個篩選都套用的方法
        /// </summary>
        /// <param name="topType"></param>
        /// <returns></returns>
        private IEnumerator ClickTopicFilter(TopicType topicType)
        {
            if (control.canTween)
            {
                control.canTween = false;
                // StartCoroutine(AlterTopicFilterColor(chatTopicListType, false));
                // StartCoroutine(AlterTopicFilterColor(topicType, true));
                //chatTopicListType = topicType;
                ChatTopicListType = topicType;
                foreach (var cell in TopicCellList)
                {
                    if (cell.aData.topicType == topicType || topicType == TopicType.None)
                    {
                        if (!cell.gameObject.activeSelf)
                        {
                            var rect = cell.GetComponent<RectTransform>();
                            rect.sizeDelta = new Vector2(900, 0);
                            //  cell.transform.localScale = new Vector3(1, 0, 1);
                            cell.gameObject.SetCanvasGroup(0);
                            cell.gameObject.SetActive(true);
                            cell.DOKill();
                            rect.DOSizeDelta(new Vector2(900, 80), 0.2f);
                            StartCoroutine(cell.gameObject.eSetCanvasGroup(1));
                        }
                    }
                    else
                    {
                        if (cell.gameObject.activeSelf)
                        {
                            if (chooseTopic != null && chooseTopic.id == cell.aData.infoData.id)
                            {
                                AdjustTopicCellCancel(chooseTopic);
                                CloseSendButton();
                                chooseTopic = null;
                            }
                            var rect = cell.GetComponent<RectTransform>();
                            rect.sizeDelta = new Vector2(900, 80);
                            cell.gameObject.SetCanvasGroup(1);

                            cell.DOKill();
                            rect.DOSizeDelta(new Vector2(900, 0), 0.2f)
                                .OnComplete(
                                () => { cell.gameObject.SetActive(false); }
                             );

                            StartCoroutine(cell.gameObject.eSetCanvasGroup(0));
                        }
                    }
                }
                yield return new WaitForSeconds(0.2f);
                control.canTween = true;
            }
        }//---func



        #region Data Func
        /// <summary>
        /// 偵測現在的對話角色是否和最後的對話相同
        /// </summary>
        /// <returns></returns>
        private bool IsLastMessageCardSameChara(string charaName)
        {
            var curCard = GetCurLastestMessageCard();
            return IsLastMessageCardSameChara(charaName, curCard);
            //  Debug.Log("###角色名稱=>"+charaName);
        }
        /// <summary>
        /// 偵測現在的對話角色是否和最後的對話相同
        /// </summary>
        /// <returns></returns>
        private bool IsLastMessageCardSameChara(string charaName, MessageCard curCard)
        {
            if (curCard == null)
            {

                return false;

            }
            else
            {
                if (curCard.MySetting.charaName == charaName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 生成常規的訊息數據(主要生頭像
        /// </summary>
        /// <param name="reply"></param>
        /// <returns></returns>
        private IEnumerator eCreateNormalMessageCardData(ReplySns reply, Action<MessageCard.MessageCardData> cb)
        {

            Sprite avatar = null;
            if (LoadResource.Instance.CharacterData.data_character_info.ContainsKey(ConvertInfoScript.GetCharaNumString(reply.iconID)))
                yield return LoadResource.Instance.GetHeroAvatar(
                    uint.Parse(ConvertInfoScript.GetCharaNumString(reply.iconID))
                    , res => { avatar = res; });

            // LoadResource.Instance.GetHeroAvatar()
            //   Debug.Log("測試0=>" + reply.mID);
            //  Debug.Log("測試1=>"+reply.charaName);

            MessageCard.MessageCardData data = new MessageCard.MessageCardData(
                reply._snsType,
                 LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, reply.charaName),
                avatar,
                LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, reply.GetCurMessage()),
            reply.imgSprite,
                reply.dialogCharaID
                );
            data.waitDurTime = reply._dialogWaitTime;
            data.intervalX = -50;
            cb(data);
            //  return data;
            //cb(data);
        }


        #endregion


        #region SetUI

        /// <summary>
        /// 將回覆選項都設置為取消選取狀態(除了參數內的選項
        /// </summary>
        private void AdjustAnswerCellListToCancelState(SnsAnswerCell exeptCell)
        {
            foreach (var cell in answerCellList)
            {
                if (cell != exeptCell)
                {
                    cell.CancelChoose();
                }
            }
        }
        /// <summary>
        /// 獲得最新的對話群組UI
        /// </summary>
        /// <returns></returns>
        private MessageCard GetCurLastestMessageCard()
        {
            if (messageCardList.Count > 0)
            {

                return messageCardList[(messageCardList.Count - 1)];

            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 將寄送按鈕的UI設置成可觸發的樣式
        /// </summary>
        private void OpenSendButton()
        {
            SendMessageButton.DOKill();
            if (!SendMessageButton.interactable)
            {
                SendMessageButton.interactable = true;
                SendMessageButton.GetComponent<Image>().DOColor(control.defaultChooseColor, 0.2f);
            }

        }
        /// <summary>
        /// 將寄送按鈕的UI設置成不可觸發的樣式
        /// </summary>
        private void CloseSendButton()
        {
            SendMessageButton.DOKill();
            if (SendMessageButton.interactable)
            {
                SendMessageButton.interactable = false;
                SendMessageButton.GetComponent<Image>().DOColor(new Color(0.65f, 0.65f, 0.65f), 0.2f);
            }

        }
        /// <summary>
        /// 將目標話題按鈕UI設置成未選取狀態
        /// </summary>
        /// <param name="_topic"></param>
        private void AdjustTopicCellCancel(SnsData.Topic_Info _topic)
        {
            if (_topic != null)
            {
                foreach (var cell in TopicCellList)
                {
                    if (cell.aData.infoData.id == _topic.id)
                    {
                        cell.ClickCancelState();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 清空話題列表
        /// </summary>
        private void ClearTopicListContentParent()
        {
            foreach (var cell in TopicCellList)
            {
                cell.CloseTouch();
                StartCoroutine
                    (
                    cell.gameObject.eSetCanvasGroup(0, () =>
                    {
                        Destroy(cell.gameObject);
                    })
                 );
            }
            TopicCellList.Clear();
        }
        /// <summary>
        /// 控制話題篩選UI的開關
        /// </summary>
        /// <param name="type"></param>
        /// <param name="_switch"></param>
        /// <returns></returns>
        private IEnumerator AlterTopicFilterColor(TopicType type, bool _switch, bool Tween = true)
        {
            Button tarBtn = null;
            switch (type)
            {
                case TopicType.None:
                    tarBtn = AllFilterButton;
                    break;
                case TopicType.Daily:
                    tarBtn = DailyFilterButton;
                    break;
                case TopicType.SideStory:
                    tarBtn = SideStoryFilterButton;
                    break;
                case TopicType.Special:
                    tarBtn = SpecialFilterButton;
                    break;
            }
            GameObject closeImg = tarBtn.transform.Find("CloseStateType").gameObject;
            GameObject selectImg = tarBtn.transform.Find("SelectType").gameObject;

            selectImg.SetActive(true);
            closeImg.SetActive(true);

            if (_switch)
            {

                if (Tween)
                {
                    selectImg.SetCanvasGroup(0);
                    StartCoroutine(selectImg.eSetCanvasGroup(1));
                    closeImg.SetCanvasGroup(1);
                    StartCoroutine(closeImg.eSetCanvasGroup(0, () => { closeImg.SetActive(false); }));
                }
                else
                {
                    closeImg.SetActive(false);
                }

            }

            else
            {
                if (Tween)
                {
                    closeImg.SetCanvasGroup(0);
                    StartCoroutine(closeImg.eSetCanvasGroup(1));
                    selectImg.SetCanvasGroup(1);
                    StartCoroutine(selectImg.eSetCanvasGroup(0, () => { selectImg.SetActive(false); }));
                }
                else
                {
                    selectImg.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        /// <summary>
        /// 設置對話框灰色背景的高度(多選項切換時會使用到
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoSetInputBackgroundHeight(bool waitNextFrame = false)
        {
            //出現選項以及選擇不同選項的時候
            if (!waitNextFrame)
            {
                while (true)
                {
                    yield return MoveMessageContentToBottom(0);
                }
            }
            else
            {
                yield return null;
                yield return MoveMessageContentToBottom(0);
            }

        }

        /// <summary>
        /// 聊天室下方彈窗彈起的高
        /// </summary>
        private readonly int bottomPopupHeight = 600;
        /// <summary>
        /// 設定下方UI的Y軸高度(選擇話題時會使用到
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetBottomPopupPositionY(bool toPopup)
        {
            if (toPopup && BottomPopup.anchoredPosition.y < bottomPopupHeight)
            {
                BottomPopupState = true;
                Debug.Log("現在的狀態是=>" + state.ToString());
                switch (state)
                {
                    case ChatRoomState.WaitOpenTopic:
                        TopicListView.gameObject.SetActive(true);
                        OptionListViewParent.gameObject.SetActive(false);
                        StartCoroutine(TopicListView.gameObject.eSetCanvasGroup(1));
                        break;
                    case ChatRoomState.WaitReply:
                        TopicListView.gameObject.SetActive(false);
                        OptionListViewParent.gameObject.SetActive(true);
                        StartCoroutine(OptionListViewParent.gameObject.eSetCanvasGroup(1));
                        break;
                }
                messageScrollViewParent.DOSizeDelta(new Vector2(960, 730), 0.2f);

                BottomPopup.anchoredPosition = new Vector2(0, 0);//先調整到預設的開始位置

                bool isBottom = IsMessageContentInToBottom();
                Debug.Log("設置位置");
                yield return BottomPopup.DOAnchorPosY(bottomPopupHeight, 0.2f).OnUpdate(() =>
                {

                    if (isBottom)
                    {
                        StartCoroutine(MoveMessageContentToBottom(0));
                    }
                }).WaitForCompletion();



            }
            else if (!toPopup)
            {
                BottomPopupState = false;

                AdjustTopicCellCancel(chooseTopic);
                chooseTopic = null;

                messageScrollViewParent.DOSizeDelta(new Vector2(960, 1175), 0.2f);

                //BottomPopup.anchoredPosition = new Vector2(0, bottomPopupHeight);//先調整到預設的開始位置
                CloseSendButton();
                answerOptionText.text = LocalizeKey.defaultAnswerDisplayerStr;
                answerOptionText.color = inputTextDefaultColor;
                yield return BottomPopup.DOAnchorPosY(0, 0.2f).WaitForCompletion();
            }
        }
        /*
        /// <summary>
        /// 調整聊天室的對話氣泡預覽範圍
        /// </summary>
        /// <param name="endValue"></param>
        /// <returns></returns>
        private IEnumerator AdjustMessageContentPosY(float endValue, float dur = 0.2f)
        {
            yield return messageContentParent.DOAnchorPosY(endValue, dur).WaitForCompletion();
        }*/

        /// <summary>
        /// 將聊天視窗移動到最下方(查看最新對話
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveMessageContentToBottom(float dur = 0.2f)
        {
            messageContentParent.DOKill();
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageContentParent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageScrollViewParent);
            float tarPosY = messageContentParent.sizeDelta.y - messageScrollViewParent.sizeDelta.y;
            tarPosY = tarPosY > 0 ? tarPosY : 0;
            if (dur > 0)
            {
                yield return messageContentParent.DOAnchorPosY(tarPosY, dur).WaitForCompletion();
            }
            else
            {
                messageContentParent.anchoredPosition = new Vector2(0, tarPosY);
            }
        }
        /// <summary>
        /// 偵測玩家訊息視窗是否移動到最底下
        /// (切換下方彈窗,回覆話題或顯示移動到最下方的ui時可以用
        /// </summary>
        /// <returns></returns>
        private bool IsMessageContentInToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageScrollViewParent);

            if (messageContentParent.anchoredPosition.y > (messageContentParent.sizeDelta.y - messageScrollViewParent.sizeDelta.y) - 100)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        #endregion
        /// <summary>
        /// 聊天室執行狀態
        /// </summary>
        public enum ChatRoomState
        {
            None,
            WaitOpenTopic,
            WaitOtherSideSendMessage,
            WaitReply,
        }


    }//class SnsChatRoom



}//name space
