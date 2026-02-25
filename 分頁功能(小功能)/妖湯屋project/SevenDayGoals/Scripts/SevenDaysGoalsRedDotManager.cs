using UniRx;
using UnityEngine;
using YKO.Network;

namespace YKO.Main
{
    public class SevenDaysGoalsRedDotManager : Singleton<SevenDaysGoalsRedDotManager>
    {
        public ReactiveProperty<int> WelfareCount { get; private set; }
        public ReactiveProperty<int> GrowCount { get; private set; }
        public ReactiveProperty<int> MissionCount { get; private set; }

        protected override void init()
        {
            base.init();

            WelfareCount = new ReactiveProperty<int>();
            GrowCount = new ReactiveProperty<int>();
            MissionCount = new ReactiveProperty<int>();
        }

        public void Bind(MonoBehaviour mono)
        {
            MessageResponseData.Instance.SubscribeLive<Proto_13601_Response>(13601, res =>
             {
                 if (res != null)
                 {
                     int welfareCount = 0;
                     int growCount = 0;

                     var welfare_list = res.welfare_list;
                     var grow_list = res.grow_list;

                     if (welfare_list != null && welfare_list.Length > 0)
                     {
                         foreach (var d in welfare_list)
                         {
                             if (d.status == 1)
                                 welfareCount++;
                         }
                     }

                     if (grow_list != null && grow_list.Length > 0)
                     {
                         foreach (var d in grow_list)
                         {
                             if (d.status == 1)
                                 growCount++;
                         }
                     }

                     WelfareCount.Value = welfareCount;
                     GrowCount.Value = growCount;
                 }
             }, true, mono);

            MessageResponseData.Instance.SubscribeLive<Proto_13604_Response>(13604, res =>
            {
                var data = MessageResponseData.Instance.GetRespose<Proto_13604_Response>(13604);
                if (data == null)
                    return;

                var list = data.list;
                if (list != null && list.Length > 0)
                {
                    int missionCount = 0;

                    foreach (var d in list)
                    {
                        if (d.finish == 1)
                            missionCount++;
                    }

                    MissionCount.Value = missionCount;
                }
            }, true, mono);
        }
    }
}