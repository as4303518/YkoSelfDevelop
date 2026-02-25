using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace YKO.SnsSetting
{
    /// <summary>
    /// 話題顯示UI
    /// </summary>
    public class TopicCell : MonoBehaviour
    {

        #region Panel UI

        /// <summary>
        /// 話題名稱
        /// </summary>
        [SerializeField] private Text topicNameText;

        /// <summary>
        /// 話題類別(日常,支線
        /// </summary>
        [SerializeField] private Image typeIconImg;
        /// <summary>
        /// 被選擇的外框
        /// </summary>
        [SerializeField] private GameObject selectOutLine;
        /// <summary>
        /// 新話題提示
        /// </summary>
        [SerializeField] private Image newIconImg;
        /// <summary>
        /// 重新開始話題
        /// </summary>
        [SerializeField] private Image againIconImg;
        /// <summary>
        /// 重新開始話題
        /// </summary>
        [SerializeField] private Image TickIconImg;
        /// <summary>
        /// 觸發範圍
        /// </summary>
        [SerializeField] private Button touchArea;

        

        #endregion

        #region UI Resources
        /// <summary>
        /// 日常圖形
        /// </summary>
        [SerializeField] private Sprite dailySprite;
        /// <summary>
        /// 支線圖形
        /// </summary>
        [SerializeField] private Sprite sideStorySprite;
        /// <summary>
        /// 特殊圖形
        /// </summary>
        [SerializeField] private Sprite SpecialSprite;

        /// <summary>
        /// 上鎖圖形
        /// </summary>
        [SerializeField] private Sprite LockSprite;

        #endregion

        #region Param
        /// <summary>
        /// 主資料
        /// </summary>
        public TopicDisplayInfo aData = null;

        /// <summary>
        /// 選擇後的回調
        /// </summary>
        public Action<TopicDisplayInfo> ChooseCallBack =null;
        /// <summary>
        /// 頁面類型
        /// </summary>
        private SnsSettingController.SnsControlPage pageType;

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(TopicDisplayInfo _data,Action<TopicDisplayInfo> chooseCB,SnsSettingController.SnsControlPage snsPage)
        {
            pageType = snsPage;
            aData = _data;
            ChooseCallBack = chooseCB;
            UpdateUI();
            SettingButton();

        }
        /// <summary>
        /// 按鈕設定
        /// </summary>
        private void SettingButton()
        {
            touchArea.onClick.RemoveAllListeners();

            switch (pageType)
            {
                case SnsSettingController.SnsControlPage.SnsTopicHistory:
                    if (aData.isLock)
                    {
                        CloseTouch();
                    }
                    break;
            }

            touchArea.OnClickAsObservable().Subscribe(_ => ClickChooseState());

        }
        /// <summary>
        /// 更新IU
        /// </summary>
        private void UpdateUI() 
        {
            newIconImg.gameObject.SetActive(false);
            if (aData.infoData.repeat>0) 
            {
                againIconImg.gameObject.SetActive(true);
            }
            else
            {
                againIconImg.gameObject.SetActive(false);
            }
            TickIconImg.gameObject.SetActive(false);
            topicNameText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData.infoData.name); 

            switch (aData.topicType)
            {

                case TopicType.Daily:
                    
                    typeIconImg.sprite = dailySprite;
                    break;
                case TopicType.SideStory:
                    typeIconImg.sprite = sideStorySprite;
                    break;
                case TopicType.Special:
                    typeIconImg.sprite = SpecialSprite;
                    break;

            }

            switch (pageType)
            {
                case SnsSettingController.SnsControlPage.SnsChatRoom:
                    if (!aData.isRead)
                    {
                        newIconImg.gameObject.SetActive(true);
                    }

                    break;

                case SnsSettingController.SnsControlPage.SnsTopicHistory:
                    if (aData.isLock && pageType == SnsSettingController.SnsControlPage.SnsTopicHistory)
                    {
                        LockCell();
                    }

                    break;

            }
        }




        /// <summary>
        /// 觸碰進入選取狀態
        /// </summary>
        private void ClickChooseState()
        {
            switch (pageType) {

                case SnsSettingController.SnsControlPage.SnsChatRoom:
                    TickIconImg.gameObject.SetActive(true);
                    againIconImg.gameObject.SetActive(false);
                    selectOutLine.SetActive(true);
                    break;

                case SnsSettingController.SnsControlPage.SnsTopicHistory:

                    break;

            }
            ChooseCallBack(aData);

        }
        /// <summary>
        /// 觸碰範圍取消狀態
        /// </summary>
        public void ClickCancelState()
        {
            switch (pageType)
            {

                case SnsSettingController.SnsControlPage.SnsChatRoom:
                    TickIconImg.gameObject.SetActive(false);
                    if (aData.topicType == TopicType.Daily)
                    {
                        againIconImg.gameObject.SetActive(true);
                    }
                    selectOutLine.SetActive(false);
                    break;

                case SnsSettingController.SnsControlPage.SnsTopicHistory:

                    break;

            }
        }
        /// <summary>   
        /// 鎖上話題選項
        /// </summary>
        public void LockCell()
        {
            gameObject.GetComponent<Image>().color = Color.gray;
            topicNameText.text = "未獲得話題"+aData.infoData.id;
            topicNameText.color = Color.white;
            typeIconImg.sprite = LockSprite;
            againIconImg.gameObject.SetActive(false);
        }

        /// <summary>
        /// 關閉按鈕選擇
        /// </summary>
        public void CloseTouch()
        {
            touchArea.interactable = false;
        }



    }//class TopicCell
}//-namespace
