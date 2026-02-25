using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{

    [CommandInfo("Motion",
        "WaitForClick",
        "Wait Player Click"
        )]
    [AddComponentMenu("")]
    public class WaitForClick : Command
    {

        [SerializeField] protected ClickMode clickMode;
        public ClickMode ClickMode { get { return clickMode; } set {  clickMode = value; } }

        [SerializeField] protected RectTransform toPosition = null;

        [SerializeField] protected float WaitSceond = 0;

        public override void OnEnter()
        {

            if (ParentBlock.GetFlowchart().mStoryControl.isSkipPlay)
            {
                Continue();
                //  StartCoroutine(SaySkip());
            }
            else
            {
                StartCoroutine(WaitForClickStart());
            }
        }





        private IEnumerator WaitForClickStart()
        {

            yield return new WaitForSeconds( WaitSceond );

            SayDialog sayDia = SayDialog.GetSayDialog();
            ClickMode origineMode = ClickMode.Disabled;
            Stage stage = ParentBlock.GetFlowchart().mStage;
            stage.CreateImageTip();
            InputCallBack.InputOptions opt=new InputCallBack.InputOptions();

            opt.parentPos = toPosition;

            switch (clickMode)
            {
                case ClickMode.ClickAnywhere:
                    opt.SetLocalPos = true;
                    RectTransform stageCanvasRect= stage.PortraitCanvas.GetComponent<RectTransform>();
                    opt.parentPos = stageCanvasRect;
                    opt.touchSize = stageCanvasRect.sizeDelta;
                    SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                    yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(clickMode,null,opt);
                    break;
                case ClickMode.ClickOnDialog:
                    origineMode = sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickOnDialog);
                    yield return sayDia.GetWriter().DoWaitForInput();
                    
                    break;
                case ClickMode.ClickOnButton:
                    opt.touchSize = toPosition.sizeDelta;
                    SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                    yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(clickMode, null, opt);
                    break;
            }
            if (origineMode != ClickMode.Disabled)
            {
                sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(origineMode);
            }
            stage.DestoryTip();
            Continue();
        }





    }//-Class WaitForClick
}//-nameSpace
