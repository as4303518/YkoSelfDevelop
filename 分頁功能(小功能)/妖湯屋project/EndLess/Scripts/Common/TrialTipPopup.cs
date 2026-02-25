using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;
using YKO.Common.Util;


namespace YKO.EndLess
{
    public class TrialTipPopup : PopupBase
    {
        // Start is called before the first frame update

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Image iconImg;

        [SerializeField]
        private Text descriptText;

        [SerializeField]
        private Text timeText;

        [SerializeField]
        private Button confirmBtn;

        [SerializeField]
        private Button closeBtn;


        [SerializeField]
        private TrialTipPopupData aData;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_data"></param>
        public void Init(TrialTipPopupData _data)
        {
            base.Init();
            aData = _data;
            UpdateUI();
            Register();
            ShowPopup();
        }
        /// <summary>
        /// 註冊按鈕功能
        /// </summary>
        private void Register()
        {
            closeBtn.OnClickAsObservable().Subscribe(_ => { ClosePopup(); });
            confirmBtn.OnClickAsObservable().Subscribe(_ => {
                if (aData.confirmCB!=null)
                {
                    aData.confirmCB();
                }
                else
                {
                    ClosePopup();
                }
            });


        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            titleText.text=aData.title;
            descriptText.text=aData.descript;
            iconImg.sprite = aData.typeSprite;
            iconImg.SetNativeSize();

            StartCoroutine(UnixTime.CountDown(aData.time,
                t => {
            timeText.text=UnixTime.CalcTimeOfDayAtSecond(t,true);
            }));


        }
        /// <summary>
        /// 屬性說明彈窗數據
        /// </summary>
        public class TrialTipPopupData
        {
            /// <summary>
            /// 顯示屬性
            /// </summary>
            public Sprite typeSprite;
            /// <summary>
            /// 標題名稱
            /// </summary>
            public string title;
            /// <summary>
            /// 標題名稱
            /// </summary>
            public string descript;
            /// <summary>
            /// 時間
            /// </summary>
            public uint time;
            /// <summary>
            /// 確定按鈕
            /// </summary>
            public Action confirmCB=null;




        }

    }
}
