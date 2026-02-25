using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using YKO.Network;

namespace YKO.StrengthGuide
{
    public  class StrengthenTutorialNetwork 
    {

        #region 
        /// <summary>
        /// 強化指引的角色回饋(需求為複數proto
        /// </summary>
        private List<Proto_11070_Response> proto11070List=new List<Proto_11070_Response>();

        public List<Proto_11070_Response> Proto11070List { get { return proto11070List; } }

        #endregion

        public StrengthenTutorialNetwork()
        {
            RefisterProto();
        }


        public void SendPorto11070(uint partner_bid) {
            Debug.Log("寄出proto11070=>"+partner_bid);
            var proto = new Proto_11070_Request();
            proto.partner_bid = partner_bid;
            NetworkManager.Instance.Send(proto);
        }
        /// <summary>
        /// 註冊Proto運行觸發
        /// </summary>
        public  void RefisterProto()
        {
            /* MessageResponseData.Instance.OnMessageResponse.Subscribe(
                 meg => {
             }).AddTo(this);*/
            MessageResponseData.Instance.OnMessageResponse.Subscribe(meg => {
                switch (meg.MessageID) {

                    case 11070:
                        proto11070List.Add((Proto_11070_Response)meg);
                        break;
                }
            });
        }

        /// <summary>
        /// 處理proto11070
        /// </summary>
        /// <param name="proto"></param>
        /// <returns></returns>
        public  IEnumerator HandleProto11070(uint partner_bid)
        {
            var protoCount=proto11070List.Count;
            SendPorto11070(partner_bid);
            yield return new WaitUntil(() => proto11070List.Count != protoCount);

        }




    }
}