using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;
using YKO.Support.Expansion;

namespace YKO.SevenDay
{
    public class SevenRewardPopup : PopupBase
    {
        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text title;
        /// <summary>
        /// 獎勵列表
        /// </summary>
        [SerializeField]
        private Transform cellContent;
        /// <summary>
        /// 關閉按鈕
        /// </summary>
        [SerializeField]
        private Button closeBtn;

        private bool firstOpen=true;

        #region

        [SerializeField]
        private GameObject sevenCellprefab;
        /// <summary>
        /// 任務卡陣列
        /// </summary>
        private List<GameObject> cellList=new List<GameObject>();

        /// <summary>
        /// 關閉彈窗後的回傳(刪除controller的彈窗存取
        /// </summary>
        private Action closePopupCB=null;
        #endregion

        public void Init(string _title,List<SevenDayRewardCell.SevenDayRewardData> _dataList,Action<uint> receiveCB,Action closeCB=null)
        {
            if (firstOpen) {
                firstOpen= false;
                base.Init();
            }
            title.text = _title;
            closePopupCB = closeCB;
            cellContent.ClearChildObj();
            foreach (var data in _dataList)
            {
                var sp = Instantiate(sevenCellprefab);
                sp.transform.SetParent(cellContent,false);
                var cell = sp.GetComponent<SevenDayRewardCell>();
                cell.Init(data);
                data.ReceiveCB =()=> {
                    cell.CompleteMask();
                    receiveCB((uint)(data.id!=0?data.id:data.goal_id));
                };
                cellList.Add(sp);

            }
            Register();
        }

        private void Register()
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.OnClickAsObservable().Subscribe(_ => ClosePopup());


        }

        public override void ClosePopup(Action onClosePopupComplete = null)
        {
            closePopupCB?.Invoke();
            base.ClosePopup(onClosePopupComplete);
        }


    }
}