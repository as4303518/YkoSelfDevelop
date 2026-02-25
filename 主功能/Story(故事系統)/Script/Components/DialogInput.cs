// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Fungus
{
    /// <summary>
    /// Supported modes for clicking through a Say Dialog.
    /// </summary>
    public enum ClickMode
    {
        /// <summary> Clicking disabled. </summary>
        Disabled,
        /// <summary> Click anywhere on screen to advance. </summary>
        ClickAnywhere,
        /// <summary> Click anywhere on Say Dialog to advance. </summary>
        ClickOnDialog,
        /// <summary> Click on continue button to advance. </summary>
        ClickOnButton
    }

    /// <summary>
    /// Input handler for say dialogs.
    /// </summary>
    public class DialogInput : MonoBehaviour
    {
        [Tooltip("Click to advance story")]
        [SerializeField] protected ClickMode clickMode;

        [Tooltip("Delay between consecutive clicks. Useful to prevent accidentally clicking through story.")]
        [SerializeField] protected float nextClickDelay = 0f;

        [Tooltip("Allow holding Cancel to fast forward text")]
        [SerializeField] protected bool cancelEnabled = true;

        [Tooltip("Ignore input if a Menu dialog is currently active")]
        [SerializeField] protected bool ignoreMenuClicks = true;

        [SerializeField] protected Canvas tarCanvas =null;

        protected bool dialogClickedFlag;

        protected bool nextLineInputFlag;

        protected float ignoreClickTimer;

        private GameObject ButtonInputArea = null;

        protected StandaloneInputModule currentStandaloneInputModule;

        protected Writer writer;

        protected virtual void Awake()
        {
            writer = GetComponent<Writer>();

            CheckEventSystem();
        }

        // There must be an Event System in the scene for Say and Menu input to work.
        // This method will automatically instantiate one if none exists.
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

        public ClickMode SetDialogInputModle(ClickMode mode)//設置對話框的dialog mode 並返回原本的(觸發完後再改回來
        {
            var origineMode= clickMode;
            clickMode = mode;
            return origineMode;

        }

        /*protected virtual void Update()
        {
            if (EventSystem.current == null||clickMode==ClickMode.ClickOnButton)
            {
                return;
            }

            if (currentStandaloneInputModule == null)
            {
                currentStandaloneInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            }

            if (writer != null)
            {
                if (Input.GetButtonDown(currentStandaloneInputModule.submitButton) ||//鍵盤
                    (cancelEnabled && Input.GetButton(currentStandaloneInputModule.cancelButton)))
                {
                    SetNextLineFlag();//偵測點擊後block command執行
                }
            }

            switch (clickMode)
            {
            case ClickMode.Disabled:
                break;
            case ClickMode.ClickAnywhere:
         if (Input.GetMouseButtonDown(0))
                      {
                          SetClickAnywhereClickedFlag();//偵測點擊任一位置後block command執行
                      }
                    ButtonAllScreenClickSetting();

                    break;
            case ClickMode.ClickOnDialog://必須點擊在話框上
                if (dialogClickedFlag)
                {
                    SetNextLineFlag();//偵測點擊後block command執行    
                    dialogClickedFlag = false;
                }
                break;
            }

            if (ignoreClickTimer > 0f)
            {
                ignoreClickTimer = Mathf.Max (ignoreClickTimer - Time.deltaTime, 0f);
            }

            if (ignoreMenuClicks)
            {
                // Ignore input events if a Menu is being displayed
                if (MenuDialog.ActiveMenuDialog != null && 
                    MenuDialog.ActiveMenuDialog.IsActive() &&
                    MenuDialog.ActiveMenuDialog.DisplayedOptionsCount > 0)
                {
                    dialogClickedFlag = false;
                    nextLineInputFlag = false;
                }
            }

            // Tell any listeners to move to the next line
            if (nextLineInputFlag)
            {
                var inputListeners = gameObject.GetComponentsInChildren<IDialogInputListener>();
                for (int i = 0; i < inputListeners.Length; i++)
                {
                    var inputListener = inputListeners[i];
                    inputListener.OnNextLineEvent();
                }
                nextLineInputFlag = false;
            }
        }*/

        public IEnumerator OpenDetectInput()
        {
            if (EventSystem.current == null || clickMode == ClickMode.ClickOnButton)
            {
               yield break;
            }

            if (currentStandaloneInputModule == null)
            {
                currentStandaloneInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            }

            if (writer != null)
            {
                if (Input.GetButtonDown(currentStandaloneInputModule.submitButton) ||//鍵盤
                    (cancelEnabled && Input.GetButton(currentStandaloneInputModule.cancelButton)))
                {
                    SetNextLineFlag();//偵測點擊後block command執行
                }
            }
            switch (clickMode)
            {
                case ClickMode.Disabled:
                    break;
                case ClickMode.ClickAnywhere:

                    ButtonAllScreenClickSetting();

                    break;
                case ClickMode.ClickOnDialog://必須點擊在話框上
                    if (dialogClickedFlag)
                    {
                        SetNextLineFlag();//偵測點擊後block command執行    
                        dialogClickedFlag = false;
                    }
                    break;
            }

            if (ignoreClickTimer > 0f)
            {
                ignoreClickTimer = Mathf.Max(ignoreClickTimer - Time.deltaTime, 0f);
            }

            if (ignoreMenuClicks)
            {
                // Ignore input events if a Menu is being displayed
                if (MenuDialog.ActiveMenuDialog != null &&
                    MenuDialog.ActiveMenuDialog.IsActive() &&
                    MenuDialog.ActiveMenuDialog.DisplayedOptionsCount > 0)
                {
                    dialogClickedFlag = false;
                    nextLineInputFlag = false;
                }
            }

            // Tell any listeners to move to the next line

            yield return new WaitUntil(() => nextLineInputFlag);

                var inputListeners = gameObject.GetComponentsInChildren<IDialogInputListener>();
                for (int i = 0; i < inputListeners.Length; i++)
                {
                    var inputListener = inputListeners[i];
                    inputListener.OnNextLineEvent();
                }
                nextLineInputFlag = false;
            

        }


        #region Public members



        /// <summary>
        /// Trigger next line input event from script.
        /// </summary>
        public virtual void SetNextLineFlag()
        {
            if(writer.IsWaitingForInput || writer.IsWriting)
            {
                nextLineInputFlag = true;
            }
        }
        /// <summary>
        /// Set the ClickAnywhere click flag.
        /// </summary>
        public virtual void SetClickAnywhereClickedFlag()
        {
            if (ignoreClickTimer > 0f)
            {
                return;
            }
            ignoreClickTimer = nextClickDelay;

            // Only applies if ClickedAnywhere is selected
            if (clickMode == ClickMode.ClickAnywhere)
            {
                SetNextLineFlag();
            }
        }
        /// <summary>
        /// Set the dialog clicked flag (usually from an Event Trigger component in the dialog UI).
        /// </summary>
        public virtual void SetDialogClickedFlag()//editor 點選
        {
            // Ignore repeat clicks for a short time to prevent accidentally clicking through the character dialogue
            if (ignoreClickTimer > 0f)
            {
                return;
            }
            ignoreClickTimer = nextClickDelay;

            // Only applies in Click On Dialog mode
            if (clickMode == ClickMode.ClickOnDialog)
            {
                dialogClickedFlag = true;
            }
        }


        public void ButtonAllScreenClickSetting()
        {
            if (ButtonInputArea==null) {

                InputCallBack.InputOptions inpOpt = new InputCallBack.InputOptions();
                inpOpt.parentPos = gameObject.GetComponent<RectTransform>();
                if (tarCanvas == null)
                {
                    tarCanvas = GetComponent<Canvas>();
                }

                inpOpt.touchSize = tarCanvas.GetComponent<RectTransform>().sizeDelta;
                inpOpt.SetLocalPos = true;
                
                ButtonInputArea = InputUISupportScript.CreateButtonArea(inpOpt, () => {
                    CloseInputButtonArea();
                    SetNextLineFlag();
                });

            }
            else
            {
                OpenInputButtonArea();
                return;
            }

        }

        public void OpenInputButtonArea()
        {
            if (ButtonInputArea!=null) {
                ButtonInputArea.GetComponent<Image>().raycastTarget = true;
            }

        }
        public void CloseInputButtonArea()
        {
            if (ButtonInputArea != null)
            {
                ButtonInputArea.GetComponent<Image>().raycastTarget = false;
            }
        }

        /// <summary>
        /// Sets the button clicked flag.
        /// </summary>
        public virtual void SetButtonClickedFlag()//點擊按紐繼續對話
        {
            // Only applies if clicking is not disabled
            if (clickMode != ClickMode.Disabled)
            {
                SetNextLineFlag();
            }
        }

        #endregion
    }
}
