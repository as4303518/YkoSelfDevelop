using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Fungus {

    public class InputCallBack : MonoBehaviour //主要功能,detect mouse input 
    {
        public ClickMode eClickMode;

        public Action SetNextLineFlag = null;

        protected StandaloneInputModule currentStandaloneInputModule;

        [SerializeField] protected bool cancelEnabled = true;

        [SerializeField] protected bool ignoreMenuClicks = true;

        protected bool dialogClickedFlag;

        protected bool nextLineInputFlag;

        public List<GameObject> TriggerObjs = new List<GameObject>();

        public List<IEnumerator> WaitIEList = new List<IEnumerator>();

        private static InputCallBack instance=null;

        public static InputCallBack  Instance(){
            return instance;
        }

        public static bool IsHaveInstance()
        {
            if (instance==null) {
            return false;
            }
            else
            {
                return true;
            }
        }

        public static InputCallBack GetInputCallBack()
        {

            GameObject sp;

            if (!InputCallBack.IsHaveInstance())
            {
                sp = new GameObject("InputSetting", typeof(InputCallBack));
            }
            else
            {
                sp = InputCallBack.Instance().gameObject;
            }
            return sp.GetComponent<InputCallBack>();
        }

        public void Awake()
        {
            if (instance==null) {
                instance = this;
            }
            CheckEventSystem();
        }

        protected virtual void CheckEventSystem()
        {
            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

            if (eventSystem == null)
            {
                // Auto spawn an Event System from the prefab
                GameObject prefab = Resources.Load<GameObject>("Prefabs/EventSystem");

                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    go.name = "EventSystem";
                }
            }
        }
        public IEnumerator CreateDetectInputCB(ClickMode clickMode, Action cb, InputOptions inpOpt = null)
        {
            IEnumerator ie = WaitClickCallBack(clickMode, cb, inpOpt);
            WaitIEList.Add(ie);
            yield return ie;
        }

        public IEnumerator WaitClickCallBack(ClickMode clickMode,Action cb,InputOptions inpOpt=null) 
        {
            eClickMode = clickMode;
            if (cb!=null) {
                SetNextLineFlag = cb;
            }
            else
            {
                SetNextLineFlag = () => { };
            }


            if (EventSystem.current == null )
            {
                Debug.Log("Not Have EventSystem");
               yield break;
            }

            if (currentStandaloneInputModule == null)
            {
                currentStandaloneInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            }

            bool isinput = false;

            

                if (Input.GetButtonDown(currentStandaloneInputModule.submitButton) ||//鍵盤
                    (cancelEnabled && Input.GetButton(currentStandaloneInputModule.cancelButton)))
                {
                    isinput = true;
                    SetNextLineFlag();//偵測點擊後block command執行
                }

                //SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();

                GameObject sp =null;

                switch (eClickMode)
                {
                    case ClickMode.Disabled:
                        break;
                    case ClickMode.ClickAnywhere:
                    
                    inpOpt.SetLocalPos = true;
                    
                    sp = InputUISupportScript.CreateButtonArea(inpOpt,
                        () => {
                        isinput = true;
                        SetNextLineFlag();
                    });
                    //sp.GetComponent<Button>().onClick.AddListener(() => { Destroy(sp); });
                    break;
                    case ClickMode.ClickOnButton:

                        if (inpOpt!=null) {
                             sp = InputUISupportScript.CreateButtonArea(inpOpt, () => {
                                isinput = true;
                                SetNextLineFlag();
                            });

                        }
                        else
                        {
                            Debug.Log("not setting");
                            yield break;
                        }
                        break;

                }
            if (sp!=null) {
                TriggerObjs.Add(sp);
            }

            while (!isinput) 
            {
                if (Flowchart.GetInstance().mStoryControl.isSkipPlay) 
                {
                    isinput = true;
                    SetNextLineFlag();
                }
                yield return null;
            }
            //yield return new WaitUntil(() => isinput);
            //fade out動畫
            SetNextLineFlag = null;
            if (TriggerObjs.Count>0) {
                foreach (GameObject obj in TriggerObjs)
                {
                    Destroy(obj);
                }
                TriggerObjs.Clear();
            }
        }

        private IEnumerator WaitClickMenuCallBack( Action cb, MenuImageOption menuOpt = null)
        {

            eClickMode =ClickMode.ClickOnButton;

            if (EventSystem.current == null)
            {
                Debug.Log("Not Have EventSystem");
                yield break;
            }

            bool isinput = false;

            GameObject sp = null;

            if (menuOpt != null)
            {
                sp = InputUISupportScript.CreateButtonArea(menuOpt._options, () => {
                    isinput = true;
                    cb();
                });


                if (menuOpt._image!=null) {

                    GameObject imObj = new GameObject("DisplayImage", typeof(RectTransform), typeof(Image),typeof(CanvasGroup));
                    imObj.transform.SetParent(sp.transform);
                    imObj.transform.localScale = Vector3.one;
                    imObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    CanvasGroup cg=imObj.GetComponent<CanvasGroup>();
                    cg.alpha = 0;

                    Image im = imObj.GetComponent<Image>();
                    im.sprite = menuOpt._image;
                    im.raycastTarget = false;

                    if (menuOpt.imageSize != Vector2.zero)
                    {
                        imObj.GetComponent<RectTransform>().sizeDelta = menuOpt.imageSize;
                    }
                    else
                    {
                        im.SetNativeSize();
                    }

                    im.preserveAspect = true;

                    LeanTween.alphaCanvas(cg, 1, 0.5f);
                    yield return new WaitForSeconds(0.5f);
                }



            }

            if (sp != null)
            {
                TriggerObjs.Add(sp);
            }
            yield return new WaitUntil(() => isinput);
            //fade out動畫
            if (TriggerObjs.Count > 0)
            {
                foreach (GameObject obj in TriggerObjs)
                {
                    Destroy(obj);
                }
                TriggerObjs.Clear();
            }

        }

        public void CreateMenuImage( MenuImageOption menuImageOptList)// onComplete 執行下一個block
        {
            　
            // Block block = Block.FindBlockByName(menuImageOptList.targetBlock);
            Block block = menuImageOptList.targetBlock;
             Flowchart fc = block.GetFlowchart();


                Action _OnComplete = () => {

                    fc.StartCoroutine(CallBlock(block));
                };
            

                    StartCoroutine(WaitClickMenuCallBack( _OnComplete, menuImageOptList));
        }



        protected IEnumerator CallBlock(Block block) //執行指定block
        {
            yield return new WaitForEndOfFrame();
            block.StartExecution();
        }

        /// <summary>
        /// 初始化觸發
        /// </summary>
        public void InitInputList()
        {
            if (WaitIEList.Count>0) {
                foreach (IEnumerator ie in WaitIEList)
                {
                    StopCoroutine(ie);
                }
                WaitIEList.Clear();
            }


            if (TriggerObjs.Count > 0)
            {
                foreach (GameObject obj in TriggerObjs)
                {
                    Destroy(obj);
                }
                TriggerObjs.Clear();
            }
            if (SetNextLineFlag != null) { 
                SetNextLineFlag();
            }

        }


        [Serializable]
        public class InputOptions {

            public RectTransform parentPos = null;
            public Vector2 touchSize = Vector2.zero;
            public bool SetLocalPos=false;//設置在呼叫Func的帶有Canvas的母物件上(//生成在canvas下)

        }


    }
}
