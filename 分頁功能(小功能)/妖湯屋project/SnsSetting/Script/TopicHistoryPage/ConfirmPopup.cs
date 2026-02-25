using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx;
using YKO.Support.Expansion;

namespace YKO.Common
{

    public class ConfirmPopup : MonoBehaviour
    {

        #region  Panel UI 
        /// <summary>
        /// 確認按鈕
        /// </summary>
        [SerializeField] private Button confirmButton;

        /// <summary>
        /// 取消按鈕
        /// </summary>
        [SerializeField] private Button cancelButton;

        /// <summary>
        /// 非觸碰顯示的空白區域
        /// </summary>
        [SerializeField] private Button filterButton;
        /// <summary>
        /// 中間內文
        /// </summary>
        [SerializeField] private Text contentText;

        /// <summary>
        /// 標題內文
        /// </summary>
        [SerializeField] private Text titleText;

        #endregion

        #region 
        /// <summary>
        /// 回掉參數數據
        /// </summary>
        private ConfirmPopupFunc mFunc = null;

        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init(ConfirmPopupFunc func)
        {
            mFunc = func;
            UpdateUI();
            SetButtonSetting();
            yield return TweenIn();
        }

        /// <summary>
        /// 設定按鈕設定
        /// </summary>
        private void SetButtonSetting()
        {
            confirmButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickConfirmButton()));
            cancelButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickCancelButton()));
            filterButton.OnClickAsObservable().Subscribe(_ => StartCoroutine(ClickFilterButton()));
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            titleText.text = mFunc._titleText;
            contentText.text = mFunc._contentText;
        }
        /// <summary>
        /// 按下確認按鍵
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickConfirmButton()
        {
            if (mFunc._confirmAction!=null) {

                mFunc._confirmAction();
            }

                yield return ClosePopup();
        }
        /// <summary>
        /// 按下取消按鍵
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickCancelButton()
        {
            if (mFunc._cancelAction != null)
            {
                mFunc._cancelAction();
            }
                yield return ClosePopup();
        }
        /// <summary>
        /// 點擊範圍外
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickFilterButton()
        {
            if (mFunc._filterAction!=null) 
            {
                mFunc._filterAction();
            }

             yield return  ClosePopup();
        }

        private IEnumerator ClosePopup() {

            yield return TweenOut();
            Destroy(gameObject);
        }


        /// <summary>
        /// 淡入動畫
        /// </summary>
        /// <returns></returns>
        private IEnumerator TweenIn()
        {
            gameObject.SetCanvasGroup(0);
            RectTransform rect = GetComponent<RectTransform>();
            rect.DOKill();
            rect.localScale = new Vector3(0.5f,0.5f,0.5f);
            StartCoroutine(gameObject.eSetCanvasGroup(1));
            yield return rect.DOScale(1, 0.2f).WaitForCompletion();
        }
        /// <summary>
        /// 淡出動畫
        /// </summary>
        /// <returns></returns>
        public IEnumerator TweenOut()
        {
            gameObject.SetCanvasGroup(1);
            RectTransform rect = GetComponent<RectTransform>();
            rect.DOKill();
            rect.localScale = Vector3.one;
            StartCoroutine( gameObject.eSetCanvasGroup(0));
            yield return rect.DOScale(0.5f,0.2f).WaitForCompletion();
        }
        /// <summary>
        /// 兩選項一內文確認視窗方法傳遞
        /// </summary>
        public class ConfirmPopupFunc 
        {
            /// <summary>
            /// 標題文字
            /// </summary>
            public string _titleText = null;
            /// <summary>
            /// 內容文字
            /// </summary>
            public string _contentText = null;

            /// <summary>
            /// 確認按鈕的方法
            /// </summary>
            public Action _confirmAction = null;
            /// <summary>
            /// 取消按鈕的方法
            /// </summary>
            public Action _cancelAction = null;

            /// <summary>
            /// 彈窗區域外的方法
            /// </summary>
            public Action _filterAction=null;

        }



    }
} 
