using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;


namespace YKO.EndLess
{
    /// <summary>
    /// 領取後
    /// </summary>
    public class EndLessNormalRewardCell : MonoBehaviour
    {
        public class LocalizeKey
        {
            public static readonly string FirstClearOfMissionNum = "首通獎勵(第[{0}]關)";
            public static readonly string FirstClearOfMissionGap = "再{0}關領取";

        }


        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text titleText;
        /// <summary>
        /// 提示文字(再[x]關領取
        /// </summary>
        [SerializeField]
        private Text tipText;

        /// <summary>
        /// 領取獎勵
        /// </summary>
        [SerializeField]
        private Button receiveRewardBtn;


        /// <summary>
        /// cell 母物件
        /// </summary>
        [SerializeField]
        private Transform content;

        /// <summary>
        /// ItemIcon Cell
        /// </summary>
        [SerializeField]
        private GameObject itemIconPrefab;
        /// <summary>
        /// 腳本資料
        /// </summary>
        [SerializeField]
        private EndLessNormalRewardCellFunc aData;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="data"></param>
        public void Init(EndLessNormalRewardCellFunc data)
        {
            aData = data;
            UpdateUI();
            Register();

        }

        private void Register()
        {
            receiveRewardBtn.OnClickAsObservable().Subscribe(_ => aData.receiveRewardCB());
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI ()
        {
            titleText.text = string.Format(LocalizeKey.FirstClearOfMissionNum,aData.missionNum);
            tipText.text= string.Format(LocalizeKey.FirstClearOfMissionGap, aData.missionGap);

            Debug.Log("顯示資料=>" + aData.missionGap);

            if (aData.missionGap>0)
            {
                receiveRewardBtn.gameObject.SetActive(false);
                tipText.gameObject.SetActive(true);
            }
            else
            {
                receiveRewardBtn.gameObject.SetActive(true);
                tipText.gameObject.SetActive(false);
            }

            foreach (var info in aData.itemInfo) 
            {
                 var sp=Instantiate(itemIconPrefab,content);
                sp.GetComponent<ItemIcon>().Init(info[0], info[1], true, () => { });
            }



        }


        /// <summary>
        /// 資料
        /// </summary>
        public class EndLessNormalRewardCellFunc
        {
            /// <summary>
            /// 關卡編號
            /// </summary>
            public uint missionNum;
            /// <summary>
            /// 關卡與領取獎勵差距
            /// </summary>
            public uint missionGap;
            /// <summary>
            /// 道具資訊
            /// </summary>
            public uint[][] itemInfo;

            /// <summary>
            /// 接收獎勵回調
            /// </summary>
            public Action receiveRewardCB;


        }
    }
}