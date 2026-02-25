using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using YKO.Common.UI;
using YKO.Common;
using YKO.Network;
using Unity.VisualScripting;
using YKO.Common.Sound;

namespace YKO.Casino
{
    public class CasinoLogsPopup : PopupBase
    {
        private class LocalizeKey
        {
            public const string Title = "下注紀錄";
            public const string SubTitle1 = "十方賭坊";
            public const string SubTitle2 = "高級賭坊";
            public const string LogString = "<color=#D92E39>{0}</color><color=black>獲得</color><color=#FF6B00>{1}x{2}</color>";
        }

        [SerializeField]
        private Text titleText;
        [SerializeField]
        private Text subTitleText;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Transform cellParent;
        [SerializeField]
        private CasinoLogsCell txtCell;
        /// <summary>
        /// 下一頁按鈕
        /// </summary>
        [SerializeField]
        private Button nextPageBtn;
        /// <summary>
        /// 當前頁數(一次顯示15條
        /// </summary>
        private int curPage=0;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="log_list"></param>
		public void Init(uint type, Proto_16637_Response.Dial_data.Log_list[] log_list)
		{
            base.Init();

            subTitleText.text = type == 2 ? LocalizeKey.SubTitle2 : LocalizeKey.SubTitle1;

            if (log_list.Length<15)
            {
                nextPageBtn.gameObject.SetActive(false);
            }
            else
            {
                nextPageBtn.gameObject.SetActive(true);
            }

            for (int i=0;i<log_list.Length;i++)
            {
                var log = log_list[i];
                var cell = Instantiate(txtCell, cellParent);
                if (i>14) 
                {
                    cell.gameObject.SetActive(false);
                }
                else
                {
                    cell.gameObject.SetActive(true);
                }

                cell.Init(
                  new CasinoLogsCell.CasinoLogsCellData()
                {
                    playerName = log.role_name,
                    itemName = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, LoadResource.Instance.GetItemName<string>(log.bid, "name")) + "x" + log.num
                }
                    );

            }

            SetButtonSetting();

        }
        /// <summary>
        /// 設置按鈕設定
        /// </summary>
        private void SetButtonSetting()
        {
            closeButton.OnClickAsObservable().Subscribe(_ => ClosePopup()).AddTo(this);
            nextPageBtn.OnClickAsObservable().Subscribe(_ => NextButton()).AddTo(this);

        }
        /// <summary>
        /// 下一頁按鈕
        /// </summary>
        private void NextButton()
        {
            bool isInit = false;
            for (int i = 0; i < 15; i++)
            {
                if (i + (curPage * 15) < cellParent.childCount)
                {
                    var cell = cellParent.GetChild(i + (curPage * 15));
                    cell.gameObject.SetActive(false);
                }

                if ((i + (curPage + 1) * 15) < cellParent.childCount)
                {
                    var openCell = cellParent.GetChild(i + ((curPage + 1) * 15));
                    openCell.gameObject.SetActive(true);
                }
                else if (i < 1)//如果已經沒有下一個需要活動的Obj 而且第一個就不需要,代表需要重新翻頁
                {
                    isInit = true;
                }
               
            }
            if (isInit)
            {
                curPage = 0;
                for (int i = 0; i < 15; i++)
                {
                    if (i + (curPage * 15) < cellParent.childCount)
                    {
                        var cell = cellParent.GetChild(i + (curPage * 15));
                        cell.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                curPage++;
            }
        }




	}
}