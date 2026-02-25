using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;
using YKO.Support.Expansion;


namespace YKO.EndLess
{

    public class EndLessMissionInfo : MonoBehaviour
    {
        public class LocalizeKey
        {
            /// <summary>
            /// 獎勵詳情
            /// </summary>
            public static readonly string RewardInfo = "獎勵詳情";
            /// <summary>
            /// 領取獎勵
            /// </summary>
            public static readonly string ReceiveReward = "領取獎勵";
            /// <summary>
            /// 通關第{0}關
            /// </summary>
            public static readonly string Clear_Ａ_Mission = "通關第{0}關";
            /// <summary>
            /// {0}關後可領
            /// </summary>
            public static readonly string AvailableAfterMissionClear = "{0}關後可領";
            /// <summary>
            /// 每日挑戰獲得
            /// </summary>
            public static readonly string ChallengeFormEveryDay = "每日挑戰獲得";

            /// <summary>
            /// 本日已結算
            /// </summary>
            public static readonly string ReceivedRewardOfToday = "本日已結算";
        }




        /// <summary>
        /// 首通獎勵標題文字
        /// </summary>
        [SerializeField]
        private Text RewardTitleText;
        /// <summary>
        /// 首通獎勵說明文字(描述關卡是在通關幾關時領取的
        /// </summary>
        [SerializeField]
        private Text RewardDescriptText;

        /// <summary>
        /// 首通獎勵獎勵詳情按鈕
        /// </summary>
        [SerializeField]
        private Button RewardInfoBtn;
        /// <summary>
        /// 首通獎勵獎勵詳情按鈕文字
        /// </summary>
        [SerializeField]
        private Text RewardInfoBtnText;
        /// <summary>
        /// 首通獎勵獎勵領取按鈕
        /// </summary>
        [SerializeField]
        private Button ReceiveRewardBtn;

        /// <summary>
        /// 首通獎勵獎勵領取按鈕文字
        /// </summary>
        [SerializeField]
        private Text ReceiveRewardBtnText;
        /// <summary>
        /// 首通獎勵目前離領獎狀況(通過3關後可領取
        /// </summary>
        [SerializeField]
        private Text RewardTipText;

        /// <summary>
        /// 首通獎勵道具母物件
        /// </summary>
        [SerializeField]
        private Transform Content;
        /// <summary>
        ///  道具預製物
        /// </summary>
        [SerializeField]
        private GameObject itemIconPrefab;
        /// <summary>
        /// 腳本資料
        /// </summary>
        private EndLessMissionInfoFunc aData;


        public void Init(EndLessMissionInfoFunc data)
        {
            aData = data;
            Content.ClearChildObj();
            UpdateUI();
            Register();


        }

        private void UpdateUI()
        {
            RewardTitleText.text = aData.title;
            RewardDescriptText.text = string.Format(LocalizeKey.Clear_Ａ_Mission, aData.tarCompleteMission);

            RewardInfoBtnText.text = LocalizeKey.RewardInfo;

            ReceiveRewardBtnText.text = LocalizeKey.ReceiveReward;

            if (aData.tarMissionGap>0) {
                RewardTipText.text = string.Format(LocalizeKey.AvailableAfterMissionClear, aData.tarMissionGap);
            }

            switch (aData.status) {
                case 0://首次通關
                    ReceiveRewardBtn.gameObject.SetActive(false);
                    RewardTipText.gameObject.SetActive(true);
                    break;
                case 1://首次通關
                    ReceiveRewardBtn.gameObject.SetActive(true);
                    RewardTipText.gameObject.SetActive(false);
                    break;
                case 2:// 日常  尚未挑戰
                    ReceiveRewardBtn.gameObject.SetActive(false);
                    RewardTipText.gameObject.SetActive(true);
                    RewardTipText.text = LocalizeKey.ChallengeFormEveryDay;
                    break;
                case 3://日常 本日已結算
                    ReceiveRewardBtn.gameObject.SetActive(false);
                    RewardTipText.gameObject.SetActive(true);
                    RewardTipText.text = LocalizeKey.ReceivedRewardOfToday;
                    break;
            }
            Debug.Log("道具數量=>"+aData.itemInfos.Length);

            if (aData.itemInfos!=null&&aData.itemInfos.Length>0) 
            {

                foreach (var info in aData.itemInfos)
                {
                    var sp = Instantiate(itemIconPrefab, Content);
                    sp.GetComponent<ItemIcon>().Init(info[0], info[1], true, () => { });
                }
            }


        }

        private void Register()
        {
            RewardInfoBtn.onClick.RemoveAllListeners();
            ReceiveRewardBtn.onClick.RemoveAllListeners();
            RewardInfoBtn.OnClickAsObservable().Subscribe(_ => aData.rewardInfoCB());
            ReceiveRewardBtn.OnClickAsObservable().Subscribe(_ => aData.receiveRewardCB());

        }



        public class EndLessMissionInfoFunc
        {
            /// <summary>
            /// 道具資訊
            /// </summary>
            public uint[][] itemInfos=null;
            /// <summary>
            /// 按鈕顯示狀態
            /// </summary>
            public uint status = 0;

            /// <summary>
            /// 標題
            /// </summary>
            public string title=null;
            /// <summary>
            /// 目標通關的關卡
            /// </summary>
            public int  tarCompleteMission=0;

            /// <summary>
            /// 關卡差距
            /// </summary>
            public int tarMissionGap = 0;

            /// <summary>
            /// 獎勵詳情方法
            /// </summary>
            public Action rewardInfoCB;

            /// <summary>
            /// 獎勵領取方法
            /// </summary>
            public Action receiveRewardCB;

        }



    }
}
