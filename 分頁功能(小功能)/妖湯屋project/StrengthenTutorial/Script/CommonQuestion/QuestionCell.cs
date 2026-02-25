using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.Font;
using YKO.DataModel;

namespace YKO.StrengthGuide
{

    public class QuestionCell : MonoBehaviour
    {

        public class LocalizeKey
        {
            public static readonly string CloseExpand = "收起";

            public static readonly string Expand = "看解答";
        }


        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField] private Text titleText;
        /// <summary>
        /// 擴展文字
        /// </summary>
        [SerializeField] private Text expandText;
        /// <summary>
        /// 擴展icon
        /// </summary>
        [SerializeField] private Image expandIconImg;
        /// <summary>
        /// 描述文字
        /// </summary>
        [SerializeField] private Text descriptText;
        /// <summary>
        /// 點擊背景UI觸發的範圍
        /// </summary>
        [SerializeField] private Button ExpandToggleAreaBtn;

        #region value or param
        /// <summary>
        /// 是否擴展
        /// </summary>
        private bool isExpand = false;
        /// <summary>
        /// 資料
        /// </summary>
        private StrongerData.Problem aData;


        #endregion

        #region Sprite
        /// <summary>
        /// 顯示點擊擴張的箭頭
        /// </summary>
        public Sprite expandArrowSprite;
        /// <summary>
        /// 顯示點擊收起的箭頭
        /// </summary>
        public Sprite unExpandArrowSprite;

        #endregion


        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(StrongerData.Problem _data)
        {
            aData = _data;
            SetButtonSetting();
            UpdateUI();

        }


        /// <summary>
        /// 設定按鈕點擊事件
        /// </summary>
        private void SetButtonSetting()
        {
            ExpandToggleAreaBtn.OnClickAsObservable().Subscribe(_ => ClickExpand());

        }


        /// <summary>
        /// 更新UI顯示
        /// </summary>
        private void UpdateUI()
        {
            titleText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData.name);
            var content= ConvertForShinyOfLight.ConvertStringToUnityEditorDisplay(LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData.desc));
            descriptText.text =content;
            isExpand = false;
        }




        /// <summary>
        /// 擴展內容
        /// </summary>
        private void ClickExpand()
        {
            isExpand = !isExpand;
            SetExpandSetting(isExpand);

        }
        /// <summary>
        /// 紀錄當前擴展動畫的秒數(用意在於玩家在動畫結束前點擊,則會記錄已執行秒數並反饋新的秒數,以免造成動畫速度體感不一的結果
        /// </summary>
        private float expandTweenTime = 0;

        private void SetExpandSetting(bool isExpand)
        {
            var rect = GetComponent<RectTransform>();

            expandText.DOKill();
            rect.DOKill();
            var destRect = descriptText.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(destRect);

            if (expandTweenTime <= 0)
            {

                expandTweenTime = 0.2f;

            }
            else
            {
                expandTweenTime = 0.2f - expandTweenTime;
            }

            if (isExpand)
            {//350
                expandIconImg.sprite = unExpandArrowSprite;
                expandText.DOColor(new Color(0.5f, 0.5f, 0.5f), expandTweenTime);
                expandText.text =LocalizeKey.CloseExpand;
                rect.DOSizeDelta(new Vector2(1000, destRect.sizeDelta.y+200), expandTweenTime)
                    .OnUpdate(() => expandTweenTime -= Time.deltaTime);

            }
            else//80
            {
                expandIconImg.sprite = expandArrowSprite;
                expandText.DOColor(new Color(0.85f, 0.17f, 0.21f), expandTweenTime);
                expandText.text = LocalizeKey.Expand;
                rect.DOSizeDelta(new Vector2(1000, 80), expandTweenTime)
                    .OnUpdate(() => expandTweenTime -= Time.deltaTime);
            }


        }



    }
}
