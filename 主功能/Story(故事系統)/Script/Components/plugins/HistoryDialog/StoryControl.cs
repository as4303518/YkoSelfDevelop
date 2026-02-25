using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;
using YKO.Story;
using DG.Tweening;
using UniRx;
using System;
using YKO.Support.Expansion;

namespace Fungus
{
    public class StoryControl : MonoBehaviour
    {
        [SerializeField] private Button btnAuto;
        [SerializeField] private Button btnLog;
        [SerializeField] private Button btnSkip;

        [HideInInspector] private Flowchart flowchart;

        public bool isAutoPlay = false;
        /// <summary>
        /// 跳過功能布林值
        /// </summary>
        public bool isSkipPlay = false;

       // [SerializeField] private bool CanSkip = true;

        private SnsManager mSnsWindow = null;
        public SnsManager SnsWindow { get { return mSnsWindow; } set { mSnsWindow = value; } }
        /// <summary>
        /// 統計歷史對話實際必須出現的數量
        /// </summary>
        private uint recordRealCount = 0;
        /// <summary>
        /// 統計歷史對話實際必須出現的數量
        /// </summary>
        public uint RecordCount { 
            get { return recordRealCount; }
            set { recordRealCount = value; } 
        }

        private List<DialogInfo> recordDialogList = new List<DialogInfo>(); // 記錄對話

        public GameObject LogWindowPopupParent = null;

        private GameObject LogWindowPopupPrefab = null;

        private GameObject mLogWindowPopup = null;

        [SerializeField]private GameObject TopFuncListParent = null;
        [SerializeField] private GameObject blockMask;

        private bool PlayAni = false;

        /// <summary>
        /// 紀錄玩家是否原本就屬於自動撥放的狀態
        /// </summary>
        [SerializeField] private bool origineAutoPlay = false;
        [SerializeField] private float timeScaleValue = 100;
        
        private bool DialogState = false; // 對話框在log顯現前最後的狀態(隱藏 or 顯現)
        private Sprite autoPlaySprite = null;
        private Sprite autoPlayingSprite = null;

        private bool hideDialog = false;
        private GameObject aHideDialog = null;


#region EditorExecute
        public void FindCameraRender(Flowchart fc)
        {
            flowchart = fc;
            UICanvasBase.CheckIsHaveUICameraBase(gameObject,TargetCameraType.StoryUICamera);

            if (GetComponent<Canvas>().worldCamera == null)
            {
                GetComponent<Canvas>().worldCamera = Camera.main;
            }
        }
#endregion

        public void Start()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            yield return LoadPrefab();
            StartCoroutine( CloseMask());
            SetButtonSetting();
        }


        public void SetButtonSetting()
        {
            btnAuto.OnClickAsObservable().Subscribe(_ => ClickButtonAuto()).AddTo(this);
            btnLog.OnClickAsObservable().Subscribe(_ => 
            { 
                StartCoroutine(ClickButtonLog()); 
            }).AddTo(this);
            btnSkip.OnClickAsObservable().Subscribe(_ => ClickButtonSkip()).AddTo(this);
        }

        public IEnumerator LoadPrefab()
        {
            if (LogWindowPopupPrefab == null)
            {
                ResourceRequest resRe = Resources.LoadAsync<GameObject>("Prefabs/PenguinPrefab/LogPopup");
                yield return new WaitUntil(() => resRe.isDone);
                LogWindowPopupPrefab = resRe.asset as GameObject;
            }
            //icon_story_auto_playing
            ResourceRequest resAutoPlay = Resources.LoadAsync<Sprite>(FungusResourcesPath.StoryImage + "icon_story_auto");

            yield return new WaitUntil(() => resAutoPlay.isDone);

            autoPlaySprite = resAutoPlay.asset as Sprite;

            ResourceRequest resAutoPlaying = Resources.LoadAsync<Sprite>(FungusResourcesPath.StoryImage + "icon_story_auto_playing");

            yield return new WaitUntil(() => resAutoPlaying.isDone);

            autoPlayingSprite = resAutoPlaying.asset as Sprite;
        }

        public void ClickButtonAuto()
        {
            if (PlayAni)
            {
                return;
            }

            if (!isAutoPlay)
            {
                AutoPlay();
            }
            else
            {
                CloseAutoPlay();
            }
        }

        private IEnumerator ClickButtonLog()
        {
            if (!PlayAni)
            {
                CloseAutoPlay();
                PlayAni = true;

                if (mLogWindowPopup == null)
                {
                    mLogWindowPopup = Instantiate(LogWindowPopupPrefab);
                    mLogWindowPopup.transform.SetParent(LogWindowPopupParent.transform, false);
                    mLogWindowPopup.GetComponent<CanvasGroup>().alpha = 0;

                    yield return mLogWindowPopup.GetComponent<StoryLogPopup>().Init(recordDialogList, () => 
                    { 
                        StartCoroutine(ClickCloseLogButton()); 
                    });

                    StartCoroutine(HideDialogAndTopUI());

                    yield return LeanTweenManager.FadeIn(mLogWindowPopup);

                    PlayAni = false;
                }
            }
        }

        private IEnumerator ClickCloseLogButton()
        {
            if (mLogWindowPopup != null && !PlayAni) 
            {
                PlayAni = true;

                StartCoroutine(ShowTopUI());
                
                yield return LeanTweenManager.FadeOut(mLogWindowPopup);

                Destroy(mLogWindowPopup);
                mLogWindowPopup = null;

                PlayAni = false;
            }
        }
        /// <summary>
        /// 按下跳過按鈕
        /// </summary>
        private void ClickButtonSkip()
        {
            if (PlayAni)
            {
                return;
            }

            if (!isSkipPlay)
            {
              StartCoroutine(  OpenMask(() => {
                    origineAutoPlay = isAutoPlay;
                    AcceleratePlayScale(true);
                    isSkipPlay = true;
                    AutoPlay();
                }));

            }

        }

        /// <summary>
        /// 跳出skip狀態
        /// </summary>
        public void DontSkipStoryPlot()//在block
        {
            AcceleratePlayScale(false);
            isSkipPlay = false;
            StartCoroutine(AdjustSkipButtonClickState(false));

            if (!origineAutoPlay)
            {
                CloseAutoPlay();
            }
           StartCoroutine( CloseMask());
        }
        /// <summary>
        /// 可以進入skip的狀態
        /// </summary>
        public void CanSkipStoryPlot()//在block
        {
            StartCoroutine(AdjustSkipButtonClickState(true));
        }

        /// <summary>
        /// 調整遊戲加速與按鈕顯示
        /// </summary>
        /// <param name="accelerate"></param>
        private void AcceleratePlayScale(bool accelerate)
        {
            if (accelerate)
            {
                AdjustAutoButtonClickState(false);

                StopCoroutine(AdjustLogButtonClickState(false));
                StartCoroutine(AdjustLogButtonClickState(false));

                Time.timeScale = timeScaleValue;
            }
            else
            {
                AdjustAutoButtonClickState(true);

                StopCoroutine(AdjustLogButtonClickState(true));
                StartCoroutine( AdjustLogButtonClickState(true));
                Time.timeScale = 1;
            }
           // Time.timeScale = accelerate ? timeScaleValue : 1;

        }


        public void SaveDialogRecord(DialogInfo dia)//儲存對話
        {
            recordDialogList.Add(dia);
        }

        /// <summary>
        /// 生成劇情功能按鈕
        /// </summary>
        /// <returns></returns>
        public IEnumerator ShowTopUI()
        {
            if (hideDialog)
            {
                SayDialog say = aHideDialog.GetComponent<SayDialog>();

                // say.gameObject.SetActive(true);
                DialogState = false;
                StartCoroutine(say.ReactionSayDialogAlpha(true));
            } 

            aHideDialog = null;
            TopFuncListParent.SetActive(true);
            yield return LeanTweenManager.FadeIn(TopFuncListParent);
        }

        public IEnumerator HideDialogAndTopUI()
        {
            SayDialog sd = SayDialog.GetSayDialog();
            DialogState = sd.NowAlphaStatus();

            if (DialogState)
            {
                aHideDialog = sd.gameObject;
                hideDialog = true;
                StartCoroutine(sd.ReactionSayDialogAlpha(false));
            }

            yield return LeanTweenManager.FadeOut(TopFuncListParent, () => { TopFuncListParent.SetActive(false); });
        }


        private void AutoPlay()
        {
            if (isAutoPlay) 
            {
                return;
            }
            isAutoPlay = true;


            SayDialog.SwitchSayDialogAutoPlayOfStatus(true);
            var img = btnAuto.transform.Find("Auto-Image").GetComponent<Image>();
            img.sprite = autoPlayingSprite;
            img.SetNativeSize();
            Stage.GetActiveStage().SetAutoPlay(true);
        }

        public void CloseAutoPlay()
        {
            if (!isAutoPlay)
            {
                return;
            }
            isAutoPlay = false;
            SayDialog.SwitchSayDialogAutoPlayOfStatus(false);
            var img = btnAuto.transform.Find("Auto-Image").GetComponent<Image>();
            img.sprite = autoPlaySprite;
            img.SetNativeSize();
            Stage.GetActiveStage().SetAutoPlay(false);
        }

        /// <summary>
        /// 調整skip功能是否可以使用的狀態
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
#region SetButtonState
        private IEnumerator AdjustSkipButtonClickState(bool state)
        {
            if (state)
            {
                Image chImg = btnSkip.transform.Find("Skip-Image").GetComponent<Image>();
                yield return chImg.DOColor(Color.white,0.2f).WaitForCompletion();
                btnSkip.interactable = true;
            }
            else
            {
                Image chImg = btnSkip.transform.Find("Skip-Image").GetComponent<Image>();
                yield return chImg.DOColor(new Color(0.5f,0.5f,0.5f), 0.2f).WaitForCompletion();
                btnSkip.interactable = false;
            }
        }
        private void AdjustAutoButtonClickState(bool state)
        {
            if (state)
            {
                btnAuto.interactable = true;
                var img = btnAuto.transform.Find("Auto-Image").GetComponent<Image>();
                var text = btnAuto.transform.Find("Auto-Text").GetComponent<Text>();
                img.DOKill();
                img.DOColor(new Color(1, 1, 1, 1), 0.2f);
                text.DOKill();
                text.DOColor(new Color(1, 1, 1, 1), 0.2f);
            }
            else
            {
                btnAuto.interactable = false;
                var img = btnAuto.transform.Find("Auto-Image").GetComponent<Image>();
                var text = btnAuto.transform.Find("Auto-Text").GetComponent<Text>();
                img.DOKill();
                img.DOColor(new Color(0.5f, 0.5f, 0.5f, 1), 0.2f);
                text.DOKill();
                text.DOColor(new Color(0.5f, 0.5f, 0.5f, 1), 0.2f);
            }
        }
        private IEnumerator AdjustLogButtonClickState(bool state)
        {
            if (state)
            {

                yield return new WaitUntil(() => recordRealCount <= recordDialogList.Count);
                btnLog.interactable = true;
                var img = btnLog.transform.Find("Log-Image").GetComponent<Image>();
                var text = btnLog.transform.Find("Log-Text").GetComponent<Text>();
                img.DOKill();
                img.DOColor(new Color(1, 1, 1, 1), 0.2f);
                text.DOKill();
                text.DOColor(new Color(1, 1, 1, 1), 0.2f);
            }
            else
            {
                btnLog.interactable = false;
                var img = btnLog.transform.Find("Log-Image").GetComponent<Image>();
                var text = btnLog.transform.Find("Log-Text").GetComponent<Text>();
                img.DOKill();
                img.DOColor(new Color(0.5f, 0.5f, 0.5f, 1), 0.2f);
                text.DOKill();
                text.DOColor(new Color(0.5f, 0.5f, 0.5f, 1), 0.2f);
            }
        }
        #endregion
        private IEnumerator OpenMask(Action completeCB)
        {
            blockMask.SetActive(true);
            blockMask.SetCanvasGroup(0);
            yield return  blockMask.eSetCanvasGroup(1,completeCB);

        }


        private IEnumerator CloseMask(Action completeCB=null) {
            if (!blockMask.activeInHierarchy)
            {
                yield break;
            }
            blockMask.SetCanvasGroup(1);
            yield return blockMask.eSetCanvasGroup(0,
                ()=> {
                    completeCB?.Invoke();
                 blockMask.SetActive(false);
                });

        }
    }

       

    public class DialogInfo
    {
        public string CharaName;
        public string DialogContent;
        public AudioClip aAudioClip = null;
        public Sprite nameImageBg;
        public Color aColor;

        public DialogInfo(string charaName, string dialogContent, Sprite _nameImageBg = null, Color _Color = default, AudioClip audioClip = null)
        {
            CharaName = charaName;
            DialogContent = dialogContent;
            aAudioClip = audioClip;
            aColor = _Color;
            nameImageBg = _nameImageBg;
        }
    }
}

