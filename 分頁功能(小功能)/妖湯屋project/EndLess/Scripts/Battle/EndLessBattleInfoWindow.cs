using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.Network;
using YKO.Support.Expansion;


namespace YKO.EndLess
{

    public class EndLessBattleInfoWindow : MonoBehaviour
    {

        public class LocalizeKey
        {
            public static readonly string IsSelectBuffEffect = "已選增益效果";

            public static readonly string AlreadyAccumulateRewardOfToday= "已累積本日獎勵";

            public static readonly string FirstClearOfMission = "[{0}]關首通獎勵";
        }

        /// <summary>
        /// 擴展按鈕
        /// </summary>
        [SerializeField]
        private Button expandBtn;
        /// <summary>
        /// 擴展案件按鈕圖示
        /// </summary>
        [SerializeField]
        private RectTransform arrowRect;
        /// <summary>
        /// 視窗背景
        /// </summary>
        [SerializeField]
        private RectTransform background;
        /// <summary>
        /// 擴展UI母物件頁
        /// </summary>
        [Header("Expand")]
        [SerializeField]
        private GameObject expandUIGroup;
        /// <summary>
        /// 已選效果標題
        /// </summary>
        [SerializeField]
        private Text buffEffectTitleText;
        /// <summary>
        /// 增益效果文字
        /// </summary>
        [SerializeField]
        private Text buffEffectText;
        /// <summary>
        /// 累積獎勵標題文字
        /// </summary>
        [SerializeField]
        private Text AccumulateRewardTitleText;
        /// <summary>
        /// 獎勵1文字
        /// </summary>
        [SerializeField]
        private Text Reward1Text;
        /// <summary>
        /// 獎勵2文字
        /// </summary>
        [SerializeField]
        private Text Reward2Text;

        /// <summary>
        /// 首通獎勵標題文字
        /// </summary>
        [SerializeField]
        private Text FirstClearRewardTitleText;
        /// <summary>
        /// 道具展示
        /// </summary>
        [SerializeField]
        private Transform itemContent;
        /// <summary>
        /// 領取首通獎勵
        /// </summary>
        [SerializeField]
        private Button expandReceiveBtn;

        [SerializeField]
        private GameObject itemIconPrefab;
        /// <summary>
        /// 不擴展UI母物件頁
        /// </summary>
        [Header("unExpand")]
        [SerializeField]
        private GameObject unexpandUIGroup;
        /// <summary>
        /// 未擴展首通獎勵標題文字
        /// </summary>
        [SerializeField]
        private Text unexpandFirstClearRewardTitleText;
        /// <summary>
        /// 領取首通獎勵
        /// </summary>
        [SerializeField]
        private Button unexpandReceiveBtn;


        private bool isTween = false;
        private bool IsExpand = false;//90 550

        public void Init()
        {
            Register();
            ExpandStatus(IsExpand);
        }

        private void Register()
        {

            expandBtn.OnClickAsObservable().Where(_ => !isTween).Subscribe(_ => ExpandStatus(!IsExpand));

            MessageResponseData.Instance.SubscribeLive<Proto_23902_Response>(23902, 
                proto => {
                    UpdateUI(proto);
            },true, this);



            MessageResponseData.Instance.SubscribeLive<Proto_23903_Response>(23903,
                proto => {
                    UpdateAndRegisterCurFirstRewardItemIcon(proto);
                }, true, this);
        }

        private void UpdateUI(Proto_23902_Response proto23902)
        {
            var fd = LoadResource.Instance.EndlessData;

            buffEffectTitleText.text = LocalizeKey.IsSelectBuffEffect;
            if (proto23902.buff_list.Length>0) {
                var buffInfo = proto23902.buff_list[0];
                buffEffectText.text = fd.data_buff_data[buffInfo.group_id.ToString()][buffInfo.id.ToString()].battle_desc;
            }

            AccumulateRewardTitleText.text = LocalizeKey.AlreadyAccumulateRewardOfToday;

           /* var accReward = JsonConvert.DeserializeObject<uint[][]>(proto23902.acc_reward.ToString()).ToList();
            Reward1Text.text = accReward.FirstOrDefault(v => v[0] ==1)[1].ToString();
            Reward2Text.text = accReward.FirstOrDefault(v => v[0] ==22)[1].ToString();*/
            if (proto23902.acc_reward.Length>0) {

                Reward1Text.text = proto23902.acc_reward.FirstOrDefault(v => v.base_id == 1).num.ToString();
                Reward2Text.text = proto23902.acc_reward.FirstOrDefault(v => v.base_id == 22).num.ToString();
            }
            else
            {
                Reward1Text.text = "0";
                Reward2Text.text = "0";
            }


            var proto23903 = MessageResponseData.Instance.GetRespose<Proto_23903_Response>(23903);
            UpdateAndRegisterCurFirstRewardItemIcon(proto23903);

        }
        /// <summary>
        /// 更新和註冊首通道具顯示狀態
        /// </summary>
        private void UpdateAndRegisterCurFirstRewardItemIcon(Proto_23903_Response proto23903)
        {
            //該關卡是可領取的通關狀態
            var fd= LoadResource.Instance.EndlessData.data_first_data[proto23903.type.ToString()];
            if (proto23903.status>=1)
            {
                expandReceiveBtn.gameObject.SetActive(true);
                unexpandReceiveBtn.gameObject.SetActive(true);
            }
            else
            {
                expandReceiveBtn.gameObject.SetActive(false);
                unexpandReceiveBtn.gameObject.SetActive(false);
            }

            itemContent.ClearChildObj();
            unexpandReceiveBtn.onClick.RemoveAllListeners();
            expandReceiveBtn.onClick.RemoveAllListeners();

            var itemList = JsonConvert.DeserializeObject<uint[][]>(fd[proto23903.id.ToString()].items.ToString());
            foreach (var item in itemList)
            {
                var sp = Instantiate(itemIconPrefab, itemContent);
                sp.GetComponent<ItemIcon>().Init(item[0], item[1], true, null);
            }

            FirstClearRewardTitleText.text = string.Format(LocalizeKey.FirstClearOfMission, fd[proto23903.id.ToString()].limit_id);
            unexpandFirstClearRewardTitleText.text = string.Format(LocalizeKey.FirstClearOfMission, fd[proto23903.id.ToString()].limit_id);

            unexpandReceiveBtn.OnClickAsObservable().Subscribe(_ => SendProto23904(proto23903.type, proto23903.id));
            expandReceiveBtn.OnClickAsObservable().Subscribe(_ => SendProto23904(proto23903.type, proto23903.id));
        }

        /// <summary>
        /// 設定擴展狀態
        /// </summary>
        /// <param name="_switch"></param>
        private void ExpandStatus(bool _switch)
        {
            IsExpand = _switch;
            isTween = true;
            float tweenTime = 0.2f;
            if (_switch) 
            {
                expandUIGroup.SetCanvasGroup(0);
                unexpandUIGroup.SetCanvasGroup(1);
                expandUIGroup.SetActive(true);
                StartCoroutine(unexpandUIGroup.eSetCanvasGroup(0, duration:tweenTime));
                StartCoroutine(expandUIGroup.eSetCanvasGroup(1,
                    ()=> {
                        unexpandUIGroup.SetActive(false);
                        isTween = false;
                    } ,duration: tweenTime));
                background.DOSizeDelta(new Vector2(700, 550), tweenTime);
                arrowRect.DORotate(new Vector3(0, 0, -90), tweenTime);

            }
            else
            {
                expandUIGroup.SetCanvasGroup(1);
                unexpandUIGroup.SetCanvasGroup(0);
                unexpandUIGroup.SetActive(true);
                StartCoroutine(unexpandUIGroup.eSetCanvasGroup(1, duration: tweenTime));
                StartCoroutine(expandUIGroup.eSetCanvasGroup(0,
                    () => { 
                        expandUIGroup.SetActive(false);
                        isTween = false;
                    },duration: tweenTime));

                background.DOSizeDelta(new Vector2(700, 90), tweenTime);
                arrowRect.DORotate(new Vector3(0, 0, 0), tweenTime);

            }
        }

        /// <summary>
        /// 領取獎勵
        /// </summary>
        /// <param name="_type"></param>
        public void SendProto23904(byte missionType = 5, uint id = 0)
        {
            Debug.Log("寄出23904=>" + missionType + "道具id=>" + id);
            Proto_23904_Request proto = new Proto_23904_Request();
            proto.type = missionType;//主試煉開場是5,其餘看試煉屬性
            proto.id = id;
            NetworkManager.Instance.Send(proto);

        }




    }
}
