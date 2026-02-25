using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using UniRx;
using YKO.Support.Expansion;
/// <summary>
/// Sns系統,玩家回覆的字卡
/// </summary>
public class SnsAnswerCell : MonoBehaviour
{
    #region Panel UI
    /// <summary>
    /// 顯示文字
    /// </summary>
    [SerializeField] private Text displaySentenceText;
    /// <summary>
    /// 是否被選擇
    /// </summary>
    [SerializeField] private GameObject chooseTick;
    /// <summary>
    /// 已經執行過的符號
    /// </summary>
    [SerializeField] private GameObject chooseInPastClock;
    /// <summary>
    /// 被選澤的外邊
    /// </summary>
    [SerializeField] private GameObject chooseOutLine;

    /// <summary>
    /// 觸發區域
    /// </summary>
     public Button AreaButton;

    #endregion


    #region Param
    /// <summary>
    /// 傳遞的數據
    /// </summary>
    public SnsAnswerCellData aData;
    // <summary>
    // 被選擇的顏色
    //</summary>
    //private Color chooseColor = new Color(0.85f, 0.17f, 0.21f);
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_data"></param>
    public void Init(SnsAnswerCellData _data)
    {
        aData = _data;
        UpdateUI();
        SetButtonSetting();

    }
    /// <summary>
    /// 設置按鈕設定
    /// </summary>
    private void SetButtonSetting()
    {
        AreaButton.OnClickAsObservable().Subscribe(_ => ClickSnsCell());

    }
    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        if (aData.chooseInPast)
        {
            chooseInPastClock.SetActive(true);
        }
        else
        {
            chooseInPastClock.SetActive(false);
        }
        displaySentenceText.text = aData.text;
    }
    private void ClickSnsCell()
    {
        aData.cbTextToDisplay();
        SuccessfulChoose();

    }
    /// <summary>
    /// 選項成功被選擇
    /// </summary>
    private void SuccessfulChoose()
    {
        if (aData.chooseInPast) {
          StartCoroutine(chooseInPastClock.eSetCanvasGroup(0, () => { chooseInPastClock.SetActive(false); }));
        }

        chooseTick.SetActive(true);
        chooseOutLine.SetActive(true);
        chooseTick.SetCanvasGroup(0);
        chooseOutLine.SetCanvasGroup(0);
        StartCoroutine(chooseTick.eSetCanvasGroup(1));
        StartCoroutine(chooseOutLine.eSetCanvasGroup(1));
        //淡入紅色

    }
    /// <summary>
    /// 設定觸發按鈕狀態
    /// </summary>
    public void SwitchAreaBtn(bool _switch)
    {
        AreaButton.enabled = _switch;
    }
    /// <summary>
    /// 選項取消選擇
    /// </summary>
    public void CancelChoose()
    {
        if (aData.chooseInPast)
        {
            chooseInPastClock.SetActive(true);
            StartCoroutine(chooseInPastClock.eSetCanvasGroup(1));
        }

        chooseTick.SetCanvasGroup(1);
        chooseOutLine.SetCanvasGroup(1);
        StartCoroutine(chooseTick.eSetCanvasGroup(0, () => { chooseTick.SetActive(false); }));
        StartCoroutine(chooseOutLine.eSetCanvasGroup(0, () => { chooseOutLine.SetActive(false); }));
    }

    /// <summary>
    /// SnsAnswerCellData傳遞的資料
    /// </summary>
    public class SnsAnswerCellData
    {
        /// <summary>
        /// 顯示內容( 部份情況 非完整句子
        /// </summary>
        public string text;
        /// <summary>
        /// 該選項在過去執行時有被選擇
        /// </summary>
        public bool chooseInPast=false;


        /// <summary>
        /// 點選選項後將文字反應在聊天框裡,並同時移除其他被選擇選項的標示
        /// </summary>
        public Action cbTextToDisplay;

        public SnsAnswerCellData(string _text, Action _cbTextToDisplay)
        {
            text = _text;
            cbTextToDisplay = _cbTextToDisplay;
        }


    }


}
