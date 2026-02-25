using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Newtonsoft.Json;
using YKO.Main;
using YKO.Common.UI;
using YKO.Network;
using YKO.Common;
//using Unity.VisualScripting;
//using MoonSharp.VsCodeDebugger.SDK;
//using MarkerMetro.Unity.WinLegacy.Reflection;
using YKO.Common.Sound;
using static YKO.CONST.MallConst;
using YKO.Storage;
using YKO.DataModel;

namespace YKO.MallShop
{

    /// <summary>
    /// 商店頁面
    /// </summary>
    public enum ShopMallPage
    {
        None = 0, //如果回傳的response對應不上type編號 則none
        Item = 1,//道具 01-1
        #region Destiny Button
        GodPersonality = 2,
        Rank = 3, // 積分
        HeroHeart = 53,//英雄之心 01-53
        Prophet = 31,//先知 01 -31 
        PlumeShop = 50,//聖羽
        Guaranteed = 998,//招喚積分
        Divination = 999,//星命兌換(星象占卜)
        #endregion
        #region RankButton
        SeaChallenge = 8,//征戰 01 -8 
        Sports = 6,//競技 01 -6 
        Guild = 5,//公會 01 -5
        Casino = 16,//賭坊 01 03 -16
        Level = 17,//段位 01 -17
        #endregion
        Skill = 9,//技能 03 -9
    }

    public partial class MallShopController : MonoBehaviour
    {
        private class LocalizeKey 
        {
            public const string Const_Refresh_Btn_Text = "補貨";
            public const string Const_Refresh_Popup_Desc_Text = "是否確定[刷新/填充]商品?\n每週更新時將重置購買次數。";
            public const string Const_23213_Popup_Desc_Text = "消耗姻緣花牌可任選一系進行召喚，必出五星傳說英雄";
            public const string Const_23201_Popup_Desc_Text = "消耗1000點積分進行積分召喚，必出5星傳說英雄";

            public const string Const_Divination_Cost = "predict_point";
        }

        public class SceneParameter
        {
            public ShopMallPage page;
        }

        /// <summary>
        ///物件屬性(ex:角色碎片
        /// </summary>
        public enum ObjectAttributeType
        {
            /// <summary>
            /// All
            /// </summary>
            All = 0,
            /// <summary>
            /// 水
            /// </summary>
            Water = 1,
            /// <summary>
            /// 火
            /// </summary>
            Fire = 2,
            /// <summary>
            /// 風
            /// </summary>
            Wind = 3,
            /// <summary>
            /// 光
            /// </summary>
            Light = 4,
            /// <summary>
            /// 暗
            /// </summary>
            Dark = 5,
        }

        #region Top UI
        [Header("Top UI")]
        [SerializeField]
        private CanvasGroup bgCanvasGroup;
        [SerializeField]
        private CanvasGroup uiCanvasGroup;
        /// <summary>
        /// 刷新按鈕
        /// </summary>
        [SerializeField]
        private CostButton refreshButton;
        /// <summary>
        /// 中間商人對話UI
        /// </summary>
        [SerializeField]
        private Text txtTipConversation = default;
        /// <summary>
        /// 通用上方UI
        /// </summary>
        [SerializeField]
        private TopPanel topPanelMain = default;
        /// <summary>
        /// 資訊按鈕（目前只有「命介 > 聖羽」使用）
        /// </summary>
        [SerializeField]
        private Button btnInfo = default;
        #endregion

        #region   Bottom Button UI
        [Header("Bottom Button UI")]
        [SerializeField]
        private Button btnBack = default;
        [SerializeField]
        private Button btnMypage = default;
        #endregion

        #region Left Button InterFace
        [Header("Left Button InterFace")]
        /// <summary>
        /// 顯示當前的按鈕位置
        /// </summary>
        [SerializeField] private RectTransform sliderImage;
        /// <summary>
        /// 道具
        /// </summary>
        [SerializeField] private Button ItemButton;
        [SerializeField] private Text txtItemButton;
        /// <summary>
        /// 命介
        /// </summary>
        [SerializeField] private Button DestinyButton;
        [SerializeField] private Text txtDestinyButton;
        /// <summary>
        /// 積分
        /// </summary>
        [SerializeField] private Button RankButton;
        [SerializeField] private Text txtRankButton;
        /// <summary>
        /// 技能
        /// </summary>
        [SerializeField] private Button SkillButton;
        [SerializeField] private Text txtSkillButton;
        #endregion

        #region MainPanel 商品圖卡
        [Header("MainPanel 商品圖卡")]
        /// <summary>
        /// ScrollView 捲動視圖組件  (部分頁面有篩選功能  必須調整此物件的高 來空出篩選UI的位置
        /// </summary>
        [SerializeField] private RectTransform ItemScrollView;
        /// <summary>
        /// 清單
        /// </summary>
        [SerializeField] private MallShopScrollViewAdapter mallShopScrollViewAdapter;

        [Header("OtherMenu")]
        /// <summary>
        /// 次級選單(集合)
        /// </summary>
        [SerializeField] private GameObject otherMenuGroup;
        //命介
        /// <summary>
        /// 神格兌換
        /// </summary>
        [SerializeField] private Toggle menuGodToggle;
        [SerializeField] private Text txtMenuGodToggle;
        /// <summary>
        /// 英魂兌換
        /// </summary>
        [SerializeField] private Toggle menuSoulToggle;
        [SerializeField] private Text txtMenuSoulToggle;
        /// <summary>
        /// 先知兌換
        /// </summary>
        [SerializeField] private Toggle menuProphetToggle;
        [SerializeField] private Text txtMenuProphetToggle;
        /// <summary>
        /// 聖羽兌換
        /// </summary>
        [SerializeField] private Toggle menuFeatherToggle;
        [SerializeField] private Text txtMenuFeatherToggle;
        /// <summary>
        /// 召喚積分兌換
        /// </summary>
        [SerializeField] private Toggle menuGuaranteedToggle;
        [SerializeField] private Text txtMenuGuaranteedToggle;
        /// <summary>
        /// 星命兌換(星象占卜)
        /// </summary>
        [SerializeField] private Toggle menuDivinationToggle;
        [SerializeField] private Text txtMenuDivinationToggle;
        [SerializeField] private RedDotObj menuDivinationToggleRedDot;
        //積分
        /// <summary>
        /// 征戰兌換
        /// </summary>
        [SerializeField] private Toggle menuSeaToggle;
        [SerializeField] private Text txtMenuSeaToggle;
        /// <summary>
        /// 競技兌換
        /// </summary>
        [SerializeField] private Toggle menuArenaToggle;
        [SerializeField] private Text txtMenuArenaToggle;
        /// <summary>
        /// 公會兌換
        /// </summary>
        [SerializeField] private Toggle menuGuildToggle;
        [SerializeField] private Text txtMenuGuildToggle;
        /// <summary>
        /// 賭坊兌換
        /// </summary>
        [SerializeField] private Toggle menuCasinoToggle;
        [SerializeField] private Text txtMenuCasinoToggle;
        /// <summary>
        /// 段位兌換
        /// </summary>
        [SerializeField] private Toggle menuLevelToggle;
        [SerializeField] private Text txtMenuLevelToggle;

        [Header("ClassFilter")]
        /// <summary>
        /// 屬性搜尋按鈕(集合)
        /// </summary>
        [SerializeField] private GameObject classFilterGroup;
        /// <summary>
        /// 無搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterAllToggle;
        /// <summary>
        /// 水屬性的搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterWaterToggle;
        /// <summary>
        /// 火屬性的搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterFireToggle;
        /// <summary>
        /// 風屬性的搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterWindToggle;
        /// <summary>
        /// 光屬性的搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterLightToggle;
        /// <summary>
        /// 暗屬性的搜尋按鈕
        /// </summary>
        [SerializeField] private Toggle filterDarkToggle;

        /// <summary>
        ///  消耗圖示(隨著每個商店使用不同的消耗型物件而改變顯示圖樣
        /// </summary>
        [SerializeField] Image coinIconImg = default;
        /// <summary>
        /// 消耗單位 (隨著每個商店使用不同的消耗型物件而改變顯示結果
        /// </summary>
        /// 
        [SerializeField] Text txtMoney3 = default;
        #endregion

        [Header("Popup")]
        /// <summary>
        /// 購買彈窗
        /// </summary>
        [SerializeField] private GameObject buyGoodsPopup;
        [SerializeField] private GameObject refreshShopPopup;
        [SerializeField] private GameObject rulePopup;
        private GameObject purchasingPopupWindow = null;

        #region Common Setting
        [Header("Common Setting")]
        /// <summary>
        /// 當前分頁
        /// </summary>
        [SerializeField] private ShopMallPage curMallPage = ShopMallPage.None;
        /// <summary>
        /// 開啟不同分頁的UI互動判斷式
        /// </summary>
        private ShopMallPage MallPage
        {
            get { return curMallPage; }
            set
            {
                if (curMallPage != value)
                {
                    curMallPage = value;

                    StartCoroutine(UpdateProductList());
                }
                else
                {
                    return;
                }
            }
        }
        /// <summary>
        /// 當前屬性分類
        /// </summary>
        [SerializeField] private ObjectAttributeType curAttributeType = ObjectAttributeType.All;
        private List<uint> _refresh_cost = new List<uint>();
        /// <summary>
        /// 偵測玩家是否可切換其他頁面(如果為true 為家可點擊按鈕
        /// </summary>
        private bool canPlayTween = false;
        /// <summary>
        /// 左邊的每格欄位高度
        /// </summary>
        private const float LeftButtonInterval = -240;

        private ProtoResponse<Proto_13401_Response> proto13401_resp = new ProtoResponse<Proto_13401_Response>();
        private ProtoResponse<Proto_13402_Response> proto13402_resp = new ProtoResponse<Proto_13402_Response>();
        private ProtoResponse<Proto_13403_Response> proto13403_resp = new ProtoResponse<Proto_13403_Response>();
        private ProtoResponse<Proto_13405_Response> proto13405_resp = new ProtoResponse<Proto_13405_Response>();
        private ProtoResponse<Proto_13407_Response> proto13407_resp = new ProtoResponse<Proto_13407_Response>();
        private ProtoResponse<Proto_23201_Response> proto23201_resp = new ProtoResponse<Proto_23201_Response>();
        private ProtoResponse<Proto_23213_Response> proto23213_resp = new ProtoResponse<Proto_23213_Response>();
        private Dictionary<uint, Proto_13403_Response> temp_13403_list = new Dictionary<uint, Proto_13403_Response>();
        private Dictionary<byte, Proto_13401_Response> temp_13401_list = new Dictionary<byte, Proto_13401_Response>();

        #endregion

        public void Start()
        {
            SoundController.Instance.PlayBGM(SoundController.BGMName.BGM_MYPAGE);
            StartCoroutine(Init());
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init()
        {
            bgCanvasGroup.alpha = 0;
            uiCanvasGroup.alpha = 0;

            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();

            topPanelMain.Init(MainManager.Instance.PopupParent);
            curAttributeType = ObjectAttributeType.All;
            curMallPage = ShopMallPage.None;

            RegisterEvent();

            SetUIText();

            // 紀錄開啟頁面要執行的proto 寄送後等待回覆
            SceneParameter sceneParam = GameSceneManager.Instance.GetSceneParam() as SceneParameter;
            if (sceneParam != null)
            {
                switch (sceneParam.page)
                {
                    case ShopMallPage.GodPersonality:
                        yield return OnDestinyButton();
                        yield return ClickGodPersonalityButton();
                        menuGodToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.HeroHeart:
                        yield return OnDestinyButton();
                        yield return ClickHeroHeartButton();
                        menuSoulToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Prophet:
                        yield return OnDestinyButton();
                        yield return ClickProphetButton();
                        menuProphetToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.PlumeShop:
                        yield return OnDestinyButton();
                        yield return ClickFeatherButton();
                        menuFeatherToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Guaranteed:
                        yield return OnDestinyButton();
                        yield return ClickFeatherButton();
                        menuGuaranteedToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Divination:
                        yield return OnDestinyButton();
                        yield return ClickDivinationButton();
                        menuDivinationToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.SeaChallenge:
                        yield return OnRankButton();
                        yield return ClickSeaButton();
                        menuSeaToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Sports:
                        yield return OnRankButton();
                        yield return ClickSportsButton();
                        menuArenaToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Guild:
                        yield return OnRankButton();
                        yield return ClickGuildButton();
                        menuGuildToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Casino:
                        yield return OnRankButton();
                        yield return ClickCasinoButton();
                        menuCasinoToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Level:
                        yield return OnRankButton();
                        yield return ClickLevelButton();
                        menuLevelToggle.SetIsOnWithoutNotify(true);
                        break;
                    case ShopMallPage.Skill:
                        yield return OnSkillButton();
                        break;
                    case ShopMallPage.Item:
                    default:
                        yield return OnItemButton();
                        break;
                }
            }
            else
            {
                yield return OnItemButton();
                //紀錄開啟頁面要執行的proto 寄送後等待回覆
            }

            MainManager.Instance.HideLoading(true);
            PlayEnterAnim(()=> 
            {
                canPlayTween = true;
            });
        }
        /// <summary>
        /// 註冊按鈕
        /// </summary>
        private void RegisterEvent()
        {
            //RefisterProto();

            /////左邊篩選
            ItemButton.OnClickAsObservable().Where(_ => canPlayTween).Subscribe(_ => StartCoroutine(OnItemButton()));
            DestinyButton.OnClickAsObservable().Where(_ => canPlayTween).Subscribe(_ => StartCoroutine(OnDestinyButton(true)));
            RankButton.OnClickAsObservable().Where(_ => canPlayTween).Subscribe(_ => StartCoroutine(OnRankButton(true)));
            SkillButton.OnClickAsObservable().Where(_ => canPlayTween).Subscribe(_ => StartCoroutine(OnSkillButton()));

            //次級選單
            //命介
            menuGodToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickGodPersonalityButton()));
            menuSoulToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickHeroHeartButton()));
            menuProphetToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickProphetButton()));
            menuFeatherToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickFeatherButton()));
            menuGuaranteedToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickGuaranteedButton()));
            menuDivinationToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickDivinationButton()));
            menuDivinationToggleRedDot.StartRegistEvent(dictionary =>
            {
                if (dictionary.TryGetValue(RedDotTypes.ITEM_RESOURCE_AMOUNT, out var eventData))
                {
                    if (eventData.Num >= 1000) return true;
                }

                return false;
            }).RegistRedDot(new RedDotRegistParam<object>(RedDotTypes.ITEM_RESOURCE_AMOUNT, LocalizeKey.Const_Divination_Cost));
            //積分
            menuSeaToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickSeaButton()));
            menuArenaToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickSportsButton()));
            menuGuildToggle.interactable = MessageResponseData.Instance.UserData.gid != 0;
            menuGuildToggle.transform.Find("Lock").gameObject.SetActive(MessageResponseData.Instance.UserData.gid == 0);
            menuGuildToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickGuildButton()));
            menuCasinoToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickCasinoButton()));
            menuLevelToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => StartCoroutine(ClickLevelButton()));

            //屬性篩選
            filterAllToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.All));
            filterWaterToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.Water));
            filterFireToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.Fire));
            filterWindToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.Wind));
            filterLightToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.Light));
            filterDarkToggle.OnValueChangedAsObservable().Where(isOn => isOn).Subscribe(_ => ClickAttributeFilter(ObjectAttributeType.Dark));

            //清單Callback
            mallShopScrollViewAdapter.Parameters.SetCheckTeamFunc(data => 
            {
                OpenPurchasingPopup(data, buyCount => 
                    data.Request_Type == 13402 ?
                        Request13402(data.Request_Id, (uint)buyCount) :
                    data.Request_Type == 13407 ?
                        Request13407(data.Request_Id, data.Request_Shop_Type, data.PayId, (uint)buyCount) :
                    data.Request_Type == 23213 ?
                        Request23213(data.Request_Id) :
                    data.Request_Type == 23201 ?//
                        Request23201(data.Request_Id) : null
                );
            });

            btnMypage.OnClickAsObservable().Subscribe(_ => ClickButtonMypage());
            btnBack.OnClickAsObservable().Subscribe(_ => ClickButtonBack());
            btnInfo.OnClickAsObservable().Subscribe(_ => ClickButtonInfo());

            refreshButton.OnClickAsObservable().Subscribe(_ => OpenRefreshShopPopup());

            proto13401_resp.OnResponse = Handle13401;
            proto13402_resp.OnResponse = Handle13402;
            proto13403_resp.OnResponse = Handle13403;
            proto13405_resp.OnResponse = Handle13405;
            proto13407_resp.OnResponse = Handle13407;
            proto23201_resp.OnResponse = Handle23201;
            proto23213_resp.OnResponse = Handle23213;
        }

        private void SetUIText()
        {
            ExchangeData exchangeData = LoadResource.Instance.ExchangeData;

            string itemButtonText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Item}"].name;
            string destinyButtonText = exchangeData.data_shop_list[$"{(int)ShopMallPage.GodPersonality}"].name;
            string rankButtonText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Rank}"].name;
            string skillButtonText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Skill}"].name;

            string menuGodToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.GodPersonality}"].name;
            string menuSoulToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.HeroHeart}"].name;
            string menuProphetToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Prophet}"].name;
            string menuFeatherToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.PlumeShop}"].name;
            //string menuGuaranteedToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Guaranteed}"].name;
            //string menuDivinationToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Divination}"].name;
            string menuSeaToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.SeaChallenge}"].name;
            string menuArenaToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Sports}"].name;
            string menuGuildToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Guild}"].name;
            string menuCasinoToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Casino}"].name;
            string menuLevelToggleText = exchangeData.data_shop_list[$"{(int)ShopMallPage.Level}"].name;

            txtItemButton.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, itemButtonText);
            txtDestinyButton.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, destinyButtonText);
            txtRankButton.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, rankButtonText);
            txtSkillButton.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, skillButtonText);

            txtMenuGodToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuGodToggleText);
            txtMenuSoulToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuSoulToggleText);
            txtMenuProphetToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuProphetToggleText);
            txtMenuFeatherToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuFeatherToggleText);
            //txtMenuGuaranteedToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuGuaranteedToggleText);
            //txtMenuDivinationToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuDivinationToggleText);
            txtMenuSeaToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuSeaToggleText);
            txtMenuArenaToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuArenaToggleText);
            txtMenuGuildToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuGuildToggleText);
            txtMenuCasinoToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuCasinoToggleText);
            txtMenuLevelToggle.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, menuLevelToggleText);
        }

        #region Button Click Event

        #region LeftButtonClickFunc
        /// <summary>
        /// 點擊道具頁按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnItemButton()
        {
            //需要在購買後清空response的資料  讓商店頁面刷新
            if (MallPage == ShopMallPage.Item)
            {
                yield break;
            }
            CloseAttributePanel();
            CloseOtherMenuPanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Item).mallType;
            if (!temp_13401_list.ContainsKey(type))
            {
                yield return Request13401(type);
            }
            StartCoroutine(SliderMove(0 * LeftButtonInterval));
            canPlayTween = true;

            MallPage = ShopMallPage.Item;
        }
        /// <summary>
        /// 點擊命介頁按鈕
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnDestinyButton(bool go_first = false)
        {
            if (MallPage == ShopMallPage.GodPersonality ||
                MallPage == ShopMallPage.HeroHeart||
                MallPage == ShopMallPage.Prophet||
                MallPage == ShopMallPage.PlumeShop
            )
            {
                yield break;
            }
            OpenAttributePanel();
            OpenOtherMenuPanel(true, go_first);

            canPlayTween = false;
            StartCoroutine(SliderMove(1 * LeftButtonInterval));
            canPlayTween = true;
        }
        /// <summary>
        /// 點擊積分頁按鈕
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnRankButton(bool go_first = false)
        {
            if (MallPage == ShopMallPage.SeaChallenge ||
                MallPage == ShopMallPage.Sports ||
                MallPage == ShopMallPage.Guild ||
                MallPage == ShopMallPage.Casino ||
                MallPage == ShopMallPage.Level
            )
            {
                yield break;
            }
            CloseAttributePanel();
            OpenOtherMenuPanel(false, go_first);

            canPlayTween = false;
            StartCoroutine(SliderMove(2 * LeftButtonInterval));
            canPlayTween = true;
        }
        /// <summary>
        /// 點擊天賦按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnSkillButton()
        {
            if (MallPage == ShopMallPage.Skill)
            {
                yield break;
            }
            CloseAttributePanel();
            CloseOtherMenuPanel();

            canPlayTween = false;
            var type = (uint)GetMallInfoPage(ShopMallPage.Skill).mallType;
            yield return Request13403(type);
            StartCoroutine(SliderMove(3 * LeftButtonInterval));
            canPlayTween = true;

            MallPage = ShopMallPage.Skill;
        }

        /// <summary>
        /// 調整滑塊的移動位置
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private IEnumerator SliderMove(float height)
        {
            sliderImage.DOKill();
            yield return sliderImage.DOAnchorPos(new Vector2(0, height - 13), 0.1f);

        }
        #endregion

        #region OtherMenuClickFunc
        /// <summary>
        /// 點擊神格按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickGodPersonalityButton()
        {
            if (MallPage == ShopMallPage.GodPersonality)
            {
                yield break;
            }
            OpenAttributePanel();

			canPlayTween = false;
            var type = (uint)GetMallInfoPage(ShopMallPage.GodPersonality).mallType;
            if (!temp_13403_list.ContainsKey(type))
            {
                yield return Request13403(type);
            }
            canPlayTween = true;

            MallPage = ShopMallPage.GodPersonality;
        }
        /// <summary>
        /// 點擊英魂之心按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickHeroHeartButton()
        {
            if (MallPage == ShopMallPage.HeroHeart)
            {
                yield break;
            }
            OpenAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.HeroHeart).mallType;
            yield return Request13401(type);
            canPlayTween = true;

            MallPage = ShopMallPage.HeroHeart;
        }
        /// <summary>
        /// 點擊先知按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickProphetButton()
        {
            if (MallPage == ShopMallPage.Prophet)
            {
                yield break;
            }
            OpenAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Prophet).mallType;
            yield return Request13401(type);
            canPlayTween = true;

            MallPage = ShopMallPage.Prophet;
        }
        /// <summary>
        /// 點擊聖羽按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickFeatherButton()
        {
            if (MallPage == ShopMallPage.PlumeShop)
            {
                yield break;
            }
            OpenAttributePanel();

            canPlayTween = false;
            //   yield return SendAndWaitDataResponse(curMallPage);
            canPlayTween = true;

            MallPage = ShopMallPage.PlumeShop;
        }
        private IEnumerator ClickGuaranteedButton()
        {
            if (MallPage == ShopMallPage.Guaranteed)
            {
                yield break;
            }
            CloseAttributePanel();
            canPlayTween = false;
            canPlayTween = true;

            MallPage = ShopMallPage.Guaranteed;
        }
        /// <summary>
        /// 點擊星命兌換(星象占卜)按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickDivinationButton()
        {
            if (MallPage == ShopMallPage.Divination)
            {
                yield break;
            }
            CloseAttributePanel();
            canPlayTween = false;
            canPlayTween = true;

            MallPage = ShopMallPage.Divination;
        }
        /// <summary>
        /// 點擊征戰按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickSeaButton()
        {
            if (MallPage == ShopMallPage.SeaChallenge)
            {
                yield break;
            }
            CloseAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.SeaChallenge).mallType;
            if (!temp_13401_list.ContainsKey(type)) 
            {
                yield return Request13401(type);
            }
            canPlayTween = true;

            MallPage = ShopMallPage.SeaChallenge;
        }
        /// <summary>
        /// 點擊競技按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickSportsButton()
        {
            if (MallPage == ShopMallPage.Sports)
            {
                yield break;
            }
            CloseAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Sports).mallType;
            if (!temp_13401_list.ContainsKey(type))
            {
                yield return Request13401(type);
            }
            canPlayTween = true;

            MallPage = ShopMallPage.Sports;
        }
        /// <summary>
        /// 點擊公會按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickGuildButton()
        {
            if (MallPage == ShopMallPage.Guild)
            {
                yield break;
            }
            CloseAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Guild).mallType;
            if (!temp_13401_list.ContainsKey(type))
            {
                yield return Request13401(type);
            }
            canPlayTween = true;

            MallPage = ShopMallPage.Guild;
        }
        /// <summary>
        /// 點擊賭場按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickCasinoButton()
        {
            if (MallPage == ShopMallPage.Casino)
            {
                yield break;
            }
            CloseAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Casino).mallType;
            if (!temp_13401_list.ContainsKey(type))
            {
                yield return Request13401(type);
            }
            yield return Request13403(type);
            canPlayTween = true;

            MallPage = ShopMallPage.Casino;
        }
        /// <summary>
        /// 點擊段位按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickLevelButton()
        {
            if (MallPage == ShopMallPage.Level)
            {
                yield break;
            }
            CloseAttributePanel();

            canPlayTween = false;
            var type = (byte)GetMallInfoPage(ShopMallPage.Level).mallType;
            yield return Request13401(type);
            canPlayTween = true;

            MallPage = ShopMallPage.Level;
        }

        /// <summary>
        /// 點擊篩選按鈕
        /// </summary>
        private void ClickAttributeFilter(ObjectAttributeType type)
        {
            curAttributeType = type;

            mallShopScrollViewAdapter.addFilterIndex((int)curAttributeType);
        }
        #endregion

        /// <summary>
        /// 開啟購買視窗
        /// </summary>
        /// <param name="protoID"></param>
        /// <param name="data"></param>
        /// <param name="sendPurchasingProto"></param>
        /// <returns></returns>
        private void OpenPurchasingPopup(ShopMallProductData data, Func<int, IEnumerator> sendPurchasingProto)
        {
            if (purchasingPopupWindow == null)
            {
                if (data.Request_Type == 23213)
                {
                    GameObject popup = Instantiate(refreshShopPopup, MainManager.Instance.PopupParent.transform);
                    popup.GetComponent<PopupCommonExtra>().Init(() =>
                    {
                        StartCoroutine(sendPurchasingProto(0));
                    });
                    popup.GetComponent<PopupCommonExtra>().AddDescBox(LocalizeKey.Const_23213_Popup_Desc_Text);

                    popup.GetComponent<PopupBase>().ShowPopup();
                    purchasingPopupWindow = popup;
                }
                else if (data.Request_Type == 23201) 
                {
                    GameObject popup = Instantiate(refreshShopPopup, MainManager.Instance.PopupParent.transform);
                    popup.GetComponent<PopupCommonExtra>().Init(() =>
                    {
                        StartCoroutine(sendPurchasingProto(0));
                    });
                    popup.GetComponent<PopupCommonExtra>().AddDescBox(LocalizeKey.Const_23201_Popup_Desc_Text);

                    popup.GetComponent<PopupBase>().ShowPopup();
                    purchasingPopupWindow = popup;
                }
                else
                {
                    GameObject popup = Instantiate(buyGoodsPopup, MainManager.Instance.PopupParent.transform);
                    switch (LoadResource.Instance.GetItemName<long>((uint)data.ProductBid, "sub_type"))
                    {
                        case (int)StorageManager.BagType.Other:
                        case (int)StorageManager.BagType.Prop:
                        case (int)StorageManager.BagType.Chip:
                            popup.GetComponent<PopupBuyGoods>().InitBuyItem((uint)data.ProductBid, (uint)data.ProductAmount,
                                data.PayId, (uint)data.RealPrice,
                                num =>
                                {
                                    StartCoroutine(sendPurchasingProto(num));
                                }, (int)(data.LimitNum > 0 ? data.LimitNum - data.HasBuy : 0), (uint)data.OriginePrice);
                            break;
                        default:
                            popup.GetComponent<PopupBuyGoods>().InitBuyEquip((uint)data.ProductBid,
                                data.PayId, (uint)data.RealPrice,
                                data.Main_attr, data.Score,
                                num =>
                                {
                                    StartCoroutine(sendPurchasingProto(num));
                                }, (int)(data.LimitNum > 0 ? data.LimitNum - data.HasBuy : 0), (uint)data.OriginePrice);
                            break;
                    }

                    popup.GetComponent<PopupBase>().ShowPopup();
                    purchasingPopupWindow = popup;
                }
            }
        }

        /// <summary>
        /// 刷新商店Popup
        /// </summary>
        private void OpenRefreshShopPopup() 
        {
            var popup = Instantiate(refreshShopPopup, MainManager.Instance.PopupParent.transform);
            popup.GetComponent<PopupCommonExtra>().Init(() => 
            {
                StartCoroutine(OnRefreshShopButton());
            }, show_close: false);
            popup.GetComponent<PopupCommonExtra>().AddDescBox(LocalizeKey.Const_Refresh_Popup_Desc_Text)
                .AddPreviewCost(_refresh_cost[0], _refresh_cost[1]);
            popup.GetComponent<PopupCommonExtra>().ShowPopup();
        }

        /// <summary>
        /// 刷新商店
        /// </summary>
        private IEnumerator OnRefreshShopButton()
        {
            Debug.Log("重新篩選");
            uint shopTypeNum = (uint)GetMallInfoPage(curMallPage).mallType;
            canPlayTween = false;

            yield return Request13405(shopTypeNum);
            Debug.Log("已收到反饋");

            canPlayTween = true;
        }
        #endregion

        public IEnumerator Request13401(byte type) 
        {
            proto13401_resp.Reset();
            NetworkManager.Instance.Send(new Proto_13401_Request()
            {
                type = type
            });

            yield return proto13401_resp;
        }
        public IEnumerator Request13402(uint eid, uint num) 
        {
            proto13402_resp.Reset();
            NetworkManager.Instance.Send(new Proto_13402_Request()
            {
                eid = eid,
                num = num
            });

            yield return proto13402_resp;
        }
        public IEnumerator Request13403(uint type) 
        {
            proto13403_resp.Reset();
            NetworkManager.Instance.Send(new Proto_13403_Request()
            {
                type = type
            });

            yield return proto13403_resp;
        }
        public IEnumerator Request13405(uint type) 
        {
            proto13405_resp.Reset();
            NetworkManager.Instance.Send(new Proto_13405_Request()
            {
                type = type
            });

            yield return proto13405_resp;
        }
        public IEnumerator Request13407(uint order, uint type, uint buy_type, uint num) 
        {
            proto13407_resp.Reset();
            NetworkManager.Instance.Send(new Proto_13407_Request()
            {
                order = order,
                type = type,
                buy_type = buy_type,
                num = num,
            });

            yield return proto13407_resp;
        }
        public IEnumerator Request23201(uint group_id) 
        {
            proto23201_resp.Reset();
            NetworkManager.Instance.Send(new Proto_23201_Request
            {
                group_id = (ushort)group_id,
                times = 1,
                recruit_type = 3
            });
            yield return proto23201_resp;
        }
        public IEnumerator Request23213(uint group_id) 
        {
            proto23213_resp.Reset();
            NetworkManager.Instance.Send(new Proto_23213_Request
            {
                group_id = (ushort)group_id
            });

            yield return proto23213_resp;
        }

        private void Handle13401(Proto_13401_Response res) 
        {
            temp_13401_list[res.type] = res;
        }
        private void Handle13402(Proto_13402_Response res) 
        {
            if (res.code == 1)
            {
                temp_13401_list[res.type].is_half = res.is_half;
                var temp_item = temp_13401_list[res.type].item_list.FirstOrDefault(i => i.item_id == res.eid);
                if (temp_item != null)
                {
                    temp_item.ext = res.ext.Select(i => new Proto_13401_Response.Item_list.Ext()
                    {
                        key = i.key,
                        val = i.val
                    }).ToArray();
                }
                else
                {
                    var temp = temp_13401_list[res.type].item_list.ToList();
                    temp.Add(new Proto_13401_Response.Item_list()
                    {
                        item_id = res.eid,
                        ext = res.ext.Select(i => new Proto_13401_Response.Item_list.Ext()
                        {
                            key = i.key,
                            val = i.val
                        }).ToArray()
                    }); temp_13401_list[res.type].item_list = temp.ToArray();
                }

                StartCoroutine(UpdateProductList());
            }
            else 
            {
                MainManager.Instance.AddRemind(LocaleManager.Instance.ParseServerMessage(res.msg));
            }
        }
        private void Handle13403(Proto_13403_Response res) 
        {
            temp_13403_list[res.type] = res;
        }
        private void Handle13405(Proto_13405_Response res) 
        {
            if (res.code == 1) 
            {
                var temp = JsonConvert.DeserializeObject<Proto_13403_Response>(JsonConvert.SerializeObject(res));
                temp_13403_list[res.type] = temp;

                StartCoroutine(UpdateProductList());
            }
        }
        private void Handle13407(Proto_13407_Response res) 
        {
            if (res.code == 1)
            {
                var temp_item = temp_13403_list[res.type].item_list.FirstOrDefault(i => i.order == res.order);
                if (temp_item != null)
                {
                    temp_item.has_buy = res.num;
                }
                else 
                {
                
                }

                StartCoroutine(UpdateProductList());
            }
            else 
            {
                MainManager.Instance.AddRemind(LocaleManager.Instance.ParseServerMessage(res.msg));
            }
        }
        private void Handle23201(Proto_23201_Response res) 
        {
            MainManager.Instance.GetHeroPopup(res.rewards.Select(i => (bid: i.base_id, lev: (ushort)1)).ToList());
            StartCoroutine(UpdateProductList());
        }
        private void Handle23213(Proto_23213_Response res) 
        {
            MainManager.Instance.GetHeroPopup(res.rewards.Select(i => (bid: i.base_id, lev: (ushort)1)).ToList());
            StartCoroutine(UpdateProductList());
        }

        /// <summary>
        /// 開啟屬性面板
        /// </summary>
        /// <param name="pageInfo"></param>
        private void OpenAttributePanel()
        {
            ClickAttributeFilter(ObjectAttributeType.All);
			filterAllToggle.SetIsOnWithoutNotify(true);
            classFilterGroup.SetActive(true);
        }
        /// <summary>
        /// 關閉屬性面板
        /// </summary>
        /// <param name="pageInfo"></param>
        private void CloseAttributePanel()
        {
            ClickAttributeFilter(ObjectAttributeType.All);
            filterAllToggle.SetIsOnWithoutNotify(true);
            classFilterGroup.SetActive(false);
        }

        /// <summary>
        /// 開啟分頁面板
        /// </summary>
        private void OpenOtherMenuPanel(bool isDestiny, bool go_first)
        {
            if (isDestiny)
            {
                menuGodToggle.gameObject.SetActive(true);
                menuSoulToggle.gameObject.SetActive(true);
                menuProphetToggle.gameObject.SetActive(true);
                menuFeatherToggle.gameObject.SetActive(true);
                menuGuaranteedToggle.gameObject.SetActive(true);
                menuDivinationToggle.gameObject.SetActive(true);
                menuSeaToggle.gameObject.SetActive(false);
                menuArenaToggle.gameObject.SetActive(false);
                menuGuildToggle.gameObject.SetActive(false);
                menuCasinoToggle.gameObject.SetActive(false);
                menuLevelToggle.gameObject.SetActive(false);

                if (go_first) 
                {
                    menuGodToggle.SetIsOnWithoutNotify(true);
                    StartCoroutine(ClickGodPersonalityButton());
                }
            }
            else
            {
                menuGodToggle.gameObject.SetActive(false);
                menuSoulToggle.gameObject.SetActive(false);
                menuProphetToggle.gameObject.SetActive(false);
                menuFeatherToggle.gameObject.SetActive(false);
                menuGuaranteedToggle.gameObject.SetActive(false);
                menuDivinationToggle.gameObject.SetActive(false);
                menuSeaToggle.gameObject.SetActive(true);
                menuArenaToggle.gameObject.SetActive(true);
                menuGuildToggle.gameObject.SetActive(true);
                menuCasinoToggle.gameObject.SetActive(true);
                menuLevelToggle.gameObject.SetActive(true);

                if (go_first) 
                {
                    menuSeaToggle.SetIsOnWithoutNotify(true);
                    StartCoroutine(ClickSeaButton());
                }
            }
            otherMenuGroup.SetActive(true);
        }
        /// <summary>
        /// 關閉分頁面板
        /// </summary>
        private void CloseOtherMenuPanel()
        {
            otherMenuGroup.SetActive(false);
        }

        /// <summary>
        /// 更新列表
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateProductList()
        {
            btnInfo.gameObject.SetActive(MallPage == ShopMallPage.PlumeShop);

            var data_list = new List<ShopMallProductData>();
            switch (MallPage) 
            {
                case ShopMallPage.Item:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_gold.Values) 
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.GodShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.GodShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.GodPersonality:
                    {
                        if (temp_13403_list.TryGetValue((uint)MallType.Recovery, out var shop_items)) 
                        {
                            foreach (var shop_item in shop_items.item_list)
                            {
                                data_list.Add(new ShopMallProductData(shop_item, MallType.Recovery) 
                                {
                                    HasBuy = shop_item.has_buy
                                });
                            }
                        }
                        var ref_item = JsonObjectTool.ObjectToListList<uint>(LoadResource.Instance.ExchangeData.data_shop_list[(uint)MallType.Recovery + ""].cost_list);
                        refreshButton.InitInfo(ref_item[0][0], ref_item[0][1], LocalizeKey.Const_Refresh_Btn_Text);
                        _refresh_cost = new List<uint>() { ref_item[0][0], ref_item[0][1] };
                        refreshButton.gameObject.SetActive(true);
                    }
                    break;
                case ShopMallPage.HeroHeart:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_herosoul.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.HeroSoulShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.HeroSoulShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Prophet:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_seer.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.Seerpalace) 
                            {
                                HasBuy = Get13401HasBuy(MallType.Seerpalace, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.PlumeShop:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_hero.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.PlumeShop));
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Guaranteed:
                    {
                        var summon_data = LoadResource.Instance.RecruitData.data_partnersummon_data["400"];
                        var cost = JsonObjectTool.ObjectToListList<uint>(summon_data.exchange_once);
                        data_list.Add(new ShopMallProductData(0, 1, cost[0][1], cost[0][1], cost[0][0].ToString(), 400, 23201, limit_vip: 3));
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Divination:
                    {
                        var group = new uint[] { 30000, 10000, 20000, 40000, 50000 };

                        for (int i = 0; i < 5; i++) 
                        {
                            var seerpalace = LoadResource.Instance.RecruitHighData.data_seerpalace_data[group[i].ToString()];
                            var cost = JsonObjectTool.ObjectToListList<long>(seerpalace.item_once);
                            data_list.Add(new ShopMallProductData(0, 1, cost[0][1], cost[0][1], cost[0][0].ToString(), group[i], 23213, limit_vip: 5));
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.SeaChallenge:
                   {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_expediton.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.FriendShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.FriendShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Sports:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_arena.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.ArenaShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.ArenaShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Guild:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_guild.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.UnionShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.UnionShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Casino:
                    {
                        if (temp_13403_list.TryGetValue((uint)MallType.GuessShop, out var shop_items))
                        {
                            foreach (var shop_item in shop_items.item_list)
                            {
                                data_list.Add(new ShopMallProductData(shop_item, MallType.GuessShop) 
                                {
                                    HasBuy = shop_item.has_buy
                                });
                            }
                        }
                        var ref_item = JsonObjectTool.ObjectToListList<uint>(LoadResource.Instance.ExchangeData.data_shop_list[(uint)MallType.GuessShop + ""].cost_list);
                        refreshButton.InitInfo(ref_item[0][0], ref_item[0][1], LocalizeKey.Const_Refresh_Btn_Text);
                        _refresh_cost = new List<uint>() { ref_item[0][0], ref_item[0][1] };
                        refreshButton.gameObject.SetActive(true);
                    }
                    break;
                case ShopMallPage.Level:
                    {
                        foreach (var shop_item in LoadResource.Instance.ExchangeData.data_shop_exchage_elite.Values)
                        {
                            data_list.Add(new ShopMallProductData(shop_item, MallType.EliteShop) 
                            {
                                HasBuy = Get13401HasBuy(MallType.EliteShop, shop_item.id)
                            });
                        }
                        refreshButton.gameObject.SetActive(false);
                    }
                    break;
                case ShopMallPage.Skill:
                    {
                        if (temp_13403_list.TryGetValue((uint)MallType.SkillShop, out var shop_items))
                        {
                            foreach (var shop_item in shop_items.item_list)
                            {
                                data_list.Add(new ShopMallProductData(shop_item, MallType.SkillShop) 
                                {
                                    HasBuy = shop_item.has_buy
                                });
                            }
                        }
                        var ref_item = JsonObjectTool.ObjectToListList<uint>(LoadResource.Instance.ExchangeData.data_shop_list[(uint)MallType.SkillShop + ""].cost_list);
                        refreshButton.InitInfo(ref_item[0][0], ref_item[0][1], LocalizeKey.Const_Refresh_Btn_Text);
                        _refresh_cost = new List<uint>() { ref_item[0][0], ref_item[0][1] };
                        refreshButton.gameObject.SetActive(true);
                    }
                    break;
            }

            yield return null;

            coinIconImg.sprite = LoadResource.Instance.GetItemLittleIcon(data_list[0].PayId);
            uint coin_value = 0;
            if (LoadResource.Instance.ItemData.data_assets_id2label.TryGetValue(data_list[0].PayId.ToString(), out var pay_name))
            {
                coin_value = GetFieldValue(MessageResponseData.Instance.GetRespose<Proto_10302_Response>(10302), pay_name.ToString());
            }
            else 
            {
                coin_value = (uint)MessageResponseData.Instance.GetBagItemAmount(data_list[0].PayId);
            }
            txtMoney3.text = NumberScale.ScaleNumber(coin_value);

            mallShopScrollViewAdapter.SetData(data_list);
        }
        /// <summary>
        /// 取出13401裡的購買數量(配合更新列表)
        /// </summary>
        /// <param name="mallType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private uint Get13401HasBuy(MallType mallType, long id) 
        {
            uint buy = 0;
            var buy_data = temp_13401_list[(byte)mallType]
                .item_list
                .FirstOrDefault(i => i.item_id == id);
            if (buy_data != null)
            {
                //var buy_ext = buy_data.ext.FirstOrDefault(i => i.key == 1);
                //if (buy_ext != null) buy = buy_ext.val;
                buy = buy_data.ext[0].val;
            }

            return buy;
        }

        /// <summary>
        /// 返回MyPage
        /// </summary>
        private void ClickButtonMypage()
        {
            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene);
            });
        }
        /// <summary>
        /// 返回Last場景
        /// </summary>
        private void ClickButtonBack()
        {
            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.BackScene();
            });
        }

        /// <summary>
        /// 顯示商店資訊
        /// </summary>
        private void ClickButtonInfo()
        {
            switch (MallPage)
            {
                case ShopMallPage.PlumeShop:
                    GameObject popup = Instantiate(rulePopup, MainManager.Instance.PopupParent.transform);
                    popup.GetComponent<MallRulePopup>().Init(
                        LocaleManager.Instance.GetLocalizedString(
                            LocaleTableEnum.ResourceData, LoadResource.Instance.HolidayClientData.data_constant["welfare_shop_rules"].desc).Replace("\\n", "\n"));
                    popup.GetComponent<MallRulePopup>().ShowPopup();
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        /// <summary>
        /// Tween過場Fade In 動畫
        /// </summary>
        /// <param name="cb"></param>
        /// <returns></returns>
        public void PlayEnterAnim(Action onComplete = null)
        {
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgCanvasGroup.DOFade(1, 0.2f / speed))
                    .Join(uiCanvasGroup.DOFade(1, 0.2f / speed))
                    .OnComplete(() => onComplete?.Invoke());
        }
        /// <summary>
        /// Tween 過場 Fade Out動畫
        /// </summary>
        /// <param name="cb"></param>
        /// <returns></returns>
        public void PlayExitAnim(Action onComplete = null)
        {
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence
                //.Append(bgCanvasGroup.DOFade(0f, 0.2f / speed))
                //.Join(uiCanvasGroup.DOFade(0f, 0.2f / speed))
                    .OnComplete(() => onComplete?.Invoke());
        }

        uint GetFieldValue(Proto_10302_Response obj, string fieldName)
        {
            Type type = typeof(Proto_10302_Response);
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

            if (field != null)
            {
                return (uint)field.GetValue(obj);
            }

            return 0;
        }

        protected void OnDestroy()
		{
            proto13401_resp.Dispose();
            proto13402_resp.Dispose();
            proto13403_resp.Dispose();
            proto13405_resp.Dispose();
            proto13407_resp.Dispose();
            proto23201_resp.Dispose();
            proto23213_resp.Dispose();
        }
	}
}
