using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using YKO.Support.Expansion;
using UniRx;
using YKO.Network;
using YKO.Common.UI;
using System;
using YKO.DataModel;
using YKO.Support;
using Newtonsoft.Json;
using YKO.Main;
using DG.Tweening;
using YKO.Common;

namespace YKO.StrengthGuide
{

    /// <summary>
    /// 強化指南class
    /// </summary>
    public class StrengthenTutorialManager : MonoBehaviour
    {

        [Header("topUI")]
        /// <summary>
        /// 黃色貨幣(數量
        /// </summary>
        [SerializeField] private Text moneyText;
        /// <summary>
        /// 紫色貨幣(數量
        /// </summary>
        [SerializeField] private Text money2Text;
        /// <summary>
        /// 郵件按鈕
        /// </summary>
        [SerializeField] private Button mailBtn;
        /// <summary>
        /// 多功能視窗
        /// </summary>
        [SerializeField] private Button settingBtn;
        /// <summary>
        /// 標題
        /// </summary>
        [SerializeField] private Text titleText;

        [SerializeField] private Text descriptionText;

        /// <summary>
        /// 強化指引按鈕
        /// </summary>
        [SerializeField] private Button StrengthenGuidelinesBtn;



        /// <summary>
        /// 推薦隊伍按鈕
        /// </summary>
        [SerializeField] private Button RecommendTeamBtn;
        /// <summary>
        /// 常見問題按鈕
        /// </summary>
        [SerializeField] private Button CommonQuestionBtn;

        [Header("BottomUI")]
        /// <summary>
        /// 返回按鈕
        /// </summary>
        [SerializeField] private Button BackBtn;
        /// <summary>
        /// 回首頁
        /// </summary>
        [SerializeField] private Button HomeBtn;

        /// <summary>
        /// 上方功能索引管理員(金幣,信件,設定等
        /// </summary>
        [Header("ManagerPage")]
        [SerializeField] private TopPanel topPanel;
        /// <summary>
        /// 導引頁面
        /// </summary>
        [SerializeField] private StrengthGuidelines StrengthenGuidelinesPage;
        /// <summary>
        /// 推薦隊伍頁面
        /// </summary>
        [SerializeField] private RecommendTeam RecommendTeamPage;
        /// <summary>
        /// 常見問題頁面
        /// </summary>
        [SerializeField] private CommonQuestion CommonQuestionPage;

        /// <summary>
        /// 靜態文檔
        /// </summary>
        [SerializeField] private TextAsset strongeJson;

        [SerializeField]
        private CanvasGroup backgroundCanvasGroup = default;
        [SerializeField]
        private CanvasGroup uiCanvasGroup = default;

        #region param

        /// <summary>
        /// 強化指南靜態資料
        /// </summary>
        private StrongerData fixedData = null;
        /// <summary>
        /// 強化指南靜態資料公用
        /// </summary>
        public StrongerData FixedData { get { return fixedData; } }
        /// <summary>
        /// 強化教學指南的線上manager
        /// </summary>
        private StrengthenTutorialNetwork netManager=null;
        /// <summary>
        /// 處理強化指南response的manager
        /// </summary>
        public StrengthenTutorialNetwork NetManager { get { return netManager; } }
        /// <summary>
        /// 可以執行tween的檢查bool
        /// </summary>
        private bool canTween = false;

        public bool CanTween { get { return canTween; } set { canTween = value; } }
        #endregion

        public void Start()
        {

            StartCoroutine(Init());
        }

        public IEnumerator Init()
        {
            backgroundCanvasGroup.alpha = 0;
            uiCanvasGroup.alpha = 0;
            topPanel.Init();
            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();
            fixedData = LoadResource.Instance.StrongerData;
            canTween = true;
            netManager = new StrengthenTutorialNetwork();
            SetButtonSetting();
            UpdateUI();
            yield return ClickStrengthBtn();
            PlayEnterAnim();
            MainManager.Instance.HideLoading(true);

        }

        /// <summary>
        /// 設定按鈕
        /// </summary>
        private void SetButtonSetting()
        {

            StrengthenGuidelinesBtn.OnClickAsObservable().Where(_=>canTween).Subscribe(_ => StartCoroutine(ClickStrengthBtn()));
            RecommendTeamBtn.OnClickAsObservable().Where(_ => canTween).Subscribe(_ => StartCoroutine(ClickRecommendTeamBtn()));
            CommonQuestionBtn.OnClickAsObservable().Where(_ => canTween).Subscribe(_ => StartCoroutine(ClickCommonQuestionBtn()));
            BackBtn.OnClickAsObservable().Subscribe(_ => ClickBackBtn());
            HomeBtn.OnClickAsObservable().Subscribe(_ => ClickHomeBtn());
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {


        }

        /// <summary>
        /// 按下強化指引
        /// </summary>
        private IEnumerator ClickStrengthBtn()
        {
            canTween = false;

            
           // yield return StrengthenGuidelinesPage.Init(this) ;

           yield return DisplayChoosePage(
                PageType.StrengthenGuidelines,
                ()=>StrengthenGuidelinesPage.Init(this)
                );
            canTween = true;

        }

        private IEnumerator ClickRecommendTeamBtn()
        {
            canTween = false;
          //  yield return RecommendTeamPage.Init(this);

            yield return  DisplayChoosePage(
                PageType.RecommendTeam,
               ()=> RecommendTeamPage.Init(this)
                );
            canTween = true;
        }


        private IEnumerator ClickCommonQuestionBtn()
        {
            canTween = false;
           // yield return CommonQuestionPage.Init(this);

            yield return DisplayChoosePage(
               PageType.CommonQuestion,
                ()=> CommonQuestionPage.Init(this)
               );

            canTween = true;
        }

        /// <summary>
        /// 返回上一個場景按鈕
        /// </summary>
        /// <returns></returns>
        private void ClickBackBtn()
        {
            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.BackScene();
            });
        }
        /// <summary>
        /// 返回主頁按鈕
        /// </summary>
        /// <returns></returns>
        private void ClickHomeBtn()
        {
            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene);
            });
        }
        /// <summary>
        /// 轉到指定場景(包含主場景退出
        /// </summary>
        /// <param name="sceneName"></param>
        public void GoToSpeciftyScene(string sceneName,object param=null) {

            //MainManager.Instance.ShowLoading(true);
            PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(sceneName,param);
            });

        }

        /// <summary>
        /// 顯示當前的切換頁面
        /// </summary>
        /// <param name="page"></param>
        private IEnumerator DisplayChoosePage(PageType page,Func<IEnumerator > executeMain)
        {
            List<GameObject> pageList = new List<GameObject>();
            List<Button> btnList = new List<Button>();
            pageList.Add(StrengthenGuidelinesPage.gameObject);
            pageList.Add(RecommendTeamPage.gameObject);
            pageList.Add(CommonQuestionPage.gameObject);

            btnList.Add(StrengthenGuidelinesBtn);
            btnList.Add(RecommendTeamBtn);
            btnList.Add(CommonQuestionBtn);

           GameObject mainPage = null;
            foreach (var curPage in pageList) {
                if (curPage.name==(page.ToString() + "Page"))
                {
                    curPage.SetActive(true);
                    curPage.SetCanvasGroup(0);
                    mainPage=curPage;

                }
                else
                {
                    curPage.SetActive(false);
                }
            
            
            }
            yield return executeMain();

            StartCoroutine(mainPage.eSetCanvasGroup(1));

            foreach (var btn in btnList)
            {
                if (btn.name.Split("-")[0] == page.ToString()) {

                    GameObject tarObj = btn.transform.Find("ChooseDisplay").gameObject;
                    tarObj.SetActive(true);
                    tarObj.SetCanvasGroup(0);
                    StartCoroutine(tarObj.eSetCanvasGroup(1, () => canTween = true));
                }
                else
                {
                    btn.transform.Find("ChooseDisplay").gameObject.SetActive(false);
                }
            }

            //Button tar = (Button)GetType().GetField(page.ToString() + "btn", BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic).GetValue(this);
        }


        /// <summary>
        /// 切入動畫
        /// </summary>
        /// <param name="onComplete">完成時回調</param>
        public void PlayEnterAnim(Action onComplete = null)
        {
            Sequence sequence = DOTween.Sequence().SetAutoKill();
            sequence.Append(backgroundCanvasGroup.DOFade(1f, 0.2f));
            sequence.Join(uiCanvasGroup.DOFade(1f, 0.2f));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                GameSceneManager.Instance?.FinishLoadScene();
            });
        }

        /// <summary>
        /// 切出動畫
        /// </summary>
        /// <param name="onComplete">完成時回調</param>
        public void PlayExitAnim(Action onComplete = null)
        {
            Sequence sequence = DOTween.Sequence().SetAutoKill();
            //sequence.Append(backgroundCanvasGroup.DOFade(0f, 0.1f));
            //sequence.Join(uiCanvasGroup.DOFade(0f, 0.1f));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 強化指南頁面
        /// </summary>
        public enum PageType
        {
            StrengthenGuidelines,
            RecommendTeam,
            CommonQuestion


        }

    }
}