using DG.Tweening;
using Fungus;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using YKO.Common.Sound;
using YKO.Common.UI;
using YKO.CONST;
using YKO.Main;
using YKO.Network;

namespace YKO.Story
{
    /// <summary>
    /// 故事場景
    /// </summary>
    public class StoryScene : SingletonMonoBehaviour<StoryScene>
    {
        public class StorySceneParam
        {
            /// <summary>
            /// 撥放的章節
            /// </summary>
            public string storyChapterName;

            /// <summary>
            /// 回到轉入story時的頁面 (撥放劇情失敗或離開
            /// </summary>
            public string origineScene;

            /// <summary>
            /// 轉場出的Scene (撥放成功後
            /// </summary>
            public string transitionsScene;
            /// <summary>
            /// 是否播放序章
            /// </summary>
            public bool startPrologue;
            /// <summary>
            /// 是否為劇情重播
            /// </summary>
            public bool isReplay;
            /// <summary>
            /// 場景撥放結束後的參數
            /// </summary>
            public object overParam = null;



            public StorySceneParam(string _storyChapterName, string _origineScene, string _transitionsScene = null, bool _startPrologue = false, bool _isReplay = false, object _overParam = null)
            {
                storyChapterName = _storyChapterName;
                origineScene = _origineScene;
                startPrologue = _startPrologue;
                isReplay = _isReplay;
                overParam = _overParam;


                if (!string.IsNullOrEmpty(_transitionsScene))
                    transitionsScene = _transitionsScene;
                else
                    transitionsScene = origineScene;

                //if (startPrologue)
                //    PlayerPrefs.SetInt(PlayerPrefsKeys.STAGE_PROLOGUE_SCHEDULE, 10007);
            }
        }

        [SerializeField]
        private Camera mainCamera = null;

        public Camera MainCamera { get { return mainCamera; } }

        [SerializeField]
        private CanvasGroup backgroundCanvasGroup = default;
        [SerializeField]
        private CanvasGroup uiCanvasGroup = default;

        // Popup
        [SerializeField]
        public GameObject popupParent = default;
        [SerializeField]
        private GameObject firstEditNamePopup = default;

        [SerializeField]
        private Flowchart mFlowchart = default;

        private StorySceneParam storySceneParam = null;

        public StorySceneParam GetStorySceneParam { get { return storySceneParam; } }

        /// <summary>
        /// 設置mFlowcahrt的 DataName
        /// mFlowchart.OverrideSaveData();根據DataName 設置文檔
        /// </summary>

        private void Start()
        {
            SoundController.Instance.StopBGM();
            StartCoroutine(Init());
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public new IEnumerator Init()
        {
            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();
            MainManager.Instance.HideLoading(true);

            storySceneParam = GameSceneManager.Instance.GetSceneParam() as StorySceneParam;

            // storySceneParam = new StorySceneParam("side_651", "stage");
            MainManager.Instance.proto30201Switch = false;//關閉通知
           yield return LoadStoryData();
            StartCoroutine(PlayEnterAnim());
        }
        /// <summary>
        /// 加載故事場景Prefab數據
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadStoryData()
        {
            string storyTitlePath = storySceneParam.storyChapterName.Split("_")[0];
            string extraPath = "";
            switch (storyTitlePath) 
            {
                case "bath":
                case "favor":
                case "flirt":
                case "star":
                case "side":
                    extraPath = storyTitlePath+"/";
                    break;
            }
            Debug.Log("尋找路徑=>" + FungusResourcesPath.StoryFlowchartPrefabsPath + extraPath + storySceneParam.storyChapterName);
            //###使用addressable
            ResourceRequest resRe = Resources.LoadAsync<GameObject>(FungusResourcesPath.StoryFlowchartPrefabsPath+extraPath + storySceneParam.storyChapterName);
            yield return new WaitUntil(() => resRe.isDone);
            var sp = Instantiate(resRe.asset) as GameObject;
            mFlowchart = sp.GetComponent<Flowchart>();
            yield return mFlowchart.LoadAssetText();
            mFlowchart.StartShowStory();
        }

        /// <summary>
        /// 一次打完Drama流程
        /// 閃爍為連續對話
        /// </summary>
        public IEnumerator SkipDramaBeforeTutorial()
        {
            // 跳過drama流程
            Proto_11100_Request proto11100Request = new Proto_11100_Request();
            proto11100Request.drama_bid = 10008;
            proto11100Request.step_id = 1;
            NetworkManager.Instance.Send(proto11100Request);

            yield return new WaitForSeconds(0.1f);

            proto11100Request.drama_bid = 10008;
            proto11100Request.step_id = 2;
            NetworkManager.Instance.Send(proto11100Request);

            yield return new WaitForSeconds(0.1f);

            proto11100Request = new Proto_11100_Request();
            proto11100Request.drama_bid = 10010;
            proto11100Request.step_id = 1;
            NetworkManager.Instance.Send(proto11100Request);

            yield return new WaitForSeconds(0.1f);

            proto11100Request.drama_bid = 10010;
            proto11100Request.step_id = 2;
            NetworkManager.Instance.Send(proto11100Request);

            yield return new WaitForSeconds(0.1f);

            //PlayerPrefs.SetInt(PlayerPrefsKeys.STAGE_PROLOGUE_SCHEDULE, 10008);
        }

        /// <summary>
        /// 發送分岐選擇
        /// </summary>
        public void SendProto13050Resquest(int GroupID, int ChoiceIndex)
        {
            Debug.Log("發送分歧=>" + GroupID + "##choice=>" + ChoiceIndex);
            //群組Id 由menu做設置=> 1 
            //選項的選擇編號 由menu做設置 (三個選項)2
            Proto_13050_Request proto = new Proto_13050_Request()
            {
                group_id = (uint)GroupID,
                choice_value = (byte)ChoiceIndex
            };

            NetworkManager.Instance.Send(proto);

        }

        /// <summary>
        /// //執行劇情關卡
        /// </summary>
        public void SendProto13033Resquest()
        {
            Proto_13033_Request request = new Proto_13033_Request();
            NetworkManager.Instance.Send(request);
        }

        /// <summary>
        /// 可能確認才會執行故事?
        /// </summary>
        /// <param name="cb"></param>
        /// <returns></returns>
        public IEnumerator WaitProto13050Response(Action<IMessage> cb)
        {
            Proto_13050_Response proto = null;

            MessageResponseData.Instance.OnMessageResponse
                .Where(meg => meg.MessageID == 13050)
                .Subscribe(meg =>
                {
                    proto = (Proto_13050_Response)meg;
                });

            yield return new WaitUntil(() => proto != null);
            cb(proto);
        }

        /// <summary>
        /// 顯示取名彈窗的Command
        /// </summary>
        public IEnumerator OnCommandShowFirstEditName(Action cb)
        {
            if (mFlowchart.mStoryControl.isAutoPlay)
            {
                mFlowchart.mStoryControl.CloseAutoPlay();
            }
            GameObject _firstEditNamePopup = Instantiate(firstEditNamePopup, popupParent.transform);
            FirstEditNickNamePopup popup = _firstEditNamePopup.GetComponent<FirstEditNickNamePopup>();
            popup.Init();
            //  popup.SetCanSkipCllBack(()=> { mFlowchart.mStoryControl.DetectSkipState(); });
            popup.ShowPopup();
            yield return popup.WaitForNickNameResponse();
            cb?.Invoke();
        }

        /// <summary>
        /// 切換Scene的Command
        /// </summary>
        public IEnumerator OnCommandChangeScene(string sceneName)
        {
            mFlowchart.mStoryControl.DontSkipStoryPlot();//切換回遊戲原本預設的timeScale
            if (false == StoryScene.Instance.GetStorySceneParam.isReplay &&
                sceneName == SceneConst.SceneName.StageScene || 
                sceneName == SceneConst.SceneName.MypageScene)
                StoryScene.Instance.SendProto13033Resquest();

            //MainManager.Instance.ShowLoading(true);
            yield return PlayExitAnim();

            ClearFungusManager();
            MainManager.Instance.proto30201Switch = true;
            GameSceneManager.Instance.AddScene(sceneName, storySceneParam?.overParam);
        }

        /// <summary>
        /// 切入動畫
        /// </summary>
        /// <param name="onComplete">完成時回調</param>
        public IEnumerator PlayEnterAnim(Action onComplete = null)
        {
            Sequence sequence = DOTween.Sequence().SetAutoKill();
            sequence.Append(backgroundCanvasGroup.DOFade(1f, 0.2f));
            sequence.Join(uiCanvasGroup.DOFade(0f, 0.2f));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                GameSceneManager.Instance?.FinishLoadScene();
            });

            yield return sequence.WaitForCompletion();
        }

        /// <summary>
        /// 切出動畫
        /// </summary>
        /// <param name="onComplete">完成時回調</param>
        public IEnumerator PlayExitAnim(Action onComplete = null)
        {
            Sequence sequence = DOTween.Sequence().SetAutoKill();
            sequence.Append(backgroundCanvasGroup.DOFade(0f, 0.1f));
            sequence.Join(uiCanvasGroup.DOFade(1f, 0.1f));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
            yield return sequence.WaitForCompletion();

            // 如果在沒取名的情況下播序章，則一次打完Drama流程
            if (storySceneParam.startPrologue)
                yield return SkipDramaBeforeTutorial();
        }

        private void ClearFungusManager()
        {
            FungusManager fungusManager = FungusManager.Instance;

            if (fungusManager != null) // 避免屏蔽screen
            {
                FungusManager.ClearFungusInstance();
            }
        }

    }
}

