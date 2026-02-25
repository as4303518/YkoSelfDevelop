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
using YKO.Common.Util;
using YKO.Network;
using YKO.Support.Expansion;
using System.Timers;

namespace YKO.SevenDay
{

    public class AdventureNoteView : MonoBehaviour
    {

        public class LocalizeKey
        {
            public static readonly string AdventureTitle = "冒險日記";
            public static readonly string AdventureExperienceTitle = "冒險日記經驗";
            public static readonly string NextStageLevelRewardTitle = "下一階段等級獎勵";
            public static readonly string LevelReward = "等級獎勵";
            public static readonly string AdventureTipText = "七日內完成每日任務升級日誌,將可獲得對應等級獎勵";
            public static readonly string RemainTime = "剩餘時間 : ";
            public static readonly string AdventureNoteDescription = "冒險筆記說明";
        }


        [Header("TopUI")]

        /// <summary>
        /// 時間文字
        /// </summary>
        [SerializeField]
        private Text timeText;


        /// <summary>
        /// 冒險筆記標題文字
        /// </summary>
        [SerializeField]
        private Text noteTitleText;
        /// <summary>
        /// 冒險筆記等級文字
        /// </summary>
        [SerializeField]
        private Text noteLevelText;

        /// <summary>
        /// 進度條文字
        /// </summary>
        [SerializeField]
        private Text barTitleText;
        /// <summary>
        /// 進度條文字
        /// </summary>
        [SerializeField]
        private Text barText;
        /// <summary>
        /// 進度條圖片
        /// </summary>
        [SerializeField]
        private Image barImg;
        /// <summary>
        /// 下一階段等級獎勵
        /// </summary>
        [SerializeField]
        private Text nextStageLevelRewardText;

        /// <summary>
        /// 查看等級獎勵文字
        /// </summary>
        [SerializeField]
        private Text checkLevelRewardText;
        /// <summary>
        /// 查看等級獎勵按鈕
        /// </summary>
        [SerializeField]
        private Button checkLevelRewardBtn;
        /// <summary>
        /// 提示是否有尚未領取的等級獎勵
        /// </summary>
        [SerializeField]
        private Transform checkLevelRewardRedDot;

        /// <summary>
        /// 七日提示文字
        /// </summary>
        [SerializeField]
        private Text noteAdventureTipText;
        /// <summary>
        /// 七日提示文字
        /// </summary>
        [SerializeField]
        private Button noteAdventureTipBtn;

        /// <summary>
        /// 下方的累積儲值
        /// </summary>
        [SerializeField]
        private SevenDayRewardCell bottomAccumulateStoredCell;



        #region prefab or Parent
        /// <summary>
        /// itemIcon prefab
        /// </summary>
        [SerializeField]
        private GameObject itemPrefab;
        /// <summary>
        /// 獎勵羅列子物件
        /// </summary>
        [SerializeField]
        private GameObject sevenDayCellPrefab;

        /// <summary>
        ///下個階段獎勵道具陣列 
        /// </summary>
        [SerializeField]
        private Transform nextStageLevelRewardContent;

        /// <summary>
        ///下個階段獎勵道具陣列 
        /// </summary>
        [SerializeField]
        private Transform BottomTaskContent;


        #endregion

        #region Data or Cb
        /// <summary>
        /// proto 13604
        /// </summary>
        private Proto_13604_Response proto13604;
        /// <summary>
        /// 接收回調
        /// </summary>
        public Action<uint> ReceiveCB;

        /// <summary>
        /// 打開冒險等級彈窗
        /// </summary>
        public Action OpenLevelRewardPopup;
        /// <summary>
        /// 打開文字介紹彈窗
        /// </summary>
        public Action<string, string, Vector3> OpenDescriptionPopup;

        /// <summary>
        /// 打開累積獎勵彈窗
        /// </summary>
        public Action OpenAccumulatePopup;
        /// <summary>
        /// 刷新時間紀錄
        /// </summary>
        private TimeSpan _refresh_time;
        /// <summary>
        /// 時間計算
        /// </summary>
        private Timer _timer;

        #endregion

        public void Init(Proto_13604_Response _data)
        {
            proto13604 = _data;
            Register();
            UpdateUI();

        }
        /// <summary>
        /// 登記proto反饋
        /// </summary>
        private void Register()
        {
            checkLevelRewardBtn.onClick.RemoveAllListeners();
            noteAdventureTipBtn.onClick.RemoveAllListeners();
            checkLevelRewardBtn.OnClickAsObservable().Subscribe(_ => { ClickCheckLevelRewardButton(); });
            noteAdventureTipBtn.OnClickAsObservable().Subscribe(_ =>
            {
                var fixedData = LoadResource.Instance.DayGoalsNewData.data_constant["tips"];
                OpenDescriptionPopup(LocalizeKey.AdventureNoteDescription, fixedData.desc, noteAdventureTipBtn.transform.position);
            });


        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            var period = proto13604.period;
            var fixedData = LoadResource.Instance.DayGoalsNewData;

            SetTimeOut();

            noteTitleText.text = LocalizeKey.AdventureTitle;
            noteLevelText.text = proto13604.lev.ToString();
            barTitleText.text = LocalizeKey.AdventureExperienceTitle;
            noteAdventureTipText.text = LocalizeKey.AdventureTipText;
            long nextLevel = 0;

            if (proto13604.lev < 8)
            {
                nextLevel = (proto13604.lev + 1);
            }
            else
            {
                nextLevel = 8;
            }

            var maxExp = fixedData.data_make_lev_list[period.ToString()][nextLevel.ToString()].exp;
            long LastmaxExp = 0;
            if (proto13604.lev <= 1)
            {
                LastmaxExp = 0;
            }
            else if (proto13604.lev >= 2)
            {
                LastmaxExp = fixedData.data_make_lev_list[period.ToString()][nextLevel.ToString()].exp;
            }


            barText.text = proto13604.exp + "/" + maxExp;


            barImg.fillAmount = (float)(proto13604.exp - LastmaxExp) / (float)(maxExp - LastmaxExp);

            nextStageLevelRewardText.text = LocalizeKey.NextStageLevelRewardTitle;

            //獎勵列表紅點
            MessageResponseData.Instance.SubscribeLive<Proto_13607_Response>(13607,
                proto =>
                {
                    checkLevelRewardRedDot.gameObject.SetActive(proto.lev > proto.reward_list.Length);
                }, true, this);

            //設置獎勵列表
            var nextStageRewards = JsonConvert.DeserializeObject<int[][]>(fixedData.data_make_lev_list[period.ToString()][nextLevel.ToString()].reward.ToString());
            nextStageLevelRewardContent.ClearChildObj();

            if (proto13604.lev < 8)
            {
                foreach (var nextStageReward in nextStageRewards)
                {
                    var sp = Instantiate(itemPrefab);
                    sp.transform.SetParent(nextStageLevelRewardContent, false);
                    sp.GetComponent<ItemIcon>().Init((uint)nextStageReward[0], amount: (uint)nextStageReward[1], null);
                }
            }
            //右方
            checkLevelRewardText.text = LocalizeKey.LevelReward;
            UpdateAccumulateStored();
            CreateBottomTasks();




        }
        /// <summary>
        /// 根據proto13604 更新儲值獎勵
        /// </summary>
        private void UpdateAccumulateStored()
        {
            var fixedDatas = LoadResource.Instance.DayGoalsNewData.data_charge_list[proto13604.period.ToString()];
            //篩選儲值獎勵 尚未完成或尚未領取 排列後的第一個

            var tasks =
                proto13604.list
                .Where(v => fixedDatas.ContainsKey(v.id.ToString()))
                .Where(v => (v.finish == 1) || (v.finish == 0))
                .ToList();
            Proto_13604_Response.List tarTask = null;
            //如果有尚未完成跟完成未領取則顯示  否則會顯示最後一個獎勵並標示已完成
            if (tasks.Count > 0)
            {
                tasks.Sort((a, b) =>
                {
                    if (a.id <= b.id)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });
                tarTask = tasks[0];
                /* var tarTask = tasks[0];
                  var fixedData = fixedDatas[tarTask.id.ToString()];

                  bottomAccumulateStoredCell
                   .Init(new SevenDayRewardCell.SevenDayRewardData() { 
                  goal_id=(int)tarTask.id,
                   status = tasks[0].finish,
                  statusDisplay= SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                  _title=fixedData.desc+string.Format("({0}/{1})",tarTask.value,tarTask.target_val),
                   itemRewardList = JsonConvert.DeserializeObject<int[][]>( fixedData.award.ToString()),
                   CanTip=SevenDayRewardCell.SevenDayRewardData.RightTopTip.CheckOther,
                   ReceiveCB = () => {ReceiveCB(tarTask.id);},
                    NextCB = () => { Transimt.TransimtOfNum(fixedData.show_icon); },
                    CheckOtherCB = OpenAccumulatePopup
              });*/

            }
            else//從已完成的任務找顯示
            {
                var tasksFinish =
                    proto13604.list
                    .Where(v => fixedDatas.ContainsKey(v.id.ToString()))
                    .Where(v => (v.finish == 2))
                    .ToList();
                tasksFinish.Sort((a, b) =>
                {

                    if (a.id >= b.id)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });
                tarTask = tasksFinish[0];
                /*
                var fixedData = fixedDatas[displayID.ToString()];
                bottomAccumulateStoredCell
                 .Init(new SevenDayRewardCell.SevenDayRewardData()
                 {
                     goal_id = (int)displayID,
                     status = tasksFinish[0].finish,
                      statusDisplay= SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                     _title = fixedData.desc,
                     itemRewardList = JsonConvert.DeserializeObject<int[][]>(fixedData.award.ToString()),
                     CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.CheckOther,
                     NextCB = () => { Transimt.TransimtOfNum(fixedData.show_icon); },
                     CheckOtherCB = OpenAccumulatePopup
                 });*/
            }

            var fixedData = fixedDatas[tarTask.id.ToString()];
            bottomAccumulateStoredCell
             .Init(new SevenDayRewardCell.SevenDayRewardData()
             {
                 goal_id = (int)tarTask.id,
                 status = tasks[0].finish,
                 statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                 _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fixedData.desc) + string.Format("({0}/{1})", tarTask.value, tarTask.target_val),
                 itemRewardList = JsonConvert.DeserializeObject<int[][]>(fixedData.award.ToString()),
                 CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.CheckOther,
                 ReceiveCB = () => { ReceiveCB(tarTask.id); },
                 NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fixedData.show_icon); },
                 CheckOtherCB = OpenAccumulatePopup
             });





        }
        /// <summary>
        /// 生成任務狀態
        /// </summary>
        private void CreateBottomTasks()
        {
            BottomTaskContent.ClearChildObj();
            var fixedDatas = LoadResource.Instance.DayGoalsNewData.data_group_list[proto13604.period.ToString()];
            var dynamicList = proto13604.list.Where(v => fixedDatas.ContainsKey(v.id.ToString())).ToList();
            dynamicList.Sort((a, b) =>
            {
                if (a.finish < b.finish)
                {
                    return -1;

                }
                else
                {
                    return 1;
                }
            });
            dynamicList.Sort((a, b) =>
            {

                if (a.finish == 1)
                {
                    return -1;

                }
                else
                {
                    return 1;
                }
            });


            foreach (var list in dynamicList)
            {
                var sp = Instantiate(sevenDayCellPrefab);
                var fixedData = fixedDatas[list.id.ToString()];
                sp.transform.SetParent(BottomTaskContent, false);
                var sevenCell = sp.GetComponent<SevenDayRewardCell>();
                sevenCell.Init(
                    new SevenDayRewardCell.SevenDayRewardData()
                    {
                        goal_id = (int)list.id,
                        status = list.finish,
                        statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                        _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fixedData.desc),
                        itemRewardList = JsonConvert.DeserializeObject<int[][]>(fixedData.award.ToString()),
                        CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.Progress,
                        targetVal = list.target_val,
                        value = list.value,
                        ReceiveCB = () =>
                        {
                            sevenCell.CompleteMask();
                            ReceiveCB(list.id);
                        },
                        NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fixedData.show_icon); }
                    });
            }

        }
        /// <summary>
        /// 變更紅點顯示
        /// </summary>
        public void CheckRedDot()
        {
            var proto13607 = MessageResponseData.Instance.ProtoResponse13607;
            checkLevelRewardRedDot.gameObject.SetActive(proto13607.lev > proto13607.reward_list.Length);

        }


        /// <summary>
        /// 點擊等級獎勵
        /// </summary>

        private void ClickCheckLevelRewardButton()
        {

            OpenLevelRewardPopup();

        }
        /// <summary>
        /// 設定時間
        /// </summary>
        private void SetTimeOut()
        {
            DateTime dateTime = UnixTime.FromUnixTime(proto13604.end_time);
            _refresh_time = dateTime - UnixTime.Now();

            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true; // 設置為自動重置
            _timer.Enabled = true; // 啟動計時器

            timeText.text = LocalizeKey.RemainTime + proto13604.end_time;

            this.ObserveEveryValueChanged(x => x._refresh_time).Subscribe(
                time =>
                {
                    timeText.text = LocalizeKey.RemainTime + UnixTime.CalcTimeOfDay(time, true);
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
            if (_refresh_time.TotalSeconds > 0)
            {
                _refresh_time = _refresh_time.Subtract(TimeSpan.FromSeconds(1));
            }
        }


    }
}
