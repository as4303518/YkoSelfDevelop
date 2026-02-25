using DG.Tweening;
using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Support.Expansion;

namespace YKO.SnsSetting
{
    /// <summary>
    /// 角色話題列表
    /// </summary>
    public class TopicHistoryControl : MonoBehaviour
    {

        #region Panel UI
        /// <summary>
        /// 角色列表按鈕
        /// </summary>
        [SerializeField]private Button charaListButton;
        /// <summary>
        /// 聊天室按鈕
        /// </summary>
        [SerializeField] private Button chatRoomButton;
        /// <summary>
        /// 篩選全部按鈕
        /// </summary>
        [SerializeField] private Button allFilterButton;
        /// <summary>
        /// 篩選支線按鈕
        /// </summary>
        [SerializeField] private Button sideStoryFilterButton;
        /// <summary>
        /// 篩選特殊按鈕
        /// </summary>
        [SerializeField] private Button specialFilterButton;
        /// <summary>
        /// 日常篩選按鈕
        /// </summary>
        [SerializeField] private Button dailyFilterButton;


        /// <summary>
        /// 聯絡人名稱
        /// </summary>
        [SerializeField] private Text contactPersonNameText;


        #endregion


        #region Prefab or Parent
        /// <summary>
        /// 話題卡
        /// </summary>
        public GameObject topicCellPrefab;
        /// <summary>
        /// 話題列表母物件
        /// </summary>
        public Transform topicListContentParent = null;
        /// <summary>
        /// 確認彈窗
        /// </summary>
        [SerializeField] private GameObject confirmWindowPopup = null;

        #endregion

        #region Param
        /// <summary>
        /// 主架構
        /// </summary>
        private SnsSettingController control;

        /// <summary>
        /// 話題按鈕陣列
        /// </summary>
        private List< GameObject> topicCellList = new List<GameObject> ();
        /*/// <summary>
        /// 按下確認後回掉的執行故事按鈕
        /// </summary>
        private Func<TopicData,  IEnumerator> executeStoryCB;*/

        /// <summary>
        /// 當前話題類別
        /// </summary>
        private TopicType topicType = TopicType.None;

        #endregion 


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_control"></param>
        /// <returns></returns>
        public IEnumerator Init(SnsSettingController _control)
        {
            
            control = _control;
           // executeStoryCB = _executeStoryCB;
            InitData();
            SetButtonSetting();
            CreateTopicCell();
            UpdateUI();
            yield return FilterTopicList(TopicType.None);

        }
        /// <summary>
        /// 初始化數據
        /// </summary>
        public void InitData() {
            topicType = TopicType.None;
            foreach (var cell in topicCellList) {
                Destroy(cell.gameObject);
            }
              topicCellList.Clear();
        }


        /// <summary>
        /// 設定按鈕
        /// </summary>
        private void SetButtonSetting()
        {
            charaListButton.onClick.RemoveAllListeners();
            chatRoomButton.onClick.RemoveAllListeners();
            allFilterButton.onClick.RemoveAllListeners();
            sideStoryFilterButton.onClick.RemoveAllListeners();
            specialFilterButton.onClick.RemoveAllListeners();
            dailyFilterButton.onClick.RemoveAllListeners();

            charaListButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ =>StartCoroutine( ClickCharaListButton()));

            chatRoomButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ =>StartCoroutine( ClickChatRoomButton()));

            allFilterButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ => StartCoroutine(FilterTopicList(TopicType.None)));

            sideStoryFilterButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ => StartCoroutine(FilterTopicList(TopicType.SideStory)));

            specialFilterButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ => StartCoroutine(FilterTopicList(TopicType.Special)));

            dailyFilterButton.OnClickAsObservable().Where(_ => control.canTween)
                .Subscribe(_ => StartCoroutine(FilterTopicList(TopicType.Daily)));

        }

        /// <summary>
        /// 更新UI顯示(篩選一開始便設定All
        /// </summary>
        private void UpdateUI()
        {
            contactPersonNameText.text= LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, control.DisplayCharaRecord.charaName);
            AlterTopicFilterColor(TopicType.None, true,false);
            AlterTopicFilterColor(TopicType.Special, false, false);
            AlterTopicFilterColor(TopicType.SideStory, false, false);
            AlterTopicFilterColor(TopicType.Daily, false, false);
        }

        /// <summary>
        /// 生成話題列表
        /// </summary>
        private void CreateTopicCell()
        {

            control.DisplayCharaRecord.topicInfoLists.Sort((a, b) =>
            {
                return a.isRead ? -1 : 1;
            });

            control.DisplayCharaRecord.topicInfoLists.Sort(
                (a, b) => {
                    return a.topicType > b.topicType ? 1 : 0;
             });

            foreach (var topic in control.DisplayCharaRecord.topicInfoLists) 
            {
                /*以下註解為通常情況下程式碼,因為現階段只有一個"日常"的對話可以撥放,而話題回顧列表沒有"日常種類"話題,為了測試故先不排除日常話題
               if (topic.topicType==TopicType.Daily) {
                    continue;
                }*/


                 GameObject sp = Instantiate(topicCellPrefab);
                topicCellList.Add(sp);
                sp.transform.SetParent(topicListContentParent, false);

                Action<TopicDisplayInfo> topicFunc = null;

                if (!control.DisplayCharaRecord.IsAllPlotOver())
                {
                    //或許後續改成點擊會出現提醒圖示
                    sp.GetComponent<TopicCell>().CloseTouch();
                }
                else
                {
                    var topicNameStr = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, topic.infoData.name);
                     topicFunc = topic => {
                        CreateConfirmPopupWindow(topicNameStr, () => {
                            Debug.Log("確認撥放話題");
                            StartCoroutine(CreateTopicDataAndExecuteStory(topic));
                        });

                    };
                }

                sp.GetComponent<TopicCell>().Init(
                    topic,
                    topicFunc,
                    SnsSettingController.SnsControlPage.SnsTopicHistory
                    );

            }



        }

        /// <summary>
        /// 生成確認視窗
        /// </summary>
        private void CreateConfirmPopupWindow(string topicName,Action cb)
        {
            GameObject sp = Instantiate(confirmWindowPopup);
            sp.transform.SetParent(transform,false);

            StartCoroutine( sp.GetComponent<ConfirmPopup>().Init(
                new ConfirmPopup.ConfirmPopupFunc() { 
                _titleText="話題回憶",
                _contentText= "要回憶["+topicName+"]嗎?",
                _confirmAction=cb,
                }
                ));
        }

        /// <summary>
        /// 開始故事
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        private IEnumerator CreateTopicDataAndExecuteStory(TopicDisplayInfo topic)
        {
            control.SendProto30202Request((uint)topic.infoData.id);
            yield return control.WaitHandleProto("30202");
           // control.CreateTopicToHistoryList(control.DisplayCharaRecord, topic.infoData.id.ToString());
            yield return ClickChatRoomButton();
        }

        /// <summary>
        /// 篩選適配的畫卡
        /// </summary>
        /// <returns></returns>
        private IEnumerator FilterTopicList(TopicType type)
        {
            if (!control.canTween) 
            {
                yield break;
            }

            control.canTween = false;
            AlterTopicFilterColor(type, true);
            AlterTopicFilterColor(topicType, false);
            topicType = type;

            foreach (var cell in topicCellList)
            {
                var topicCell = cell.GetComponent<TopicCell>();
                if (topicCell.aData.topicType==type||type==TopicType.None)
                {
                    if (!cell.activeSelf) 
                    {
                        cell.SetActive(true);
                        cell.SetCanvasGroup(0);
                       StartCoroutine( cell.eSetCanvasGroup(1));
                        RectTransform rect = cell.GetComponent<RectTransform>();
                        rect.DOKill();
                        rect.sizeDelta = new Vector2(900, 0);
                        rect.DOSizeDelta(new Vector2(900, 80),0.2f);
                    }
                }
                else
                {
                    if (cell.activeSelf)
                    {
                        cell.SetCanvasGroup(1);
                        StartCoroutine(cell.eSetCanvasGroup(0));
                        RectTransform rect = cell.GetComponent<RectTransform>();
                        rect.DOKill();
                        rect.sizeDelta = new Vector2(900, 80);
                        rect.DOSizeDelta(new Vector2(900, 0), 0.2f).OnComplete(() => { cell.SetActive(false); });
                    }

                }

            }

            control.canTween = true;

        }

        /// <summary>
        /// 調整篩選按鈕的樣式
        /// </summary>
        /// <param name="type">調用的按鈕類型</param>
        /// <param name="_switch">開關</param>
        /// <param name="tween">是否有過渡動畫</param>
        /// <returns></returns>
        private void AlterTopicFilterColor(TopicType type,bool _switch,bool tween=true)
        {

            Button tarBtn = null;
            switch (type) {

                case TopicType.None:
                    tarBtn = allFilterButton;
                    break;
                case TopicType.Daily:
                    tarBtn = dailyFilterButton;
                    break;
                case TopicType.SideStory:
                    tarBtn = sideStoryFilterButton;
                    break;
                case TopicType.Special:
                    tarBtn = specialFilterButton;
                    break;

            }

            GameObject selectImg = tarBtn.transform.Find("SelectIcon").gameObject;
            GameObject hideImg = tarBtn.transform.Find("HideIcon").gameObject;

            if (_switch)
            {
                selectImg.SetActive(true);
                if (tween) 
                {
                    selectImg.SetCanvasGroup(0);
                    StartCoroutine( selectImg.eSetCanvasGroup(1));

                    hideImg.SetCanvasGroup(1);
                   StartCoroutine( hideImg.eSetCanvasGroup(0, 
                        () => { hideImg.SetActive(false); }));
                }
                else
                {
                    selectImg.SetCanvasGroup(1);
                    hideImg.SetActive(false);
                }

            }
            else
            {
                hideImg.SetActive(true);

                if (tween)
                {
                    hideImg.SetCanvasGroup(0);
                   StartCoroutine( hideImg.eSetCanvasGroup(1));

                    selectImg.SetCanvasGroup(1);
                  StartCoroutine(  selectImg.eSetCanvasGroup(0, () => { selectImg.SetActive(false); }));

                }
                else
                {
                    hideImg.SetCanvasGroup(1);
                    selectImg.SetActive(false);
                }


            }


        }


        /// <summary>
        /// 點擊角色列表按鈕
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickCharaListButton() {
             yield return control.TopicHistoryGoToCharaListPage();
        }
        /// <summary>
        /// 前往聊天室
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickChatRoomButton()
        {
            yield return control.GoChatRoomPage();

        }


    }//--class topicHistory



}//--nameSpace 
