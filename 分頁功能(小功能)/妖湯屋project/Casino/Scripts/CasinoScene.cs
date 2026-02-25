using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using YKO.Common.UI;
using YKO.Main;
using System;
using DG.Tweening;
using YKO.Network;
using YKO.Common;
using System.Linq;
using YKO.Common.Util;
using YKO.Common.Sound;
using YKO.Support;
using YKO.MallShop;

namespace YKO.Casino
{
    public class CasinoScene : MonoBehaviour
    {
        /// <summary>
        /// 多語言
        /// </summary>
          public class LocalizeKey{
            public static readonly string LotteryConfirmText = "再抽1次";
            public static readonly string LotteryOfTenConfirmText = "再抽15次";
            public static readonly string LotteryCloseText = "確認";

            public static readonly string LimitLevel30 = "等級30開啟";
            public static readonly string LimitLevel = "等級{0}解鎖";
            public static readonly string LimitVIP = "vip等級{0}解鎖";
        }




        [Header("Anime")]
        [SerializeField]
        private CanvasGroup bgCanvasGroup;
        [SerializeField]
        private CanvasGroup uiCanvasGroup;

        [Header("Base")]
        [SerializeField]
        private Button btnBack = default;
        [SerializeField]
        private Button btnHome = default;
        [SerializeField]
        private TopPanel topPanel;
        [SerializeField]
        private GameObject popupParent;

        [Header("Accumulated")]
        [SerializeField]
        private Text accumulatedValueText;
        /// <summary>
        /// 顯示幸運值的底層圖片
        /// </summary>
        [SerializeField]
        private Image luckValueBgImg;
        [SerializeField]
        private Image accumulatedValueBar;
        [SerializeField]
        private CasinoItemCell[] accumulatedAwards;

        [Header("Turntable")]
        [SerializeField]
        private CasinoItemCell[] turntableItems;
        [SerializeField]
        private Button rateButton;
        [SerializeField]
        private GameObject RateMaskImg;

        [SerializeField]
        private GameObject block;
        [SerializeField]
        private GameObject spotlight;
        /// <summary>
        /// 輪盤圖片
        /// </summary>
        [SerializeField]
        private Image turntableBgImg;

        [Header("Other")]
        [SerializeField]
        private CustomTabButton normalTab;
        [SerializeField]
        private GameObject normalTabRedPoint;

        [SerializeField]
        private CustomTabButton highTab;
        [SerializeField]
        private GameObject highTabRedPoint;
        /// <summary>
        /// tab未過條件,遮擋並觸發彈窗的button
        /// </summary>
        [SerializeField]
        private Button limitLevelMaskBtn;

        [SerializeField]
        private Button shopButton;
        [SerializeField]
        private Button tipsButton;
        [SerializeField]
        private Text haveItemText;
        [SerializeField]
        private Button recordButton;
        [SerializeField]
        private Button onceButton;
        [SerializeField]
        private Button moreButton;
        [SerializeField]
        private Button refreshButton;
        [SerializeField]
        private GameObject freeText;
        [SerializeField]
        private GameObject payText;

        [SerializeField]
        private Text freeRefreshTimeText;
        /// <summary>
        /// 貨幣 icon(切換type時一次轉換 
        /// </summary>
        [SerializeField]
        private Image[] currencyImg;

        [Header("Popup")]
        [SerializeField]
        private GameObject casinoRatePopup;
        [SerializeField]
        private GameObject casinoLogsPopup;
        [SerializeField]
        private GameObject casinoDescriptPopup;

        private Timer _timer;
        private TimeSpan _refresh_time = new TimeSpan();

        private Proto_16637_Response.Dial_data[] _dial_datas;
        Sequence guruguru;
        private bool _isReady = false;
        /// <summary>
        /// 是否初始化(避免抽獎時抽到一次性獎品,動畫還沒開始,icon率先反灰的視覺觀感問題)
        /// </summary>
        private bool isInit=false;
        /// <summary>
        /// 是否刷新(避免抽獎時抽到一次性獎品,動畫還沒開始,icon率先反灰的視覺觀感問題)
        /// </summary>
        private bool isRefreshUI = false;


        private bool isTen = false;
        // Start is called before the first frame update
        void Start()
        {
            SoundController.Instance.PlayBGM(SoundController.BGMName.BGM_MYPAGE);
            StartCoroutine(Init());
        }

        private void OnDestroy()
        {
            guruguru.Kill();
        }

        private IEnumerator Init()
        {
            bgCanvasGroup.alpha = 0;
            uiCanvasGroup.alpha = 0;

            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();

            topPanel.Init(popupParent);

            // 設置計時器，每秒觸發一次
            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true; // 設置為自動重置
            _timer.Enabled = true; // 啟動計時器

            RegisterEvent();

            NetworkManager.Instance.Send(new Proto_16637_Request());

            MainManager.Instance.HideLoading(true);
            PlayEnterAnim(() =>
            {
                _isReady = true;
                normalTab.isOn = true;
                Test();
            });
        }

        private IEnumerator SetUI(long type)
        {

            //設幸運值Item
            MainManager.Instance.SetCanInput(false);
            switch (type)
            {
                case 1:

                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Icon_luk.png",
                       res =>
                       {
                           luckValueBgImg.sprite = res;
                       }
                          )
                              );


                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Roulette_normal.png",
                       res =>
                       {
                           turntableBgImg.sprite = res;
                       }
                          )
                              );


                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Icon_chippurple.png",
                       res =>
                       {
                           foreach (var img in currencyImg)
                           {
                               img.sprite = res;
                           
                           }
                       }
                          )
                              );


                    break;
                case 2:

                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Icon_luk2.png",
                       res =>
                       {
                           luckValueBgImg.sprite = res;
                       }
                          )
                              );

                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Roulette_gold.png",
                       res =>
                       {
                           turntableBgImg.sprite = res;
                       }
                          )
                              );

                    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Icon_chipgold.png",
                       res =>
                       {
                           foreach (var img in currencyImg)
                           {
                               img.sprite = res;

                           }
                       }
                          )
                              );

                    break;
            }

            //偵測tab紅點


            var data = _dial_datas.FirstOrDefault(i => i.type == type);
            if (data != null)
            {
                var fixData = LoadResource.Instance.DialData.data_get_lucky_award.Where(i => i.type == type).ToList();
                for (int i = 0; i < fixData.Count; i++)
                {
                    var lucky_award = fixData[i];

                    var award = JsonObjectTool.ObjectToListList<uint>(lucky_award.award)[0];
                    var num = i;
                    accumulatedAwards[i].Init(//需要先初始化
                        award[0],
                        award[1],
                        lucky_award.lucky_val,
                        () =>
                        {
                            if (accumulatedAwards[num].ReceiveRewardImg.activeSelf)
                            {
                                
                                StartCoroutine(accumulatedAwards[num].ReverseGray(true));
                                accumulatedAwards[num].SetReceiveReward(false);
                                OnReceiveAward(lucky_award.id);
                                isRefreshUI = true;
                            }
                            else
                            {
                                MainManager.Instance.ShowDefaultTipPopup(award[0]);
                            }

                        });
                }
                DetectNotReveiceLuckReward();
                //時間
                DateTime dateTime = UnixTime.FromUnixTime(data.end_time);
                _refresh_time = dateTime - UnixTime.Now();
                if (_refresh_time.TotalSeconds > 0)
                {
                    freeText.SetActive(false);
                    payText.SetActive(true);
                }
                else
                {
                    freeText.SetActive(true);
                    payText.SetActive(false);

                }

                //券數量
                haveItemText.text = MessageResponseData.Instance.BagItemList
                    //37001普通券, 37002高級券
                    .Where(bag => bag.Value.base_id == (uint)(type == 2 ? 37002 : 37001))
                    .Sum(item => item.Value.quantity).ToString();
                //幸運值
                accumulatedValueText.text = data.lucky.ToString();
                accumulatedValueBar.DOFillAmount(data.lucky / 1000f, 0.5f);

                int luckReward = type==1?1:6;

                //幸運值獎勵
                for (int i = 0; i < accumulatedAwards.Length; i++)
                {
                    var cell = accumulatedAwards[i];


                    if (data.lucky_award.FirstOrDefault(award => award.lucky == i + luckReward) != null)//還沒領
                    {
                       yield return cell.ReverseGray(true);
                        //     cell.SetItemSelected(true);
                        Debug.Log("關閉"+cell.gameObject.name+"的紅點");
                        cell.SetReceiveReward(false);
                    }
                    else
                    {
                      yield return cell.ReverseGray(false);
                        // cell.SetItemSelected(false);
                        if (cell.luckVal<=data.lucky) 
                        {
                            Debug.Log("開啟" + cell.gameObject.name + "的紅點");
                            cell.SetReceiveReward(true);
                        }
                        else
                        {
                            Debug.Log("關閉2" + cell.gameObject.name + "的紅點");
                            cell.SetReceiveReward(false);
                        }
                    }
                }



                //轉盤物品
                foreach (var rand in data.rand_lists)
                {
                    if (LoadResource.Instance.DialData.data_get_rand_list.TryGetValue(rand.id.ToString(), out var item))
                    {
                        turntableItems[rand.pos - 1].Init(
                            (uint)item[0].item_id,
                            amount: (uint)item[0].item_num,
                            0,
                            () =>
                            {

                                MainManager.Instance.ShowDefaultTipPopup((uint)item[0].item_id);
                            });
                         yield return turntableItems[rand.pos - 1].ReverseGray(rand.status == 1);
                     //   turntableItems[rand.pos - 1].SetItemSelected(rand.status == 1);

                    }
                }
            }
            MainManager.Instance.SetCanInput(true);

        }
        /// <summary>
        /// 偵測是否有尚未領取獎勵的tab
        /// </summary>
        private void DetectNotReveiceLuckReward()
        {
            for (int type=1;type<3;type++) 
            {
                var fixData = LoadResource.Instance.DialData.data_get_lucky_award.Where(i => i.type == type).ToList();
                var data = _dial_datas.FirstOrDefault(i => i.type == type);
                bool isNotReceiveReward = false;
                //int luckReward = type == 1 ? 1 : 6;

                for (int i=0;i<fixData.Count;i++)
                {
                    var cell = fixData[i];
                    
                    if (cell.lucky_val <= data.lucky && data.lucky_award.FirstOrDefault(award => award.lucky == cell.id) == null)
                    {
                        isNotReceiveReward = true;
                        break;
                    }
                }


                if (isNotReceiveReward) {

                    switch (type) 
                    {
                        case 1:
                            normalTabRedPoint.gameObject.SetActive(true);
                            break;
                        case 2:
                            highTabRedPoint.gameObject.SetActive(true);
                            break;
                    }

                }
                else
                {
                    switch (type)
                    {
                        case 1:
                            normalTabRedPoint.gameObject.SetActive(false);
                            break;
                        case 2:
                            highTabRedPoint.gameObject.SetActive(false);
                            break;
                    }
                }
            }

        }

        private void RegisterEvent()
        {
            //券數量
            MessageResponseData.Instance.SubscribeBagList(bag =>
            {
                if (highTab.isOn)
                    haveItemText.text = bag.Where(item => item.Value.base_id == 37002).Sum(item => item.Value.quantity).ToString();
                else
                    haveItemText.text = bag.Where(item => item.Value.base_id == 37001).Sum(item => item.Value.quantity).ToString();
            }, this);
            //抽奖活动信息
            MessageResponseData.Instance.SubscribeLive<Proto_16637_Response>(16637, res =>
            {
                _dial_datas = res.dial_data;
                if (!isInit) {
                    isInit = true;
                    StartCoroutine( SetUI(highTab.isOn ? 2 : 1));
                }
            }, false, this);

            //抽奖活动抽奖
            MessageResponseData.Instance.SubscribeLive<Proto_16638_Response>(16638, res =>
            {
                if (res.code == 1)
                {
                    block.SetActive(true);
                    rateButton.gameObject.SetActive(false);
                    RateMaskImg.SetActive(true);
                  /*  StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Btn_chance_bg.png",
                        res => {
                            rateButton.GetComponent<Image>().sprite = res;
                    }));*/
                    
                    //抽獎動畫
                    //獲獎彈窗
                    //消除Block
                    GachaAnime(res.awards3[0].pos, () =>
                    {
                        var awards = new List<(uint bid, uint num)>();
                        //獲得的兩個獎勵
                        awards.AddRange(res.awards1.Select(i => (bid: i.bid, num: i.num)));
                        awards.AddRange(res.awards2.Select(i => (bid: i.bid, num: i.num)));

                        //MainManager.Instance.GetItemPopup(awards);

                        MainManager.Instance.GetItemTwoOptionPopup(
                            new GetItemPopupTwoOption.GetItemPopupTwoOptionFunc()
                            { 
                                items=awards,
                                CloseBtnString=LocalizeKey.LotteryCloseText,
                                ConfirmBtnString=isTen?LocalizeKey.LotteryOfTenConfirmText:LocalizeKey.LotteryConfirmText,
                                Confirmfunc=popup=> {
                                    OnLotteryButton(isTen);
                                    popup.ClosePopup();
                                }
                              }
                            );

                    /*    StartCoroutine(LoadAssetManager.LoadAsset<Sprite>("Assets/Application/Casino/Sprite/AtlasParts/Btn_chance.png",
                            res => {
                                rateButton.GetComponent<Image>().sprite = res;
                            }));*/
                        rateButton.gameObject.SetActive(true);
                        RateMaskImg.SetActive(false);
                        block.SetActive(false);
                        MainManager.Instance.SetCanInput(true);
                        Test();
                        StartCoroutine( SetUI(highTab.isOn ? 2 : 1));
                    });
                }
                else
                {
                    MainManager.Instance.SetCanInput(true);
                    //MainManager.Instance.AddRemind(res.msg);
                    block.SetActive(false);
                }
            }, false, this);


            //抽奖活动推送数据变化
            MessageResponseData.Instance.SubscribeLive<Proto_16639_Response>(16639, res =>
            {
                var temp = _dial_datas.FirstOrDefault(i => i.type == res.type);
                temp.count = res.count;
                temp.lucky = res.lucky;
                temp.frist_type = res.frist_type;
                temp.end_time = res.end_time;
                temp.lucky_award = res.lucky_award.Select(i => new Proto_16637_Response.Dial_data.Lucky_award()
                {
                    lucky = i.lucky
                }).ToArray();
                temp.rand_lists = res.rand_lists.Select(i => new Proto_16637_Response.Dial_data.Rand_lists()
                {
                    pos = i.pos,
                    id = i.id,
                    status = i.status
                }).ToArray();
                if (isRefreshUI) {
                    isRefreshUI = false;
                    StartCoroutine( SetUI(highTab.isOn ? 2 : 1));
                }
                DetectNotReveiceLuckReward();
            }, false, this);
            //抽奖领取幸运值奖励(基本不用回應)
            MessageResponseData.Instance.SubscribeLive<Proto_16640_Response>(16640, res =>
            {
                if (res.code == 1)
                {
                    
                }
                else
                {
                    MainManager.Instance.AddRemind(LocaleManager.Instance.ParseServerMessage(res.msg));
                }
            }, false, this);

            MessageResponseData.Instance.SubscribeLive<Proto_16642_Response>(16642, res =>
            {
                if (res.code == 1)
                {
                    freeText.SetActive(false);
                    payText.SetActive(true);
                }
                else
                {
                    MainManager.Instance.AddRemind(LocaleManager.Instance.ParseServerMessage(res.msg));
                }
            }, false, this);
            //免費刷新時間
            this.ObserveEveryValueChanged(x => x._refresh_time).Subscribe(time =>
            {
                if (time.TotalSeconds > 0)
                {
                    Debug.Log("時間計算");
                    freeRefreshTimeText.text = string.Format("{0}:{1}:{2}", time.Hours.ToString("00"), time.Minutes.ToString("00"), time.Seconds.ToString("00"));
                }
                else
                {
                    Debug.Log("時間歸零");
                    freeRefreshTimeText.text = "00:00:00";
                    freeText.SetActive(true);
                    payText.SetActive(false);
                }
            }).AddTo(this);
            shopButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnShopButton()).AddTo(this);
            tipsButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnTipsButton()).AddTo(this);
            onceButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnLotteryButton(false)).AddTo(this);
            moreButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnLotteryButton(true)).AddTo(this);
            refreshButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnRefreshButton()).AddTo(this);
            btnHome.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnHomeButton()).AddTo(this);
            btnBack.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnBackButton()).AddTo(this);
            normalTab.OnValueChangedAsObservable().Where(on => _isReady && on).Subscribe(_ =>StartCoroutine( SetUI(1))).AddTo(this);
            if ( MessageResponseData.Instance.UserData.lev < 30)
            {
                highTab.enabled = false;
                limitLevelMaskBtn.gameObject.SetActive(true);
                limitLevelMaskBtn.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => MainManager.Instance.AddRemind(LocalizeKey.LimitLevel30)).AddTo(this);
            }
            else
            {
                highTab.enabled = true;
                limitLevelMaskBtn.gameObject.SetActive(false);
                highTab.OnValueChangedAsObservable().Where(on => _isReady && on).Subscribe(_ => StartCoroutine(SetUI(2))).AddTo(this);
            }

            rateButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnWatchRateButton()).AddTo(this);
            recordButton.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnRecordButton()).AddTo(this);
        }
        /// <summary>
        /// 抽獎
        /// </summary>
        /// <param name="isTen"></param>
        private void OnLotteryButton(bool _isTen)
        {
            isTen = _isTen;
            var limit = LoadResource.Instance.DialData.data_get_limit_open
                .FirstOrDefault(i => i.type == (uint)(highTab.isOn ? 2 : 1) &&
                                i.type2 == (uint)(isTen ? 2 : 1));
            var limit_open = JsonObjectTool.ObjectToListList<string>(limit.limit_open);

            if (MessageResponseData.Instance.UserData.lev < int.Parse(limit_open[0][1]))
            {
                MainManager.Instance.AddRemind(string.Format(LocalizeKey.LimitLevel, limit_open[0][1]) );
            }
            else if (limit_open.Count > 1 && MessageResponseData.Instance.UserData.vip_lev < int.Parse(limit_open[1][1]))
            {
                MainManager.Instance.AddRemind( string.Format(LocalizeKey.LimitVIP ,limit_open[1][1]));
             //   MainManager.Instance.ShowCommonPopup("提示", "vip等級" + limit_open[1][1] + "開啟");
            }
            else
            {

                MainManager.Instance.SetCanInput(false);
                NetworkManager.Instance.Send(new Proto_16638_Request()
                {
                    type = (uint)(highTab.isOn ? 2 : 1),
                    type2 = (uint)(isTen ? 2 : 1)
                });
            }
        }
        /// <summary>
        /// 刷新
        /// </summary>
        private void OnRefreshButton()
        {

            NetworkManager.Instance.Send(new Proto_16642_Request()
            {
                type = (uint)(highTab.isOn ? 2 : 1)
            });
            isRefreshUI = true;
        }
        /// <summary>
        /// 獲得幸運值獎勵(上方獎勵)
        /// </summary>
        /// <param name="id"></param>
        private void OnReceiveAward(long id)
        {
            NetworkManager.Instance.Send(new Proto_16640_Request()
            {
                type = (uint)(highTab.isOn ? 2 : 1),
                id = (uint)id
            });
        }
        /// <summary>
        /// 機率彈窗
        /// </summary>
        private void OnWatchRateButton()
        {
            var popup = Instantiate(casinoRatePopup, popupParent.transform);
            popup.GetComponent<CasinoWatchRatePopup>().Init(highTab.isOn ? 2 : 1);
            popup.GetComponent<CasinoWatchRatePopup>().ShowPopup();
        }
        /// <summary>
        /// 紀錄彈窗
        /// </summary>
        private void OnRecordButton()
        {
            var popup = Instantiate(casinoLogsPopup, popupParent.transform);
            popup.GetComponent<CasinoLogsPopup>().Init(
                (uint)(highTab.isOn ? 2 : 1),
                _dial_datas.FirstOrDefault(i => i.type == (highTab.isOn ? 2 : 1)).log_list
                );
            popup.GetComponent<CasinoLogsPopup>().ShowPopup();
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

        private void OnShopButton()
        {
            MallShopController.SceneParameter mallSceneParameter = new MallShopController.SceneParameter
            {
                page = ShopMallPage.Casino
            };
            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.MallScene,mallSceneParameter);
            });
        }

        private void OnTipsButton()
        {
            var popup = Instantiate(casinoDescriptPopup, popupParent.transform);
            popup.GetComponent<CasinoillustratePopup>().Init(new CasinoillustratePopup.CasinoillustratePopupData(
                LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData, LoadResource.Instance.DialData.data_const[highTab.isOn ? "game_rule2" : "game_rule1"].desc)
                ));
            popup.GetComponent<CasinoillustratePopup>().ShowPopup();

        }

        /// <summary>
        /// 計時器呼叫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 在這裡放置計時器每次觸發時要執行的任務或程式碼
            if (_refresh_time.TotalSeconds > 0)
            {
                _refresh_time = _refresh_time.Subtract(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// 入場動畫
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayEnterAnim(Action onComplete = null)
        {
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgCanvasGroup.DOFade(1, 0.2f / speed))
                    .Join(uiCanvasGroup.DOFade(1, 0.2f / speed))
                    .OnComplete(() => { onComplete?.Invoke(); GameSceneManager.Instance?.FinishLoadScene(); });
        }

        /// <summary>
        /// 淡出動畫
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayExitAnim(Action onComplete = null)
        {
            _isReady = false;
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence
    //            .Append(bgCanvasGroup.DOFade(0f, 0.2f / speed))
				//.Join(uiCanvasGroup.DOFade(0f, 0.2f / speed))
                .OnComplete(() => onComplete?.Invoke());
        }

        private void GachaAnime(byte pos, Action onComplete = null)
        {
            var speed = 10;
            if (guruguru.IsActive()) guruguru.Kill();

            guruguru = DOTween.Sequence();
            //繞六圈
            for (int i = 0; i < turntableItems.Length; i++)
            {
                guruguru.Append(spotlight.transform.DOLocalMove(turntableItems[i].transform.localPosition, 0))
                        .AppendInterval(1f / speed);
            }
            guruguru.SetLoops(3);
            guruguru.OnComplete(() =>
            {
                guruguru = DOTween.Sequence();
                //繞剩下
                for (int i = 0; i < pos; i++)
                {
                    guruguru.Append(spotlight.transform.DOLocalMove(turntableItems[i].transform.localPosition, 0))
                            .AppendInterval(1);
                }
                guruguru.OnComplete(() => onComplete?.Invoke());
            });
        }

        public void Test()
        {
            var speed = 1;
            if (guruguru.IsActive()) guruguru.Kill();

            guruguru = DOTween.Sequence();
            for (int i = 0; i < turntableItems.Length; i++)
            {
                guruguru.Append(spotlight.transform.DOLocalMove(turntableItems[i].transform.localPosition, 0))
                        .AppendInterval(1 / speed);
            }
            guruguru.SetLoops(6);
        }
    }
}