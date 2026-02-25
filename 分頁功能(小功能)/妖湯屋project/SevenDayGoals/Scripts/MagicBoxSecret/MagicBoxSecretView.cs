using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.Util;
using System.Timers;
using YKO.Support.Expansion;
using YKO.Common.UI;

namespace YKO.SevenDay
{

    public class MagicBoxSecretView : MonoBehaviour
    {

        public class LocalizeKey
        {

            public static readonly string MagicBoxSecret = "魔盒秘密";
            public static readonly string TodayProgress = "今日進度";
            public static readonly string CheckMagicBoxSecret = "查看魔盒秘密";
            public static readonly string MagicBoxSecretDescription = "魔盒秘密說明";
            public static readonly string AccumulateHopeSymbol = "已累積希望印記";
            public static readonly string RefreshTaskAt12Everyday = "每日0點刷新任務 : ";
        }


        /// <summary>
        /// 標題文字
        /// </summary>
        [SerializeField]
        private Text titleText;
        /// <summary>
        /// 時間文字
        /// </summary>
        [SerializeField]
        private Text timeText;
        /// <summary>
        /// 魔盒資訊彈窗按鈕
        /// </summary>
        [SerializeField]
        private Button magicBoxInfoBtn;

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
        /// 消耗道具標題
        /// </summary>
        [SerializeField]
        private Text consumeTitleText;
        /// <summary>
        /// 消耗道具數量
        /// </summary>
        [SerializeField]
        private Text consumeText;
        /// <summary>
        /// 查看魔盒秘密標題文字
        /// </summary>
        [SerializeField]
        private Text checkMagicBoxTitleText;
        /// <summary>
        /// 查看魔盒秘密按鈕
        /// </summary>
        [SerializeField]
        private Button checkMagicBoxBtn;
        /// <summary>
        /// 主要顯示領取任務cell
        /// </summary>
        [SerializeField]
        private List<GameObject> boxCells = new List<GameObject>();
        /// <summary>
        /// 下方的累積儲值
        /// </summary>
        [SerializeField]
        private SevenDayRewardCell bottomAccumulateStoredCell;
        /// <summary>
        /// proto資料
        /// </summary>
        private Proto_13604_Response proto13604;

        private TimeSpan _refresh_time = new TimeSpan();

        private Timer _timer;

        #region callBack
        /// <summary>
        /// 打開文字介紹談窗
        /// </summary>
        public Action<uint> ReceiveCB;
        /// <summary>
        /// 打開累積獎勵(儲值)視窗
        /// </summary>
        public Action OpenAccumulatePopup;
        /// <summary>
        /// 打開希望徽章累積獎勵
        /// </summary>
        public Action OpenHopeSymbolPopup;

        /// <summary>
        /// 打開文字介紹談窗
        /// </summary>
        public Action<string, string, Vector3> OpenDescriptionPopup;



        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_data"></param>
        public void Init(Proto_13604_Response _data)
        {
            proto13604 = _data;
            Register();
            UpdateUI();

        }
        /// <summary>
        /// 登入按鈕功能
        /// </summary>
        private void Register()
        {
            checkMagicBoxBtn.OnClickAsObservable().Subscribe(_ => OpenMagicSerectHopeSymbolPopup());
            magicBoxInfoBtn.OnClickAsObservable().Subscribe(_ => OpenMagicSerectDescription());

        }
        /// <summary>
        /// 更新UI狀態
        /// </summary>
        private void UpdateUI()
        {
            titleText.text = LocalizeKey.MagicBoxSecret;

            SetTimeOut();

            //很神奇的,玩家一開始從0等開始算
            var levListFD = LoadResource.Instance.DayGoalsNewData.data_make_lev_list[proto13604.period.ToString()];


            var tarLev = 0;

            if (proto13604.lev >= 7)
            {
                tarLev = 7;
            }
            else
            {
                tarLev = (int)proto13604.lev + 1;
            }

            var tarExp = LoadResource.Instance.DayGoalsNewData.data_make_lev_list[proto13604.period.ToString()][tarLev.ToString()];

            barTitleText.text = LocalizeKey.TodayProgress;
            barText.text = proto13604.exp + "/" + tarExp.exp;


            if (proto13604.lev <= 0)
            {
                barImg.fillAmount = (float)proto13604.exp / (float)tarExp.exp;
            }
            else
            {
                var lastLev = 0;

                if (proto13604.lev >= 7)
                {

                    lastLev = 7;
                }
                else
                {
                    lastLev = (int)proto13604.lev;
                }

                var lastExp = LoadResource.Instance.DayGoalsNewData.data_make_lev_list[proto13604.period.ToString()][lastLev.ToString()];
                barImg.fillAmount = (float)(proto13604.exp - tarExp.exp) / (float)(tarExp.exp - lastExp.exp);
            }


            consumeTitleText.text = LocalizeKey.AccumulateHopeSymbol;
            consumeText.text = proto13604.exp.ToString();

            checkMagicBoxTitleText.text = LocalizeKey.CheckMagicBoxSecret;

            var groupListFD = LoadResource.Instance.DayGoalsNewData.data_group_list[proto13604.period.ToString()];
            var dynamiclist_Group = proto13604.list.Where(v => groupListFD.ContainsKey(v.id.ToString())).ToList();
            dynamiclist_Group.SmallSizeSort("id");
            int num = 0;
            foreach (var list in dynamiclist_Group)
            {
                var cell = boxCells[num];
                var fd = groupListFD[list.id.ToString()];
                var itemInfo = JsonConvert.DeserializeObject<uint[][]>(fd.award.ToString())[0];
                cell.GetComponent<MagicBoxCell>().Init(
                    new MagicBoxCell.MagicBoxData()
                    {
                        _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd.desc),
                        itemInfo = (itemInfo[0], itemInfo[1]),
                        status = list.finish,
                        NextSceneCB = () =>
                        {
                            PageJumpController.Instance?.PageJumpByTransimtOfNum(fd.show_icon);
                        },
                        ReceiveRewardCB = () =>
                        {
                            ReceiveCB(list.id);
                        }
                    });
                num++;
            }


            var chargeListFD = LoadResource.Instance.DayGoalsNewData.data_charge_list[proto13604.period.ToString()];
            var dynamiclist_Charge = proto13604.list.Where(v => chargeListFD.ContainsKey(v.id.ToString())).ToList();

            var notFinish = dynamiclist_Charge.Where(v => v.finish == 0 || v.finish == 1).ToList();

            if (notFinish.Count > 0)//有沒完成的
            {
                notFinish.SmallSizeSort("id");
                var dynamicData = notFinish[0];
                var fd = chargeListFD[dynamicData.id.ToString()];
                bottomAccumulateStoredCell.Init(
                    new SevenDayRewardCell.SevenDayRewardData()
                    {
                        goal_id = (int)fd.goal_id,
                        id = (int)fd.id,
                        _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd.desc) + string.Format("({0}/{1})", dynamicData.value, dynamicData.target_val),
                        status = dynamicData.finish,
                        itemRewardList = JsonConvert.DeserializeObject<int[][]>(fd.award.ToString()),
                        CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.CheckOther,
                        statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                        CheckOtherCB = OpenAccumulatePopup,
                        ReceiveCB = () => { ReceiveCB((uint)fd.goal_id); },
                        NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fd.show_icon); }
                    });


            }
            else//全都完成了
            {
                dynamiclist_Charge.LargeSizeSort("id");
                var dynamicData = dynamiclist_Charge[0];
                var fd = chargeListFD[dynamicData.id.ToString()];

                bottomAccumulateStoredCell.Init(
                    new SevenDayRewardCell.SevenDayRewardData()
                    {
                        goal_id = (int)fd.goal_id,
                        id = (int)fd.id,
                        _title = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, fd.desc) + string.Format("({0}/{1})", dynamicData.value, dynamicData.target_val),
                        status = dynamicData.finish,
                        itemRewardList = JsonConvert.DeserializeObject<int[][]>(fd.award.ToString()),
                        CanTip = SevenDayRewardCell.SevenDayRewardData.RightTopTip.CheckOther,
                        statusDisplay = SevenDayRewardCell.SevenDayRewardData.CellDisplayStatus.BorderLength900,
                        NextCB = () => { PageJumpController.Instance?.PageJumpByTransimtOfNum(fd.show_icon); },
                        CheckOtherCB = OpenAccumulatePopup,
                    });
            }


        }



        /// <summary>
        /// 打開希望徽章獎勵進度彈窗
        /// </summary>
        private void OpenMagicSerectHopeSymbolPopup()
        {
            OpenHopeSymbolPopup();
        }

        /// <summary>
        /// 打開魔盒秘密說明
        /// </summary>
        private void OpenMagicSerectDescription()
        {
            var fd = LoadResource.Instance.DayGoalsNewData.data_constant["tips_1"];
            OpenDescriptionPopup(LocalizeKey.MagicBoxSecretDescription, fd.desc, magicBoxInfoBtn.transform.position);

        }

        private void SetTimeOut()
        {

            DateTime dateTime = UnixTime.FromUnixTime(proto13604.end_time);
            _refresh_time = dateTime - UnixTime.Now();

            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true; // 設置為自動重置
            _timer.Enabled = true; // 啟動計時器

            this.ObserveEveryValueChanged(x => x._refresh_time).Subscribe(time =>
            {
                timeText.text = LocalizeKey.RefreshTaskAt12Everyday + UnixTime.CalcTimeOfHour(time);
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
