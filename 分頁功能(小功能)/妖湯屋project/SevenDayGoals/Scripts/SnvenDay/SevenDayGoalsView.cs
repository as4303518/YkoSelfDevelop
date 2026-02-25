using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.DataModel;
using YKO.Network;
using System.Timers;
using YKO.Common.Util;
using DG.Tweening;
using YKO.Support.Expansion;

namespace YKO.SevenDay
{
    public class SevenDayGoalsView : MonoBehaviour
    {
        private class LocalizeKey
        {
            public static readonly string SettingTitle = "七日目標";
            public static readonly string FinishCount = "完成數";
            public static readonly string RemainTime = "剩餘時間 : ";
            public static readonly string WelfareTabName = "福利領取 ";
            public static readonly string WelfarePresentTabName = "剩餘時間 : ";
        }
        /// <summary>
        /// 七日目標
        /// </summary>
        [Header("TopUI")]

        [SerializeField]
        private Text titleText;



        /// <summary>
        /// 七日登入道具
        /// </summary>
        [SerializeField]
        public GameObject[] sevenDayObjs = null;

        /// <summary>
        /// 寶箱
        /// </summary>
        [SerializeField]
        private GameObject[] treasures = null;
        /// <summary>
        /// 寶箱的獎勵狀態(未領取狀態時,被點擊要顯示,紀錄完成進度 id等
        /// </summary>
        private List<DayGoalsData.All_Target> treasureRewardStatusList = new List<DayGoalsData.All_Target> { };

        /// <summary>
        /// 七日進度跑條
        /// </summary>
        [SerializeField]
        private Image SevenDaybar;

        /// <summary>
        /// 時間文字
        /// </summary>
        [SerializeField]
        private Text timeText;
        /// <summary>
        /// 完成數 標題
        /// </summary>
        [SerializeField]
        private Text finishTitleText;
        /// <summary>
        /// 完成數量
        /// </summary>
        [SerializeField]
        private Text finishCountText;




        /// <summary>
        /// 控制content可滑動的主動權
        /// </summary>
        [Header("MidUI")]
        [SerializeField]
        private ScrollRect SevenDayContentScrollRect;

        /// <summary>
        /// 福利領取
        /// </summary>
        [SerializeField]
        private CustomTabButton tab1 = null;

        /// <summary>
        /// tab1的道具陣列
        /// </summary>
        [SerializeField]
        private Transform tab1ListContent;

        /// <summary>
        /// tab切換1
        /// </summary>
        [SerializeField]
        private CustomTabButton tab2 = null;

        /// <summary>
        /// tab2的道具陣列
        /// </summary>
        [SerializeField]
        private Transform tab2ListContent;
        /// <summary>
        ///  tab切換2
        /// </summary>
        [SerializeField]
        private CustomTabButton tab3 = null;
        /// <summary>
        /// tab3的道具陣列
        /// </summary>
        [SerializeField]
        private Transform tab3ListContent;
        /// <summary>
        /// 福麗禮包
        /// </summary>
        [SerializeField]
        private CustomTabButton tab4 = null;

        /// <summary>
        /// tab4的道具陣列
        /// </summary>
        [SerializeField]
        private Transform tab4ListContent;

        /// <summary>
        /// 獎勵卡內容
        /// </summary>
        [SerializeField]
        private GameObject RewardListContent;
        /// <summary>
        /// 右上角色
        /// </summary>
        [Header("Tween")]
        [SerializeField]
        private GameObject Chara;
        /// <summary>
        /// 寶箱遮罩
        /// </summary>
        [SerializeField]
        private RectTransform treasureMask;



        #region prefab or sprite
        /// <summary>
        /// 獎勵陣列卡
        /// </summary>
        [SerializeField]
        private GameObject RewardCellPrefab;
        /// <summary>
        /// 獲得哪些道具的提示
        /// </summary>
        [SerializeField]
        private GameObject itemTipPrefab;
        /// <summary>
        /// 寶箱關閉跟開啟圖片狀態
        /// </summary>
        [SerializeField]
        private Sprite[] treasureOpenSprite;

        /// <summary>
        /// 上方七日道具背景(3種 白 黃 紅
        /// </summary>
        [SerializeField]
        private Sprite[] sevenDayItenBackgrounds;



        #endregion

        /// <summary>
        /// 接收獎勵,購買
        /// </summary>
        public Action<byte, uint, uint, uint> ReceiveCB = null;
        /// <summary>
        /// 前往場景
        /// </summary>
        public Action NextCB = null;
        /// <summary>
        /// 查看更多
        /// </summary>
        public Action CheckOtherCB = null;

        #region
        /// <summary>
        /// 首次打開
        /// </summary>
        private bool firstOpen = true;
        /// <summary>
        /// 當前觀看的天數
        /// </summary>
        private int curFoucsDay = 0;
        /// <summary>
        /// 計時器
        /// </summary>
        private Timer _timer;
        /// <summary>
        /// 剩餘秒數
        /// </summary>
        private uint remainSecond = 0;

        /// <summary>
        /// 更新的proto
        /// </summary>
        private Proto_13601_Response proto13601;

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public IEnumerator Init(Proto_13601_Response proto)
        {
            proto13601 = proto;

            if (firstOpen)
            {
                SetButtonSetting();
                curFoucsDay = 1;
            }
            UpdateUI();
            if (firstOpen)
            {
                yield return TweenIn();
            }
            firstOpen = false;
        }
        /// <summary>
        /// 設定按鈕設定
        /// </summary>
        private void SetButtonSetting()
        {

            tab1.OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                ClickTabContentChange();
            });

            tab2.OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                ClickTabContentChange();
            });

            tab3.OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                ClickTabContentChange();
            });

            tab4.OnValueChangedAsObservable()
            .Subscribe(_ =>
            {
                ClickTabContentChange();
            });


        }


        /// <summary>
        /// 根據proto去更新UI
        /// </summary>
        private void UpdateUI()
        {
            SetTimeOut();

            var fixedData = LoadResource.Instance.DayGoalsData;
            var period = proto13601.period;
            titleText.text = LocalizeKey.SettingTitle;
            finishTitleText.text = LocalizeKey.FinishCount;
            finishCountText.text = proto13601.num.ToString();
            int[] daySevenIconIDs = null;

            switch (period)
            {
                case 1:
                    daySevenIconIDs = JsonConvert.DeserializeObject<int[]>(fixedData.data_constant["day_item1"].val.ToString());
                    break;
                case 4:
                    daySevenIconIDs = JsonConvert.DeserializeObject<int[]>(fixedData.data_constant["day_item2"].val.ToString());
                    break;
                case 7:
                    daySevenIconIDs = JsonConvert.DeserializeObject<int[]>(fixedData.data_constant["day_item3"].val.ToString());
                    break;
                default:
                    daySevenIconIDs = JsonConvert.DeserializeObject<int[]>(fixedData.data_constant["day_item"].val.ToString());
                    break;
            }

            ///顯示七日UI

            for (int i = 0; i < daySevenIconIDs.Length; i++)
            {
                var id = daySevenIconIDs[i];
                var item = sevenDayObjs[i];
                //item.transform.Find("Item-Image").GetComponent<Image>().sprite = LoadResource.Instance.GetItemIcon((uint)id);
                LoadResource.Instance.GetItemIconAsync((uint)id, sprite =>
                {
                    if (item.transform.Find("Item-Image").GetComponent<Image>() != null)
                    {
                        item.transform.Find("Item-Image").GetComponent<Image>().sprite = sprite;
                    }
                });
                var itemBackImg = item.GetComponent<Image>();


                if ((i + 1) > proto13601.cur_day)
                {
                    itemBackImg.sprite = sevenDayItenBackgrounds[0];
                }
                else
                {
                    if (firstOpen)
                    {
                        if ((i + 1) == 1)
                        {
                            itemBackImg.sprite = sevenDayItenBackgrounds[2];
                        }
                        else
                        {
                            itemBackImg.sprite = sevenDayItenBackgrounds[1];
                        }
                    }


                    item.GetComponent<Button>().onClick.RemoveAllListeners();

                    //註冊點擊切換
                    var day = (i + 1);

                    item.GetComponent<Button>().OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            curFoucsDay = day;
                            UpdateContentData(period, curFoucsDay);
                            foreach (var obj in sevenDayObjs)//修改原本被框選狀態的七日選項
                            {
                                if (obj.GetComponent<Image>().sprite == sevenDayItenBackgrounds[2])
                                {
                                    obj.GetComponent<Image>().sprite = sevenDayItenBackgrounds[1];
                                }
                            }
                            itemBackImg.sprite = sevenDayItenBackgrounds[2];
                        }).AddTo(this);

                }


            }

            //設置上方寶箱
            treasureRewardStatusList.Clear();
            float protoNum = (float)proto13601.num;
            if (protoNum <= 20)
            {

                SevenDaybar.fillAmount = (protoNum * 5f / 4f) / 100f;
            }
            else if (protoNum <= 50)
            {
                SevenDaybar.fillAmount = (25f + ((protoNum - 20f) * 5f / 6f)) / 100f;
            }
            else if (protoNum <= 70)
            {

                SevenDaybar.fillAmount = (50f + ((protoNum - 50f) * 5f / 4f)) / 100f;
            }
            else
            {
                SevenDaybar.fillAmount = (75f + ((protoNum - 70f) * 5f / 6f)) / 100f;
            }

            // SevenDaybar.fillAmount = (float)proto13601.num/100f;


            var finishList = proto13601.finish_list.ToList();
            finishList.SmallSizeSort("goal_id");
            uint num = 0;
            foreach (var treasureData in finishList)
            {
                var fixedTreasureData = LoadResource.Instance.DayGoalsData.data_all_target[period.ToString()][treasureData.goal_id.ToString()][0];

                treasureRewardStatusList.Add(fixedTreasureData);

                var treasureBtn = treasures[num].transform.Find("Treasure-Btn").GetComponent<Button>();

                if (treasureData.status == 0)//等待領取
                {
                }

                if (treasureData.status == 1)//等待領取
                {
                    treasures[num].transform.Find("Oval").GetComponent<Image>().color = new Color(0.925f, 0.667f, 0.274f);
                }

                if (treasureData.status == 2)//已領取
                {
                    treasures[num].transform.Find("Oval").GetComponent<Image>().color = new Color(0.925f, 0.667f, 0.274f);
                    switch (fixedTreasureData.goal) //不同的解鎖寶箱sprite
                    {
                        case 20:
                            treasureBtn.GetComponent<Image>().sprite = treasureOpenSprite[0];
                            break;
                        case 50:
                            treasureBtn.GetComponent<Image>().sprite = treasureOpenSprite[0];
                            break;
                        case 70:
                            treasureBtn.GetComponent<Image>().sprite = treasureOpenSprite[1];
                            break;
                        case 100:
                            treasureBtn.GetComponent<Image>().sprite = treasureOpenSprite[2];
                            break;
                    }
                }

                treasureBtn.onClick.RemoveAllListeners();
                var number = num;
                treasureBtn.onClick.AddListener(() =>
                {
                    switch (treasureData.status)
                    {
                        case 0:
                            var sp = Instantiate(itemTipPrefab, transform);
                            sp.transform.position = treasures[number].transform.position;
                            sp.transform.localScale = Vector3.one;
                            var reward = JsonConvert.DeserializeObject<uint[][]>(fixedTreasureData.award.ToString());
                            List<(uint a, uint b)> items = new List<(uint a, uint b)>();

                            foreach (var item in reward)
                            {
                                items.Add((item[0], item[1]));
                            }
                            sp.GetComponent<ItemTip>().Init(
                                items,
                                new ItemTip.DisplaySpecifity()
                                {
                                    forward = ItemTip.DisplaySpecifity.Forward.Top,
                                    gradeDistance = 180
                                }
                              );
                            //出現提示視窗
                            break;
                        case 1:
                            ReceiveCB(5, 0, treasureData.goal_id, 0);
                            //等待領取 proto13602
                            break;
                        case 2:
                            //已領取
                            break;

                    }
                });
                num++;
            }

            //下方獎勵列表更新
            UpdateContentData(period, curFoucsDay);
            CheckRedDotStatus();
        }




        /// <summary>
        /// 切換下方資料
        /// </summary>
        private void UpdateContentData(int period, int curDay)
        {

            ClearList(tab1ListContent);
            ClearList(tab2ListContent);
            ClearList(tab3ListContent);
            ClearList(tab4ListContent);
            //生成福利領取
            var fixedData_Welfare = LoadResource.Instance.DayGoalsData.data_welfarecollection[period.ToString()][curDay.ToString()];
            var welfareList = proto13601.welfare_list.Where(list => list.day == curDay).ToList();
            welfareList.SpeciftyCondition(v => v.status == 0);
            welfareList.SpeciftyCondition(v => v.status == 1);
            //  welfareList.LargeSizeSort("status");
            //  welfareList.AddRange(proto13601.welfare_list.Where(list => list.day == curDay && list.status == 2).ToList());




            // welfareList.InsertRange(0, proto13601.welfare_list.Where(list => list.day == curDay && list.status == 2).ToList());
            foreach (var cellData in welfareList)
            {

                var fixedData = fixedData_Welfare.FirstOrDefault(list => list.goal_id == cellData.goal_id);
                var sp = Instantiate(RewardCellPrefab);

                sp.transform.SetParent(tab1ListContent, false);
                var conditions = JsonConvert.DeserializeObject<object[][]>(fixedData.condition.ToString());
                List<(string, int)> complete = new List<(string, int)>();

                for (int i = 0; i < conditions.Length; i++)
                {
                    var data = conditions[i];
                    complete.Add((data[0].ToString(), int.Parse(data[1].ToString())));
                }

                //var protoData = proto13601.welfare_list.FirstOrDefault(list => list.goal_id == cellData.goal_id);
                SevenDayRewardCell rewardCell = sp.GetComponent<SevenDayRewardCell>();

                rewardCell.Init
                 (
                    new SevenDayRewardCell.SevenDayRewardData()
                    {
                        goal_id = (int)cellData.goal_id,
                        status = cellData.status,
                        _title = LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData, fixedData.desc),
                        CanTip =SevenDayRewardCell.SevenDayRewardData.RightTopTip.Progress,
                        itemRewardList = JsonConvert.DeserializeObject<int[][]>(fixedData.award1.ToString()),
                        itemConditionList = complete,
                        targetVal = cellData.target_val,
                        value = cellData.value,
                        ReceiveCB = () => { ReceiveCB(1, (uint)curDay, cellData.goal_id, (uint)tab1ListContent.childCount); },
                        NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fixedData.show_icon); }
                    }
                 );
            }

            //生成Tab1 ,Tab2的物件
            var fixedData_Other = LoadResource.Instance.DayGoalsData.data_growthtarget[period.ToString()][curDay.ToString()];
            bool tab1Name = false;
            bool tab2Name = false;
            List<SevenDayRewardCell.SevenDayRewardData> growSortList2 = new List<SevenDayRewardCell.SevenDayRewardData>();
            List<SevenDayRewardCell.SevenDayRewardData> growSortList3 = new List<SevenDayRewardCell.SevenDayRewardData>();

            var otherData = proto13601.grow_list.Where(list => list.day == curDay).ToList();
            otherData.SpeciftyCondition(v => v.status == 0);
            otherData.SpeciftyCondition(v => v.status == 1);
            //  otherData.LargeSizeSort("status");
            //  otherData.AddRange( proto13601.grow_list.Where(list => list.day == curDay && list.status == 2).ToList());
            // otherData.InsertRange(0,proto13601.grow_list.Where(list => list.day == curDay && list.status == 2).ToList());

            foreach (var cellData in otherData)
            {
                Debug.Log("執行順序=>" + cellData.status);


                var fixedData = fixedData_Other.FirstOrDefault(list => list.goal_id == cellData.goal_id);

                if (!tab1Name&&cellData.target_type==1) {
                    tab2.titleName =LocaleManager.Instance.GetLocalizedString( LocaleTableEnum.ResourceData, fixedData.type_name);
                    tab1Name = true;
                }
                if (!tab2Name && cellData.target_type == 2)
                {
                    tab3.titleName = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fixedData.type_name);
                    tab2Name = true;
                }

                var sp = Instantiate(RewardCellPrefab);
                var rewardCell = sp.GetComponent<SevenDayRewardCell>();
                rewardCell.Init
                 (
                    new SevenDayRewardCell.SevenDayRewardData()
                    {
                        goal_id = (int)cellData.goal_id,
                        status = cellData.status,
                        _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fixedData.desc),
                        CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.Progress,
                        itemRewardList = JsonConvert.DeserializeObject<int[][]>(fixedData.award1.ToString()),
                        targetVal = cellData.progress[0].target_val,
                        value = cellData.progress[0].value,
                        NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fixedData.show_icon); }
                    }
                 );

                //初始化cell
                switch (cellData.target_type)
                {
                    case 1:
                        sp.transform.SetParent(tab2ListContent, false);
                        rewardCell.aData.ReceiveCB = () => { ReceiveCB(2, (uint)curDay, cellData.goal_id, (uint)tab2ListContent.childCount); };
                        growSortList2.Add(rewardCell.aData);
                        break;

                    case 2:
                        sp.transform.SetParent(tab3ListContent, false);
                        rewardCell.aData.ReceiveCB = () => { ReceiveCB(3, (uint)curDay, cellData.goal_id, (uint)tab3ListContent.childCount); };
                        growSortList3.Add(rewardCell.aData);
                        break;
                }
            }

            /*for (int i = 0; i < growSortList2.Count; i++)
            {
                var child = growSortList2[i];
                var objs = tab2ListContent.GetComponentsInChildren<SevenDayRewardCell>();
                var tarObj = objs.FirstOrDefault(obj => obj.aData.id == child.id);
                tarObj.transform.SetSiblingIndex(i); 
            }
            for (int i = 0; i < growSortList3.Count; i++)
            {
                var child = growSortList3[i];
                var objs = tab3ListContent.GetComponentsInChildren<SevenDayRewardCell>();
                var tarObj = objs.FirstOrDefault(obj => obj.aData.id == child.id);
                tarObj.transform.SetSiblingIndex(i); 
            }*/
            //生成福利禮包
            // var fixedData_HalfDiscount= LoadResource.Instance.DayGoalsData.data_halfdiscount[period.ToString()][curDay.ToString()];

            var fixedData_HalfDiscount = LoadResource.Instance.DayGoalsData.data_halfdiscount[period.ToString()];
            var filterData = fixedData_HalfDiscount.Where(data => data.Value.Where(v => v.day == curDay).Count() >= 1).ToList();

            var vipLev = MessageResponseData.Instance.UserData.vip_lev;
            int index = 0;

            if (tab4ListContent.childCount > 0)
            {
                for (int i = 0; i < tab4ListContent.childCount; i++)
                {
                    var child = tab4ListContent.GetChild(i);
                    Destroy(child.gameObject);
                }
            }

            List<SevenDayRewardCell.SevenDayRewardData> sortList = new List<SevenDayRewardCell.SevenDayRewardData>();

            foreach (var dicListData in filterData)
            {
                foreach (var cellData in dicListData.Value)
                {
                    if (vipLev >= cellData.vip)
                    {
                        var sp = Instantiate(RewardCellPrefab);
                        sp.transform.SetParent(tab4ListContent, false);
                        var rewardCell = sp.GetComponent<SevenDayRewardCell>();
                        var status = proto13601.price_list.FirstOrDefault(list => list.day == cellData.id).status;
                        rewardCell.Init
                         (
                            new SevenDayRewardCell.SevenDayRewardData()
                            {
                                id = (int)cellData.id,
                                _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, cellData.desc),
                                _price = (int)cellData.price1,
                                _discountPrice = (int)cellData.price2,
                                isTask = false,
                                value = (uint)(status > 0 ? 1 : 0),
                                targetVal = 1,
                                status = status,
                                CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.PurchasingProgress,
                                itemRewardList = JsonConvert.DeserializeObject<int[][]>(cellData.award1.ToString()),
                                ReceiveCB = () => { ReceiveCB(4, (uint)curDay, (uint)cellData.id, (uint)index); },
                            }
                         );
                        sortList.Add(rewardCell.aData);
                    }
                    index++;
                }
            }

            sortList.SmallSizeSort("status");
            for (int i = 0; i < sortList.Count; i++)
            {
                var child = sortList[i];
                var objs = tab4ListContent.GetComponentsInChildren<SevenDayRewardCell>();
                var tarObj = objs.FirstOrDefault(obj => obj.aData.id == child.id);
                tarObj.transform.SetSiblingIndex(i); //GetComponent<SevenDayRewardCell>().SetSortToLastBottom();
            }


            /*    for (int i=0;i<tab4ListContent.childCount;i++) 
                {
                    var child=tab4ListContent.GetChild(i);
                    child.GetComponent<SevenDayRewardCell>().SetSortToLastBottom();
                }*/

        }

        #region button click recation
        /// <summary>
        /// 隨著點擊tab切換content的狀態
        /// </summary>
        private void ClickTabContentChange()
        {
            tab1ListContent.gameObject.SetActive(false);
            tab2ListContent.gameObject.SetActive(false);
            tab3ListContent.gameObject.SetActive(false);
            tab4ListContent.gameObject.SetActive(false);

            if (tab1.isOn)
            {
                var rect = tab1ListContent.GetComponent<RectTransform>();
                tab1ListContent.gameObject.SetActive(true);
                rect.anchoredPosition = Vector3.zero;
                SevenDayContentScrollRect.content = rect;
            }
            if (tab2.isOn)
            {
                var rect = tab2ListContent.GetComponent<RectTransform>();
                tab2ListContent.gameObject.SetActive(true);
                rect.anchoredPosition = Vector3.zero;
                SevenDayContentScrollRect.content = rect;
            }
            if (tab3.isOn)
            {
                var rect = tab3ListContent.GetComponent<RectTransform>();
                tab3ListContent.gameObject.SetActive(true);
                rect.anchoredPosition = Vector3.zero;
                SevenDayContentScrollRect.content = rect;
            }
            if (tab4.isOn)
            {
                var rect = tab4ListContent.GetComponent<RectTransform>();
                tab4ListContent.gameObject.SetActive(true);
                rect.anchoredPosition = Vector3.zero;
                SevenDayContentScrollRect.content = rect;
            }

        }

        #endregion

        /// <summary>
        /// 檢查七日選擇紅點
        /// </summary>
        private void CheckRedDotStatus()
        {
            for (int i = 1; i <= sevenDayObjs.Length; i++)
            {
                if (proto13601.cur_day < i)
                {
                    continue;
                }
                bool openRedDot = false;
                var dayObj = sevenDayObjs[(i - 1)];

                if (proto13601.welfare_list.Where(list => list.day == i && list.status == 1).Count() > 0) openRedDot = true;
                if (proto13601.grow_list.Where(list => list.day == i && list.status == 1).Count() > 0) openRedDot = true;

                var discountData = LoadResource.Instance.DayGoalsData.data_halfdiscount[proto13601.period.ToString()];

                foreach (var priceData in proto13601.price_list)
                {
                    var disFD = discountData[priceData.day.ToString()][0];
                    if (proto13601.cur_day >= disFD.day && disFD.day == i && disFD.price2 == 0 && priceData.status == 0) openRedDot = true;
                }

                if (openRedDot)
                {
                    dayObj.transform.Find("RedDot").gameObject.SetActive(true);
                }
                else
                {
                    dayObj.transform.Find("RedDot").gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 清理Content
        /// </summary>
        /// <param name="content"></param>
        private void ClearList(Transform content)
        {
            if (content.childCount > 0)
            {
                for (int i = 0; i < content.childCount; i++)
                {
                    var child = content.GetChild(i);
                    Destroy(child.gameObject);
                }
                content.DetachChildren();
            }
        }
        private void SetTimeOut()
        {

            remainSecond = proto13601.end_time;

            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true; // 設置為自動重置
            _timer.Enabled = true; // 啟動計時器

            this.ObserveEveryValueChanged(_ => _.remainSecond)
             .Subscribe(t =>
             {
                 Debug.Log("測試偵查");
                 if (remainSecond > 0)
                 {
                     timeText.text = LocalizeKey.RemainTime + UnixTime.CalcTimeOfDayAtSecond(remainSecond, true);
                 }
             }).AddTo(this);

        }


        public void OnDestroy()
        {

            _timer.Elapsed -= TimerElapsed;
        }
        /// <summary>
        /// 計時器呼叫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 在這裡放置計時器每次觸發時要執行的任務或程式碼
            Debug.Log("當前秒數=>" + remainSecond);
            if (remainSecond > 0)
            {
                remainSecond -= 1;

            }
        }
        /// <summary>
        /// 頁面tween進入
        /// </summary>
        /// <returns></returns>
        private IEnumerator TweenIn()
        {
            titleText.gameObject.SetCanvasGroup(0);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.localPosition = new Vector3(0, 263, 0);
            StartCoroutine(titleText.gameObject.eSetCanvasGroup(1, duration: 1));
            titleRect.DOLocalMoveX(-488, 1);


            timeText.gameObject.SetCanvasGroup(0);
            var timeRect = timeText.GetComponent<RectTransform>();
            timeRect.localPosition = new Vector3(-1000, 201, 0);
            StartCoroutine(timeText.gameObject.eSetCanvasGroup(1, duration: 1));
            timeRect.DOLocalMoveX(-488, 1);

            float interTime = 0;
            foreach (var obj in sevenDayObjs)
            {
                obj.SetCanvasGroup(0);
                obj.transform.localScale = new Vector3(1.3f, 1.3f, 1);
                IEnumerator tween()
                {
                    var waitSecond = interTime;
                    yield return new WaitForSeconds(waitSecond);
                    obj.transform.DOScale(Vector3.one, 1);
                    yield return obj.eSetCanvasGroup(1, duration: 1);

                }
                StartCoroutine(tween());
                interTime += 0.15f;
            }
            treasureMask.sizeDelta = new Vector2(0, 175);


            treasureMask.DOSizeDelta(new Vector2(1080, 175), 1);

            //ui tween 初始點
            var charaRect = Chara.GetComponent<RectTransform>();
            charaRect.localPosition = new Vector3(590, -110, 0);
            Chara.SetCanvasGroup(0);
            charaRect.DOLocalMoveX(290, 1);
            yield return Chara.eSetCanvasGroup(1, duration: 1);



        }




    }

}