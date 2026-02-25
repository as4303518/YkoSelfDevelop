using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.Main;
using YKO.Network;
using YKO.Storage;
using YKO.Support.Expansion;
using static YKO.CONST.MallConst;

namespace YKO.MallShop {

    public class ShopMallProductData
    {
        #region 固定資料
        /// <summary>
        /// 商品道具Bid
        /// </summary>
        public long ProductBid { get; private set; }
        /// <summary>
        /// 商品單次數量
        /// </summary>
        public long ProductAmount { get; private set; }
        /// <summary>
        /// 商品原始價格
        /// </summary>
        public long OriginePrice { get; private set; }
        /// <summary>
        /// 商品真實價格
        /// </summary>
        public long RealPrice { get; private set; }
        /// <summary>
        /// 商品折扣(0 = 沒打折, 3 = 30%OFF)
        /// </summary>
        public byte OffLabel { get; private set; }
        /// <summary>
        /// 消耗Type
        /// </summary>
        public string PayType { get; private set; }
        public uint PayId { 
            get {
                if (uint.TryParse(PayType, out var val)) 
                {
                    return val;
                }
                return Convert.ToUInt32(LoadResource.Instance.ItemData.data_assets_label2id[PayType]); 
            } 
        } 
        /// <summary>
        /// 限購類型(0 = 沒限購 or 每次限購, 1 = 每日, 2 = 每週, 3 = 每月)
        /// </summary>
        public byte LimitType { get; private set; }
        /// <summary>
        /// 限購數量(0 = 沒限購)
        /// </summary>
        public long LimitNum { get; private set; }
        /// <summary>
        /// VIP限制(0 = 沒限制)
        /// </summary>
        public byte LimitVip { get; private set; }

        /// <summary>
        /// 主属性(來源是Proto_13403_Response專用)
        /// </summary>
        public Proto_13403_Response.Item_list.Main_attr[] Main_attr { get; private set; }
        /// <summary>
        /// 評分(來源是Proto_13403_Response專用)
        /// </summary>
        public uint Score { get; private set; }
        #endregion

        #region 變動資料
        /// <summary>
        /// 是否售完
        /// </summary>
        public bool SoldOut { get 
            {
                if (LimitNum > 0 && HasBuy >= LimitNum) return true;
                else return false;
            } 
        }
        /// <summary>
        /// 已購買次數
        /// </summary>
        public uint HasBuy { get; set; }
        #endregion

        #region Request用
        /// <summary>
        /// 傳送Request的id(13402的eid, 13407的order, 23213的group_id)
        /// </summary>
        public uint Request_Id { get; private set; }
        /// <summary>
        /// 13401->13402, 13403->13407, 23201(積分兌換), 23213(赤月)
        /// </summary>
        public uint Request_Type { get; private set; } = 13402;
        /// <summary>
        /// 商城类型
        /// </summary>
        public uint Request_Shop_Type { get; private set; }
        #endregion

        public int camp_type = -1;

        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Gold data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit,
                data.limit_vip);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Herosoul data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Seer data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Hero data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.price,
                data.pay_type,
                (uint)data.id,
                limit_type: limit_type,
                limit_num: limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Expediton data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Arena data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Guild data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(DataModel.ExchangeData.Shop_Exchage_Elite data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = 0;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_bid,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type,
                (uint)data.id,
                data.label,
                limit_type,
                limit);
        }
        public ShopMallProductData(Activity.UI.ExchangeListPopup.PopupData.ExchangeItem data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = data.Limit;
            byte limit_type = 0;
            UpdateData(data.Item.ItemID,
                data.Item.ItemNum,
                data.ConsumeItem.ItemNum,
                data.ConsumeItem.ItemNum,
                data.ConsumeItem.ItemID.ToString(),
                (uint)data.ID,
                0,
                limit_type,
                limit);
        }
        public ShopMallProductData(Proto_13403_Response.Item_list data, MallType mallType)
        {
            Request_Shop_Type = (uint)mallType;

            long limit = data.limit_count;
            byte limit_type = 0;
            if (data.limit_day != 0)
            {
                limit = data.limit_day;
                limit_type = 1;
            }
            else if (data.limit_week != 0)
            {
                limit = data.limit_week;
                limit_type = 2;
            }
            else if (data.limit_month != 0)
            {
                limit = data.limit_month;
                limit_type = 3;
            }
            UpdateData(data.item_id,
                data.item_num,
                data.price,
                data.discount == 0 ? data.price : data.discount,
                data.pay_type.ToString(),
                data.order,
                data.discount_type,
                limit_type,
                limit);
            Request_Type = 13407;
            Main_attr = data.main_attr;
            Score = data.score;
        }

        public ShopMallProductData(long product_bid,
            long product_amount,
            long origine_price,
            long real_price,
            string pay_type,
            uint request_id,
            uint request_type,
            long label = 0,
            byte limit_type = 0,
            long limit_num = 0,
            long limit_vip = 0) 
        {
            UpdateData(product_bid,
                product_amount,
                origine_price,
                real_price,
                pay_type,
                request_id,
                label,
                limit_type,
                limit_num,
                limit_vip);
            Request_Type = request_type;
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="product_bid">產品ID</param>
        /// <param name="product_amount">產品數量</param>
        /// <param name="origine_price">原始價格</param>
        /// <param name="real_price">真實價格</param>
        /// <param name="pay_type">消耗Type</param>
        /// <param name="request_id">傳送Request_id</param>
        /// <param name="label">X折</param>
        /// <param name="limit_type"></param>
        /// <param name="limit_num"></param>
        /// <param name="limit_vip"></param>
        private void UpdateData(long product_bid,
            long product_amount,
            long origine_price,
            long real_price,
            string pay_type,
            uint request_id,
            long label = 0,
            byte limit_type = 0,
            long limit_num = 0,
            long limit_vip = 0
        )
        {
            ProductBid = product_bid;
            ProductAmount = product_amount;
            OriginePrice = origine_price;
            RealPrice = real_price;
            PayType = pay_type;
            Request_Id = request_id;
            OffLabel = (byte)(label == 0 ? 0 : 10 - label);
            LimitType = limit_type;
            LimitNum = limit_num;
            LimitVip = (byte)limit_vip;
        }
    }

    public class MallSellItemCell : MonoBehaviour
    {
        private class LocalizeKey 
        {
            public const string Const_No_Limit_Text = "沒有限購";
            public const string Const_Month_Limit_Text = "每月限購({0}/{1})";
            public const string Const_Week_Limit_Text = "每周限購({0}/{1})";
            public const string Const_Day_Limit_Text = "每日限購({0}/{1})";
            public const string Const_Time_Limit_Text = "限購({0}/{1})";
            public const string Const_Vip_Limit_Text = "VIP{0}方可召喚";
            public const string Const_Guaranteed_Name_Text = "積分召喚";
        }

        [SerializeField] private Image bg;
        /// <summary>
        /// 道具名稱
        /// </summary>
        [SerializeField] private Text itemNameText;
        /// <summary>
        /// 道具圖示
        /// </summary>
        [SerializeField] private ItemIcon itemIcon;
        /// <summary>
        /// 英雄碎片圖示
        /// </summary>
        [SerializeField] private HeroIcon heroIcon;

        /// <summary>
        /// 限制提示(ex:每日限購
        /// </summary>
        [SerializeField] private Text LimitTipText;

        /// <summary>
        /// 打折的圖示
        /// </summary>
        [SerializeField] private GameObject DisCountParent;

        /// <summary>
        /// 打折優惠文字
        /// </summary>
        [SerializeField] private Text DisCountText;
        /// <summary>
        /// 玩家購買到的價格
        /// </summary>
        [SerializeField] private Text RealPriceText;
        /// <summary>
        /// 原本的價格(只有在打折時顯現
        /// </summary>
        [SerializeField] private Text OriginePriceText;
        /// <summary>
        /// 金幣圖示(根據不同的所需材料,顯示不同的條件
        /// </summary>
        [SerializeField] private Image CoinImg;

        /// <summary>
        /// 購買狀態的遮罩(如果無法購買,可能只開起這個遮罩?
        /// </summary>
        [SerializeField] private Image BlackMaskParent;

        /// <summary>
        /// 購買遮罩的圖示(ex:完成購買的勾勾遮罩
        /// </summary>
        [SerializeField] private Image SoldOutIcomImg;
        /// <summary>
        /// 觸碰按鈕範圍
        /// </summary>
        [SerializeField] private Button touchAreaBtn;

        [SerializeField] private Sprite[] bgs;

        public bool IsInit { get; private set; } = false;
        /// <summary>
        /// 道具顯示資料
        /// </summary>
        private ShopMallProductData _data = null;
        /// <summary>
        /// 方法儲存
        /// </summary>
        private Action<ShopMallProductData> _callback = null;
        private string _remindText = "";

		protected void Start()
		{
            SettingButton();
        }

		/// <summary>
		/// 初始化,更新數據
		/// </summary>
		/// <param name="_data"></param>
		/// <returns></returns>
		public void Init(Action<ShopMallProductData> callback) 
        {
            if (!IsInit) 
            {
                _callback = callback;
                IsInit = true;
            }
        }

        public void UpdateData(ShopMallProductData data) 
        {
            _data = data;
            InitUIStatus();
            UpdateUI();
        }

        /// <summary>
        /// 設定按鈕
        /// </summary>
        private void SettingButton()
        {
            touchAreaBtn.onClick.RemoveAllListeners();
            touchAreaBtn.OnClickAsObservable().Subscribe(_ => _callback(_data)).AddTo(this);
            BlackMaskParent.GetComponent<Button>().OnClickAsObservable().Subscribe(_ => OnBlickMaskButton()).AddTo(this);
        }

        /// <summary>
        /// 初始化UI的狀態
        /// </summary>
        private void InitUIStatus()
        {
            BlackMaskParent.gameObject.SetActive(false);
            SoldOutIcomImg.gameObject.SetActive(false);
            DisCountParent.gameObject.SetActive(false);
            OriginePriceText.gameObject.SetActive(false);
            itemIcon.gameObject.SetActive(false);
            heroIcon.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新道具的顯示UI
        /// </summary>
        private void UpdateUI()
        {
            _data.camp_type = -1;
            _remindText = "";

            //道具 or 英雄
            if (_data.Request_Type == 23213)
            {
                var item_data = LoadResource.Instance.RecruitHighData.data_seerpalace_data[_data.Request_Id.ToString()];
                itemNameText.text = item_data.name;
            }
            else if (_data.Request_Type == 23201) 
            {
                itemNameText.text = LocalizeKey.Const_Guaranteed_Name_Text;
            }
            else
            {
                var item_data = LoadResource.Instance.GetItemData((uint)_data.ProductBid);
                itemNameText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, LoadResource.Instance.GetItemName<string>(item_data, "name"));
                var sub_type = LoadResource.Instance.GetItemName<long>(item_data, "sub_type");
                switch (sub_type)
                {
                    case (int)StorageManager.BagType.Other:
                        {
                            var quality = LoadResource.Instance.GetItemName<long>(item_data, "quality");
                            itemIcon.Init((uint)_data.ProductBid, (uint)quality, (uint)_data.ProductAmount, 0, false, isShowAmount: true, isShowLongPanel: false);
                            itemIcon.gameObject.SetActive(true);
                        }
                        break;
                    case (int)StorageManager.BagType.Equipment:
                        {
                            var star = LoadResource.Instance.GetItemName<long>(item_data, "eqm_star");
                            var quality = LoadResource.Instance.GetItemName<long>(item_data, "quality");
                            itemIcon.Init((uint)_data.ProductBid, (uint)quality, (uint)_data.ProductAmount, star, false, isShowAmount: true, isShowLongPanel: false);
                            itemIcon.gameObject.SetActive(true);
                        }
                        break;
                    case (int)StorageManager.BagType.Artifact:
                        {
                            var quality = LoadResource.Instance.GetItemName<long>(item_data, "quality");
                            itemIcon.Init((uint)_data.ProductBid, (uint)quality, (uint)_data.ProductAmount, 0, false, isShowAmount: true, isShowLongPanel: false);
                            itemIcon.gameObject.SetActive(true);
                        }
                        break;
                    case (int)StorageManager.BagType.Prop:
                        {
                            var star = LoadResource.Instance.GetItemName<long>(item_data, "eqm_jie");
                            var quality = LoadResource.Instance.GetItemName<long>(item_data, "quality");
                            itemIcon.Init((uint)_data.ProductBid, (uint)quality, (uint)_data.ProductAmount, star, false, isShowAmount: true, isShowLongPanel: false);
                            itemIcon.gameObject.SetActive(true);
                        }
                        break;
                    case (int)StorageManager.BagType.Chip:
                        var type = LoadResource.Instance.GetItemName<long>(item_data, "type");
                        long campType = LoadResource.Instance.GetItemName<long>(item_data, "lev");
                        _data.camp_type = (int)campType;
                        if (type == 102)
                        {
                            JArray effect = LoadResource.Instance.GetItemName<JArray>(item_data, "effect");
                            // 碎片
                            if (effect.Count > 0)
                            {
                                if (effect[0].SelectToken("effect_type").ToString() == "5")
                                {
                                    JToken heroBid = effect[0].SelectToken("val");
                                    heroIcon.InitChip((uint)heroBid, (uint)_data.ProductBid, -1, (uint)_data.ProductAmount, true);
                                    heroIcon.gameObject.SetActive(true);
                                }
                            }
                            else
                            {
                                // 隨機碎片
                                //long campType = LoadResource.Instance.GetItemName<long>(item_data, "lev");
                                heroIcon.InitChip(0, (uint)_data.ProductBid, campType, (uint)_data.ProductAmount, true);
                                heroIcon.gameObject.SetActive(true);
                            }
                        }
                        else if (type == 0)
                        {

                        }
                        break;
                }
            }

            //支付
            CoinImg.sprite = LoadResource.Instance.GetItemLittleIcon(_data.PayId);
            if (_data.OffLabel > 0)
            {
                DisCountParent.gameObject.SetActive(true);
                DisCountText.text = _data.OffLabel + "0%OFF";
                OriginePriceText.gameObject.SetActive(true);
                OriginePriceText.text = _data.OriginePrice.ToString();
                bg.sprite = bgs[1];
            }
            else 
            {
                bg.sprite = bgs[0];
            }

            RealPriceText.text = _data.RealPrice.ToString();

            if (_data.LimitNum > 0)
            {
                switch (_data.LimitType) 
                {
                    case 0:
                        LimitTipText.text = string.Format(LocalizeKey.Const_Time_Limit_Text, _data.HasBuy, _data.LimitNum);
                        break;
                    case 1:
                        LimitTipText.text = string.Format(LocalizeKey.Const_Day_Limit_Text, _data.HasBuy, _data.LimitNum);
                        break;
                    case 2:
                        LimitTipText.text = string.Format(LocalizeKey.Const_Week_Limit_Text, _data.HasBuy, _data.LimitNum);
                        break;
                    case 3:
                        LimitTipText.text = string.Format(LocalizeKey.Const_Month_Limit_Text, _data.HasBuy, _data.LimitNum);
                        break;
                }
            }
            else 
            {
                LimitTipText.text = string.Format(LocalizeKey.Const_No_Limit_Text);
            }

            if (_data.SoldOut)
            {
                BlackMaskParent.gameObject.SetActive(true);
                SoldOutIcomImg.gameObject.SetActive(true);
            }

            if (_data.LimitVip != 0 && MessageResponseData.Instance.UserData.vip_lev < _data.LimitVip) 
            {
                _remindText = string.Format(LocalizeKey.Const_Vip_Limit_Text, _data.LimitVip);
                BlackMaskParent.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 條件未達成
        /// </summary>
        private void OnBlickMaskButton() 
        {
            if(_remindText != string.Empty)
                MainManager.Instance.AddRemind(_remindText);
        }
    }
}
