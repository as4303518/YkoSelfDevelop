using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YKO.Common;
using YKO.DataModel;
using YKO.Support;

public static class SnsFixedData 
{
    /// <summary>
    /// sns話題靜態資料(話題資訊,對話詳細資訊
    /// </summary>
    public static SnsData snsFixedData { get; private set; } = null;
    /// <summary>
    /// 玩家回覆靜態資料
    /// </summary>
    public static ChoiceData choiceFixedData { get; private set; } = null;

    /// <summary>
    /// 角色資訊靜態資料
    /// </summary>
    public  static CharacterData characterFixedData { get; private set; } = null;

    /// <summary>
    /// 加載靜態資料
    /// </summary>
    /// <returns></returns>
    public  static void LoadStaticData()
    {
        choiceFixedData = LoadResource.Instance.ChoiceData;
        snsFixedData = LoadResource.Instance.SnsData;
        characterFixedData = LoadResource.Instance.CharacterData;
       /* if (choiceFixedData == null)
        {
            yield return LoadAssetManager.LoadAsset<TextAsset>(
                String.Format("Assets/Application/Common/Json/{0}.json", "choice_data"),
                res =>
                {
                    
                    choiceFixedData = JsonConvert.DeserializeObject<ChoiceData>(res.text);
                });
        }

        if (snsFixedData == null)
        {
            yield return LoadAssetManager.LoadAsset<TextAsset>(
                  String.Format("Assets/Application/Common/Json/{0}.json", "sns_data"),
            res =>
            {
                snsFixedData = JsonConvert.DeserializeObject<SnsData>(res.text);
            });
        }


        if (characterFixedData == null)
        {
            yield return LoadAssetManager.LoadAsset<TextAsset>(
                  String.Format("Assets/Application/Common/Json/{0}.json", "character_data"),
            res =>
            {
                characterFixedData = JsonConvert.DeserializeObject<CharacterData>(res.text);
            });
        }*/
    }

    /// <summary>
    /// 透過話題編號獲得話題對話資訊
    /// </summary>
    /// <param name="topicID"></param>
    /// <returns></returns>
    public static Dictionary<string, SnsData.Message_Info> GetTopicData(string topicID)
    {

        if (snsFixedData.data_message_info.ContainsKey(topicID))
        {
            return snsFixedData.data_message_info[topicID];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 透過角色名稱獲得話題資訊
    /// </summary>
    /// <param name="charaID"></param>
    /// <returns></returns>
    public static List<SnsData.Topic_Info> GetTopicInfo(string charaID)
    {
        
        if (snsFixedData.data_topic_info.ContainsKey(charaID))
        {
            return snsFixedData.data_topic_info[charaID];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 多選項回覆靜態資料
    /// </summary>
    /// <param name="groupID"></param>
    /// <returns></returns>
    public static List<ChoiceData.Sns_Choice> GetUserReply(string groupID)
    {
        if (choiceFixedData.data_sns_choice.ContainsKey(groupID))
        {
            return choiceFixedData.data_sns_choice[groupID];
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 獲得角色資料
    /// </summary>
    /// <param name="charaID"></param>
    /// <returns></returns>
    public static CharacterData.Character_Info GetCharacterInfo(string charaID)
    {

        if (characterFixedData.data_character_info.ContainsKey(charaID)) 
        {
            return characterFixedData.data_character_info[charaID];

        }
        else
        {
            return null;
        }

    }

}
