using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YKO.Support.Expansion;
using System;
using UnityEngine.UI;
using Fungus;
using YKO.Support;
using DG.Tweening;
using YKO.SnsSetting;
using UniRx;
using YKO.Common.UI;
using YKO.Common;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
using Newtonsoft.Json;

namespace YKO.SnsSetting
{
    /// <summary>
    /// Sns好友選擇頁面
    /// </summary>
    public class SnsCharaListControl : MonoBehaviour
    {

        #region Panel UI 
        /// <summary>
        /// 聯絡人陣列 (母物件
        /// </summary>
        [SerializeField] private RectTransform contentParent;
        /// <summary>
        /// 聯絡人滾軸
        /// </summary>
        [SerializeField] private ScrollRect contentScrollView;
        /// <summary>
        /// 關閉Sns系統按鈕
        /// </summary>
        [SerializeField] private Button closeSnsButton;

        #endregion

        #region Prefab

        /// <summary>
        /// 聯絡人按鈕
        /// </summary>
        [SerializeField] private GameObject contactPersonCellPrefab;

        #endregion

        #region Param

        /// <summary>
        /// 聯絡人obj
        /// </summary>
        private List<ContactPersonCell> contactPersonCellList = new List<ContactPersonCell>();


        /// <summary>
        /// 主控腳本
        /// </summary>
        private SnsSettingController control;

        private bool firstOpen = true;

        #endregion

        public IEnumerator Init(SnsSettingController _control)
        {
            control = _control;
            InitUI();
            yield return CreateContactPersonCellList();
            SetButtonSetting();
        }
        /// <summary>
        /// UI初始化
        /// </summary>
        private void InitUI()
        {
            contentScrollView.verticalNormalizedPosition = 1;
            contactPersonCellList.ForEach(cell => Destroy(cell.gameObject));
            contactPersonCellList.Clear();
        }

        private void SetButtonSetting()
        {
            closeSnsButton.onClick.RemoveAllListeners();
            closeSnsButton.OnClickAsObservable().Subscribe(_ => ClickCloseButton());

        }

        /// <summary>
        /// 生成聯絡人資訊
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateContactPersonCellList()
        {
            uint loadCompleteCount = 0;
            foreach (var record in control.CharaTopicList)
            {
              StartCoroutine(  LoadContactPerson(record,()=>loadCompleteCount++));
            }
            yield return new WaitUntil(() => (loadCompleteCount >= control.CharaTopicList.Count));
            contactPersonCellList.Sort((a1, a2) =>
            {
                if (a1.Data.chara_id > a2.Data.chara_id)
                    return 1;
                else
                    return -1;
            });
            contactPersonCellList.ForEach(cell => cell.transform.SetParent(contentParent, false));
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
            if (!firstOpen)
                contentScrollView.verticalNormalizedPosition = control.interFaceInfo.charaListVerticalNormalizedPosition;
            else
                firstOpen = false;
            
        }

        /// <summary>
        /// 加載聯絡人資料
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private IEnumerator LoadContactPerson(CharaSnsRecord record,Action completeCB)
        {
            var curTopic = record.GetCurLastNewTopic();
            GameObject sp = Instantiate(contactPersonCellPrefab);
            sp.transform.SetParent(transform, false);
            ContactPersonCell cell = sp.GetComponent<ContactPersonCell>();
            contactPersonCellList.Add(cell);
            cell.AdjustButtonEnable(false);

            ContactPersonState personState = ContactPersonState.None;
            string curDisplay = "";

            if (curTopic != null)
            {
                /* if (curTopic.ReplyDic != null && curTopic.ReplyDic.Count > 0)
                 {
                     var testData = JsonConvert.SerializeObject(curTopic.ReplyDic);
                     Debug.Log("#@#顯示當前話題的排列組合=>" + testData);
                 }*/
                personState = curTopic.state;
                //生成提示用

                if (curTopic.GetCurReply() != null && curTopic.GetCurReply().monoUserDialog)//因為server回傳的reply index已經沒辦法當做完成依據,所以必須另外寫判斷式
                {
                    ReplySns tarReply = null;
                    foreach (var reply in curTopic.ReplyDic.Values)
                    {
                        if (reply.GetTargetID() == curTopic.GetCurReply().mID)
                        {
                            tarReply = reply;
                            break;
                        }
                    }
                    curDisplay = tarReply.GetCurMessage();
                }
                else
                {
                    if (curTopic.GetTarReply()!=null&&curTopic.GetTarReply().isOptionReply&&curTopic.GetTarReply().GetTargetID()==0) 
                    {
                       curDisplay= curTopic.GetTarReply().GetCurMessage();
                    }
                    else
                    {
                        curDisplay = curTopic.GetCurReply()?.GetCurMessage();
                    }
                }
            }

            Sprite avatar = null;
            var iconID = uint.Parse(record.iconID.Remove(record.iconID.IndexOf("char"), 4));
            if (LoadResource.Instance.CharacterData.data_character_info.ContainsKey(iconID.ToString()))
            {   
                yield return LoadResource.Instance.GetHeroAvatar(iconID, spr =>
                {
                    avatar = spr;
                });
            }

            yield return cell.Init(new ContactPersonCell.ContactPersonCellData(
            personState,
            uint.Parse(record.charaID),
            LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, record.charaName),
            LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, curDisplay),
            avatar,
            () =>
            {
                control.SetDisplayCharaRecord(record.charaName);
                control.StartCoroutine(control.GoChatRoomPage());
                control.interFaceInfo.charaListVerticalNormalizedPosition = contentScrollView.verticalNormalizedPosition;
            }
            ));
            completeCB?.Invoke();

        }


        /// <summary>
        /// 點擊關閉Sns系統按鈕
        /// </summary>
        private void ClickCloseButton()
        {
            control.transform.parent.GetComponent<PopupBase>().ClosePopup();
        }


        /// <summary>
        /// 設定所有聯絡人的按鈕開關(避免在動畫進行時,點擊複數的狀況
        /// </summary>
        /// <param name="state"></param>
        public void SettingContactPersonButtonEnable(bool state)
        {
            foreach (var cell in contactPersonCellList)
            {
                cell.AdjustButtonEnable(state);
            }
        }




    }//--class SnsCharaList 


}//-nameSpace
