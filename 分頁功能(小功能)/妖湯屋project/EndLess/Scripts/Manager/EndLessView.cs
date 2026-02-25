using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Casino;
using YKO.Common;
using YKO.Common.Data;
using YKO.Common.Font;
using YKO.Common.UI;
using YKO.CONST;
using YKO.LeaderboardEntrance;
using YKO.Main;
using YKO.Network;
using YKO.Support.Expansion;
using YKO.Welfare;

namespace YKO.EndLess
{

    public class EndLessView : MonoBehaviour
    {

        public class SceneParameter
        {
            /// <summary>
            /// 組對面板是否在進入頁面後預設開啟
            /// </summary>
            public bool openDungeonDetailPanel = false;

            public bool enterAttributePanel= false;

        }

        public class LocalizeKey
        {
            public static readonly string EndLessTitle = "無間虛淵";
            #region LeftRank
            public static readonly string MissionRankTitle = "闖關排名";

            public static readonly string NotRankPlayer = "[虛位以待]";
            #endregion

            #region RightNextPreview
            public static readonly string NextPreview = "下期預告";

            #endregion

            #region TabName
            public static readonly string MixTrial = "綜合試煉";

            #endregion
            #region MID
            public static readonly string MyselfRecord= "個人紀錄";

            public static readonly string FirstClearReward = "首通獎勵";

            public static readonly string DailyRewardTitle = "日常獎勵起點";

            public static readonly string FriendHelper = "好友助陣";
            public static readonly string Select_A_FriendHelper = "選擇好友的一位英雄幫助";
            #endregion
            #region Bottom
            public static readonly string MissionCompleteInToday = "今日已通關{0}關";

            public static readonly string StartTheChallengeFromTheLevel = "[從第{0}關開始挑戰]";

            public static readonly string StartChallenge = "開始挑戰";
            /// <summary>
            /// 綜合試煉{0}關後解鎖
            /// </summary>
            public static readonly string OldChallengeUnlock = "綜合試煉{0}關後解鎖";
            #endregion
            #region Popup
            public static readonly string Tip = "提示";

            public static readonly string CompleteChallengeTip = "本日已結算日常通關累積獎勵，不會重複結算，但依然可以獲得首通獎勵和排行榜排名獎勵\n\n是否繼續挑戰";


            #endregion
        }
        #region UI
        #region TopUI
        [Header("TopUI")]
        
        [SerializeField]
        private TopPanel topPanel;
        /// <summary>
        /// 玩法說明按鈕
        /// </summary>
        [SerializeField]
        private Button illustrateBtn;

        /// <summary>
        /// 標題名稱
        /// </summary>
        [SerializeField]
        private Text titleText;
        /// <summary>
        /// 左上排名標題
        /// </summary>
        [SerializeField]
        private Text rankTitle;
        
        /// <summary>
        /// 左上排名資訊
        /// </summary>
        [SerializeField]
        private Text[] rankNameList;
        /// <summary>
        /// 左上排名資訊通過的關卡數
        /// </summary>
        [SerializeField]
        private Text[] rankMissionList;


        /// <summary>
        /// 獎勵詳情按鈕(排行榜
        /// </summary>
        [SerializeField]
        private Button rewardInfoBtn;

        /// <summary>
        /// 排名詳情按鈕(排行榜
        /// </summary>
        [SerializeField]
        private Button rankInfoBtn;
        /// <summary>
        /// 下期預告按鈕
        /// </summary>
        [Header("MidTipUI")]
        [SerializeField]
        private Button nextPreviewBtn;
        /// <summary>
        /// 下期預告屬性圖片
        /// </summary>
        [SerializeField]
        private Image nextPreviewIcon;
        /// <summary>
        /// 下期預告文字
        /// </summary>
        [SerializeField]
        private Text nextPreviewText;
        /// <summary>
        /// 屬性圖
        /// </summary>
        [SerializeField]
        private Sprite[] iconAttributes;

        /// <summary>
        /// 綜合標題
        /// </summary>
        [SerializeField]
        private CustomTabButton comprehensiveTab;
        /// <summary>
        /// 屬性試煉
        /// </summary>
        [SerializeField]
        private CustomTabButton attributeTab;
        #endregion
        #region MidUI
        /// <summary>
        /// 個人紀錄文字
        /// </summary>
        [Header("MidUI")]
        [SerializeField]
        private Text myselfRecond;
        /// <summary>
        /// 當前最高通關
        /// </summary>
        [SerializeField]
        private Text maxStageText;
        /// <summary>
        /// 首通獎勵資訊
        /// </summary>
        [SerializeField]
        private EndLessMissionInfo firstClearInfo;
        /// <summary>
        /// 日常獎勵資訊
        /// </summary>
        [SerializeField]
        private EndLessMissionInfo dailyInfo;


        /// <summary>
        /// 好友助陣標題
        /// </summary>
        [SerializeField]
        private Text friendHelperTitleText;


        /// <summary>
        /// 好友助陣文字(好友助陣
        /// </summary>
        [SerializeField]
        private Text friendHelperText;

        /// <summary>
        /// 英雄頭像展示
        /// </summary>
        [SerializeField]
        private HeroIcon heroIcon;
        /// <summary>
        /// 玩家名稱
        /// </summary>
        [SerializeField]
        private Text playerNameText;
        /// <summary>
        /// 戰力顯示
        /// </summary>
        [SerializeField]
        private Text powerText;

        /// <summary>
        /// 好友支援按鈕
        /// </summary>
        [SerializeField]
        private Button firendSupportBtn;
        #endregion
        #region BottomUI
        /// <summary>
        /// 挑戰提示文字
        /// </summary>
        [Header("BottomUI")]
        [SerializeField]
        private Text bottomTipText;
        /// <summary>
        /// 開始挑戰文字
        /// </summary>
        [SerializeField]
        private Text startChallengeBtnText;

        /// <summary>
        /// 開始挑戰
        /// </summary>
        [SerializeField]
        private Button startChallengeBtn;

        [SerializeField]
        private DungeonDetailView dungeonView;

        /// <summary>
        /// 回上一頁
        /// </summary>
        [SerializeField]
        private Button backBtn;
        /// <summary>
        /// 回首頁
        /// </summary>
        [SerializeField]
        private Button homeBtn;
        /// <summary>
        /// 背景
        /// </summary>
        [Header("Tween")]
        [SerializeField]
        private GameObject background;
        /// <summary>
        /// 左上排行榜
        /// </summary>
        [SerializeField]
        private GameObject topLeftRankPanel;
        /// <summary>
        /// 下期預告群組
        /// </summary>
        [SerializeField]
        private RectTransform nextPreviewGroup;


        #endregion
        #endregion


        #region Param or Prefab[
        [Header("Prefab")]
        [SerializeField]
        private Transform popupParent;

        /// <summary>
        /// 屬性彈窗
        /// </summary>
        [SerializeField]
        private GameObject trialTipPopupPrefab;
        /// <summary>
        /// 獎勵彈窗
        /// </summary>
        [SerializeField]
        private GameObject rewardInfoPopupPrefab;
        /// <summary>
        /// 好友支援彈窗
        /// </summary>
        [SerializeField]
        private GameObject supportPopupPrefab;

        /// <summary>
        /// 兩個選項彈窗
        /// </summary>
        [SerializeField]
        private GameObject twoOptionPopupPrefab;
        /// <summary>
        /// 描述提示
        /// </summary>
        [SerializeField]
        private GameObject illustrateTipPopupPrefab;
        

        /// <summary>
        ///道具的prefab
        /// </summary>
        [SerializeField]
        private GameObject itemIconPrefab;

        private bool firstOpen = true;

        /// <summary>
        /// 當前頁面屬性
        /// </summary>
        private byte pageType;

        public byte PageType { get { return pageType; } }
        /// <summary>
        /// 無間虛淵 controller
        /// </summary>
        private EndLessController manager;

        private SceneParameter mParam = null;

        #endregion



        public IEnumerator Init(EndLessController _manager)
        {
            manager = _manager;
            yield return UpdateUI();
            Register();
            if (firstOpen)
            {
                topPanel.Init();

                if (GameSceneManager.Instance.GetSceneParam()!=null&&GameSceneManager.Instance.GetSceneParam().GetType() == typeof(SceneParameter)) 
                {
                    mParam = GameSceneManager.Instance.GetSceneParam() as SceneParameter;
                }

                if (mParam!=null&& mParam.openDungeonDetailPanel)
                {
                    //開啟下方面板
                    StartCoroutine( OpenDungeonView());
                    if (mParam.enterAttributePanel)
                    {
                        attributeTab.isOn = true;
                    }
                    else
                    {
                        comprehensiveTab.isOn = true;
                    }
                }
                firstOpen = false;
                PlayEnterAnim();
            }
        }
        /// <summary>
        /// 註冊按鈕
        /// </summary>
        private void Register()
        {

            MessageResponseData.Instance.SubscribeLive<Proto_23913_Response>(23913,
                meg => {
                    //回傳顯示的資料是我派出去的支援
                    var proto23900 = manager.proto23900;
                    var data=JsonConvert.SerializeObject(meg.rank_list);
                    if (pageType==5) 
                    {
                        proto23900.rank_list = JsonConvert.DeserializeObject<Proto_23900_Response.Rank_list[]>(data);
                    }
                    else
                    {
                        proto23900.new_rank_list = JsonConvert.DeserializeObject<Proto_23900_Response.New_rank_list[]>(data);
                    }
                    RefreshLeftRankBoard(pageType);

                }, false, this);


            nextPreviewBtn.onClick.RemoveAllListeners();
            rewardInfoBtn.onClick.RemoveAllListeners();
            rankInfoBtn.onClick.RemoveAllListeners();
            startChallengeBtn.onClick.RemoveAllListeners();
            illustrateBtn.onClick.RemoveAllListeners();
            backBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.RemoveAllListeners();
            firendSupportBtn.onClick.RemoveAllListeners();

            comprehensiveTab.OnValueChangedAsObservable().Where(on => on).Subscribe(_ => {
                StartCoroutine(UpdateUI(manager.proto23900.type));
            }).AddTo(this);

            attributeTab.OnValueChangedAsObservable().Where(on => on).Subscribe(_ => {
                StartCoroutine(UpdateUI(manager.proto23900.select_type));
            }).AddTo(this);

            nextPreviewBtn.OnClickAsObservable().Subscribe(_=>OpenTrialTipPopup()).AddTo(this);
            rewardInfoBtn.OnClickAsObservable().Subscribe(_ => OpenLeaderBoard()).AddTo(this);
            rankInfoBtn.OnClickAsObservable().Subscribe(_ => OpenLeaderBoard()).AddTo(this);
            backBtn.OnClickAsObservable().Subscribe(_ => OnBackButton());
            homeBtn.OnClickAsObservable().Subscribe(_ => OnHomeButton());
            startChallengeBtn.OnClickAsObservable().Subscribe(_ => {StartCoroutine( OpenDungeonView());}).AddTo(this);
            illustrateBtn.OnClickAsObservable().Subscribe(_ => OpenIllustratePopup()).AddTo(this);
            firendSupportBtn.OnClickAsObservable().Subscribe(_ => OpenSupportPanel()).AddTo(this);

        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private IEnumerator UpdateUI(byte missionType=5)
        {
            pageType=missionType;
            RefreshLeftRankBoard(missionType);
            UpdateFriendHelper();
            titleText.text = LocalizeKey.EndLessTitle;
            nextPreviewText.text = LocalizeKey.NextPreview;
            comprehensiveTab.titleName = LocalizeKey.MixTrial;
            myselfRecond.text = LocalizeKey.MyselfRecord;
            friendHelperTitleText.text= LocalizeKey.FriendHelper;
            friendHelperText.text = LocalizeKey.FriendHelper;

            var proto23900 = manager.proto23900;
            var nextType = proto23900.next_type;
            var curType = proto23900.select_type;
            
            nextPreviewIcon.sprite = iconAttributes[(nextType-1)];
            attributeTab.titleName=LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData,  LoadResource.Instance.EndlessData.data_new_type[curType.ToString()].title);


            Proto_23903_Response tarProto23903 = null;
            yield return manager.GetProto23903((byte)pageType,( proto,haveData) => { tarProto23903 = proto; });
            var firstFixedData = LoadResource.Instance.EndlessData.data_first_data[pageType.ToString()];

            firstClearInfo.Init(new EndLessMissionInfo.EndLessMissionInfoFunc()
            {
                title = LocalizeKey.FirstClearReward,
                itemInfos= JsonConvert.DeserializeObject<uint[][]>(firstFixedData[tarProto23903.id.ToString()].items.ToString()),
                status =tarProto23903.status,
                tarCompleteMission = (int)firstFixedData[tarProto23903.id.ToString()].limit_id,
                tarMissionGap= (int)(firstFixedData[tarProto23903.id.ToString()].limit_id - tarProto23903.max_id),
                rewardInfoCB = () => {
                    OpenRewardPopup(tarProto23903.type);
                //打開獎勵列表彈窗
                },
                receiveRewardCB = () => {
                    manager.SendProto23904(tarProto23903.type,tarProto23903.id);
                //接收獎勵
                }
            });
            //  56     42   40    24  34
            var dailyFixedData = LoadResource.Instance.EndlessData.data_floor_data[pageType.ToString()];
            switch (pageType)
            {
                case 5:
                    maxStageText.text = "Stage  " +proto23900.max_round;

                    dailyInfo.Init(
                        new EndLessMissionInfo.EndLessMissionInfoFunc()
                        {
                            title = LocalizeKey.DailyRewardTitle,
                            itemInfos = JsonConvert.DeserializeObject<uint[][]>(dailyFixedData[proto23900.current_round.ToString()].items.ToString()),
                            status=(uint)(proto23900.is_reward>0?3:2),
                            tarCompleteMission=proto23900.current_round,
                            rewardInfoCB = () => {
                                OpenRewardPopup( EndLessRewardPopup.eEndLessRewardPopupPage.Daily);
                                //打開獎勵列表彈窗
                            }
                        });
                    bottomTipText.text = string.Format(LocalizeKey.MissionCompleteInToday, proto23900.day_pass_round);
                    startChallengeBtn.interactable = true;
                    if (proto23900.current_round>1) 
                    {
                        startChallengeBtnText.text = string.Format(LocalizeKey.StartTheChallengeFromTheLevel, proto23900.current_round);
                    }
                    else
                    {
                        startChallengeBtnText.text = LocalizeKey.StartChallenge;
                    }


                    break;
                default:
                    maxStageText.text = "Stage  " +proto23900.new_max_round.ToString();

                    dailyInfo.Init(
                        new EndLessMissionInfo.EndLessMissionInfoFunc()
                        {
                            title = LocalizeKey.DailyRewardTitle,
                            itemInfos = JsonConvert.DeserializeObject<uint[][]>(dailyFixedData[proto23900.new_current_round.ToString()].items.ToString()),
                            status = (uint)(proto23900.is_reward> 0 ? 3 : 2),
                            tarCompleteMission = proto23900.new_current_round,
                            rewardInfoCB = () => {
                                OpenRewardPopup(EndLessRewardPopup.eEndLessRewardPopupPage.Daily);
                            }
                        });

                    if (proto23900.max_round >= 200)
                    {
                        bottomTipText.text = string.Format(LocalizeKey.MissionCompleteInToday, proto23900.new_day_pass_round);
                        startChallengeBtn.interactable = true;
                    }
                    else
                    {
                        bottomTipText.text = string.Format(LocalizeKey.OldChallengeUnlock, 200);
                        startChallengeBtn.interactable = false;
                    }
                    if (proto23900.new_current_round > 1)
                    {
                        startChallengeBtnText.text = string.Format(LocalizeKey.StartTheChallengeFromTheLevel, proto23900.new_current_round);
                    }
                    else
                    {
                        startChallengeBtnText.text = LocalizeKey.StartChallenge;
                        
                    }


                    break;


            }
           // attributeTab.titleName
           //  var 

        }
        /// <summary>
        /// 如果顯示頁面跟更新proto相同,更新頁面
        /// </summary>
        /// <param name="type"></param>
        public void JudgeUpdate(byte type)
        {
            if (pageType==type) 
            {
                StartCoroutine(UpdateUI(type));
            }

        }
        /// <summary>
        /// 更新下方英雄顯示UI
        /// </summary>
        public void UpdateFriendHelper()
        {
            Debug.Log("更新好友支援");
            var proto23900=manager.proto23900;

            if (proto23900.list.Length>0) {
                Debug.Log("有");
                var heroInfo = proto23900.list[0];
                heroIcon.gameObject.SetActive(true);
                powerText.transform.parent.gameObject.SetActive(true);
                heroIcon.InitHero(heroInfo.bid, heroInfo.star)
                        .SetHeroLevel(heroInfo.lev);
                powerText.text = heroInfo.power.ToString();
                playerNameText.text = manager.proto23907.list.FirstOrDefault(v => v.id == heroInfo.id).name;
            }
            else
            {
                Debug.Log("沒有");
                heroIcon.gameObject.SetActive(false);
                powerText.transform.parent.gameObject.SetActive(false);
                playerNameText.text = LocalizeKey.Select_A_FriendHelper;
            }
            

        }


        /// <summary>
        /// 更新左上 闖關排名
        /// </summary>
        private void RefreshLeftRankBoard(byte type)
        {
            rankTitle.text = LocalizeKey.MissionRankTitle;
            switch (type) 
            {
                case 5:
            var rankList = manager.proto23900.rank_list;
            for (int i = 0; i < rankNameList.Length; i++)
            {
                var rankNameText = rankNameList[i];
                var rankMission = rankMissionList[i];
                Proto_23900_Response.Rank_list rankInfo =
                    rankList.Any(v => v.idx == i) ?
                    rankList.FirstOrDefault(v => v.idx == i) :
                    null;
                if (rankInfo != null)
                {
                    rankNameText.text = rankInfo.name;
                    rankMission.text = string.Format("[{0}]", rankInfo.val);
                }
                else
                {
                    rankNameText.text = LocalizeKey.NotRankPlayer;
                    rankMission.text = "[--]";
                }
            }
                    break;

                default:
                    var newRankList = manager.proto23900.new_rank_list;
                    for (int i = 0; i < rankNameList.Length; i++)
                    {
                        var rankNameText = rankNameList[i];
                        var rankMission = rankMissionList[i];
                        Proto_23900_Response.New_rank_list newRankInfo =
                            newRankList.Any(v => v.idx == i) ?
                            newRankList.FirstOrDefault(v => v.idx == i) :
                            null;
                        if (newRankInfo != null)
                        {
                            rankNameText.text = newRankInfo.name;
                            rankMission.text = string.Format("[{0}]", newRankInfo.val);
                        }
                        else
                        {
                            rankNameText.text = LocalizeKey.NotRankPlayer;
                            rankMission.text = "[--]";
                        }
                    }


                    break;
        }


        }


        /// <summary>
        /// 打開好友支援面板
        /// </summary>
        private void OpenSupportPanel()
        {
            var sp = Instantiate(supportPopupPrefab, popupParent);
            sp.GetComponent<EndLessSupportPopup>().Init(manager);
        }

        /// <summary>
        /// 打開戰鬥組隊狀況面板
        /// </summary>
        private IEnumerator OpenDungeonView()
        {
            manager.SendProto11211((byte)GetForm());
            manager.SendProto26555((byte)GetForm());
            uint waitProto = 0;

            MessageResponseData.Instance.SubscribeOnce<Proto_11211_Response>(11211,
                proto => {
                    waitProto++;
            },this);

            MessageResponseData.Instance.SubscribeOnce<Proto_26555_Response>(26555,
                proto => {
                    waitProto++;
                }, this);


            yield return new WaitUntil(() => waitProto >= 2);

            dungeonView.Init(
                new DungeonDetailView.DungeonDetailViewFunc()
                {
                    endLessView = this,
                    startChallengeCB = ClickStartChallenge
                }
             );


        }
        /// <summary>
        /// 點擊開始挑戰
        /// </summary>
        /// <param name="pos"></param>
        private void ClickStartChallenge(List<FormData.PosInfo>pos,UInt16 formation,uint hollow)
        {
            bool isattribute = pageType==5?false:true;
            void StartChallenge()
            {
                BattleManager.Instance.EndLessBattleStart(this, () => {
                    PlayExitAnim(() =>
                    {
                        GameSceneManager.Instance.SetCurSceneRecord(new SceneParameter() {  enterAttributePanel=isattribute});
                        GameSceneManager.Instance.AddScene(
                            SceneConst.SceneName.BattleScene,
                               new Battle.BattleScene.SceneParameter()
                            );
                    });
                });

                manager.SendProto23901(
                 pageType,
                 formation,
                 hollow,
                 pos
                 );
            }

            switch (pageType)
            {
                case 5:
                    if (manager.proto23900.is_reward>=1)
                    {
                        MainManager.Instance.HideLoading();
                        OpenTipPopup(() => {
                            //開啟組隊面板
                            StartChallenge();
                        });
                    }
                    else
                    {
                        StartChallenge();
                    }
                    break;
                default:
                    if (manager.proto23900.is_reward >= 1)
                    {
                        MainManager.Instance.HideLoading();
                        OpenTipPopup(() => {

                            StartChallenge();
                        });

                    }
                    else
                    {
                        StartChallenge();
                    }
                    break;
            }
        }

        /// <summary>
        /// 獎勵彈窗
        /// </summary>
        /// <param name="page"></param>
        private void OpenRewardPopup(EndLessRewardPopup.eEndLessRewardPopupPage page) {

            OpenRewardPopup((byte)page);
        
        }
        /// <summary>
        /// 開啟獎勵資訊彈窗
        /// </summary>
        private void OpenRewardPopup(byte type)
        {
            var proto23900 = manager.proto23900;
            var sp = Instantiate(rewardInfoPopupPrefab, popupParent);
            switch (type)
            {
                case 0://前往日常
                   StartCoroutine( sp.GetComponent<EndLessRewardPopup>().Init(
                        new EndLessRewardPopup.EndLessRewardPopupFunc()
                        {
                            type = pageType,
                            curentRound = proto23900.current_round,
                            speciftyPage = EndLessRewardPopup.eEndLessRewardPopupPage.Daily,
                            receiveReward = manager.SendProto23904,
                            getProto23903 = manager.GetProto23903
                        }
                      ));
                    break;
                case 5:     //前往基礎的首通
                    StartCoroutine(sp.GetComponent<EndLessRewardPopup>().Init(
                        new EndLessRewardPopup.EndLessRewardPopupFunc()
                        {
                            type = type,
                            curentRound = proto23900.current_round,
                            speciftyPage = EndLessRewardPopup.eEndLessRewardPopupPage.FirstClearOld,
                            receiveReward = manager.SendProto23904,
                            getProto23903=manager.GetProto23903
                        }
                      ));
                    break;
                default://前往其他的首通
                    StartCoroutine(sp.GetComponent<EndLessRewardPopup>().Init(
                        new EndLessRewardPopup.EndLessRewardPopupFunc()
                        {
                            type = type,
                            curentRound = proto23900.new_current_round,
                            speciftyPage = (EndLessRewardPopup.eEndLessRewardPopupPage)Enum.ToObject(typeof(EndLessRewardPopup.eEndLessRewardPopupPage),type),
                            receiveReward = manager.SendProto23904,
                            getProto23903 = manager.GetProto23903
                        }
                      ));
                    break;
            }

        }


        /// <summary>
        /// 打開排行榜
        /// </summary>
        private void OpenLeaderBoard()
        {
            //MainManager.Instance.ShowLoading(true);
            LeaderboardEntranceView.LeaderboardType leaderType = LeaderboardEntranceView.LeaderboardType.Endless;
            switch (pageType)
            {
                case 1:
                    leaderType = LeaderboardEntranceView.LeaderboardType.Endless_water;
                    break;
                case 2:
                    leaderType = LeaderboardEntranceView.LeaderboardType.Endless_fire;
                    break;
                case 3:
                    leaderType = LeaderboardEntranceView.LeaderboardType.Endless_wind;
                    break;
                case 4:
                    leaderType = LeaderboardEntranceView.LeaderboardType.Endless_lightdark;
                    break;
                case 5:
                    leaderType = LeaderboardEntranceView.LeaderboardType.Endless;
                    break;
            }

                LeaderboardEntranceView.SceneParameter leaderboardEntranceSceneParameter = new LeaderboardEntranceView.SceneParameter
                {
                    entranceMode = LeaderboardEntranceView.EntranceMode.OtherPage,
                    leaderboardType = leaderType
                };

            GameSceneManager.Instance.AddScene(SceneConst.SceneName.LeaderboardEntranceScene, leaderboardEntranceSceneParameter);

        }

        /// <summary>
        /// 開啟下期屬性視窗
        /// </summary>
        private void OpenTrialTipPopup()
        {
            
            var type = manager.proto23900.next_type;
            var fixedData = LoadResource.Instance.EndlessData.data_new_type[type.ToString()];
            var sp = Instantiate(trialTipPopupPrefab, popupParent);
            sp.GetComponent<TrialTipPopup>().Init(new TrialTipPopup.TrialTipPopupData()
            {
                typeSprite = iconAttributes[(type - 1)],
                title =LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData,fixedData.title),
                descript = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fixedData.describe),
                time = manager.proto23900.next_time
            });
        }

        /// <summary>
        /// 生成提示雙選項視窗
        /// </summary>
        private void OpenTipPopup(Action clickConfirmFunc)
        {
            var popup = Instantiate(twoOptionPopupPrefab,popupParent);
            popup.GetComponent<TipTwoOptionsPopup>().Init(
                new TipTwoOptionsPopup.TipTwoOptionsPopupFunc()
                    { 
                        title=LocalizeKey.Tip,
                        content=LocalizeKey.CompleteChallengeTip,
                         clickConfirmBtn = clickConfirmFunc
                    }
                );

        }
        /// <summary>
        /// 開啟說明視窗
        /// </summary>
        private void OpenIllustratePopup()
        {
            var fd = LoadResource.Instance.EndlessData.data_explain["1"];

            var popup = Instantiate(illustrateTipPopupPrefab, popupParent);

            string descript = 
                LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd["1"].title) + "\n\n" +
                LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd["1"].desc) + "\n\n" +
                LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd["2"].title )+ "\n\n" +
                LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd["2"].desc);

            popup.GetComponent<CasinoillustratePopup>().Init(new CasinoillustratePopup.CasinoillustratePopupData(
                ConvertForShinyOfLight.ConvertStringToUnityEditorDisplay(descript)
                ));
            popup.GetComponent<CasinoillustratePopup>().ShowPopup();
        }


        private void OnHomeButton()
        {
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene);
            });
        }

        private void OnBackButton()
        {
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.BackScene();
            });
        }


        /// <summary>
        /// 返回戰鬥類型
        /// </summary>
        /// <returns></returns>
        public PartnerConst.Fun_Form GetForm()
        {
            switch (PageType)
            {
                case 1:
                    return PartnerConst.Fun_Form.EndLessWater;
                case 2:
                    return  PartnerConst.Fun_Form.EndLessFire;
                case 3:
                    return PartnerConst.Fun_Form.EndLessWind;
                case 4:
                    return PartnerConst.Fun_Form.EndLessLightDark;
                case 5:
                    return PartnerConst.Fun_Form.EndLess;
                default:
                    return PartnerConst.Fun_Form.EndLess;

            }
        }

        /// <summary>
        /// 入場動畫
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayEnterAnim(Action onComplete = null)
        {
            float tweenDurationTime = 0.5f;
            onComplete?.Invoke();
            background.SetCanvasGroup(0);
            StartCoroutine( background.eSetCanvasGroup(1,duration: tweenDurationTime));
            var rectRankPanel = topLeftRankPanel.GetComponent<RectTransform>();

            rectRankPanel.anchoredPosition = new Vector2(-250, 424);
            rectRankPanel.DOAnchorPosX(240, tweenDurationTime);

            nextPreviewGroup.sizeDelta = Vector2.zero;

            nextPreviewGroup.DOSizeDelta(new Vector2(150, 150), tweenDurationTime);

            var bottomTipRect = bottomTipText.GetComponent<RectTransform>();
            bottomTipRect.anchoredPosition = new Vector2(470, 53);
            var startChallengeRect=startChallengeBtn.GetComponent<RectTransform>();
            startChallengeRect.anchoredPosition = new Vector2(470, -13);

            bottomTipRect.DOAnchorPosX(10, tweenDurationTime);
            startChallengeRect.DOAnchorPosX(0, tweenDurationTime);

            /*var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgCanvasGroup.DOFade(1, 0.2f / speed))
                    .Join(uiCanvasGroup.DOFade(1, 0.2f / speed))
                    .OnComplete(() => onComplete?.Invoke());*/
            GameSceneManager.Instance?.FinishLoadScene();
        }

        /// <summary>
        /// 淡出動畫
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayExitAnim(Action onComplete = null)
        {
            onComplete?.Invoke();
          /*  _isReady = false;
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgCanvasGroup.DOFade(0f, 0.2f / speed))
                    .Join(uiCanvasGroup.DOFade(0f, 0.2f / speed))
                    .OnComplete(() => onComplete?.Invoke());*/
        }

        /// <summary>
        /// 無盡挑戰的英雄資訊(因為有好友英雄,所以需要另外紀錄擁有者id
        /// </summary>
        public class Pos_info
        {
            public byte pos; //位置
            public uint owner_id; //拥有者的id
            public string owner_srv_id; //服务器id
            public uint id; //伙伴id
        }

    }
}
