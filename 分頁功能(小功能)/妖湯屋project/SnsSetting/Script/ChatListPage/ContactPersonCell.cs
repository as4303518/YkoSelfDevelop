using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.SnsSetting;
using YKO.Common.UI;
using YKO.Main;
using static YKO.Main.SNSRedDotManager;

namespace YKO.SnsSetting
{

    /// <summary>
    /// 聯絡人讀卡
    /// </summary>
    /// 
    public class ContactPersonCell : MonoBehaviour
    {

        #region CellUI
        /// <summary>
        /// 頭像image
        /// </summary>
        [SerializeField]
        private Image avatarImg;
        /// <summary>
        /// 角色名稱
        /// </summary>
        [SerializeField]
        private Text charaName;
        /// <summary>
        /// 顯示的末條訊息
        /// </summary>
        [SerializeField]
        private Text displayMessage;

        /// <summary>
        /// 有未曾閱讀過的新話題
        /// </summary>
        [SerializeField]
        private RedDotObj haveNewTopicRedDot;

        /// <summary>
        /// 話題進行中紅點
        /// </summary>
        [SerializeField]
        private RedDotObj notCompleteRedDot;
        /// <summary>
        /// 點擊觸發區域
        /// </summary>
        [SerializeField]
        private Button clickArea;
        #endregion

        #region param
        /// <summary>
        /// 接收的數據方法
        /// </summary>
        private ContactPersonCellData aData;

        public ContactPersonCellData Data { get { return aData; } }
        #endregion


        /// <summary>
        /// 初始化
        /// 傳入角色名稱與顯示資訊
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init(ContactPersonCellData _cellData)
        {
            aData = _cellData;
            yield return UpdateUI();
            SetButtobSetting();
        }

        /// <summary>
        /// 更新UI顯示
        /// </summary>
        private IEnumerator UpdateUI()
        {
            //根據data名字 獲取人物的image頭像

            // avatarImg.sprite = Resources.Load<Sprite>("");
            charaName.text = aData.chara.ToString();
            displayMessage.text = aData.displayMessage;
            avatarImg.sprite = aData.charaAvatar;
            avatarImg.SetNativeSize();
          /*  switch (aData.state)
            {

                case ContactPersonState.NotLoading:
                    notLoading.SetActive(true);
                    notReply.gameObject.SetActive(false);
                    break;
                case ContactPersonState.NotReply:
                    notLoading.SetActive(false);
                    notReply.gameObject.SetActive(true);
                    break;
                default:
                    notLoading.SetActive(false);
                    notReply.gameObject.SetActive(false);
                    break;

            }*/

            yield return null;
        }
        /// <summary>
        /// 設定按鈕
        /// </summary>
        private void SetButtobSetting()
        {
            clickArea.OnClickAsObservable().Subscribe(_ =>
            {
                aData.enterConversationOfSnsAction();
            });
            var redDotDataArray = new []
            {
                new RedDotRegistParam<object>(
                    RedDotTypes.SNS_TOPICS_RECORD,
                    new SNSRedDotData(aData.chara_id, 0))
            };
            notCompleteRedDot.StartRegistEvent(dict => 
                {
                    if (dict.TryGetValue(RedDotTypes.SNS_TOPICS_RECORD, out var data)) 
                    {
                        var other = data.Other as SNSCharaRecordTopicStatus;
                        return other.status == 0;
                    }
                    return false;
                })
                .RegistRedDot(redDotDataArray);

            var redDotForNewTopic = new[]
            {
                new RedDotRegistParam<object>(
                    RedDotTypes.SNS_NEW_TOPICS,
                    new SNSRedDotData(aData.chara_id, 0))
            };

            haveNewTopicRedDot.StartRegistEvent().RegistRedDot(redDotForNewTopic);


        }

        #region public func
        /// <summary>
        /// 設定按鈕點擊觸發的開關
        /// </summary>
        /// <param name="state"></param>
        public void AdjustButtonEnable(bool state)
        {
            clickArea.interactable = state;
        }


        #endregion


        /// <summary>
        /// 聯絡人的傳遞數據類別
        /// </summary>
        public class ContactPersonCellData
        {
            public ContactPersonState state;
            public uint chara_id;
            public string chara;
            public string displayMessage;
            public Sprite charaAvatar;
            public Action enterConversationOfSnsAction;
            public ContactPersonCellData(ContactPersonState _state, uint _chara_id, string _chara, string _displayMessage,Sprite avatar, Action _enterConversationOfSnsAction)
            {
                state = _state;
                chara_id = _chara_id;
                chara = _chara;
                displayMessage =(!string.IsNullOrWhiteSpace( _displayMessage)&& _displayMessage!= "NaN" ) ? _displayMessage:"";
                charaAvatar = avatar;
                enterConversationOfSnsAction = _enterConversationOfSnsAction;
             }

        }

    }
}//-namespace
