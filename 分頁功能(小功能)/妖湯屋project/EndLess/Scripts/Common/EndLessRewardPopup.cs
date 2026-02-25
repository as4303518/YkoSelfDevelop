using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.Network;


namespace YKO.EndLess {
    public class EndLessRewardPopup : PopupBase
    {

        public class LocalizeKey
        {
            /// <summary>
            /// 獎勵一覽
            /// </summary>
            public static readonly string RewardInfo = "獎勵一覽";

            /// <summary>
            /// 首通獎勵
            /// </summary>
            public static readonly string FirstClearReward = "首通獎勵";

            /// <summary>
            /// tab 日常獎勵
            /// </summary>
            public static readonly string DailyReward = "日常獎勵";

        }

        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField]
        private Text titleText;

        /// <summary>
        /// 關閉按鈕
        /// </summary>
        [SerializeField]
        private Button closeBtn;
        /// <summary>
        /// 普通獎勵標籤
        /// </summary>
        [SerializeField]
        private CustomTabButton firstClearTab;
        /// <summary>
        /// 日常獎勵標籤
        /// </summary>
        [SerializeField]
        private CustomTabButton dailyTab;

        #region firstClear
        /// <summary>
        /// 普通獎勵分頁
        /// </summary>
         [Header("FirstClear")]
        [SerializeField]
        private GameObject firstClearPage;


        [SerializeField]
        private Button[] attributeBtnList;
        /// <summary>
        /// 首通屬性分頁
        /// </summary>
        private Dictionary<eEndLessRewardPopupPage, GameObject> RewardPages = new Dictionary<eEndLessRewardPopupPage, GameObject>();

        [SerializeField]
        private Text firstClearTip;
        /// <summary>
        /// 捲動視圖(設定切頁時的content
        /// </summary>
        [SerializeField]
        private ScrollRect firstClearScrollView;

        /// <summary>
        /// 基礎頁
        /// </summary>
        [SerializeField]
        private GameObject firstClearBaseContent;

        /// <summary>
        /// 基礎頁
        /// </summary>
        [SerializeField]
        private GameObject firstClearPrefab;


        #endregion

        #region daily
        /// <summary>
        /// 日常獎勵分頁
        /// </summary>
        /// 
        [Header("Daily")]
        [SerializeField]
        private GameObject dailyPage;

        /// <summary>
        /// daily 母物件
        /// </summary>
        [SerializeField]
        private Transform dailyContent;



        /// <summary>
        /// 基礎頁
        /// </summary>
        [SerializeField]
        private GameObject dailyPrefab;



        #endregion
        /// <summary>
        /// 當前顯示的頁面
        /// </summary>
        private eEndLessRewardPopupPage curPage;
        /// <summary>
        /// 資料
        /// </summary>
        private EndLessRewardPopupFunc aData;



        public IEnumerator Init(EndLessRewardPopupFunc data)
        {
            base.Init();
            aData = data;
            yield return UpdateUI();
            Register();
            ShowPopup();

        }
        /// <summary>
        /// 註冊按鈕
        /// </summary>
        private void Register()
        {
            closeBtn.OnClickAsObservable().Subscribe(_ => ClosePopup());
            MessageResponseData.Instance.SubscribeLive<Proto_23903_Response>(
                23903,
                proto => {

                    eEndLessRewardPopupPage tarPage = (eEndLessRewardPopupPage)Enum.ToObject(typeof(eEndLessRewardPopupPage), proto.type);
                    if (RewardPages.ContainsKey(tarPage)) 
                    {
                        Destroy( RewardPages[tarPage]);
                        RewardPages.Remove(tarPage);
                    }
                    CreateFirstStatusPage(proto,(curPage==tarPage));
                },false,this);

            firstClearTab.OnValueChangedAsObservable().Where(on=>on)
                .Subscribe(_ => StartCoroutine(OpenPage((eEndLessRewardPopupPage)Enum.ToObject(typeof(eEndLessRewardPopupPage), aData.type)))).AddTo(this);
            dailyTab.OnValueChangedAsObservable().Where(on => on)
                .Subscribe(_ => StartCoroutine(OpenPage(eEndLessRewardPopupPage.Daily))).AddTo(this);

            for (int i=0; i<attributeBtnList.Length;i++)
            { 
            var  btn=attributeBtnList[i];
                int type = i;
                btn.OnClickAsObservable()
                 .Subscribe(_ => {
                     Debug.Log("按下按鈕=>"+ (eEndLessRewardPopupPage)   Enum.ToObject(typeof(eEndLessRewardPopupPage), (type + 1)));

                     StartCoroutine(OpenPage((eEndLessRewardPopupPage)Enum.ToObject(typeof(eEndLessRewardPopupPage), (type + 1))));
                }).AddTo(this);
            }

        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateUI()
        {
            titleText.text = LocalizeKey.RewardInfo;
            firstClearTab.titleName = LocalizeKey.FirstClearReward;
            dailyTab.titleName = LocalizeKey.DailyReward;

            yield return OpenPage(aData.speciftyPage);

        }
        /// <summary>
        /// 開啟頁面
        /// </summary>
        /// <param name="ePage"></param>
        /// <returns></returns>
        private IEnumerator OpenPage(eEndLessRewardPopupPage ePage)
        {
            Debug.Log("執行頁面=>" + ePage);

            curPage= ePage;
            if (RewardPages.ContainsKey(ePage))
            {
                if (ePage== eEndLessRewardPopupPage.Daily)
                {
                    firstClearPage.gameObject.SetActive(false);
                    dailyPage.gameObject.SetActive(true);
                    dailyTab.isOn = true;

                }
                else
                {
                    firstClearPage.gameObject.SetActive(true);
                    dailyPage.gameObject.SetActive(false);
                    firstClearTab.isOn = true;

                    foreach (var page in RewardPages.Values)
                    { 
                         page.SetActive(false);
                    }
                    RewardPages[ePage].SetActive(true);
                    firstClearScrollView.content = RewardPages[ePage].GetComponent<RectTransform>();
                    firstClearScrollView.verticalNormalizedPosition = 1;
                }
            }
            else
            {

                byte type = 0;
                switch (ePage)
                {
                    case eEndLessRewardPopupPage.Daily:
                        var dailyFD= LoadResource.Instance.EndlessData.data_floor_data[aData.type.ToString()];
                        //生成daily頁面
                        for (uint i=0;i<10;i++)
                        {
                            uint tarRound = (aData.curentRound + i);
                            if (!dailyFD.ContainsKey(tarRound.ToString()))
                            {
                                break;
                            }
                            var tarFD = dailyFD[tarRound.ToString()];

                            var sp = Instantiate(dailyPrefab, dailyContent);

                            sp.GetComponent<EndLessDailyRewardCell>().Init(
                                new EndLessDailyRewardCell.EndLessDailyRewardCellFunc()
                                   {
                                    missionNum=tarRound,
                                    itemInfo = JsonConvert.DeserializeObject<uint[][]>(tarFD.items.ToString()),
                                    firstCell=(i<=0)
                                    }
                                );

                        }
                        RewardPages.Add(ePage, dailyPage);
                        firstClearPage.gameObject.SetActive(false);
                        dailyPage.gameObject.SetActive(true);
                        yield break;
                    case eEndLessRewardPopupPage.FirstClearOld:
                        type = 5;
                        break;
                    case eEndLessRewardPopupPage.FirstClearWater:
                        type = 1;
                        break;
                    case eEndLessRewardPopupPage.FirstClearFire:
                        type = 2;
                        break;
                    case eEndLessRewardPopupPage.FirstClearWind:
                        type = 3;
                        break;
                    case eEndLessRewardPopupPage.FirstClearLightAndDark:
                        type = 4;
                        break;
                }

                Proto_23903_Response proto23903 = null;
                yield return aData.getProto23903(type,( proto ,haveData)=> {
                    proto23903 = proto;
                    if (haveData) {
                        CreateFirstStatusPage(proto,true);
                    }
                });

             /*   var content = Instantiate(firstClearBaseContent, firstClearScrollView.viewport.transform);
                content.SetActive(true);
                RewardPages.Add(ePage, content);
                firstClearScrollView.content = content.GetComponent<RectTransform>();
                Debug.Log("回傳的type=>"+type);
                var fixedData = LoadResource.Instance.EndlessData.data_first_data[type.ToString()];

                uint multID = 0;
                for (int i = 0; i < 10; i++)
                {
                    uint tarID = proto23903.id + multID ;
                    while (proto23903.rewarded.Any(v => v.id ==(UInt16)tarID))
                    {
                        multID++;
                        tarID = proto23903.id +multID ;
                    }
                    if (!fixedData.ContainsKey(tarID.ToString()))//已經無法顯示更多的通關獎勵(通關可領的獎勵只剩不到10個)
                    {
                        break;
                    }

                    var tarFD = fixedData[tarID.ToString()];
                    var sp = Instantiate(firstClearPrefab, content.transform);

                    sp.GetComponent<EndLessNormalRewardCell>().Init(
                        new EndLessNormalRewardCell.EndLessNormalRewardCellFunc()
                        {
                            missionNum = (uint)tarFD.limit_id,
                            missionGap = (uint)tarFD.limit_id - proto23903.max_id,
                            itemInfo = JsonConvert.DeserializeObject<uint[][]>(tarFD.items.ToString()),
                            receiveRewardCB = () => 
                            {
                                Debug.Log("領取獎勵=>"+tarID);
                                aData.receiveReward(type, tarID);
                            //接收獎勵
                            }
                        });
                    multID++;
                }//------for*/
                firstClearPage.gameObject.SetActive(true);
                dailyPage.gameObject.SetActive(false);
            }//---------else
        }
        /// <summary>
        /// 生成首次通關頁面
        /// </summary>
        private void CreateFirstStatusPage(Proto_23903_Response proto23903,bool _switch)
        {
            var ePage = (eEndLessRewardPopupPage)Enum.ToObject(typeof(eEndLessRewardPopupPage), proto23903.type);
            var content = Instantiate(firstClearBaseContent, firstClearScrollView.viewport.transform);
            content.name = ePage.ToString() + "Page";
            content.SetActive(true);
            RewardPages.Add(ePage, content);
            firstClearScrollView.content = content.GetComponent<RectTransform>();
            firstClearScrollView.verticalNormalizedPosition = 1;
            var fixedData = LoadResource.Instance.EndlessData.data_first_data[proto23903.type.ToString()];

            uint multID = 0;
            for (int i = 0; i < 10; i++)
            {
                uint tarID = proto23903.id + multID;
                while (proto23903.rewarded.Any(v => v.id == (UInt16)tarID))
                {
                    multID++;
                    tarID = proto23903.id + multID;
                }
                if (!fixedData.ContainsKey(tarID.ToString()))//已經無法顯示更多的通關獎勵(通關可領的獎勵只剩不到10個)
                {
                    break;
                }

                var tarFD = fixedData[tarID.ToString()];
                var sp = Instantiate(firstClearPrefab, content.transform);

                int gap = ((int)tarFD.limit_id -(int)proto23903.max_id)>0? ((int)tarFD.limit_id - (int)proto23903.max_id):0;
                sp.GetComponent<EndLessNormalRewardCell>().Init(
                    new EndLessNormalRewardCell.EndLessNormalRewardCellFunc()
                    {
                        missionNum = (uint)tarFD.limit_id,
                        missionGap = (uint)gap,
                        itemInfo = JsonConvert.DeserializeObject<uint[][]>(tarFD.items.ToString()),
                        receiveRewardCB = () =>
                        {
                            aData.receiveReward(proto23903.type, tarID);
                            //接收獎勵
                        }
                    });
                multID++;
            }//------for

    }


        /// <summary>
        /// 獎勵彈窗資料
        /// </summary>
        public class EndLessRewardPopupFunc
        {
            /// <summary>
            /// 獎勵屬性(影響到首通的預設tab跟日常的顯示
            /// </summary>
            public byte type;

            /// <summary>
            /// 當前日常開始關卡
            /// </summary>
            public uint curentRound;

            /// <summary>
            /// 指定開啟頁面(根據開啟的位置不同)
            /// </summary>
            public eEndLessRewardPopupPage speciftyPage;

            /// <summary>
            /// 接收獎勵
            /// </summary>
            public Action<byte, uint> receiveReward = null;

            /// <summary>
            /// 獲取首通領取資訊
            /// </summary>
            public Func<byte, Action<Proto_23903_Response,bool>, IEnumerator> getProto23903 = null;



        }
        /// <summary>
        /// 分頁
        /// </summary>
        public enum eEndLessRewardPopupPage
        {
            Daily,
            FirstClearWater,
            FirstClearFire,
            FirstClearWind,
            FirstClearLightAndDark,
            FirstClearOld
        }


    }
}
