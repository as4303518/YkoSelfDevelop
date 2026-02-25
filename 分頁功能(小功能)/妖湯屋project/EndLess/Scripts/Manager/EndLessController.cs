using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using YKO.Common;
using YKO.Common.UI;
using YKO.Main;
using YKO.Network;



namespace YKO.EndLess
{

    public class EndLessController : MonoBehaviour
    {
     /// <summary>
     /// 顯示view
     /// </summary>
        [SerializeField]
        private EndLessView view;

        public Proto_20060_Response proto20060=null;

        public Proto_23900_Response proto23900=null;

        public Proto_23905_Response proto23905 = null;

        public Proto_23907_Response proto23907 = null;

        public Proto_11211_Response proto11211 = null;

        public Proto_26555_Response proto26555 = null;
        /// <summary>
        /// 根據不同屬性關卡,回傳不同的proto
        /// </summary>
        private Dictionary< byte,Proto_23903_Response> proto23903List=new Dictionary<byte, Proto_23903_Response>();

        


        public void Start()
        {
            StartCoroutine(Init());
        }

        public IEnumerator Init()
        {
            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();
            Register();
            yield return  WaitFirstEnter();
            /*if (proto20060.type <= 0)
            {
                yield return view.Init(this);
                MainManager.Instance.HideLoading();
            }*/
        }

        private void Register()
        {

            MessageResponseData.Instance.SubscribeLive<Proto_11211_Response>(11211,
                meg => {
                    proto11211 = meg;
                }, false, this);

            MessageResponseData.Instance.SubscribeLive<Proto_20060_Response>(20060,
                meg => {
                    proto20060 = meg;
                }, false, this);


            BattleManager.Instance.SubscribeLive<Proto_20013_Response>(20013,
                meg => {
                    Handle20013();
                }, false, this);


            MessageResponseData.Instance.SubscribeLive<Proto_23900_Response>(23900,
                meg => {
                    proto23900 = meg;
                    if (proto20060.type<=0)
                    {
                        StartCoroutine(view.Init(this));
                        MainManager.Instance.HideLoading();
                    }
            }, false, this);


            MessageResponseData.Instance.SubscribeLive<Proto_23903_Response>(23903,
                meg => {

                if (!proto23903List.ContainsKey(meg.type)) 
                    {
                    proto23903List.Add(meg.type, meg);
                    }
                    else
                    {
                        proto23903List[meg.type] = meg;
                    }
                    view.JudgeUpdate(meg.type);
                        
                }, false, this);

            MessageResponseData.Instance.SubscribeLive<Proto_23905_Response>(23905,
                meg => {
                    //回傳顯示的資料是我派出去的支援
                    proto23905 = meg;

                }, false, this);


            MessageResponseData.Instance.SubscribeLive<Proto_23907_Response>(23907,
                meg => {
                    //回傳顯示的資料是我派出去的支援
                    proto23907 = meg;

                }, false, this);


            MessageResponseData.Instance.SubscribeLive<Proto_26555_Response>(26555,
                meg => {
                    proto26555 = meg;
                }, false, this);



        }
        /// <summary>
        /// 獲取對應的 23903 proto
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cb"> 布林值是偵測是否原本就有資料</param>
        /// <returns></returns>
        public IEnumerator GetProto23903(byte type,Action<Proto_23903_Response,bool>cb)
        {
            Debug.Log("嘗試獲取23903=>"+type);
            if (!proto23903List.ContainsKey(type))
            {
                SendProto23903(type);
                yield return new WaitUntil(() => proto23903List.ContainsKey(type));
                cb(proto23903List[type],false);
                yield break;
            }
            else
            {
                cb(proto23903List[type], true);
            }

        }
        /// <summary>
        /// 陣法訊息
        /// </summary>
        /// <param name="type"></param>
        public void SendProto11211(byte type)
        {
            Proto_11211_Request proto = new Proto_11211_Request();
            proto.type = type;
            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// 請求戰鬥類型
        /// </summary>
        private void SendProto20060()
        {
            Proto_20060_Request proto = new Proto_20060_Request();
            proto.combat_type = 11;
            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// 獲取無間基本資料
        /// </summary>
        private void SendProto23900()
        {
            Debug.Log("寄出23900=>" );
            Proto_23900_Request proto = new Proto_23900_Request();
            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// 進入戰鬥
        /// </summary>
        public  void SendProto23901(byte type,UInt16 formationType,uint hallows_id,List< FormData.PosInfo> pos)
        {
            Debug.Log("寄出23901=>");
            Proto_23901_Request proto = new Proto_23901_Request();
            List<Proto_23901_Request.Pos_info> posInfo = new List<Proto_23901_Request.Pos_info>();
            foreach (var p in pos) 
            {
                posInfo.Add(new Proto_23901_Request.Pos_info()
                {
                    pos= p.pos,
                    id= p.id,
                    owner_id=p.rid,
                    owner_srv_id= p.srv_id
                });
            }
            proto.type = type;
            proto.formation_type = formationType;
            proto.pos_info = posInfo.ToArray();
            proto.hallows_id = hallows_id;
            NetworkManager.Instance.Send(proto);

        }
        /// <summary>
        /// 該屬性試煉首次通關的獎勵與通關狀況
        /// </summary>
        /// <param name="_type"></param>
        private void SendProto23903(byte missionType=5)
        {
            Debug.Log("寄出23903=>" + missionType);
            Proto_23903_Request proto = new Proto_23903_Request();
            proto.type = missionType;//主試煉開場是5,其餘看試煉屬性
            NetworkManager.Instance.Send(proto);

        }
        /// <summary>
        /// 領取獎勵
        /// </summary>
        /// <param name="_type"></param>
        public void SendProto23904(byte missionType = 5, uint id = 0)
        {
            Debug.Log("寄出23904=>" + missionType + "道具id=>" + id);
            Proto_23904_Request proto = new Proto_23904_Request();
            proto.type = missionType;//主試煉開場是5,其餘看試煉屬性
            proto.id = id;
            NetworkManager.Instance.Send(proto);

        }
        /// <summary>
        /// 我的支援 
        /// </summary>
        /// <param name="_type"></param>
        public void SendProto23905()
        {
            Debug.Log("寄出23905=>" );
            Proto_23905_Request proto = new Proto_23905_Request();
            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// 朋友的支援
        /// </summary>
        /// <param name="_type"></param>
        public void SendProto23907()
        {
            Debug.Log("寄出23907=>");
            Proto_23907_Request proto = new Proto_23907_Request();
            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// 設定我的支援(
        /// </summary>
        /// <param name="_type"></param>
        public void SendProto23908(uint id)
        {
            
            Proto_23908_Request proto = new Proto_23908_Request();
            proto.id = id;
            NetworkManager.Instance.Send(proto);

        }
        /// <summary>
        /// 選擇支援夥伴或取消
        /// </summary>
        /// <param name="proto23909"></param>
        public void SendProto23909(Proto_23909_Request proto23909)
        {
            Debug.Log("寄出23909=>"+JsonConvert.SerializeObject(proto23909));
            NetworkManager.Instance.Send(proto23909);

        }
        /// <summary>
        /// 精靈陣容消息
        /// </summary>
        /// <param name="type"></param>
        public void SendProto26555(byte type)
        {
            Proto_26555_Request proto = new Proto_26555_Request();
            proto.type = type;
            NetworkManager.Instance.Send(proto);

        }

        private void Handle20013()
        {

            if (proto20060 != null)
            {
                if (proto20060.type == 1)
                {
                        view.PlayExitAnim(() =>
                        {
                            GameSceneManager.Instance.AddScene(SceneConst.SceneName.BattleScene,
                                new Battle.BattleScene.SceneParameter()
                                {
                                    id = BattleManager.Instance.GetRespose<Proto_20013_Response>(20013).extra_args[0].param,
                                    isContinue = true
                                });
                        });
                }
            }
        }

        /// <summary>
        /// 首次登入等待的proto
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitFirstEnter()
        {
                 SendProto20060();
                SendProto23903();
                SendProto23907();
                yield return new WaitUntil(() => proto23903List.ContainsKey(5) == true);
                yield return new WaitUntil(() => proto23907 != null);
                yield return new WaitUntil(() => proto20060 != null);
                 SendProto23900();
                yield return new WaitUntil(() => proto23900 != null);
        }

    }

    /*
    /// <summary>
    /// 關卡屬性
    /// </summary>
    public enum MissionAttribute
    {
        None,
        Water,
        Fire,
        Wind,
        LightAndDark,
        Old


    }*/


}
