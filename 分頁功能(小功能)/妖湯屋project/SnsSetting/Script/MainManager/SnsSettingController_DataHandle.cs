using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using YKO.Network;
using YKO.Support.Expansion;

namespace YKO.SnsSetting
{
    /// <summary>
    /// SnsSettingController  
    /// 管理資料傳遞
    /// </summary>
    public partial class SnsSettingController
    {

        /// <summary>
        /// 根據角色獲得對應的話題狀態
        /// </summary>
        private List<CharaSnsRecord> charaTopicList = new List<CharaSnsRecord>();

        public List<CharaSnsRecord> CharaTopicList { get { return charaTopicList; } }


        #region 
        /// <summary>
        /// 主要儲存所有聯絡人及轉化相關信息的Proto,其他proto也是更新這個proto
        /// </summary>
        private Proto_30200_Response proto_30200Data = null;

        private static object _lock = new object();

        /// <summary>
        /// 當前執行完成的proto
        /// </summary>
        private static IMessage handleFinishProto = null;

        #endregion

        #region ReceiveResponse
        /// <summary>
        /// 註冊Response反饋
        /// </summary>
        private void RegisterProtoHandle()
        {
            MessageResponseData.Instance.OnMessageResponse
                 //  .Where(meg=>meg.MessageID.ToString() =="30200"||"30201"||"30202"||"30203"||"30204"||"30205"||"30290")
                 .Where(meg =>
                 meg.MessageID == 30200 ||
                 meg.MessageID == 30201 ||
                 meg.MessageID == 30202 ||
                 meg.MessageID == 30203 ||
                 meg.MessageID == 30204 ||
                 meg.MessageID == 30205 ||
                 meg.MessageID == 30290
                 )
                .Subscribe(meg =>
                {
                    Debug.Log("@@@@Proto" + meg.MessageID + "=>" + JsonConvert.SerializeObject(meg));
                    switch (meg.MessageID)
                    {

                        case 30200://完全更新CharaTopicList 角色對話,歷史對話等
                                   // HandleProto30200((Proto_30200_Response)meg);
                            StartCoroutine(HandleProto30200((Proto_30200_Response)meg));
                            ///獲得靜態資料 並根據靜態資料生成對應的資訊
                            break;
                        case 30201://公告顯示,如果再sns階段,解鎖新的話題並更新至CharaTopicList(主要更新)
                            HandleProto30201((Proto_30201_Response)meg);
                            break;
                        case 30202://開始新話題 //設定開始故事給伺服器  
                            HandleProto30202((Proto_30202_Response)meg);
                            //話題清單必須先設定成已看過
                            break;
                        case 30203://話題被next與指向等被更新(主要更新故事後續的next)
                            HandleProto30203((Proto_30203_Response)meg);
                            break;
                        case 30204://設定回覆給server的回傳
                            HandleProto30204((Proto_30204_Response)meg);
                            break;
                        case 30205://設定回覆給server的回傳
                            HandleProto30205((Proto_30205_Response)meg);
                            break;
                        case 30290://清空所有紀錄 重新歸零帳戶
                            HandleProto30290((Proto_30290_Response)meg);
                            break;

                    }

                    lock (_lock)//放後面是為了讓前面的proto設置完成  再調用
                    {

                        handleFinishProto = meg;
                    }
                })
                .AddTo(this);
        }
        /// <summary>
        /// 等待proto回傳,如有code則回傳結果
        /// </summary>
        /// <param name="protoName"></param>
        /// <returns></returns>
        public IEnumerator WaitHandleProto(string protoName, Action<bool> resultCB = null)
        {

            ShowLoading();
            lock (_lock)
            {
                yield return new WaitUntil(() => protoName == handleFinishProto?.MessageID.ToString());

                if (handleFinishProto.GetType().GetField("code") != null)
                {
                    bool result = false;
                    if ((byte)handleFinishProto.GetType().GetField("code").GetValue(handleFinishProto) > 0)
                    {
                        result = true;
                    }
                    if (resultCB != null)
                    {
                        resultCB(result);
                    }
                }
            }
            CloseLoading();
            handleFinishProto = null;
        }

        public string[] bugTopicID = new string[] {
        "5551"

        };
        /*    /// <summary>
            /// 將proto轉化並加入charaRecord 資料
            /// </summary>
            /// <param name="proto30200"></param>
            private void HandleProto30200(Proto_30200_Response proto30200)
            {
                if (proto_30200Data == null)
                {
                    proto_30200Data = proto30200;

                    //當前解鎖話題列表
                    foreach (var unlockCharaRecord in proto_30200Data.unlocked_topics)//先分開每隻角色
                    {
                        Debug.Log("生成角色ID=>"+unlockCharaRecord.chara_id);


                        if (bugTopicID.Contains(unlockCharaRecord.chara_id.ToString())) 
                        {
                            continue;
                        }


                        CharaSnsRecord record = new CharaSnsRecord(unlockCharaRecord.chara_id.ToString());

                        foreach (var topicInfo in SnsFixedData.GetTopicInfo(unlockCharaRecord.chara_id.ToString()))//角色擁有的每個靜態話題
                        {
                            Dictionary<long, List<int>> _choiceHistory = new Dictionary<long, List<int>>();
                            bool isRead = false;
                            bool isLock = true;
                            foreach (var sTopics in unlockCharaRecord.topics) //該角色的話題
                            {
                                if (sTopics.topic_id==topicInfo.id)
                                {
                                    isLock = false;
                                    if (sTopics.read>0)
                                    {
                                        isRead = true;
                                    }
                                }
                                foreach (var choiceInfo in sTopics.choice_histoty) 
                                {
                                    List<int>pastChoice=new List<int>();
                                    foreach (var choiceValue in choiceInfo.choice_list)
                                    {
                                        pastChoice.Add((choiceValue.choice_value-1));
                                    }

                                    var groupID = SnsFixedData.GetTopicData(sTopics.topic_id.ToString())[choiceInfo.index.ToString()].group_id;

                                    if (!_choiceHistory.ContainsKey(groupID)) 
                                    {
                                        _choiceHistory.Add( groupID , pastChoice );
                                    }
                                    else
                                    {
                                        var pastAnswer=_choiceHistory[groupID];
                                        foreach (var answer in pastChoice)
                                        {
                                            if (!pastAnswer.Contains(answer))
                                            {
                                                pastAnswer.Add(answer);
                                            }
                                        }
                                    }
                                 }
                            }
                            record.topicInfoLists.Add(new TopicDisplayInfo(topicInfo, isLock, isRead)
                            {
                                choiceHistory = _choiceHistory
                            }) ;
                            //加入top
                        }
                        charaTopicList.Add( record );

                    }


                    foreach (var historyData in proto_30200Data.topic_records)
                    {
                        if (bugTopicID.Contains(historyData.chara_id.ToString()))
                        {
                            continue;
                        }

                            var record = charaTopicList.Find(_record => _record.charaID == historyData.chara_id.ToString());
                            int topicOrder = 0;

                            foreach (var topicRecord in historyData.records) {
                                Debug.Log("製作話題紀錄=>" + topicOrder + "次,該話題是id=>" + topicRecord.topic_id);
                                topicOrder++;
                                TopicData topic = CreateTopicToHistoryList(record, topicRecord.topic_id.ToString());
                                topic.curID = topicRecord.message_index;
                                topic.topicSort = topicOrder;

                                var megInfo = SnsFixedData.GetTopicData(topicRecord.topic_id.ToString())[topicRecord.message_index.ToString()];

                                if (megInfo.chara_id > 0)
                                {
                                    var reply = topic.ReplyDic[topicRecord.message_index];
                                    reply.monoUserDialog = true;
                                }

                                if (topicRecord.status <= 0)
                                {
                                    if (topicRecord.message_status <= 0)
                                    {
                                        topic.state = ContactPersonState.NotLoading;
                                    }
                                    else
                                    {
                                        topic.state = ContactPersonState.NotReply;
                                    }
                                }
                                else
                                {
                                    topic.state = ContactPersonState.Finish;
                                }

                                foreach (var variable in topicRecord.history)
                                {
                                    var megFixedData = SnsFixedData.GetTopicData(topicRecord.topic_id.ToString())[variable.index.ToString()];
                                    var tarReply = topic.ReplyDic[variable.index];

                                    if (megFixedData.group_id > 0)
                                    {

                                        foreach (var meg in tarReply.messageList)
                                        {
                                            meg.tarID = megFixedData.group_id;
                                        }

                                        var userReply = topic.ReplyDic[megFixedData.group_id];

                                        if (variable.choice_value > 0)
                                        {
                                            userReply.curIndex = (variable.choice_value - 1);
                                        }
                                        else
                                        {
                                            userReply.curIndex = variable.choice_value;
                                        }

                                        if (variable.next > 0) {
                                            foreach (var meg in userReply.messageList)
                                            {
                                                meg.tarID = variable.next;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        foreach (var meg in tarReply.messageList)
                                        {
                                            meg.tarID = variable.next;
                                        }
                                    }

                                }
                            }
                    }

                }
                else
                {
                    Debug.LogError("Already data ,  Can Not Catch Proto30200  In This Timer,");
                }
            }*/


        private bool firstLoad = false;

        /// <summary>
        /// 將proto轉化並加入charaRecord 資料
        /// </summary>
        /// <param name="proto30200"></param>
        private IEnumerator HandleProto30200(Proto_30200_Response proto30200)
        {
            if (proto_30200Data == null)
            {
                proto_30200Data = proto30200;

                yield return LoadPlayerSnsTopicStateInfo(proto30200);
                charaTopicList.SmallSizeSort("charaID");
              //  yield return new WaitUntil(() => (maxFinishExecuteCount <= curFinishExecuteCount)&&!charaTopicList.Any(v=>v==null));
                /*  Debug.Log("話題數量=>" + charaTopicList.Count);
                  int frequency = 0;
                   foreach (var rec in charaTopicList)
                 {
                      Debug.Log("次數=>" + frequency + "rec=>" + rec);
                      var recJson = JsonConvert.SerializeObject(rec);
                      Debug.Log("json=>" + recJson);
                      frequency++;
                  }
                  Debug.Log("加載歷史話題");*/

                yield return LoadPlayerTopicHistoryData(proto30200);

                firstLoad = true;
            }
            else
            {
                Debug.LogError("Already data ,  Can Not Catch Proto30200  In This Timer,");
            }
        }

        /// <summary>
        /// 加載玩家當前話題狀態的資訊
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadPlayerSnsTopicStateInfo(Proto_30200_Response proto30200)
        {
            List<Task> taskList = new List<Task>();
            //當前解鎖話題列表
            foreach (var unlockCharaRecord in proto30200.unlocked_topics)//先分開每隻角色
            {
                if (bugTopicID.Contains(unlockCharaRecord.chara_id.ToString()))
                {
                    continue;
                }

                taskList.Add(Task.Run(() => { 
                    CreateCharaSnsRecord(unlockCharaRecord);
                   // curFinishExecuteCount++;
                }));
            }
            //maxFinishExecuteCount=taskList.Count;
            yield return RunTaskAsCoroutine(taskList);
            //Debug.Log("加載完topicInfo了");

        }
        private void CreateCharaSnsRecord(Proto_30200_Response.Unlocked_topics unlockCharaRecord)
        {
            CharaSnsRecord record = new CharaSnsRecord(unlockCharaRecord.chara_id.ToString());
            var fixedData = SnsFixedData.GetTopicInfo(unlockCharaRecord.chara_id.ToString());

            foreach (var topicFd in fixedData)//角色擁有的每個靜態話題
            {
                var topicInfo = new TopicDisplayInfo();
                bool isRead = false;
                bool isLock = true;
                Dictionary<long, List<int>> _choiceHistory = new Dictionary<long, List<int>>();

                var topics = unlockCharaRecord.topics.ToList().Find(x => x.topic_id == topicFd.id);

                if (topics != null)
                {
                    isLock = false;
                    if (topics.read > 0)
                        isRead = true;
                    topics.choice_histoty.ToList().ForEach(choiceInfo =>
                    {
                        List<int> pastChoice = choiceInfo.choice_list.ToList().Select(v => (v.choice_value - 1)).ToList();
                        var groupID = SnsFixedData.GetTopicData(topics.topic_id.ToString())[choiceInfo.index.ToString()].group_id;
                        if (!_choiceHistory.ContainsKey(groupID))
                            _choiceHistory.Add(groupID, pastChoice);
                        else
                        {
                            var pastAnswer = _choiceHistory[groupID];
                            pastChoice.ForEach(answer =>
                            {
                                if (!pastAnswer.Contains(answer)) pastAnswer.Add(answer);
                            });
                        }
                    });
                    topicInfo.Init(topicFd, isLock, isRead, _choiceHistory);
                    //加入話題=>sns_topic_3511003
                    record.topicInfoLists.Add(topicInfo);
                }
            }
            lock (_lock)
            {
                charaTopicList.Add(record);
            }
        }

        private IEnumerator LoadPlayerTopicHistoryData(Proto_30200_Response proto30200)
        {
            List<Task> taskList = new List<Task>();
            foreach (var historyData in proto30200.topic_records)
            {
                if (bugTopicID.Contains(historyData.chara_id.ToString()))
                {
                    continue;
                }

                var record = charaTopicList.Find(_record =>
                {
                    return _record.charaID == historyData.chara_id.ToString();
                });
                var task = Task.Run(() =>
                {
                    int topicOrder = 0;
                    foreach (var topicRecord in historyData.records)
                    {
                        //Debug.Log("執行歷史話題資料結構整合=>"+topicRecord.topic_id+"當前index=>"+topicRecord.message_index);

                        topicOrder++;
                        TopicData topic = CreateTopicToHistoryList(record, topicRecord.topic_id.ToString());
                        topic.curID = topicRecord.message_index;
                        topic.topicSort = topicOrder;

                        var megInfo = SnsFixedData.GetTopicData(topicRecord.topic_id.ToString())[topicRecord.message_index.ToString()];

                        if (megInfo.chara_id > 0)
                        {
                            var reply = topic.ReplyDic[topicRecord.message_index];
                            reply.monoUserDialog = true;
                        }

                        if (topicRecord.status <= 0)
                        {
                            if (topicRecord.message_status <= 0)
                            {
                                topic.state = ContactPersonState.NotLoading;
                            }
                            else
                            {
                                topic.state = ContactPersonState.NotReply;
                            }
                        }
                        else
                        {
                            topic.state = ContactPersonState.Finish;
                        }

                        foreach (var history in topicRecord.history)
                        {
                            var megFixedData = SnsFixedData.GetTopicData(topicRecord.topic_id.ToString())[history.index.ToString()];
                            var tarReply = topic.ReplyDic[history.index];

                            if (megFixedData.group_id > 0)
                            {

                                foreach (var meg in tarReply.messageList)
                                {
                                    meg.tarID = megFixedData.group_id;
                                }

                                var userReply = topic.ReplyDic[megFixedData.group_id];

                                if (history.choice_value > 0)
                                {
                                    userReply.curIndex = (history.choice_value - 1);
                                }
                                else
                                {
                                    userReply.curIndex = history.choice_value;
                                }

                                if (history.next > 0)
                                {
                                    foreach (var meg in userReply.messageList)
                                    {
                                        meg.tarID = history.next;
                                    }
                                }

                            }
                            else
                            {
                                foreach (var meg in tarReply.messageList)
                                {
                                    meg.tarID = history.next;
                                }
                            }
                        }
                    }
                    //根據tarID去加入進topicData
                });

                taskList.Add(task);
            }
            yield return RunTaskAsCoroutine(taskList);
        }

        private IEnumerator RunTaskAsCoroutine(List<Task> tasks)
        {

            yield return Task.WhenAll(tasks);

            // 若有例外拋出，處理它
            foreach (var t in tasks)
            {
                if (t.IsFaulted)
                    throw t.Exception;
            }
            Debug.Log("加載完topicInfo了");
        }
        /// <summary>
        /// 更新話題資訊的解鎖狀況
        /// </summary>
        /// <param name="proto30201"></param>
        private void HandleProto30201(Proto_30201_Response proto30201)
        {
            foreach (var unLockTopic in proto30201.unlocked_topics)
            {
                var record = charaTopicList.Find(record => record.charaID == unLockTopic.chara_id.ToString());

                if (record == null)
                {

                    var topicInfoList = SnsFixedData.GetTopicInfo(unLockTopic.chara_id.ToString());

                    record = new CharaSnsRecord(unLockTopic.chara_id.ToString());

                    foreach (var topicFd in topicInfoList)
                    {
                        bool isLock = true;
                        foreach (var topicID in unLockTopic.topic_ids)
                        {
                            if (topicID.topic_id == topicFd.id)
                            {
                                isLock = false;
                            }
                        }
                        var topicInfo = new TopicDisplayInfo();
                        topicInfo.Init(topicFd, isLock, false);
                        record.topicInfoLists.Add(topicInfo);
                    }
                    charaTopicList.Add(record);
                }
                else
                {
                    foreach (var topicID in unLockTopic.topic_ids)
                    {
                        var tarInfo = record.topicInfoLists.Find(topicRec => topicRec.infoData.id == topicID.topic_id);
                        tarInfo.isLock = false;
                    }
                }

            }
        }
        /// <summary>
        /// 開始新話題的回傳提醒(加入故事進history,更新topiclist的topicinfo  isRead=true
        /// </summary>
        private void HandleProto30202(Proto_30202_Response proto30202)
        {
            switch (proto30202.code)
            {
                case 0:
                    Debug.Log("proto30202加載失敗=>" + LocaleManager.Instance.ParseServerMessage(proto30202.msg));
                    break;
                case 1:
                    Debug.Log("proto30202加載成功");
                    break;
            }

        }

        /// <summary>
        /// 更新後續的劇情
        /// </summary>
        /// <param name="proto30203"></param>
        /// 

        private void HandleProto30203(Proto_30203_Response proto30203)//新增話題或以讀話題,或要更新話題後續的next
        {
            var record = charaTopicList.Find(record => record.charaID == proto30203.chara_id.ToString());

             record.topicInfoLists.First(r => r.infoData.id == proto30203.topic_id).isRead=true;
            var proto = MessageResponseData.Instance.ProtoResponse30200;

            proto.unlocked_topics.First(t => t.chara_id == proto30203.chara_id).topics.First(t => t.topic_id == proto30203.topic_id).read = 1;


            TopicData topic = null;
            if (record.historytopicLists.Count <= 0 ||
               (record.historytopicLists.Count > 0 && record.GetCurLastNewTopic().IsPlotOver()))///開始新的話題
            {
                topic = CreateTopicToHistoryList(record, proto30203.topic_id.ToString());
                topic.topicSort = record.GetTopicNumberOfBiggest() + 1;
                Debug.Log("生成的新話題sort=>" + topic.topicSort);
            }
            else///更新舊的話題
            {
                topic = record.GetCurLastNewTopic();
            }

            // topic.curID = topicRecord.message_index;
            if (proto30203.status <= 0)//進行中
            {
                if (proto30203.message_status <= 0)
                {
                    topic.state = ContactPersonState.NotLoading;
                }
                else
                {
                    topic.state = ContactPersonState.NotReply;
                }
            }
            else//已結束
            {
                topic.state = ContactPersonState.Finish;
            }

            foreach (var variable in proto30203.history)
            {
                var megFixedData = SnsFixedData.GetTopicData(proto30203.topic_id.ToString())[variable.index.ToString()];
                var tarReply = topic.ReplyDic[variable.index];


                if (megFixedData.chara_id > 0)
                {
                    tarReply.monoUserDialog = true;
                }


                if (megFixedData.group_id > 0)
                {

                    foreach (var meg in tarReply.messageList)
                    {
                        meg.tarID = megFixedData.group_id;
                    }

                    var userReply = topic.ReplyDic[megFixedData.group_id];
                    if (variable.choice_value > 0)
                    {
                        userReply.curIndex = (variable.choice_value - 1);
                    }
                    else
                    {
                        userReply.curIndex = variable.choice_value;
                    }

                    foreach (var meg in userReply.messageList)
                    {
                        if (variable.next > 0)
                        {
                            meg.tarID = variable.next;
                        }
                    }

                }
                else
                {
                    foreach (var meg in tarReply.messageList)
                    {
                        if (variable.next > 0)
                        {
                            meg.tarID = variable.next;
                        }
                    }
                }

            }//---foreach topicRecord.history
        }//---func over

        /// <summary>
        /// 給予server玩家的選項回覆的伺服器回傳
        /// </summary>
        private void HandleProto30204(Proto_30204_Response proto30204)
        {

            switch (proto30204.code)
            {
                case 0:
                    //     Debug.Log("proto30204加載失敗=>" + proto30204.msg);
                    break;
                case 1:
                    //     Debug.Log("proto30204加載成功");
                    break;
            }
        }
        /// <summary>
        /// 玩家以讀後的回傳
        /// </summary>
        /// <param name="proto30205"></param>
        private void HandleProto30205(Proto_30205_Response proto30205)
        {
            switch (proto30205.code)
            {
                case 0:
                    //     Debug.Log("proto30204加載失敗=>" + proto30204.msg);
                    break;
                case 1:
                    //     Debug.Log("proto30204加載成功");
                    break;
            }



        }

        /// <summary>
        /// 清除所有歷史紀錄(以免反覆測試,記錄過長
        /// </summary>
        private void HandleProto30290(Proto_30290_Response proto30290)
        {
            switch (proto30290.code)
            {
                case 0:
                    Debug.Log("proto30290加載失敗=>" + proto30290.msg);
                    break;
                case 1:
                    Debug.Log("proto30290加載成功");
                    break;
            }
        }


        /// <summary>
        /// 生成劇情轉化為TopicData
        /// </summary>
        /// <param name="chara_id"></param>
        /// <param name="topic_id"></param>
        /// <returns></returns>
        public TopicData CreateTopicToHistoryList(CharaSnsRecord record, string topic_id)
        {
            var topicInfoFixedData = SnsFixedData
            .GetTopicInfo(record.charaID)
            .Find(topicInfo => topicInfo.id.ToString() == topic_id);

            TopicData topic = new TopicData();

            //topic.topicLabel = LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData ,topicInfoFixedData.name);
            topic.topicLabel = topicInfoFixedData.name;

            topic.topicID = uint.Parse(topic_id);
            topic.curID = 0;

            var topicMessageList = SnsFixedData.GetTopicData(topic_id);


            for (int i = 0; i < topicMessageList.Count; i++)
            {
                var fixedMeg = topicMessageList[(i + 1).ToString()];

                ReplySns reply = null;


                if (fixedMeg.chara_id <= 0)
                {
                    reply = new ReplySns(fixedMeg, record.charaID, (i + 1))
                    {
                        iconID = record.iconID
                    };
                }
                else
                {
                    reply = new ReplySns(fixedMeg, "1", (i + 1))
                    {
                        iconID = "char001"
                    };
                }

                topic.ReplyDic.Add((i + 1), reply);
                var _iconID = fixedMeg.chara_id > 0 ? record.iconID : "char001";
                if (fixedMeg.group_id > 0 && !topic.ReplyDic.ContainsKey(fixedMeg.group_id))
                {

                    var groupReply = new ReplySns(SnsFixedData.GetUserReply(fixedMeg.group_id.ToString()), JsonConvert.DeserializeObject<long[]>(fixedMeg.next_index.ToString()))
                    {
                        iconID = _iconID,
                        isOptionReply = true
                    };

                    for (int a = 0; a < groupReply.messageList.Count; a++)
                    {
                        var _meg = groupReply.messageList[a];
                        long[] nextList = JsonConvert.DeserializeObject<long[]>(fixedMeg.next_index.ToString());

                        if (nextList.Length > a)
                        {
                            _meg.tarID = nextList[a];
                        }
                        else
                        {
                            _meg.tarID = nextList[(nextList.Length - 1)];
                        }
                    }
                    topic.ReplyDic.Add(fixedMeg.group_id, groupReply);

                }
            }
            record.historytopicLists.Add(topic);
            return topic;

        }

        #endregion

        #region sendProto

        /// <summary>
        /// 開始新話題
        /// </summary>
        /// <param name="topic_id"></param>
        public void SendProto30202Request(uint topic_id)
        {
            Debug.Log("開始話題=>" + topic_id);
            Proto_30202_Request meg = new Proto_30202_Request();
            meg.topic_id = topic_id;
            NetworkManager.Instance.Send(meg);
        }

        /// <summary>
        /// 回答選項回覆
        /// </summary>
        /// <param name="topic_id"></param>
        /// <param name="index"></param>
        /// <param name="choiceValue"></param>
        public void SendProto30204Request(uint topic_id, uint index, uint choiceValue)
        {
            Debug.Log("#3話題id=>" + topic_id + "話題排序=>" + index + "話題選擇=>" + choiceValue);
            Proto_30204_Request meg = new Proto_30204_Request();
            meg.topic_id = topic_id;
            meg.index = index;
            meg.choice_value = choiceValue;
            NetworkManager.Instance.Send(meg);

        }
        /// <summary>
        /// 標示玩家已讀取
        /// </summary>
        /// <param name="topic_id"></param>
        public void SendProto30205Request(uint topic_id)
        {
            Proto_30205_Request meg = new Proto_30205_Request();
            meg.topic_id = topic_id;
            NetworkManager.Instance.Send(meg);
        }
        /// <summary>
        /// 寄送請求
        /// </summary>
        public void SendProtoRequest(string protoNum)
        {
            IMessage meg = null;
            switch (protoNum)
            {
                case "30200":
                    meg = new Proto_30200_Request();//獲取所有sns的資訊
                    break;
                case "30201":
                    meg = new Proto_30201_Request();//新解鎖SNS通知 類似公告
                    break;
                case "30203"://理論上不會打  都是由server 傳response回來
                    meg = new Proto_30203_Request();//話題新增/變更通知
                    break;
                case "30290":
                    meg = new Proto_30290_Request(); //gm - 清除SNS紀錄
                    break;
            }
            Debug.Log("已寄送=>" + protoNum);
            NetworkManager.Instance.Send(meg);
        }
        #endregion

        /// <summary>
        /// 整理所有角色的話題順序(由早至晚,由大至小
        /// </summary>
        private void SortTopicList(List<TopicData> data = null)
        {
            if (data == null)
            {
                foreach (var curCharaSns in charaTopicList)
                {
                    curCharaSns.SortTopicList();
                }
            }
            else
            {
                data.Sort((top1, top2) =>
                {
                    if (top1.topicSort >= top2.topicSort)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                });
            }
        }

        /// <summary>
        /// 透過角色獲得角色資料
        /// </summary>
        private CharaSnsRecord GetCharaSnsRecordFormConversationChara(string chara)
        {
            return charaTopicList.Find(record => record.charaName == chara);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

    }
}
