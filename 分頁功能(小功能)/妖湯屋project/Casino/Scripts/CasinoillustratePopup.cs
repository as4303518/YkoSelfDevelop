using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;

namespace YKO.Casino
{
    /// <summary>
    /// 賭坊說明彈窗
    /// </summary>
    public class CasinoillustratePopup : PopupBase
    {
        private class LocalizeKey
        {
            public const string Title = "玩法說明";
        }
        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text title;
        /// <summary>
        /// 說明
        /// </summary>
        [SerializeField]
        private Text descript;


        private CasinoillustratePopupData aData;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(CasinoillustratePopupData data)
        {
            base.Init();
            aData = data;
            UpdateUI();

        }

        private void UpdateUI()
        {
            if (string.IsNullOrWhiteSpace( aData.title)) 
            {

                title.text = LocalizeKey.Title;
            }
            else
            {
                title.text = aData.title;
            }
            descript.text = aData.descript;

        }

        /// <summary>
        /// 賭坊說明彈窗數據
        /// </summary>
        public class CasinoillustratePopupData
        {
            /// <summary>
            /// 標題文字
            /// </summary>
            public string title = null;
            /// <summary>
            /// 說明
            /// </summary>
            public string descript;


            public CasinoillustratePopupData(string _descript)
            {
                descript = _descript;
            }
        }


    }
}