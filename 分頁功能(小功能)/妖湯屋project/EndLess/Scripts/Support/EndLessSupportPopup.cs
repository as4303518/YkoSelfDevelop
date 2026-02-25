using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;
using YKO.DataModel;
using YKO.Main;
using YKO.Network;
using YKO.Support.Expansion;

namespace YKO.EndLess
{
    public class EndLessSupportPopup : PopupBase
    {
        public class LocalizeKey
        {
            public static readonly string FriendSupport = "好友支援";
            public static readonly string MySupport = "我的支援";
            public static readonly string CurrentSupport = "當前支援";
            public static readonly string BorrowHero= "可借英雄";
            public static readonly string NotDispatch = "還未派遣好友英雄,選擇英雄幫助好友吧";
            public static readonly string NotSelectMoreThan120PercentPowerSupportOfHero= "無法選擇超出自身命介120%戰力的命介作為支援";
            public static readonly string DispatchSupportHeroGetRewardInEveryDay = "每日派遣支援英雄可獲得獎勵,英雄被選擇自己將獲得友情點";
            public static readonly string IsChooseHeroInTodayPleaseOperateAgainAtTomorrow = "今天已选择过英雄了哦！请明天再来更换";
        }
        [Header("TopUI")]
        [SerializeField]
        private Text titleText;

        [SerializeField]
        private CustomTabButton friendSupportTab;

        [SerializeField]
        private CustomTabButton mySupportTab;
        /// <summary>
        /// 右上關閉按鈕
        /// </summary>
        [SerializeField]
        private Button closeBtn;

        [Header("firendSupport")]
        [SerializeField]
        private GameObject friendSupportPage;

        /// <summary>
        /// 當前支援title
        /// </summary>
        [SerializeField]
        private Text friendSupportBorrowHeroTitleText;
        /// <summary>
        /// 當前選擇英雄cell的母物件
        /// </summary>
        [SerializeField]
        private Transform friendSupportBorrowHeroContent;
        /// <summary>
        /// 好友支援下方提示
        /// </summary>
        [SerializeField]
        private Text friendSupportBottomTipText;


        [Header("mySupport")]
        [SerializeField]
        private GameObject mySupportPage;

        /// <summary>
        /// 當前支援title
        /// </summary>
        [SerializeField]
        private Text selectHeroTitleText;
        /// <summary>
        /// 當前選擇英雄cell的母物件
        /// </summary>
        [SerializeField]
        private Transform selectHeroContent;
        /// <summary>
        /// 尚未選擇英雄的提示
        /// </summary>
        [SerializeField]
        private GameObject NotYetDispatchTip;
        /// <summary>
        /// 尚未選擇英雄的提示文字
        /// </summary>
        [SerializeField]
        private Text NotYetDispatchText;

        /// <summary>
        /// 出借英雄title
        /// </summary>
        [SerializeField]
        private Text mBorrowHeroTitleText;
        /// <summary>
        /// 當前選擇英雄cell的母物件
        /// </summary>
        [SerializeField]
        private Transform mBorrowHeroContent;

        /// <summary>
        /// 我的支援下方提示文字
        /// </summary>
        [SerializeField]
        private Text mBottomTipText;


        [Header("Prefab ")]
        [SerializeField]
        private GameObject supportCellPrefab;

        private EndLessController manager;

        private List<EndLessSupportCell> friendSupportCellList = new List<EndLessSupportCell>();

        private SupportPage curPage = SupportPage.FirendSupport;

        /// <summary>
        /// 偵測是否開啟對應的頁面
        /// </summary>
        private bool isOpen=false;

        private bool isFirst = true;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(EndLessController _manager)
        {
            manager = _manager;
            base.Init();
            UpdateUI();
            Register();
            if (isFirst) 
            {
                isFirst = false;
                manager.SendProto23907();
            }
            ShowPopup();

        }
        /// <summary>
        /// 註冊按鈕
        /// </summary>
        private void Register()
        {

            MessageResponseData.Instance.SubscribeLive<Proto_23900_Response>(23900,
                meg => {

                    // UpdateFriendSupportPage(meg);
                    foreach (var cell in friendSupportCellList)
                    {
                        if (cell.IsUnSelect()) 
                        {
                            continue;
                        }
                        if (meg.list.Any(v=>v.id==cell.aData.id)) 
                        {
                            cell.ChangeCellStatus(true);
                        }
                        else
                        {
                            cell.ChangeCellStatus(false);
                        }
                    }
                }, false, this);

            MessageResponseData.Instance.SubscribeLive<Proto_23905_Response>(23905,
                meg => {
                    //更新我的支援
                    Debug.Log("透過proto");
                        UpdateMySupportPage(meg);

                }, false, this);

            MessageResponseData.Instance.SubscribeLive<Proto_23907_Response>(23907,
                meg => {

                       UpdateFriendSupportPage(meg);

                }, false, this);


            /*  MessageResponseData.Instance.SubscribeLive<Proto_23908_Response>(23908,
                  meg => {

                      UpdateFriendSupportPage(meg);

                  }, false, this);*/


            closeBtn.OnClickAsObservable().Subscribe(_ => ClosePopup());

            friendSupportTab.OnValueChangedAsObservable().Where(on => on)
                .Subscribe(_ =>StartCoroutine( OpenFriendSupportPage()));
            mySupportTab.OnValueChangedAsObservable().Where(on => on)
                .Subscribe(_ => StartCoroutine(OpenMySupportPage()));


        }
        /// <summary>
        /// 更新UI顯示
        /// </summary>
        private void UpdateUI()
        {
            titleText.text = LocalizeKey.FriendSupport;
            friendSupportTab.titleName=LocalizeKey.FriendSupport;
            mySupportTab.titleName = LocalizeKey.MySupport;
            friendSupportBorrowHeroTitleText.text = LocalizeKey.BorrowHero;
            friendSupportBottomTipText.text = LocalizeKey.NotSelectMoreThan120PercentPowerSupportOfHero;
            selectHeroTitleText.text = LocalizeKey.CurrentSupport;
            NotYetDispatchText.text = LocalizeKey.NotDispatch;
            mBorrowHeroTitleText.text= LocalizeKey.BorrowHero;
            mBottomTipText.text = LocalizeKey.DispatchSupportHeroGetRewardInEveryDay;
            friendSupportTab.isOn = true;
        }
        /// <summary>
        /// 開啟好友支援頁
        /// </summary>
        private IEnumerator OpenFriendSupportPage()
        {
            curPage = SupportPage.FirendSupport;
            yield return new WaitUntil(() => manager.proto23907 != null);
            friendSupportPage.gameObject.SetActive(true);
            mySupportPage.gameObject.SetActive(false);
            UpdateFriendSupportPage(manager.proto23907);


        }

        /// <summary>
        /// 更新好友支援頁面
        /// </summary>
        private void UpdateFriendSupportPage(Proto_23907_Response proto23907)
        {
            friendSupportBorrowHeroContent.ClearChildObj();
            friendSupportCellList.Clear();
            var myMaxPower = BattleManager.Instance.HeroList.Values.ToList();
            myMaxPower.Sort((a, b) =>
            {
                if (a.power < b.power)//是以a的移動為主
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });

            foreach (var info in proto23907.list) 
            {
                bool isSelect = manager.proto23900.list.Any(v => v.id == info.id);
                var sp = Instantiate(supportCellPrefab, friendSupportBorrowHeroContent);
                var cell = sp.GetComponent<EndLessSupportCell>();
                cell.Init(
                    new EndLessSupportCell.EndLessSupportCellFunc() 
                     {
                              id=info.id,
                              playerName=info.name,
                              power=info.power,
                              bid=info.bid,
                              lev=info.lev,
                              star=info.star,
                             selfMaxPower = myMaxPower[0].power,
                             isSelect=isSelect,
                             selectCB=()=> {
                                 if (manager.proto23900.list.Length>0) 
                                 {
                                     var tar = proto23907.list.FirstOrDefault(v=>v.id== manager.proto23900.list[0].id) ;
                                     manager.SendProto23909(
                                         new Proto_23909_Request()
                                         {
                                             rid = tar.rid,
                                             srv_id = tar.srv_id,
                                             id = tar.id,
                                             flag = 0
                                         }
                                       );
                                 }
                                 manager.SendProto23909(
                                     new Proto_23909_Request()
                                     {
                                         rid = info.rid,
                                         srv_id = info.srv_id,
                                         id = info.id,
                                         flag = 1
                                     }
                                   ); 

                             },
                             cancelSelectCB = () => {

                            manager.SendProto23909(
                                new Proto_23909_Request()
                                {
                                    rid = info.rid,
                                    srv_id = info.srv_id,
                                    id = info.id,
                                    flag = 0
                                }
                              );
                        },
                    });
            friendSupportCellList.Add(cell);
            }


        }


        /// <summary>
        /// 開啟我方支援頁
        /// </summary>
        private IEnumerator OpenMySupportPage()
        {
            curPage = SupportPage.MySupport;
            if (manager.proto23905==null) 
            {
                manager.SendProto23905();
            }
            yield return new WaitUntil(() => manager.proto23905 != null);
            friendSupportPage.gameObject.SetActive(false);
            mySupportPage.gameObject.SetActive(true);
            UpdateMySupportPage(manager.proto23905);
            //已經有選當前英雄 則被選擇的英雄則要切換生成cell的按鈕,並且顯示在選擇上
        }

        /// <summary>
        /// 開啟我方支援頁
        /// </summary>
        private void UpdateMySupportPage(Proto_23905_Response proto23905)
        {

            selectHeroContent.ClearChildObj();
            mBorrowHeroContent.ClearChildObj();

            var myMaxPower = BattleManager.Instance.HeroList.Values.ToList();
            //myMaxPower.LargeSizeSort("power");

            myMaxPower.Sort((a, b) =>
            {
                if (a.power<b.power)//是以a的移動為主
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });

            if (proto23905.list.Length>0)
            {
                foreach (var info in proto23905.list)
                {
                    var sp = Instantiate(supportCellPrefab, selectHeroContent);

                    sp.GetComponent<EndLessSupportCell>().Init(
                        new EndLessSupportCell.EndLessSupportCellFunc()
                        {
                            id = info.id,
                            playerName = BattleManager.Instance.HeroList[info.id].name,
                            power = info.power,
                            bid = info.bid,
                            lev = info.lev,
                            star = info.star,
                            selfMaxPower = myMaxPower[0].power,
                            supportExhibit = true
                        }
                     );
                }
                NotYetDispatchTip.gameObject.SetActive(false);
            }
            else
            {
                NotYetDispatchTip.gameObject.SetActive(true);
            }

            var heroList = BattleManager.Instance.HeroList.Values.ToList();
            //heroList.LargeSizeSort("power");
            heroList.Sort((a, b) =>
            {
                if (a.power < b.power)//是以a的移動為主
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });

            foreach (var info in heroList) 
            {
                if (proto23905.list.Any(v=>v.id==info.partner_id))
                {
                    continue;
                }
                else
                {
                    var sp = Instantiate(supportCellPrefab, mBorrowHeroContent);
                    var cell = sp.GetComponent<EndLessSupportCell>();
                    cell.Init(
                        new EndLessSupportCell.EndLessSupportCellFunc()
                        {
                            id = info.partner_id,
                            playerName = BattleManager.Instance.HeroList[info.partner_id].name,
                            power = info.power,
                            bid = info.bid,
                            lev = info.lev,
                            star = info.star,
                            selfMaxPower = myMaxPower[0].power,
                            selectCB = () => {
                                if (proto23905.list.Length > 0)
                                {
                                    MainManager.Instance.AddRemind(LocalizeKey.IsChooseHeroInTodayPleaseOperateAgainAtTomorrow);
                                }
                                else
                                {
                                    manager.SendProto23908(info.partner_id);
                                }
                            }
                        }
                     );
                }
            }


        }

            //已經有選當前英雄 則被選擇的英雄則要切換生成cell的按鈕,並且顯示在選擇上
        }


        /// <summary>
        /// 開啟頁面
        /// </summary>
        public enum SupportPage
        {
            FirendSupport,
            MySupport
        }




    }


