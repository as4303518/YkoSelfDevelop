using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YKO.Network;
using UniRx;
using YKO.Main;
using YKO.Common.UI;
using System.Linq;
using DG.Tweening;
using System;
using UnityEngine.UI;
using YKO.Common;
using Newtonsoft.Json;
using YKO.Support.Expansion;


namespace YKO.SevenDay
{

    public class SevenDayGoalsController : MonoBehaviour
    {

        public class LocalizeKey
        {
            public static readonly string AccumulateReward = "累積獎勵";
            public static readonly string MagicBoxReward = "魔盒獎勵";
            public static readonly string LevelReward = "等級獎勵";
        }

        [Header("view")]
        /// <summary>
        /// 七日登陸view
        /// </summary>
        [SerializeField]
        private SevenDayGoalsView sevenView;
        /// <summary>
        /// 冒險筆記view
        /// </summary>
        [SerializeField]
        private AdventureNoteView adventureView;
        /// <summary>
        /// 魔盒冒險view
        /// </summary>
        [SerializeField]
        private MagicBoxSecretView magicBoxView;
        /// <summary>
        /// 上方面板
        /// </summary>
        [Header("UI")]
        [SerializeField]
        private TopPanel topPanel;

        /// <summary>
        /// 整個頁面UI
        /// </summary>
        [SerializeField]
        private CanvasGroup uiCanvasGroup;
        /// <summary>
        /// 返回按鈕
        /// </summary>
        [SerializeField]
        private Button btnBack = default;
        /// <summary>
        /// 回主頁按鈕
        /// </summary>
        [SerializeField]
        private Button btnHome = default;
        /// <summary>
        /// 彈窗顯示母物件
        /// </summary>
        [SerializeField]
        private Transform popupParent;

        /// <summary>
        /// 背景canvas圖層
        /// </summary>
        [SerializeField]
        private CanvasGroup backgroundCanvasGroup;

        /// <summary>
        /// 背景圖
        /// </summary>
        [SerializeField]
        private GameObject[] backgroundImages;

        /// <summary>
        /// 階段獎勵彈窗
        /// </summary>
        private GameObject sevenRewardPopup = null;





        #region prefab
        /// <summary>
        /// 階段獎勵彈窗預製物
        /// </summary>
        [SerializeField]
        private GameObject sevenRewardPopupPrefab;
        /// <summary>
        /// 文字介紹彈窗預製物
        /// </summary>
        [SerializeField]
        private GameObject descriptionPopupPrefab;

        #endregion

        #region Data

        /// <summary>
        /// proto13601 資料
        /// </summary>
        private Proto_13601_Response proto13601;

        /// <summary>
        /// proto13604 資料
        /// </summary>
        private Proto_13604_Response proto13604;
        /// <summary>
        /// 頁面準備狀態
        /// </summary>
        private bool _isReady = false;


        private ViewState viewState;
        #endregion

        private void Start()
        {
            StartCoroutine(Init());
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        private IEnumerator Init()
        {
            MainManager.Instance.SetCanInput(false);
            uiCanvasGroup.alpha = 0f;
            backgroundCanvasGroup.alpha = 0f;
            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();

            proto13601 = MessageResponseData.Instance.ProtoResponse13601;
            topPanel.Init();
            RegisterEvent();
        }
        /// <summary>
        /// 登入proto
        /// </summary>
        private void RegisterEvent()
        {
            btnHome.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnHomeButton()).AddTo(this);
            btnBack.OnClickAsObservable().Where(_ => _isReady).Subscribe(_ => OnBackButton()).AddTo(this);
            MessageResponseData.Instance.OnMessageResponse
            .Subscribe(meg =>
            {
                switch (meg.MessageID)
                {
                    case 13601:
                        proto13601 = (meg as Proto_13601_Response);
                        break;
                    case 13602://領取後回傳
                        HandleProto13602(meg as Proto_13602_Response);
                        //出現彈窗
                        break;
                    case 13603://更新七日ui狀態
                        //更新view視窗
                        break;

                    case 13604:
                        proto13604 = (meg as Proto_13604_Response);
                        switch (viewState)
                        {
                            case ViewState.AdventureNote:
                                StartCoroutine(OpenView(ViewState.AdventureNote, proto13604));
                                PlayEnterAnim(() =>
                                {
                                    _isReady = true;
                                });
                                break;
                            case ViewState.MagicBox:
                                StartCoroutine(OpenView(ViewState.MagicBox, proto13604));
                                PlayEnterAnim(() =>
                                {
                                    _isReady = true;
                                });
                                break;
                        }
                        break;
                    case 13605:
                        HandleProto13605(meg as Proto_13605_Response);
                        break;
                    case 13606:
                        break;
                    case 13607://開啟彈窗
                        HandleProto13607(meg as Proto_13607_Response);
                        break;
                    case 13609://開啟更新等級狀態
                        HandleProto13609(meg as Proto_13609_Response);
                        break;


                }
            }).AddTo(this);


            var period = proto13601.period > 0 ? proto13601.period : MessageResponseData.Instance.GetRespose<Proto_13604_Response>(13604).period;
            var weekPeriodto3 = period % 3;
            var weekPeriodto2 = period % 2;

            if (period >= 8)
            {
                switch (weekPeriodto2)
                {
                    case 0://冒險筆記
                        SendProto13604();
                        viewState = ViewState.AdventureNote;
                        break;
                    case 1://魔盒的秘密
                        SendProto13604();
                        viewState = ViewState.MagicBox;
                        break;
                }
            }
            else
            {
                switch (weekPeriodto3)
                {
                    case 0://魔盒的秘密
                        SendProto13604();
                        viewState = ViewState.MagicBox;
                        break;
                    case 1://七日登入
                        SendProto13601();
                        StartCoroutine(OpenView(ViewState.SevenDay, proto13601));
                        viewState = ViewState.SevenDay;
                        PlayEnterAnim(() =>
                        {
                            _isReady = true;
                        });
                        break;
                    case 2://冒險筆記
                        SendProto13604();
                        viewState = ViewState.AdventureNote;
                        break;
                }

            }



        }
        private IEnumerator OpenView(ViewState view, IMessage proto)
        {

            switch (view)
            {
                case ViewState.SevenDay:
                    backgroundImages[0].gameObject.SetActive(true);
                    sevenView.gameObject.SetActive(true);
                    adventureView.gameObject.SetActive(false);
                    magicBoxView.gameObject.SetActive(false);
                    sevenView.ReceiveCB = SendProto13602;
                    StartCoroutine(sevenView.Init((proto as Proto_13601_Response)));
                    break;
                case ViewState.AdventureNote:
                    backgroundImages[1].gameObject.SetActive(true);
                    sevenView.gameObject.SetActive(false);
                    adventureView.gameObject.SetActive(true);
                    magicBoxView.gameObject.SetActive(false);
                    adventureView.OpenLevelRewardPopup = SendProto13607;
                    adventureView.ReceiveCB = SendProto13606;
                    adventureView.OpenDescriptionPopup = OpenDescriptionPopup;
                    adventureView.OpenAccumulatePopup = OpenAccumulatePage;
                    adventureView.Init((proto as Proto_13604_Response));
                    break;
                case ViewState.MagicBox:
                    backgroundImages[2].gameObject.SetActive(true);
                    sevenView.gameObject.SetActive(false);
                    adventureView.gameObject.SetActive(false);
                    magicBoxView.gameObject.SetActive(true);
                    magicBoxView.OpenHopeSymbolPopup = SendProto13607;
                    magicBoxView.ReceiveCB = SendProto13606;
                    magicBoxView.OpenDescriptionPopup = OpenDescriptionPopup;
                    magicBoxView.OpenAccumulatePopup = OpenAccumulatePage;
                    magicBoxView.Init((proto as Proto_13604_Response));

                    break;

            }

            yield return new WaitForSeconds(0.2f);
            MainManager.Instance.HideLoading(true);
            MainManager.Instance.SetCanInput(true);
        }




        /// <summary>
        /// 更新七日數據
        /// </summary>
        private void SendProto13601()
        {
            Proto_13601_Request proto = new Proto_13601_Request();
            NetworkManager.Instance.Send(proto);
        }
        /// <summary>
        /// 領取任務獎勵
        /// </summary>
        /// <param name="type">//领取类型，1:福利  2:目标任务一  3:目标任务二  4：折扣购买  5:全目标(寶箱)</param>
        /// <param name="day_type">領取的天數</param>
        /// <param name="id">goal_id</param>
        /// <param name="item">好像是該道具所在的客戶端index 方便response回來偵測用  上方獎勵回傳是0</param>
        private void SendProto13602(byte type, uint day_type, uint id, uint item = 1)
        {
            Proto_13602_Request proto = new Proto_13602_Request();
            proto.type = type;
            proto.day_type = day_type;
            proto.id = id;
            proto.item = item;
            NetworkManager.Instance.Send(proto);
        }
        private void SendProto13604()
        {
            Proto_13604_Request proto = new Proto_13604_Request();
            NetworkManager.Instance.Send(proto);

        }
        private void SendProto13606(uint id)
        {
            Debug.Log("寄送13606=>" + id);
            Proto_13606_Request proto = new Proto_13606_Request();
            proto.id = id;
            NetworkManager.Instance.Send(proto);
        }
        private void SendProto13607()
        {
            Proto_13607_Request proto = new Proto_13607_Request();
            NetworkManager.Instance.Send(proto);
        }
        /// <summary>
        /// 獲取魔盒及冒險等級獎勵
        /// </summary>
        /// <param name="id"></param>
        private void SendProto13608(uint id)
        {

            Proto_13608_Request proto = new Proto_13608_Request();
            proto.id = (UInt16)id;
            NetworkManager.Instance.Send(proto);
        }

        /// <summary>
        /// 處理13602  更新7日數據
        /// </summary>
        private void HandleProto13602(Proto_13602_Response proto)
        {
            //更新七日
            if (proto.flag <= 0)
            {
                Debug.Log("####error=>" + LocaleManager.Instance.ParseServerMessage(proto.msg));
                return;
            }
            proto13601.num = proto.num;
            var allTargetData = LoadResource.Instance.DayGoalsData.data_all_target[proto13601.period.ToString()];

            foreach (var list in proto13601.finish_list)
            {
                var allFD = allTargetData[list.goal_id.ToString()][0];
                if (list.status == 0 && proto13601.num >= allFD.goal)
                {
                    list.status = 1;
                }
            }

            switch (proto.type)
            {
                case 1:
                    var welData = proto13601.welfare_list.Where(list => list.day == proto.day_type).FirstOrDefault(list => list.goal_id == proto.id);
                    welData.status = proto.status;
                    break;
                case 2:
                    var grow1data = proto13601.grow_list.Where(list => list.day == proto.day_type).FirstOrDefault(list => list.goal_id == proto.id);
                    grow1data.status = proto.status;
                    break;
                case 3:
                    var grow2data = proto13601.grow_list.Where(list => list.day == proto.day_type).FirstOrDefault(list => list.goal_id == proto.id);
                    grow2data.status = proto.status;
                    break;
                case 4:
                    var priceData = proto13601.price_list.FirstOrDefault(list => list.day == proto.id);
                    priceData.status = proto.status;

                    //proto13601.price_list
                    break;
                case 5://上方獎勵
                    Debug.Log("更改了type5,他的id=>" + proto.id + "狀態=>" + proto.status);
                    var treasureData = proto13601.finish_list.FirstOrDefault(list => list.goal_id == proto.id);
                    treasureData.status = proto.status;
                    //proto13601.finish_list
                    break;
            }
            StartCoroutine(sevenView.Init(proto13601));
        }
        /// <summary>
        /// 似乎沒執行到
        /// </summary>
        /// <param name="proto"></param>
        private void HandleProto13603(Proto_13603_Response proto)
        {



        }

        /// <summary>
        /// 更新13604 list 資訊 (累積儲值獎勵,下方任務
        /// </summary>
        /// <param name="proto"></param>
        private void HandleProto13605(Proto_13605_Response proto)
        {

            var bindFlag = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            foreach (var list in proto.list)
            {

                var tar = proto13604.list.FirstOrDefault(v => v.id == list.id);

                foreach (var field in list.GetType().GetFields(bindFlag))
                {
                    tar.GetType().GetField(field.Name, bindFlag).SetValue(tar, field.GetValue(list));
                }
            }

            switch (viewState)
            {
                case ViewState.AdventureNote:
                    adventureView.Init(proto13604);
                    break;
                case ViewState.MagicBox:
                    magicBoxView.Init(proto13604);
                    break;

            }


        }

        /// <summary>
        /// 開啟彈窗
        /// </summary>
        private void HandleProto13607(Proto_13607_Response proto)
        {
            bool isHave = true;
            //開啟彈窗
            if (sevenRewardPopup == null)
            {
                sevenRewardPopup = Instantiate(sevenRewardPopupPrefab);
                sevenRewardPopup.transform.SetParent(popupParent, false);
                isHave = false;
            }

            var sevenPopup = sevenRewardPopup.GetComponent<SevenRewardPopup>();

            var period = proto13601.period > 0 ? proto13601.period : proto13604.period;

            var fixedData = LoadResource.Instance.DayGoalsNewData.data_make_lev_list[period.ToString()];
            List<SevenDayRewardCell.SevenDayRewardData> cellDatas = new List<SevenDayRewardCell.SevenDayRewardData>();


            foreach (var levData in fixedData.Values)
            {
                var dynamicData = proto.reward_list.FirstOrDefault(v => v.id == levData.lev);

                SevenDayRewardCell.SevenDayRewardData data = new SevenDayRewardCell.SevenDayRewardData()
                {
                    id = (int)levData.lev,
                    _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, levData.title_name),
                    status = (dynamicData != null) ? (byte)2 : proto.lev >= levData.lev ? (byte)1 : (byte)0,
                    statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900Btn275,
                    itemRewardList = JsonConvert.DeserializeObject<int[][]>(levData.reward.ToString()),
                    CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.None
                };

                cellDatas.Add(data);

            }

            cellDatas.SmallSizeSort("status");

            cellDatas.Sort((a, b) =>
            {

                if (a.status == 1)
                {

                    return -1;

                }
                else
                {
                    return 0;
                }
            });


            switch (viewState)
            {
                case ViewState.AdventureNote:
                    sevenPopup.Init(LocalizeKey.LevelReward, cellDatas, SendProto13608, () => { sevenRewardPopup = null; });
                    adventureView.CheckRedDot();
                    MainManager.Instance.HideLoading(true);
                    break;
                case ViewState.MagicBox:
                    sevenPopup.Init(LocalizeKey.MagicBoxReward, cellDatas, SendProto13608, () => { sevenRewardPopup = null; });
                    MainManager.Instance.HideLoading(true);
                    break;
            }
            if (!isHave)
            {
                Debug.Log("打開階段獎勵彈窗");
                sevenPopup.ShowPopup();
            }

        }

        /// <summary>
        /// 設定冒險筆記與魔盒日記更新後的狀態
        /// </summary>
        /// <param name="proto"></param>
        private void HandleProto13609(Proto_13609_Response proto)
        {
            //SendProto13607();
            proto13604.exp = proto.exp;
            proto13604.lev = proto.lev;

        }

        /// <summary>
        /// 顯示儲值獎勵
        /// </summary>
        private void OpenAccumulatePage()
        {
            List<SevenDayRewardCell.SevenDayRewardData> cellDatas = new List<SevenDayRewardCell.SevenDayRewardData>();
            var fixedData = LoadResource.Instance.DayGoalsNewData.data_charge_list[proto13604.period.ToString()];
            var dynamicList = proto13604.list.Where(v => fixedData.ContainsKey(v.id.ToString())).ToList();

            //dynamicList.SmallSizeSort("id");

            dynamicList.SpeciftyCondition(
                v =>
                {
                    return v.finish == 0;
                }
                );

            dynamicList.SpeciftyCondition(
                v =>
                {
                    return v.finish == 1;
                }
                );

            foreach (var list in dynamicList)
            {
                var fx = fixedData[list.id.ToString()];
                SevenDayRewardCell.SevenDayRewardData data = new SevenDayRewardCell.SevenDayRewardData()
                {
                    goal_id = (int)list.id,
                    _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fx.desc),
                    CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.Progress,
                    targetVal = list.target_val,
                    value = list.value,
                    status = list.finish,
                    statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900Btn275,
                    itemRewardList = JsonConvert.DeserializeObject<int[][]>(fx.award.ToString()),
                    NextCB = () => PageJumpController.Instance?.PageJumpByTransimtOfNum(fx.show_icon)

                };

                cellDatas.Add(data);

            }


            sevenRewardPopup = Instantiate(sevenRewardPopupPrefab);
            sevenRewardPopup.transform.SetParent(popupParent, false);


            var sevenPopup = sevenRewardPopup.GetComponent<SevenRewardPopup>();
            sevenPopup.Init(LocalizeKey.AccumulateReward, cellDatas, SendProto13606);
            sevenPopup.ShowPopup();

        }


        /// <summary>
        /// 開啟描述彈窗
        /// </summary>
        /// <param name="_title"></param>
        /// <param name="_descript"></param>
        /// <param name="pos"></param>
        private void OpenDescriptionPopup(string _title, string _descript, Vector3 pos)
        {
            var sp = Instantiate(descriptionPopupPrefab);
            sp.transform.SetParent(popupParent, false);
            // sp.GetComponent<RectTransform>().anchoredPosition = pos;
            sp.transform.position = pos;
            var tipPopup = sp.GetComponent<SevenDayTipPopup>();
            tipPopup.Init(_title, _descript);
            tipPopup.ShowPopup();


        }


        /// <summary>
        /// 回主頁
        /// </summary>
        private void OnHomeButton()
        {
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene);
            });
        }
        /// <summary>
        /// 回上一頁
        /// </summary>
        private void OnBackButton()
        {
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.BackScene();
            });
        }
        /// <summary>
        /// 入場動畫
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayEnterAnim(Action onComplete = null)
        {
            var speed = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(uiCanvasGroup.DOFade(1, 0.2f / speed))
                              .Join(backgroundCanvasGroup.DOFade(1, 0.2f / speed))
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
                    //.Append(uiCanvasGroup.DOFade(0f, 0.2f / speed))
                    //    .Join(backgroundCanvasGroup.DOFade(0, 0.2f / speed))
                    .OnComplete(() => onComplete?.Invoke());
        }
        /// <summary>
        /// 顯示設置
        /// </summary>
        private enum ViewState
        {

            SevenDay,
            AdventureNote,
            MagicBox


        }

    }
}
