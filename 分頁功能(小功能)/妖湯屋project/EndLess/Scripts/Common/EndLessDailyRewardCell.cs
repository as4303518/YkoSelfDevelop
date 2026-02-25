using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;


namespace YKO.EndLess
{
    public class EndLessDailyRewardCell : MonoBehaviour
    {
        public class LocalizeKey
        {
            /// <summary>
            /// 通關第[{0}]關
            /// </summary>
            public static readonly string MissionCompleteOfNum = "通關第[{0}]關";
            /// <summary>
            /// (當前關卡)
            /// </summary>
            public static readonly string CurrentMission = "(當前關卡)";
        }
        /// <summary>
        /// 描述文字
        /// </summary>
        [SerializeField]
        private Text descriptText;
        /// <summary>
        /// 提示文字
        /// </summary>
        [SerializeField]
        private Text tipText;
        /// <summary>
        /// 道具icon 母物件
        /// </summary>
        [SerializeField]
        private Transform itemContent;
        /// <summary>
        /// 道具icon
        /// </summary>
        [SerializeField]
        private GameObject itemIconPrefab;

        [SerializeField]
        private EndLessDailyRewardCellFunc aData;

        public void Init(EndLessDailyRewardCellFunc data) 
        {
            aData = data;
            UpdateUI();
        
        
        
        }

        private void UpdateUI()
        {
            descriptText.text = string.Format(LocalizeKey.MissionCompleteOfNum, aData.missionNum);
            tipText.text = LocalizeKey.CurrentMission;
            if (aData.firstCell)
            {
                tipText.gameObject.SetActive(true);
            }
            else
            {
                tipText.gameObject.SetActive(false);
            }

            foreach (var info in aData.itemInfo) 
            {
                var sp = Instantiate(itemIconPrefab, itemContent);
                sp.GetComponent<ItemIcon>().Init(info[0], info[1], true, () => { });
            }


        }

        /// <summary>
        /// 資料
        /// </summary>
        public class EndLessDailyRewardCellFunc
        {
            /// <summary>
            /// 道具資訊
            /// </summary>
            public uint[][] itemInfo;
            /// <summary>
            /// 關卡編號
            /// </summary>
            public uint missionNum;
            /// <summary>
            /// 是否為當前關卡
            /// </summary>
            public bool firstCell=false;

        }



    }
}
