using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.Data;
using YKO.Common.UI;


namespace YKO.EndLess
{


    public class EndLessSupportCell : MonoBehaviour
    {
        public class LocalizeKey
        {

            public static readonly string BeyondCombatRange = "超出戰力範圍";
            public static readonly string Select = "選擇";
            public static readonly string CancelSelect = "取消選擇";
            public static readonly string UnSelect= "無法選擇";
            public static readonly string CurrentSupport = "當前支援中";
        }

        [SerializeField]
        private HeroIcon heroIcon;
        /// <summary>
        /// 角色職業圖像
        /// </summary>
        [SerializeField]
        private Image careerIcon;
        /// <summary>
        /// 角色職業圖像
        /// </summary>
        [SerializeField]
        private Image careerBackground;

        /// <summary>
        /// 角色職業圖像陣列
        /// </summary>
        [SerializeField]
        private Sprite[] careerIconArray;

        /// <summary>
        /// 角色職業圖像陣列
        /// </summary>
        [SerializeField]
        private Sprite[] careerBackgroundArray;

        [SerializeField]
        private Text charaNameText;

        [SerializeField]
        private Text powerText;

        [SerializeField]
        private Text tipText;
        /// <summary>
        /// 選擇(黃色
        /// </summary>
        [SerializeField]
        private Button selectBtn;
        /// <summary>
        /// 選擇按鈕文字
        /// </summary>
        [SerializeField]
        private Text selectBtnText;
        /// <summary>
        /// 取消選擇(紫色
        /// </summary>
        [SerializeField]
        private Button cancelSelectBtn;
        /// <summary>
        /// 選擇按鈕文字
        /// </summary>
        [SerializeField]
        private Text cancelSelectBtnText;
        /// <summary>
        /// 無法選擇(反灰
        /// </summary>
        [SerializeField]
        private Button unSelectBtn;
        /// <summary>
        /// 無法選擇按鈕文字
        /// </summary>
        [SerializeField]
        private Text unSelectBtnText;

        /// <summary>
        /// 支援中文字
        /// </summary>
        [SerializeField]
        private Text exhibitText;

        /// <summary>
        /// 資料
        /// </summary>
        public EndLessSupportCellFunc aData;

        public void Init(EndLessSupportCellFunc  data)
        {

            aData = data;
            UpdateUI();
            Register();

        }
        private void Register()
        {
            selectBtn.onClick.RemoveAllListeners();
            cancelSelectBtn.onClick.RemoveAllListeners();
            unSelectBtn.onClick.RemoveAllListeners();
            
            
            selectBtn.OnClickAsObservable().Subscribe(_=> aData.selectCB());
            cancelSelectBtn.OnClickAsObservable().Subscribe(_ => aData.cancelSelectCB());

        }

        private void UpdateUI()
        {
            var fd = LoadResource.Instance.PartnerData.data_partner_base[aData.bid.ToString()];

            careerIcon.sprite = careerIconArray[(fd.type - 2)];
            careerBackground.sprite= careerBackgroundArray[(fd.type - 2)];

            charaNameText.text =LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData, aData.playerName);
            powerText.text = aData.power.ToString();
            selectBtnText.text = LocalizeKey.Select;
            cancelSelectBtnText.text = LocalizeKey.CancelSelect;
            unSelectBtnText.text = LocalizeKey.UnSelect;
            exhibitText.text = LocalizeKey.CurrentSupport;

            heroIcon.InitHero(aData.bid, aData.star)
                    .SetHeroLevel((ushort)aData.lev);
            
            if (aData.supportExhibit)
            {
                exhibitText.gameObject.SetActive(true);
                tipText.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(false);
                cancelSelectBtn.gameObject.SetActive(false);
                unSelectBtn.gameObject.SetActive(false);
                return;
            }
            else
            {
                exhibitText.gameObject.SetActive(false);
            }

            if (aData.selfMaxPower > 0)
            {

                if (IsUnSelect())
                {

                    tipText.text = LocalizeKey.BeyondCombatRange;
                    tipText.gameObject.SetActive(true);
                    unSelectBtn.gameObject.SetActive(true);
                }
                else
                {
                    tipText.gameObject.SetActive(false);

                    ChangeCellStatus(aData.isSelect);
                }

            }
            else
            {
                tipText.gameObject.SetActive(false);
                ChangeCellStatus(aData.isSelect);
            }




        }
        /// <summary>
        /// 切換cell顯示狀態
        /// true:被選擇
        /// false:未選擇(
        /// </summary>
        /// <param name="_switch"></param>
        public void ChangeCellStatus(bool _switch)
        {
            if (_switch) 
            {
                selectBtn.gameObject.SetActive(false);
                cancelSelectBtn.gameObject.SetActive(true);
                unSelectBtn.gameObject.SetActive(false);
            }
            else
            {
                selectBtn.gameObject.SetActive(true);
                cancelSelectBtn.gameObject.SetActive(false);
                unSelectBtn.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 是否無法選擇
        /// </summary>
        /// <returns></returns>
        public bool IsUnSelect()
        {

            return (Mathf.FloorToInt(aData.selfMaxPower * 1.2f)) < aData.power;

        }

        /// <summary>
        /// 好友支援cell 資料
        /// </summary>
        public class EndLessSupportCellFunc 
        {
            /// <summary>
            /// 玩家專屬id
            /// </summary>
            public uint id;

            /// <summary>
            /// 玩家名稱
            /// </summary>
            public string playerName;
            /// <summary>
            /// cell顯示的戰力
            /// </summary>
            public uint power;
            /// <summary>
            /// 英雄bid
            /// </summary>
            public uint bid;
            /// <summary>
            /// 英雄等級
            /// </summary>
            public uint lev;
            /// <summary>
            /// 英雄星級
            /// </summary>
            public byte star;

            /// <summary>
            /// 自己的最大戰力
            /// </summary>
            public uint selfMaxPower = 0;
            /// <summary>
            /// 是否已經選擇?
            /// </summary>
            public bool isSelect = false;

         /*   /// <summary>
            /// 忽視戰力計算(通常為自己的角色不會有戰力超出的問題
            /// </summary>
            public bool ignoreCombatPowerCalculations = false;*/

            /// <summary>
            /// 以選擇的角色(展示角色 關閉所有按鈕UI
            /// </summary>
            public bool supportExhibit = false;

            public Action selectCB=null;
            public Action cancelSelectCB=null;

        }




    }
}
