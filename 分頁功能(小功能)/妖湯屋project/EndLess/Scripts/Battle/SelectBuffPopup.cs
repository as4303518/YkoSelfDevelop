using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.UI;
using YKO.Main;
using YKO.Network;
using YKO.Support.Expansion;
using static Proto_23910_Response;

namespace YKO.EndLess
{

    public class SelectBuffPopup : PopupBase
    {
        public class LocalizeKey
        {
            public static readonly string MustSelectMissionChallengeNextMission = "必須選擇一個BUFF才能繼續挑戰第{0}關";
            public static readonly string MyTeam = "我的陣容";


        }

        /// <summary>
        /// 關閉按鈕
        /// </summary>
      //  [SerializeField]
      //  private Button closeBtn;
        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Text tipText;

        [SerializeField]
        private Text myTeamText;
        /// <summary>
        /// 陣行名稱
        /// </summary>
        [SerializeField]
        private Text myTeamNameText;

        [SerializeField]
        private Image formationImg;

        [SerializeField]
        private Transform avatarContent;

        [SerializeField]
        private Transform buffContent;
        /// <summary>
        /// buff選擇能力的sprite
        /// </summary>
        [SerializeField]
        private Sprite[] buffSprites;
        /// <summary>
        /// 陣容位置 sprites
        /// </summary>
        [SerializeField]
        private Sprite[] formationSprites;

        [Header("Prefab")]
        [SerializeField]
        private GameObject avatarCellPrefab;

        [SerializeField]
        private GameObject buffCellPrefab;

        private SelectBuffPopupFunc aData;
        /// <summary>
        /// 等待玩家做出buff選擇(選完後視窗關閉,遊戲進行
        /// </summary>
        private bool waitWindowChooseFinish = false;

        public IEnumerator Init(SelectBuffPopupFunc data)
        {
            aData = data;
            cgFilter.alpha = 0f;
            cgPopup.alpha = 0f;
            popUp = cgPopup.gameObject;
            Register();
            yield return new WaitUntil(() => waitWindowChooseFinish);

        }
        private void Register()
        {

            MessageResponseData.Instance.SubscribeLive<Proto_23910_Response>(23910,
                proto => {
                    UpdateUI(proto);
                    ShowPopup();
                }, true, this);

            MessageResponseData.Instance.SubscribeLive<Proto_23911_Response>(23911,
                proto => 
                {
                    Debug.Log("SelectBuffPopup接收到23911");
                    if (proto.code>=1) {
                        waitWindowChooseFinish=true;
                        aData.selectCompleteCB();
                        ClosePopup();
                    }
                    else
                    {
                        MainManager.Instance.AddRemind(LocaleManager.Instance.ParseServerMessage(proto.msg));
                    }
                }, false, this);
          //  closeBtn.OnClickAsObservable().Subscribe(_ => ClosePopup());

        }

        private void UpdateUI(Proto_23910_Response proto23910)
        {
            avatarContent.ClearChildObj();
            buffContent.ClearChildObj();
            myTeamText.text = LocalizeKey.MyTeam;
            myTeamNameText.text = string.Format("[{0}]Lv.{1}", proto23910.formation_type, proto23910.formation_lev);
            tipText.text = string.Format(LocalizeKey.MustSelectMissionChallengeNextMission, proto23910.round);
            formationImg.sprite = formationSprites[(proto23910.formation_type - 1)];

            foreach (var partner in proto23910.partner)
            {
                var sp = Instantiate(avatarCellPrefab, avatarContent);
                sp.GetComponent<EndLessAvatarCell>().Init(partner);
            }

            var buffFD = LoadResource.Instance.EndlessData.data_buff_data;
            foreach (var Info in proto23910.list)
            {
                var tarBuffInfo = buffFD[Info.group_id.ToString()][Info.buff_id.ToString()];
                var buffEffectStr =ConvertBuffStr(LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData,tarBuffInfo.desc));

                var sp = Instantiate(buffCellPrefab, buffContent);
                sp.GetComponent<EndLessBuffCell>().Init(
                    new EndLessBuffCell.EndLessBuffCellFunc() {
                      buffEffect= buffEffectStr,
                       buffSprite=buffSprites[(int.Parse( tarBuffInfo.icon )-1)],
                        clickSelectCB = () => 
                        {
                            SendProto23911(Info.buff_id);
                        }
                    });

            }
        }
        /// <summary>
        /// 轉換buff效果文字
        /// </summary>
        private string ConvertBuffStr(string str)
        {
            string result = "";

            if (str.Contains("\n"))
            { 
            var split=str.Split('\n');
                result += split[0] + "\n<color=#808080>" + split[1]+ "</color>";
            }
            else
            {
                result = str;
            }

            return result;
        }


        /// <summary>
        ///寄出選擇的buff效果
        /// </summary>
        private void SendProto23911(UInt16 buffID)
        {
            Proto_23911_Request proto = new Proto_23911_Request();
            proto.buff_id = buffID;
            NetworkManager.Instance.Send(proto);
        }
        /// <summary>
        /// 資料
        /// </summary>
        public class SelectBuffPopupFunc
        {
            /// <summary>
            /// 選擇buff完成後的回調
            /// </summary>
            public Action selectCompleteCB;



        }



    }
}
