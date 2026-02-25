using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using System;
using System.Linq;

namespace Fungus
{

    public enum ClickAeraSetting
    {
        Default,
        Customize
    }
    [AddComponentMenu("")]
    [CommandInfo("Narrative",
                  "SpineCharaAni",//顯示名稱 類別是抓繼承command
                  "Controls a Spine Animation.")]
    //flow command
    public class SpineCharaAni : ControlWithDisplay<DisplayType>
    {
        // Start is called before the first frame update
        public Character aTarget;
        public Vector2 testVec2 = new Vector2(17, 123);

        [SerializeField] protected Stage stage;
        [SerializeField] protected FacingDirection facing;

        [SerializeField] protected RectTransform fromPosition = null;

        [SerializeField] protected RectTransform toPosition = null;

        [SerializeField] protected RectTransform ClickPos = null;

        [SerializeField] protected Vector2 ClickButtonSize = new Vector2(300, 300);

        [SerializeField] protected ClickAeraSetting aClickButtonSizeSetting = ClickAeraSetting.Default;

        [SerializeField] protected LeanTweenType easeType = LeanTweenType.linear;

        public bool orderSwitch = false;
        [SerializeField] protected string orderName = null;
        [SerializeField] protected int spineOrder = 0;

        [SerializeField] protected string aAnimation;//要執行的動畫
        [SerializeField] protected string aFinishDefaultAnimation;//執行主動畫後的預設動畫 
        [SerializeField] public string aInitialSkinName;//裝備

        [SerializeField] protected Vector3 offest;//位移差,部分角色的位置需要特別調整

        [SerializeField] protected bool move;
        [SerializeField] protected bool scaleAni;

        [SerializeField] protected bool loop;//是否持續
        [SerializeField] protected bool waitAnimationFinish = false;//等待動畫完成後,接著撥放
        /// <summary>
        /// 等待fade動畫完成
        /// </summary>
        [SerializeField] protected bool waitFadeFinish = false;
        /// <summary>
        /// 效果和動畫同時執行
        /// </summary>
        [SerializeField] protected bool effectAndAnimationSimultaneouslyExecute = true;

        //[SerializeField] protected bool waitForClick = false;//點集才可進入下一段動畫

        // [SerializeField] protected bool waitForButton = false;//必須點擊某處才會進入下一階段

        [SerializeField] protected ClickMode clickMode = ClickMode.Disabled;//必須點擊某處才會進入下一階段

        [SerializeField] protected bool fade = false;

        [SerializeField] protected Vector3 effectScale = Vector3.zero;

        public bool StartDraw = false;

        [SerializeField] public TweenTime aTween = new TweenTime();//儲存所有時間

        public virtual RectTransform FromPosition { get { return fromPosition; } set { fromPosition = value; } }

        public virtual RectTransform ToPosition { get { return toPosition; } set { toPosition = value; } }

        public virtual string FinishDefaultAnimation { get { return aFinishDefaultAnimation; } set { aFinishDefaultAnimation = value; } }

        public virtual ClickMode mClickMode { get { return clickMode; } set { clickMode = value; } }

        public virtual Stage _Stage { get { return stage; } set { stage = value; } }

        public virtual bool Fade { get { return fade; } set { fade = value; } }

        public virtual bool Move { get { return move; } set { move = value; } }

        public override void OnEnter()//Cammand執行邏輯
        {

            if (stage == null)
            {
                // If no default specified, try to get any portrait stage in the scene

                stage = Stage.GetActiveStage();
                // If portrait stage does not exist, do nothing
                if (stage == null)
                {
                    Continue();
                    return;
                }
            }

            if (IsDisplayNone(display))
            {
                Continue();
                return;
            }

            SpineCharaAniOptions opt = new SpineCharaAniOptions();

            if (fade)
            {
                effectAndAnimationSimultaneouslyExecute = true;
            }
            opt._effectAndAnimationSimultaneouslyExecute = effectAndAnimationSimultaneouslyExecute;
            opt._charaName = aTarget.name;
            opt.aTween = aTween;
            opt._SpineCharaPrefab = aTarget.aSkeletonGraphic;
            opt._offest = offest + aTarget.mSet.Offest;
            opt.origineScale = aTarget.mSet.Scale;
            opt._easeType = easeType;
            opt.tarSaydialog = aTarget.SetSayDialog;
            if (scaleAni)
            {
                opt._scale = effectScale;
            }
            else
            {
                if (aTarget.mSet.Scale != Vector2.zero)
                {
                    opt._scale = aTarget.mSet.Scale;
                }
                else
                {
                    opt._scale = Vector2.one;
                }
            }

            opt._OnComplete = Continue;
            opt._waitAnimationFinish = waitAnimationFinish;

            if (GetNextCommandType() == typeof(SpineCharaAni)
                && (GetNextCommand() as SpineCharaAni).Display != display
                && (GetNextCommand() as SpineCharaAni).Fade == true
                && fade)
            {
                waitFadeFinish = true;
            }

            opt._waitFadeFinish = waitFadeFinish;

            if (orderSwitch)
            {


                if (!string.IsNullOrWhiteSpace(orderName) && orderName != "None")
                {
                    opt.orderName = orderName;
                }
                else//預設圖層為character 不然會被背景擋住
                {
                    opt.orderName = "Character";
                }

                opt._spineOrder = spineOrder;

            }
            else
            {
                opt.orderName = "Character";
                opt._spineOrder = 0;
            }

            opt._clickButtonSize = ClickButtonSize;

            if (!string.IsNullOrEmpty(aFinishDefaultAnimation))
            {
                opt._FinishDefaultAnimation = aFinishDefaultAnimation;
            }

            if (!string.IsNullOrEmpty(aAnimation))//沒指定動畫
            {
                opt._animation = aAnimation;//使用指定動畫
            }

            var skins = aTarget.aSkeletonGraphic.GetSkinStrings();


            if (!string.IsNullOrEmpty(aInitialSkinName) && skins.Contains(aInitialSkinName))
            {
                opt._skin = aInitialSkinName;//使用指定造型

            }
            else
            {
                if (!string.IsNullOrEmpty(aTarget.DefaultSkin) && skins.Contains(aTarget.DefaultSkin))//有預設造型
                {
                    opt._skin = aTarget.DefaultSkin;//使用預設造型
                }
                else
                {
                    //如果沒有該造型或者設置為none 則抓skins中的第一個預設
                    if (!string.IsNullOrEmpty(aTarget.aSkeletonGraphic.initialSkinName) && skins.Contains(aTarget.aSkeletonGraphic.initialSkinName))//有預設造型
                    {

                    }
                    else
                    {
                        opt._skin = aTarget.aSkeletonGraphic.GetSkinStrings().FirstOrDefault();
                    }
                }
            }

            opt._display = display;
            opt._reverse = IsReverse();

            opt._move = move;
            opt._scaleAni = scaleAni;

            if (!string.IsNullOrWhiteSpace(aFinishDefaultAnimation))
            {
                opt._loop = true;
            }
            else
            {
                opt._loop = loop;
            }
            opt._fade = fade;

            opt._clickMode = clickMode;

            // opt._waitForButton = waitForButton;
            // opt._waitForClick = waitForClick;

            if (toPosition == null)
            {
                toPosition = stage.DefaultPosition;
            }


            if (fromPosition == null)
            {//避免移動動畫時,找不到生成位置而把生成跟到達位置設置在一起,導致失去位移效果
                fromPosition = stage.DefaultPosition;
            }

            opt._fromPosition = FromPosition;
            opt._toPosition = ToPosition;

            switch (clickMode)
            {
                case ClickMode.Disabled:
                    break;
                case ClickMode.ClickAnywhere:
                    opt._clickButtonSize = stage.PortraitCanvas.GetComponent<RectTransform>().sizeDelta;
                    break;
                case ClickMode.ClickOnDialog:
                    break;
                case ClickMode.ClickOnButton:
                    if (ClickPos == null)
                    {
                        ClickPos = stage.DefaultPosition;
                    }
                    opt._clickPosition = ClickPos;

                    switch (aClickButtonSizeSetting)
                    {
                        case ClickAeraSetting.Default:
                            opt._clickButtonSize = ClickPos.sizeDelta;
                            break;
                        case ClickAeraSetting.Customize:
                            opt._clickButtonSize = ClickButtonSize;
                            break;
                    }
                    break;
            }



            stage.RunSpineCommand(opt);

        }

        public bool IsReverse()//偵測角色是否需要反轉
        {
            if (aTarget.mSet.Facing == FacingDirection.None)
            {
                aTarget.mSet.Facing = FacingDirection.Right;
            }

            if (facing != aTarget.mSet.Facing && facing != FacingDirection.None)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void AdjustCommandExecuteSpeedAcceleration(bool _switch)
        {
            if (_switch)
            {
                aTween.SetToInit();
                clickMode = ClickMode.Disabled;
            }

        }

        public override string GetSummary()
        {
            if (aTarget != null)
            {
                return "當前角色=>" + aTarget.NameText;
            }
            else
            {
                return "";
            }
        }

        public void DrawTexture()
        {
            if (!StartDraw)
            {
                return;
            }
            bool isThis = false;
            foreach (var com in Flowchart.GetInstance().SelectedCommands)
            {
                if (com == this)
                {
                    isThis = true;
                }

            }

            if (!isThis)
            {
                return;
            }


            GameObject sp = new GameObject("displayRect", typeof(RectTransform));
            sp.transform.SetParent(_Stage.DefaultPosition.parent, false);

            RectTransform newRect = sp.GetComponent<RectTransform>();

            //    RectTransform newRect = new RectTransform();

            if (ClickPos != null)
            {

                newRect.position = ClickPos.position;
                newRect.pivot = ClickPos.pivot;
            }
            else
            {

                newRect.position = _Stage.DefaultPosition.position;
                newRect.pivot = _Stage.DefaultPosition.pivot;
            }

            switch (aClickButtonSizeSetting)
            {

                case ClickAeraSetting.Default:
                    if (ClickPos != null)
                    {
                        newRect.sizeDelta = new Vector2(ClickPos.sizeDelta.x, ClickPos.sizeDelta.y);
                    }
                    else
                    {
                        newRect.sizeDelta = new Vector2(_Stage.DefaultPosition.sizeDelta.x, _Stage.DefaultPosition.sizeDelta.y);
                    }
                    break;
                case ClickAeraSetting.Customize:
                    newRect.sizeDelta = ClickButtonSize;
                    break;

            }
            DrawGizmoLine.DrawTexture(newRect);
            DestroyImmediate(sp);


        }

        public void OnDrawGizmos()
        {
            DrawTexture();
        }

        /*  public override void SetSaveData(CommandSaveData saveData)
          {
              (saveData as SpineCharaAniData).SetDataBaseToSpineCharaAni(this);

          }*/

        /*public class SpineCharaAniData:CommandSaveData
        {
            public FacingDirection facing;
            public string fromPositionName;
            public string toPositionName;
            public string clickPosName;
            public Vector2 ClickButtonSize;
            public ClickAeraSetting aClickButtonSizeSetting;
            public int spineOrder;
            public string aAnimation;
            public string aInitialSkinName;
            public Vector3 offest;
            public bool move;
            public bool loop;
            public bool waitAnimationFinish = false;//等待動畫完成後,接著撥放
            public bool waitDialog = false;
            public ClickMode clickMode = ClickMode.Disabled;//必須點擊某處才會進入下一階段
            public bool fade = false;
            public TweenTime aTween = new TweenTime();//儲存所有時間

            public SpineCharaAniData(SpineCharaAni data) { 

                facing = data.facing;

                if (data.fromPosition) {
                    fromPositionName = data.fromPosition.name;
                }
                else
                {
                    fromPositionName = null;
                }
                if (data.toPosition) {
                    toPositionName = data.toPosition.name;
                }
                else
                {
                    toPositionName = null;
                }
                if (data.ClickPos) {
                    clickPosName = data.ClickPos.name;
                }
                else
                {
                    clickPosName = null;
                }



                ClickButtonSize = data.ClickButtonSize;
                aClickButtonSizeSetting = data.aClickButtonSizeSetting;
                spineOrder = data.spineOrder;
                aAnimation = data.aAnimation;
                aInitialSkinName = data.aInitialSkinName;
                offest = data.offest;
                move = data.move;
                loop = data.loop;
                waitAnimationFinish = data.waitAnimationFinish;
                waitDialog = data.waitDialog;
                clickMode=data.clickMode;
                fade = data.fade;
                aTween = data.aTween;

                Debug.Log("資料都獲取完");

            }

            public void SetDataBaseToSpineCharaAni(SpineCharaAni data)
            {
                data.facing=facing;

                data.fromPosition.name= fromPositionName ;
                data.toPosition.name= toPositionName;
                clickPosName = data.ClickPos.name;

                data.fromPosition = GetStageRectTransform(fromPositionName);
                data.toPosition=GetStageRectTransform(toPositionName);
                data.ClickPos = GetStageRectTransform(clickPosName);




                data.ClickButtonSize= ClickButtonSize;

                data.aClickButtonSizeSetting = aClickButtonSizeSetting;
                data.spineOrder= spineOrder;
                data.aAnimation= aAnimation;


                data.aInitialSkinName= aInitialSkinName ;

                data.offest= offest ;
                data.move= move ;
                data.loop= loop ;
                data.waitAnimationFinish= waitAnimationFinish ;
                data.waitDialog= waitDialog ;
                data.clickMode= clickMode;
                data.fade= fade ;
                data.aTween = aTween;


            }


        }*/


    }
}