using Fungus;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YKO.DataModel;
using YKO.Support;
using static Proto_30200_Response.Unlocked_topics;
using static YKO.DataModel.SnsData;

namespace YKO.SnsSetting
{

    /// <summary>
    /// 角色Sns紀錄
    /// </summary>
    public class CharaSnsRecord
    {
        /// <summary>
        /// 對話的角色
        /// </summary>
        public string charaName = "";
        /// <summary>
        /// 角色的ID
        /// </summary>
        public string charaID = "";
        /// <summary>
        /// 頭像id
        /// </summary>
        public string iconID = "";

        /// <summary>
        /// 話題數據
        /// </summary>
        public List<TopicDisplayInfo> topicInfoLists = new List<TopicDisplayInfo>();

        /// <summary>
        /// 歷史紀錄
        /// </summary>
        public List<TopicData> historytopicLists = new List<TopicData>();

        /// <summary>
        /// 偵測當前腳色是否有正在執行的話題
        /// </summary>
        /// <returns></returns>
        public bool IsAllPlotOver()
        {
            bool isHaveStoryPlotContinue = true;
            foreach (var topic in historytopicLists)
            {
                if (!topic.IsPlotOver())
                {
                    isHaveStoryPlotContinue = false;
                }
            }
            return isHaveStoryPlotContinue;
        }


        /// <summary>
        /// 獲得目前話題裡排序最大的編號(最後面,最新的對話
        /// </summary>
        /// <returns></returns>
        public int GetTopicNumberOfBiggest()
        {
            int tarIndex = 0;

            foreach (var topic in historytopicLists)
            {
                if (topic.topicSort > tarIndex)
                {
                    tarIndex = topic.topicSort;
                }
            }
            return tarIndex;

        }

        /// <summary>
        /// 整理所有角色的話題順序(由早至晚,由小至大,數字越小越下面
        /// </summary>
        public void SortTopicList()
        {
            historytopicLists.Sort((top1, top2) =>
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
        /// <summary>
        /// 檢查話題是否曾經播放過
        /// </summary>
        /// <returns></returns>
        public bool JudgeTopicHasBeenPlayed(uint topicID)
        {
            TopicDisplayInfo tarTopic = null;
             tarTopic = topicInfoLists.First(t=>t.infoData.id==topicID);
            if (tarTopic == null) 
            {
                Debug.Log("#@#Cant find topicID=>"+topicID);
                return true;
            }
            else
            {
                //話題不可重複    且曾經已出現該對話話題
                return tarTopic.infoData.repeat <= 0 && historytopicLists.Any(v => v.topicID == topicID);
            }

        }


        /// <summary>
        /// 獲得角色當前話題資料
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TopicData GetCurLastNewTopic()
        {
            TopicData curTopic = null;
            curTopic = historytopicLists.Find(topic => topic.topicSort == GetTopicNumberOfBiggest());
            return curTopic;
        }

        /// <summary>
        /// 根據enum篩選不同分類的topic(通常enum為StoryStartCondition, ContactPersonState,TopicType
        /// </summary>
        /// <returns></returns>
        public Dictionary<Enum, List<TopicDisplayInfo>> GetTopicListState<T>() where T : Type
        {
            Dictionary<Enum, List<TopicDisplayInfo>> curDic = new Dictionary<Enum, List<TopicDisplayInfo>>();
            foreach (var curEnum in Enum.GetNames(typeof(T)))
            {
                var filterTopic = topicInfoLists.FindAll
                 (
                    topic =>
                    {
                        foreach (var field in topic.GetType().GetFields())
                        {
                            if (field.FieldType == typeof(T))
                            {
                                if (field.GetValue(topic).Equals((T)Enum.Parse(typeof(T), curEnum)))
                                {
                                    return true;
                                }
                            }
                        }
                        return false;
                    }
                 );
                curDic.Add((Enum)Enum.Parse(typeof(T), curEnum), filterTopic);
            }
            return curDic;
        }


        public CharaSnsRecord(string _charaID)
        {
            //Debug.Log("加入的角色id=>" + _charaID);
            charaID = _charaID;
            var fixedData = SnsFixedData.characterFixedData.data_character_info[_charaID];
            charaName = fixedData.name;
            iconID = fixedData.icon;
        }

    }

    /// <summary>
    /// 話題資料
    /// </summary>
    public class TopicData
    {
        /// <summary>
        /// sns事件觸發條件
        /// </summary>
        public StoryStartCondition eventCondition;
        /// <summary>
        /// 話題狀態
        /// </summary>
        public ContactPersonState state;
        /// <summary>
        /// 話題類別
        /// </summary>
        public TopicType topicType = TopicType.None;

        public bool isLock = false;

        /*
        /// <summary>
        /// 話題是否可以重新
        /// (不可重新的話題如果重新,會先刪除原本在聊天室窗上的話題?
        /// </summary>
        public bool canAgain;
        */
        /// <summary>
        /// 話題識別碼
        /// </summary>
        public uint topicID;

        /// <summary>
        /// 話題的名字
        /// </summary>
        public string topicLabel;

        /// <summary>
        /// 整個話題的對話紀錄
        /// </summary>
        public Dictionary<long, ReplySns> ReplyDic = new Dictionary<long, ReplySns>();

        /// <summary>
        /// 當前話題停留的位置
        /// 這個是已經完成的index  代表之後sns開啟時,要執行(curindex+1)
        /// </summary>
        public long curID = 1;

        /// <summary>
        /// 話題被提起的順序(聊天紀錄話題的顯示順序,數字越小,代表越早被提起 (-1)代表沒被提起過
        /// </summary>
        public int topicSort = 1;



      /*  public TopicData(TopicData topicdata)
        {
            TopicData topic = new TopicData();
            foreach (var field in topicdata.GetType().GetFields())
            {

                if (field.Name == "ReplyDic")
                {

                    foreach (var reply in topicdata.ReplyDic)
                    {
                        topic.ReplyDic.Add(reply.Key, new ReplySns(reply.Value));

                    }
                }
                else
                {

                    field.SetValue(topic, field.GetValue(topicdata));
                }

            }
        }*/

        public TopicData()
        {
        }

        /// <summary>
        /// 獲得聯絡人狀態
        /// </summary>
        /// <returns></returns>
        public ContactPersonState GetContactPersonState()
        {
            var reply = GetTarReply();
            if (curID == 0)
            {
                return ContactPersonState.None;
            }

            if (reply == null)
            {
                return ContactPersonState.Finish;
            }

            switch (reply._snsType)
            {

                case SnsType.Message:
                    return ContactPersonState.NotLoading;
                case SnsType.Reply:
                    return ContactPersonState.NotReply;

            }

            return ContactPersonState.Finish;

        }
        /// <summary>
        /// 劇情是否結束?
        /// 因為沒有目標id了,代表沒話題了
        /// </summary>
        /// <returns></returns>
        /// 
        public bool IsPlotOver()
        {
            /*   Debug.Log("curid是=>"+curID);
               Debug.Log("Targetid是=>" + ReplyDic[curID].GetTargetID());
               if (curID<=0) 
               {
                   return false;
               }

               return ReplyDic[curID].GetTargetID()<=0;*/
            return state == ContactPersonState.Finish;
        }
        /// <summary>
        /// 獲得當前的對話(已經對話過
        /// 不太可能有下個順位,但curIndex的可能,因為sort有列位,
        /// 代表對方已經密玩家,或玩家已經開啟話題
        /// </summary>
        /// <returns></returns>
        public ReplySns GetCurReply()
        {
            if (curID > 0)
            {
                /*if (state== ContactPersonState.Finish){
                      if (ReplyDic[curID].GetTargetID()!=0) {
                          //給最新對話的狀態
                          return ReplyDic[ReplyDic[curID].GetTargetID()];
                      }
                      else
                      {
                          return ReplyDic[curID];
                      }

                  }
                  else
                  {
                      return ReplyDic[curID];
                  }*/
                return ReplyDic[curID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 獲得下一個目標Reply
        /// </summary>
        /// <returns></returns>
        public ReplySns GetTarReply()
        {
            if (curID > 0)
            {
                if (ReplyDic[curID].GetTargetID() > 0)
                {
                    return ReplyDic[ReplyDic[curID].GetTargetID()];
                }
                else if (curID == 0)
                {
                    return ReplyDic[1];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return ReplyDic[1];
            }
        }
        /// <summary>
        /// 等待對話加入
        /// 等待後端判斷回傳
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitAddReply()
        {
            long judgeID = curID <= 0 ? 1 : curID;
            yield return new WaitUntil(() => ReplyDic.ContainsKey(judgeID));
        }

        /// <summary>
        /// 默認Array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReplySns this[long index]
        {
            get { return ReplyDic[index]; }
        }

    }
    /// <summary>
    /// Sns對話
    /// </summary>
    public class ReplySns
    {
        /// <summary>
        /// 對話ID(跟字典的編號一致
        /// </summary>
        public long mID;

        /// <summary>
        /// 對話方式
        /// </summary>
        public SnsType _snsType = SnsType.None;
        /// <summary>
        /// 對話的角色
        /// 藉由此拿到名字與頭像
        /// </summary>
        public string charaName;
        /// <summary>
        /// 對話的角色
        /// 藉由此拿到名字與頭像
        /// </summary>
        public string dialogCharaID;
        /// <summary>
        /// 角色頭像
        /// </summary>
        public string iconID;

        /// <summary>
        /// 等待傳遞訊息的時間
        /// </summary>
        public float _dialogWaitTime = 0;

        /// <summary>
        /// 對話(多選項 通常為玩家自己
        /// </summary>
        public List<Message> messageList = new List<Message>();

        /// <summary>
        /// 對話的指數(通常為0,玩家對話(複選項)才會大於0,-1通常是沒被選過
        /// </summary>
        public int curIndex = 0;
        /* /// <summary>
         /// 自己的ID
         /// </summary>
         public int SelfID = 0;*/

        /// <summary>
        /// 訊息傳送的圖片
        /// </summary>
        public Sprite imgSprite = null;
        /// <summary>
        /// 是否為玩家沒選項的對話
        /// </summary>
        public bool monoUserDialog = false;

        public bool isOptionReply = false;


        public ReplySns(ReplySns copyValue)
        {

            foreach (var field in typeof(ReplySns).GetFields())
            {
                field.SetValue(this, field.GetValue(copyValue));
            }
        }
        /// <summary>
        /// 從對方對話的靜態資料轉化成ReplySns
        /// </summary>
        /// <param name="meg"></param>
        public ReplySns(Message_Info meg, string chara_id, int id)
        {

            if (meg.chara_id > 0)
            {
                // _snsType = SnsType.Reply;
                monoUserDialog = true;
            }
            else
            {
                //  _snsType = SnsType.Message;
            }
            _snsType = SnsType.Message;

            mID = id;
            dialogCharaID = chara_id;
            curIndex = 0;

            charaName = SnsFixedData.GetCharacterInfo(dialogCharaID).name;






            _dialogWaitTime = (meg.delay / 1000f);
            Message newMeg = new Message();

            if (meg.group_id > 0)
            {

                newMeg.tarID = meg.group_id;
            }
            else
            {
                if (meg.next_index != null)
                {
                    newMeg.tarID = JsonConvert.DeserializeObject<long[]>(meg.next_index.ToString())[0];//對方回覆都預設為0
                }
                else
                {
                    newMeg.tarID = 0;
                }
            }
            newMeg.message = meg.text;
            messageList.Add(newMeg);

            // foreach (var field in ) { }


        }
        /// <summary>
        /// 從對方對話的靜態資料轉化成ReplySns
        /// </summary>
        /// <param name="meg"></param>
        public ReplySns(List<ChoiceData.Sns_Choice> choiceList, long[] nextArr)
        {
            _snsType = SnsType.Reply;
            dialogCharaID = "1";//預設玩家
            charaName = SnsFixedData.GetCharacterInfo(dialogCharaID).name;
            _dialogWaitTime = 0;
            mID = choiceList[0].group_id;
            curIndex = 0;
            for (int i = 0; i < choiceList.Count; i++)
            {
                var choice = choiceList[i];

                long next = nextArr[nextArr.Length - 1];
                if (nextArr.Length > i)
                {
                    next = nextArr[i];
                }

                Message newMeg = new Message();
                newMeg.synopsisOfMessage = choice.abstract_arg;
                newMeg.message = choice.text;
                newMeg.tarID = next;
                messageList.Add(newMeg);

            }




            // foreach (var field in ) { }


        }


        /// <summary>
        /// 目標對話ID
        /// </summary>
        /// <returns></returns>
        public long GetTargetID()
        {
            return messageList[curIndex].tarID;

        }

        /// <summary>
        /// 獲得當前的訊息
        /// </summary>
        /// <returns></returns>
        public string GetCurMessage()
        {
            return messageList[curIndex].message;
        }

        public string this[int index]
        {
            get { return messageList[index].message; }
        }
    }//--replySns

    /// <summary>
    /// Sns系統的訊息
    /// </summary>
    public class Message
    {
        /*
        /// <summary>
        /// 訊息條件(可能會根據條件更改tarID
        /// </summary>
        public List<MessageCondition> conditions;*/

        /// <summary>
        /// 目標訊息ID(根據條件篩選
        /// </summary>
        public long tarID;
        /// <summary>
        /// 訊息主要內容
        /// </summary>
        public string message;
        /// <summary>
        /// 訊息概要
        /// </summary>
        public string synopsisOfMessage;

        /*   public static implicit operator string(Message meg) 
           {
               return meg.synopsisOfMessage;
           }*/
    }
    /// <summary>
    /// 話題卡顯示資訊
    /// </summary>
    public class TopicDisplayInfo
    {
        /// <summary>
        /// 話題資訊
        /// </summary>
        public Topic_Info infoData;
        /// <summary>
        /// 話題類別
        /// </summary>
        public TopicType topicType = TopicType.None;
        /// <summary>
        /// sns事件觸發條件
        /// </summary>
        /// 
        public StoryStartCondition eventCondition;
        /// <summary>
        /// 話題是否已撥放過
        /// </summary>
        public bool isRead;
        /// <summary>
        /// 是否尚未解鎖
        /// </summary>
        public bool isLock = false;

        /// <summary>
        /// 過去的選擇  index    int[]
        /// </summary>
        public Dictionary<long, List<int>> choiceHistory = new Dictionary<long, List<int>>();

        /// <summary>
        /// 傳入初始化參數
        /// </summary>
        /// <param name="info">靜態資輛</param>
        /// <param name="_isLock"></param>
        /// <param name="_isRead"></param>
        /// <param name="_choiceHistory"></param>
        public void Init(Topic_Info info, bool _isLock, bool _isRead, Dictionary<long, List<int>> _choiceHistory=null)
        {
            infoData = info;
            isLock = _isLock;
            isRead = _isRead;
            if(_choiceHistory!=null) choiceHistory = _choiceHistory;

            if (info.trigger_type > 0)
            {
                eventCondition = StoryStartCondition.Player;
            }
            else
            {
                eventCondition = StoryStartCondition.System;
            }

            switch (info.type)
            {
                case 0:
                    topicType = TopicType.None;
                    break;
                case 1:
                    topicType = TopicType.Daily;
                    break;
                case 2:
                    topicType = TopicType.SideStory;
                    break;
                case 3:
                    topicType = TopicType.Special;
                    break;
            }
        }

         /*public TopicDisplayInfo(Topic_Info info, bool _isLock, bool _isRead)
         {
             infoData = info;
             isLock = _isLock;
             isRead = _isRead;
             if (info.trigger_type > 0)
             {
                 eventCondition = StoryStartCondition.Player;
             }
             else
             {
                 eventCondition = StoryStartCondition.System;
             }

             switch (info.type)
             {
                 case 0:
                     topicType = TopicType.None;
                     break;
                 case 1:
                     topicType = TopicType.Daily;
                     break;
                 case 2:
                     topicType = TopicType.SideStory;
                     break;
                 case 3:
                     topicType = TopicType.Special;
                     break;
             }
         }//構造函數end*/



        /// <summary>
        /// 新增歷史選項紀錄進client 端 choice history
        /// </summary>
        /// <param name="index"></param>
        /// <param name="choiceValue"></param>
        public void AddHistoryRecordInChoice(long index, int choiceValue)
        {
            if (!choiceHistory.ContainsKey(index))
            {
                choiceHistory.Add(index, new List<int>());
            }
            List<int> choiceList = choiceHistory[index];

            if (!choiceList.Contains(choiceValue))
            {
                choiceList.Add(choiceValue);
            }
        }

    }

    /*

    /// <summary>
    /// 訊息條件判斷
    /// 後面會根據條件傳遞相應的參數進這裡
    /// 前端可能只接受後端回傳的結果,不需要判斷
    /// </summary>
    public class MessageCondition
    {
        public bool canPass;

        public static  implicit operator bool(MessageCondition cond)
        {
            return cond.canPass;
        }
        /// <summary>
        /// 條件類別
        /// </summary>
        public  enum ConditionType
        {
            /// <summary>
            /// 根據某個值
            /// </summary>
            value

        }

    }*/



    /// <summary>
    /// 話題的擴展方法
    /// </summary>
    public static class SnsExpansion
    {
        /// <summary>
        /// 根據enum篩選不同分類的topic(通常enum為=>StoryStartCondition, ContactPersonState,TopicType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topicLists"></param>
        /// <returns></returns>
        public static Dictionary<T, List<TopicData>> GetTopicListState<T>(this List<TopicData> topicLists) where T : Enum
        {
            Dictionary<T, List<TopicData>> curDic = new Dictionary<T, List<TopicData>>();
            foreach (var curEnum in Enum.GetNames(typeof(T)))
            {

                var filterTopic = topicLists.FindAll(topic =>
                {
                    foreach (var field in topic.GetType().GetFields())
                    {
                        if (field.FieldType == typeof(T))
                        {
                            if (field.GetValue(topic).Equals((T)Enum.Parse(typeof(T), curEnum)))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                });

                curDic.Add((T)Enum.Parse(typeof(T), curEnum), filterTopic);
            }

            return curDic;
        }



    }

}//---namespace

/// <summary>
/// 話題類型分類(分類用 daily
/// </summary>
public enum TopicType
{
    None,
    Daily,//日常
    SideStory,//支線
    Special      //特殊

}

/// <summary>
/// Sns觸發的契機條件(System or player
/// </summary>
public enum StoryStartCondition
{
    /// <summary>
    /// 系統觸發(理論上是對方發訊息給玩家
    /// </summary>
    System,
    /// <summary>
    /// 玩家觸發(理論上是玩家可選擇話題主題,並主動發給對方
    /// </summary>
    Player

}


/// <summary>
/// 聯絡人與玩家的狀態 lock
/// </summary>
public enum ContactPersonState
{
    /// <summary>
    /// 未解鎖或未開始,所以無狀態
    /// </summary>
    None,

    /// <summary>
    /// 未讀(出現未讀題示,話題尚未承接的狀況 通常為玩家在對方還未回覆時,就離開
    /// </summary>
    NotLoading,

    /// <summary>
    /// 未回應(未完成話題 通常為玩家看完對方訊息,沒有回應
    /// </summary>
    NotReply,

    /// <summary>
    /// 已經完成對話
    /// </summary>
    Finish
}




