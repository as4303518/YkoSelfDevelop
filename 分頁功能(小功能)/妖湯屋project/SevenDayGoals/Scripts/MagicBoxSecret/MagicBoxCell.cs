using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;

namespace YKO.SevenDay
{

    public class MagicBoxCell : MonoBehaviour
    {
        

        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text titleText;
        /// <summary>
        /// 前往場景按鈕(未領取
        /// </summary>
        [SerializeField]
        private Button nextBtn;
        /// <summary>
        /// 領取按鈕
        /// </summary>
        [SerializeField]
        private Button receiveBtn;
        /// <summary>
        /// 以領取文字
        /// </summary>
        [SerializeField]
        private Text ReceivedText;

        /// <summary>
        /// 顯示道具prefab
        /// </summary>
        [SerializeField]
        private ItemIcon itemIcon;


        private MagicBoxData aData=null;

        public void Init(MagicBoxData _data) 
        {
            aData = _data;
            Register();
            UpdateUI();
        }

        private void Register()
        {
            nextBtn.onClick.RemoveAllListeners();
            receiveBtn.onClick.RemoveAllListeners();
            nextBtn.OnClickAsObservable().Subscribe(_ => aData.NextSceneCB());
            receiveBtn.OnClickAsObservable().Subscribe(_ => aData.ReceiveRewardCB());

        }

        private void UpdateUI()
        {
            titleText.text = aData._title;
            itemIcon.Init(aData.itemInfo.Item1, aData.itemInfo.Item2,null);
            switch (aData.status)
            {

                case 0://前往場景
                    nextBtn.gameObject.SetActive(true);
                    receiveBtn.gameObject.SetActive(false);
                    ReceivedText.gameObject.SetActive(false);
                    break;
                case 1://接收道具
                    nextBtn.gameObject.SetActive(false);
                    receiveBtn.gameObject.SetActive(true);
                    ReceivedText.gameObject.SetActive(false);
                    break;
                case 2://已接收
                    nextBtn.gameObject.SetActive(false);
                    receiveBtn.gameObject.SetActive(false);
                    ReceivedText.gameObject.SetActive(true);
                    break;
            }
        }


        public class MagicBoxData
        {
            /// <summary>
            /// 標題文字
            /// </summary>
            public string _title;
            /// <summary>
            /// item資訊(id ,數量)
            /// </summary>
            public (uint, uint) itemInfo;
            /// <summary>
            /// 道具卡狀態(0:前往 1:領取 2:已領取
            /// </summary>
            public uint status;
            /// <summary>
            /// 前往場景回調
            /// </summary>
            public Action NextSceneCB;

            /// <summary>
            /// 接受獎勵
            /// </summary>
            public Action ReceiveRewardCB;
        
        }

    }

}
