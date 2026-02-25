using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.Support.Expansion;

namespace YKO.SevenDay
{



    public class SevenDayRewardCell : MonoBehaviour
    {

        private class LocalizeKey
        {
            public static readonly string Progress = "進度";
            public static readonly string Remain= "剩餘";
            public static readonly string CheckOther = "查看更多";
            public static readonly string Receive= "領取";
            public static readonly string NotReceive = "未達成";
        }
        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text title;

        /// <summary>
        /// 顯示進度物件
        /// </summary>
        [SerializeField]
        private GameObject progressObj;
        /// <summary>
        /// 進度描述文字
        /// </summary>
        [SerializeField]
        private Text progressDesText;
        /// <summary>
        /// 進度文字
        /// </summary>
        [SerializeField]
        private  Text progressText;
        /// <summary>
        /// 查看更多按鈕
        /// </summary>
        [SerializeField]
        private Button checkOtherBtn;
        /// <summary>
        /// 按鈕群組
        /// </summary>
        [SerializeField]
        private Transform buttonGroup;

        /// <summary>
        /// 接收領取
        /// </summary>
        [SerializeField]
        private Button receiveBtn;

        /// <summary>
        /// 接收領取文字
        /// </summary>
        [SerializeField]
        private Text receiveBtnText;

        /// <summary>
        /// 前往按鈕
        /// </summary>
        [SerializeField]
        private Button nextBtn;

        /// <summary>
        /// 前往按鈕文字
        /// </summary>
        [SerializeField]
        private Text nextBtnText;

        /// <summary>
        /// 購買按鈕
        /// </summary>
        [SerializeField]
        private Button purchasingBtn;

        /// <summary>
        /// 折扣文字
        /// </summary>
        [SerializeField]
        private Text discountPriceText;
        /// <summary>
        /// 原價文字
        /// </summary>
        [SerializeField]
        private Text originePriceText;

        [SerializeField]
        private Text notReceiveText;

        /// <summary>
        /// 已完成任務的遮罩
        /// </summary>
        [SerializeField]
        private GameObject completeMask;

        #region prefab or parent
        /// <summary>
        /// itemIcon的母物件
        /// </summary>
        [SerializeField]
        private Transform itemContent;
        /// <summary>
        /// 道具iconPrefab
        /// </summary>
        [SerializeField]
        private GameObject itemIconPrefab = null;


        #endregion

        #region  callback 
        /// <summary>
        /// 該cell是否已經完成
        /// </summary>
        private bool isFinish = false;

        #endregion
        /// <summary>
        /// 基本資料
        /// </summary>
        public SevenDayRewardData aData;

        

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(SevenDayRewardData _data)
        {
            aData = _data;
            SetButtonSetting();
            UpdateUI();

        }
        /// <summary>
        /// 設定按鈕設定
        /// </summary>
        private void SetButtonSetting()
        {
            receiveBtn.onClick.RemoveAllListeners();
            purchasingBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.RemoveAllListeners();
            checkOtherBtn.onClick.RemoveAllListeners();

            receiveBtn.OnClickAsObservable().Where(_ => aData.ReceiveCB != null).Subscribe(_ => aData.ReceiveCB());

            purchasingBtn.OnClickAsObservable().Where(_ => aData.ReceiveCB != null).Subscribe(_ => aData.ReceiveCB());

            nextBtn.OnClickAsObservable().Where(_ => aData.NextCB != null).Subscribe(_ => aData.NextCB());

            checkOtherBtn.OnClickAsObservable().Where(_ => aData.CheckOtherCB != null).Subscribe(_ => aData.CheckOtherCB());

        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            title.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData._title);
            Status = aData.status;
            notReceiveText.text = LocalizeKey.NotReceive;
            itemContent.ClearChildObj();
            switch (aData.statusDisplay)
            {
                case SevenDayRewardData.CellDisplayStatus.BorderLength900:
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 265);
                    break;
                case SevenDayRewardData.CellDisplayStatus.BorderLength1000:
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 265);
                    break;
                case SevenDayRewardData.CellDisplayStatus.BorderLength900Btn275:
                    transform.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 265);
                    buttonGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(275, -25);
                    break;

            }

            switch (aData.CanTip)
            {
                case SevenDayRewardData.RightTopTip.None:
                    progressObj.SetActive(false);
                    checkOtherBtn.gameObject.SetActive(false);
                    break;
                case SevenDayRewardData.RightTopTip.Progress:
                    progressObj.SetActive(true);
                    checkOtherBtn.gameObject.SetActive(false);
                    if (aData.targetVal > 0)
                    {
                        progressText.text = aData.value + "/" + aData.targetVal;
                    }
                    progressDesText.text = LocalizeKey.Progress;
                    break;
                case SevenDayRewardData.RightTopTip.PurchasingProgress:
                    progressObj.SetActive(true);
                    checkOtherBtn.gameObject.SetActive(false);
                    if (aData.targetVal > 0)
                    {
                        progressText.text = aData.value + "/" + aData.targetVal;
                    }
                    progressDesText.text = LocalizeKey.Remain;
                    break;
                case SevenDayRewardData.RightTopTip.CheckOther:
                    progressObj.SetActive(false);
                    checkOtherBtn.gameObject.SetActive(true);
                    checkOtherBtn.transform.Find("Text").GetComponent<Text>().text = LocalizeKey.CheckOther;
                    break;
            }
            foreach (var itemData in aData.itemRewardList) 
            {
                uint bid = (uint)itemData[0];
                uint count = (uint)itemData[1];
                var rar = LoadResource.Instance.GetItemName<long>(bid, "quality");

                var sp = Instantiate(itemIconPrefab);
                sp.transform.SetParent(itemContent, false);
                sp.gameObject.SetActive(true);
                sp.GetComponent<ItemIcon>().Init(bid, amount:count,rarity:(uint)rar);

            }

            //按鈕需要後台資料給與判斷後做出效果

        }
        /// <summary>
        /// 按鈕狀態機
        /// </summary>
        public byte Status {
            get { return aData.status; }
            set {
            aData.status = value;
                switch (value)
                {
                    case 0:
                        if (aData.isTask)
                        {
                            receiveBtn.gameObject.SetActive(false);
                            if (aData.CanTip== SevenDayRewardData.RightTopTip.None) 
                            {
                                nextBtn.gameObject.SetActive(false);
                                notReceiveText.gameObject.SetActive(true);
                            }
                            else
                            {
                                nextBtn.gameObject.SetActive(true);
                                notReceiveText.gameObject.SetActive(false);
                            }

                            purchasingBtn.gameObject.SetActive(false);

                        }
                        else
                        {
                            receiveBtn.gameObject.SetActive(false);
                            nextBtn.gameObject.SetActive(false);
                            notReceiveText.gameObject.SetActive(false);
                            purchasingBtn.gameObject.SetActive(true);
                            discountPriceText.text = aData._discountPrice.ToString();
                            originePriceText.text=""+aData._price;
                        }
                        break;
                    case 1:
                        if (aData.isTask)
                        {
                            receiveBtn.gameObject.SetActive(true);
                            nextBtn.gameObject.SetActive(false);
                            purchasingBtn.gameObject.SetActive(false);
                        }
                        else
                        {
                            progressText.text = 1 + "/" +1;
                            CompleteMask();
                        }
                        break;
                    case 2:
                        CompleteMask();
                        break;
                }
            } 
        }

        /// <summary>
        /// 完成任務遮罩
        /// </summary>
        public void CompleteMask()
        {
            completeMask.gameObject.SetActive(true);
            receiveBtn.gameObject.SetActive(false);
            nextBtn.gameObject.SetActive(false);
            purchasingBtn.gameObject.SetActive(false);
            notReceiveText.gameObject.SetActive(false);
            isFinish = true;
        }

        /// <summary>
        /// 將cell排到最下方
        /// </summary>
        public void SetSortToLastBottom()
        {
            if (isFinish) 
            {
                transform.SetSiblingIndex((transform.parent.childCount - 1));
            }
        }


        public class SevenDayRewardData
        {
            /// <summary>
            /// 任務id 設置後端回傳的任務進度用
            /// </summary>
            public int goal_id=0;
            /// <summary>
            /// 任務id 禮包跟上方獎勵
            /// </summary>
            public int id=0;
            /// <summary>
            /// 按鈕狀態
            /// 任務的状态(0:未达到领取条件    1:可领取     2:已领取)
            /// 購買的状态(0:未购买    1:已购买)
            /// </summary>
            public byte status=0;

            /// <summary>
            /// 按鈕顯示狀態
            /// </summary>
            public CellDisplayStatus statusDisplay= CellDisplayStatus.BorderLength1000;

            /// <summary>
            /// 是否為任務性質 否則為購買性質
            /// </summary>
            public bool isTask=true;

            /// <summary>
            /// 標題文字
            /// </summary>
            public string _title;
            /// <summary>
            /// 最大目標值
            /// </summary>
            public uint targetVal=0; 
            /// <summary>
            /// 當前值
            /// </summary>
            public uint value = 0; 

            /// <summary>
            /// 原價
            /// </summary>
            public int _price=-1;

            /// <summary>
            /// 折扣價格
            /// </summary>
            public int _discountPrice=0;

            /// <summary>
            /// 道具陣列表
            /// </summary>
            public int[][] itemRewardList;

            /// <summary>
            /// 道具條件列表
            /// </summary>
            public List<(string, int)> itemConditionList;

            /// <summary>
            /// 是否有右上提示(進度條,查看更多按鈕
            /// </summary>
            public RightTopTip CanTip=RightTopTip.None;

            #region
            /// <summary>
            /// 接收獎勵
            /// </summary>
            public Action ReceiveCB = null;
            /// <summary>
            /// 前往
            /// </summary>
            public Action NextCB = null;
            /// <summary>
            /// 查看更多
            /// </summary>
            public Action CheckOtherCB = null;


            #endregion


            /// <summary>
            /// 設定右上出現的按鈕
            /// </summary>
            public enum RightTopTip
            {

                None,
                CheckOther,
                Progress,
                /// <summary>
                /// 顯示文字與progress略有不同
                /// </summary>
                PurchasingProgress 

            }

            /// <summary>
            /// 七日獎勵陣列卡顯示模式
            /// </summary>
            public enum CellDisplayStatus
            {
                BorderLength900,
                BorderLength1000,
                BorderLength900Btn275

            }

            public  SevenDayRewardData()
            {



            }



        }




    }
}
