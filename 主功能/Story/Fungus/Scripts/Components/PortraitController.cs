// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using MoonSharp.Interpreter;
using Spine.Unity;
using DG.Tweening;
using YKO.Support.Expansion;

namespace Fungus
{
    /// <summary>
    /// Types of display operations supported by portraits.
    /// </summary>
    public enum DisplayType//圖片顯示模式
    {
        /// <summary> Do nothing. </summary>
        None,
        /// <summary> Show the portrait. </summary>
        Show,
        /// <summary> Hide the portrait. </summary>
        Hide,
        /// <summary> Replace the existing portrait. </summary>
        Replace,
        /// <summary> Move portrait to the front. </summary>
        MoveToFront
    }

    /// <summary>
    /// Directions that character portraits can face.
    /// </summary>
    public enum FacingDirection
    {
        /// <summary> Unknown direction </summary>
        None,
        /// <summary> Facing left. </summary>
        Left,
        /// <summary> Facing right. </summary>
        Right
    }

    /// <summary>
    /// Offset direction for position.
    /// </summary>
    public enum PositionOffset
    {
        /// <summary> Unknown offset direction. </summary>
        None,
        /// <summary> Offset applies to the left. </summary>
        OffsetLeft,
        /// <summary> Offset applies to the right. </summary>
        OffsetRight
    }


    [AddComponentMenu("")]
    /// <summary>
    /// Controls the Portrait sprites on stage. 
    /// 
    /// Is only really used via Stage, it's child class. This class continues to exist to support existing API
    /// dependant code. All functionality is stage dependant.
    /// </summary>
    public class PortraitController : MonoBehaviour
    {
        // Timer for waitUntilFinished functionality
        protected float waitTimer;

        protected Stage stage;

        [SerializeField] private bool WaitUserInput = false;

        [SerializeField] private bool InAutoPlay=false;

        [SerializeField] private bool InSkipPlay = false;

        private SpineCharaAniOptions aOptions;//給予自動撥放功能觸發用

        protected virtual void Awake()
        {
            stage = GetComponentInParent<Stage>();
            
        }

        protected virtual void FinishCommand(PortraitOptions options)
        {
            if (options.onComplete != null)
            {
                if (!options.waitUntilFinished)
                {
                    options.onComplete();
                }
                else
                {
                    StartCoroutine(WaitUntilFinished(options.fadeDuration, options.onComplete));
                }
            }
            else
            {
                StartCoroutine(WaitUntilFinished(options.fadeDuration));
            }
        }

        protected virtual void FinishCommand(SpineCharaAniOptions options)
        {
            if (options._OnComplete != null)
            {
                if (options.aTween.aFinishDuration <= 0)
                {
                    options._OnComplete();
                }
                else
                {
                    StartCoroutine(WaitUntilFinished(options.aTween.aFinishDuration, options._OnComplete));
                }
            }
            else
            {
                StartCoroutine(WaitUntilFinished(options.aTween.aFinishDuration));
            }
        }

     


        protected virtual SpineCharaAniOptions CleanPortraitOptions(SpineCharaAniOptions opt)
        {
            // Use default stage settings
           /* if (opt.aTween.aEffectAniDuration <= 0)
            {
                opt.aTween.aEffectAniDuration = 0.4f;
            }*/

          /*  if (opt.aTween.aFadeAniDuration <= 0)
            {
                opt.aTween.aFadeAniDuration = 0.4f;
            }*/

            if (opt.CharaObj == null)
            {
                CreateSpineObject(opt);
            }
            if (opt.CharaObj!=null) 
            {
                LeanTween.cancel(opt.CharaObj);
            }
            JudgeCharaSetDimmed(opt._charaName);
            return opt;
        }

        protected virtual void CreateSpineObject(SpineCharaAniOptions opt)
        {
            if (opt.CharaObj == null)//生成角色
            {
                bool listIsHave = false;
                foreach (var optList in stage.SpineSkeletonGraphicOnStageList)
                {
                    if (opt._charaName == optList.name)
                    {
                        opt.CharaObj = optList.gameObject;
                        opt.CharaObj.SetCanvasGroup(1);
                        opt.CharaObj.SetActive(true);
                        listIsHave = true;
                    }
                }

                if (opt._display == DisplayType.Hide && opt.CharaObj == null)
                {
                    Debug.Log("抓不到角色");
                    return;
                }

                if (!listIsHave)
                {
                    var sp= Instantiate(opt._SpineCharaPrefab);
                    sp.name = opt._charaName;
                    sp.transform.SetParent(stage.CharaParent,false);
                    
                    sp.gameObject.layer = LayerMask.NameToLayer("StoryContent");
                    opt.CharaObj = sp.gameObject;
                   // CharaRenderer charaRender= sp.GetComponent<CharaRenderer>();
                   //charaRender.Init(opt,stage);
                   //  sp.transform.position = new Vector3(30*stage.SpineCharaOnStageList.Count, 0, 0);

                    stage.SpineSkeletonGraphicOnStageList.Add(sp);
                }

                if (opt._spineOrder!=0) 
                {
                    Canvas can = opt.CharaObj.GetComponent<Canvas>();
                    if (!can) 
                    {
                        can = opt.CharaObj.gameObject.AddComponent<Canvas>();
                    }
                    can.overrideSorting = true;
                    
                    can.sortingLayerID = SortingLayer.NameToID(opt.orderName);
                    //can.sortingLayerName = "Character";
                    can.sortingOrder=opt._spineOrder;
                }
                var rect = opt.CharaObj.GetComponent<RectTransform>();
                // opt._SpineChara.color = new Color(1f, 1f, 1f, 1f);
                if (opt._move)
                {
                    if (opt._fromPosition != null)
                    {
                        Vector3 toPos = opt._fromPosition.position;
                        SetRectTransform(rect, toPos);
                        //調整位移差
                        rect.localPosition = new Vector3(rect.localPosition.x + opt._offest.x, rect.localPosition.y + opt._offest.y, 0);
                    }
                    else
                    {
                        if (opt._toPosition!=null) //listIsHavec剛出現,還沒被預設位置|| opt._toPosition有指定移動位置
                        {
                            Vector3 toPos = opt._toPosition.position;
                            SetRectTransform(rect, toPos);
                            rect.localPosition = new Vector3(rect.localPosition.x + opt._offest.x, rect.localPosition.y + opt._offest.y, 0);
                        }
                        else
                        {
                            Vector3 toPos = stage.DefaultPosition.position;
                            SetRectTransform(rect, toPos);
                            rect.localPosition = new Vector3(rect.localPosition.x + opt._offest.x, rect.localPosition.y + opt._offest.y, 0);

                        }
                    }


                }
                else
                {
                    if (opt._toPosition != null) //listIsHavec剛出現,還沒被預設位置|| opt._toPosition有指定移動位置
                    {
                        Vector3 toPos = opt._toPosition.position;
                        SetRectTransform(rect, toPos);
                        rect.localPosition = new Vector3(rect.localPosition.x + opt._offest.x, rect.localPosition.y + opt._offest.y, 0);
                    }
                    else
                    {
                        Vector3 toPos = stage.DefaultPosition.position;
                        SetRectTransform(rect, toPos);
                        rect.localPosition = new Vector3(rect.localPosition.x + opt._offest.x, rect.localPosition.y + opt._offest.y, 0);

                    }
                }

            }
        }

        protected virtual IEnumerator WaitUntilFinished(float duration, Action onComplete = null)
        {
            // Wait until the timer has expired
            // Any method can modify this timer variable to delay continuing.

            waitTimer = duration;
            while (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                yield return null;
            }

            // Wait until next frame just to be safe
            yield return new WaitForEndOfFrame();

            if (onComplete != null)
            {
                onComplete();
            }
        }

        protected virtual void SetupPortrait(PortraitOptions options)//處理鏡像
        {
            if (options.character.State.holder == null)
                return;

            SetRectTransform(options.character.State.holder, options.fromPosition);

            if (options.character.State.facing != options.character.PortraitsFace)
            {
                options.character.State.holder.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                options.character.State.holder.localScale = new Vector3(1f, 1f, 1f);
            }

            if (options.facing != options.character.PortraitsFace)
            {
                options.character.State.holder.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                options.character.State.holder.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        protected virtual void SetupPortrait(SpineCharaAniOptions options)//處理鏡像
        {
            if (options._SpineCharaPrefab == null)
            {
                return;
            }
            if (options.CharaObj == null)
            {
                Debug.Log("***Not Have Instance");//無生產實例
                return;
            }

            if (options._reverse)
            {
                options.CharaObj.transform.localEulerAngles = new Vector3(0, options.CharaObj.transform.localEulerAngles.y + 180, 0);
            }

        }

   



        /// <summary>
        /// Performs a deep copy of all values from one RectTransform to another.
        /// </summary>
        public static void SetRectTransform(RectTransform target, RectTransform from)
        {

           /* target.eulerAngles = from.eulerAngles;
            target.rotation = from.rotation;
            target.sizeDelta = from.sizeDelta;
          //  target.anchorMax = from.anchorMax;
        //    target.anchorMin = from.anchorMin;
            target.pivot = from.pivot;*/
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);
            target.anchoredPosition3D = new Vector3(from.anchoredPosition.x, from.anchoredPosition.y, 0);


        }

        /// <summary>
        /// Performs a deep copy of all values from one RectTransform to another.
        /// </summary>
        public static void SetRectTransform(RectTransform target, Vector3 from)
        {

            /* target.eulerAngles = from.eulerAngles;
             target.rotation = from.rotation;
             target.sizeDelta = from.sizeDelta;
           //  target.anchorMax = from.anchorMax;
         //    target.anchorMin = from.anchorMin;
             target.pivot = from.pivot;*/
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);
            target.position = new Vector2(from.x, from.y);
        }

        public virtual void RunSpineCommand(SpineCharaAniOptions opt)
        {
           // StopAllCoroutines();
            waitTimer = 0f;
            // If no character specified, do nothing
            if (opt._SpineCharaPrefab == null)
            {
                opt._OnComplete();
                return;
            }

            // If Replace and no replaced character specified, do nothing
            if (opt._display == DisplayType.Replace && opt._ReplacedCharacter == null)
            {
                opt._OnComplete();
                return;
            }
            opt = CleanPortraitOptions(opt);


            if (opt._display == DisplayType.Hide && opt.CharaObj == null)
            {
                opt._OnComplete();
                return;
            }
            switch (opt._display)
            {
                case (DisplayType.Show):
                    StartCoroutine(Show(opt));
                    break;

                case (DisplayType.Hide):
                    StartCoroutine(Hide(opt));
                    break;

                case (DisplayType.Replace):
                    // Show(options);
                    // Hide(options.replacedCharacter, options.replacedCharacter.State.position.name);
                    break;

                case (DisplayType.MoveToFront):
                    // MoveToFront(options);
                    break;
            }



        }
        /// <summary>
        /// Moves Character in front of other characters on stage
        /// </summary>
        public virtual void MoveToFront(PortraitOptions options)
        {
            options.character.State.holder.SetSiblingIndex(options.character.State.holder.parent.childCount);
            options.character.State.display = DisplayType.MoveToFront;
            FinishCommand(options);
        }
        /// <summary>
        /// 確保每個控制的角色都在最前面
        /// </summary>
      //  private int displayOrder = 0;
        public virtual void MoveToFront(SpineCharaAniOptions options)
        {
           // displayOrder++;
            options.CharaObj.transform.SetSiblingIndex(options.CharaObj.transform.parent.childCount);

        //    options.CharaObj.SetCanvasToOrder(displayOrder+1);
            // options.character.State.display = DisplayType.MoveToFront;
            // FinishCommand(options);
        }

        /// <summary>
        /// 展示角色
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public virtual IEnumerator Show(SpineCharaAniOptions opt)
        {
            aOptions = opt;
            // opt = CleanPortraitOptions(opt);//生成
            SetupPortrait(opt);//鏡像設置

            // LeanTween doesn't handle 0 duration properly
            MoveToFront(opt);//將目標設置在前面
            opt.SetCharaSetting();
            if (InSkipPlay) 
            {
                opt.aTween.RetureZeroValue();
            }

            if (opt._effectAndAnimationSimultaneouslyExecute)
            {
                opt.SetMyAnimation(opt._animation);
            }

            if (opt._move)
            {
                Vector3 toPos = opt._toPosition.anchoredPosition3D + opt._offest;
                var sp = new GameObject("temp", typeof(RectTransform));
                sp.transform.SetParent(stage.CharaParent,false);
                var rect = sp.GetComponent<RectTransform>();
                var charaRect = opt.CharaObj.GetComponent<RectTransform>();
                rect.pivot = charaRect.pivot;
                rect.anchorMin = charaRect.anchorMin;
                rect.anchorMax=charaRect.anchorMax;

                rect.anchoredPosition3D = toPos;
                Vector3 purpose = sp.transform.position;
                Destroy(sp);
                opt.CharaObj.GetComponent<RectTransform>().DOMove(purpose, opt.aTween.aEffectAniDuration);

            }
            if (opt._scaleAni)
            {
               StartCoroutine(SpineTween.SpineScale(opt.CharaObj, opt._scale, opt._easeType, opt.aTween.aEffectAniDuration));
            }
            if (opt._fade)
            {
                //opt._rawImage.gameObject.SetCanvasGroup(0);
                var charaRender = stage.CharaRendererParent.GetComponent<CharaRenderer>();

                StartCoroutine(charaRender.FadeIn(opt, stage.CharaParent, "StoryContent"));
                

             //   StartCoroutine(opt.CharaObj.eSetCanvasGroup(1,null,0,opt.aTween.aFadeAniDuration));
            }

            float waitTime = 0;
                if ((opt._move || opt._scaleAni) && opt._fade)
                {
                    // waitTime=options.aTween.aFadeAniDuration>options.aTween.aMoveAniDuration?options.aTween.aFadeAniDuration:options.aTween.aMoveAniDuration;
                    waitTime = Mathf.Max(opt.aTween.aFadeAniDuration, opt.aTween.aEffectAniDuration);
                }
                else if ((opt._move || opt._scaleAni) && !opt._fade)
                {
                    waitTime = opt.aTween.aEffectAniDuration;
                }
                else if (opt._waitFadeFinish) 
                {
                    waitTime = opt.aTween.aFadeAniDuration+0.1f;
                 }
                 
               
                yield return new WaitForSeconds(waitTime);//位移動畫或淡入
            
            if (!opt._effectAndAnimationSimultaneouslyExecute)
            {
                opt.SetMyAnimation(opt._animation);
            }

            IEnumerator StartDefaultAnimation(float waitSecond=0)
            {
                yield return new WaitForSeconds(waitSecond);
                opt.SetMyAnimation(opt._FinishDefaultAnimation);
            }

            if (opt._waitAnimationFinish)
            {
                yield return new WaitForSeconds(opt.aTween.aAnimationPlayRoundTime);//等待spine動畫結束
                if (opt.CharaObj.activeSelf)
                    opt.CharaObj?.GetComponent<SkeletonGraphic>().StartCoroutine(StartDefaultAnimation());
                
            }
            else
            {
                if (opt.CharaObj.activeSelf) 
                opt.CharaObj?.GetComponent<SkeletonGraphic>().StartCoroutine(StartDefaultAnimation(opt.aTween.aAnimationPlayRoundTime));
            }

            if (!InAutoPlay) {
                SayDialog sayDia = SayDialog.GetSayDialog(opt.tarSaydialog);
                WaitUserInput = true;
                InputCallBack.InputOptions setting = new InputCallBack.InputOptions();
                setting.parentPos = opt._toPosition;
                setting.touchSize = opt._clickButtonSize;
                ClickMode origineMode = ClickMode.Disabled;

                switch (opt._clickMode)//返回舊有的對話模式
                {

                    case ClickMode.ClickAnywhere:
                        //origineMode=  sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickAnywhere);
                        // yield return sayDia.GetWriter().WaitForClick();
 
                        SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                        yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(opt._clickMode, null, setting);

                        break;
                    case ClickMode.ClickOnDialog:
                        origineMode = sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickOnDialog);
                        yield return sayDia.GetWriter().DoWaitForInput();

                        break;
                    case ClickMode.ClickOnButton:

                        SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                        Stage stage = Stage.GetActiveStage();
                        stage.CloseOtherRaycastTarget(setting.parentPos);

                        yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(opt._clickMode, null, setting);
                        stage.OpenAllPositionRaycastTarget();
                        break;

                    default:
                        origineMode = sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickAnywhere);
                        break;
                }

                if (origineMode != ClickMode.Disabled) {    
                    sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(origineMode);
                }
                WaitUserInput = false;
            }
            FinishCommand(opt);

        }

        public virtual IEnumerator Hide(SpineCharaAniOptions opt)
        {
            // CleanPortraitOptions(opt);

            if (opt._display == DisplayType.None)
            {
                yield break;
            }
            opt.SetCharaSetting(false);
            SetupPortrait(opt);//處理鏡像
            if (!InAutoPlay)
            {
                SayDialog sayDia = SayDialog.GetSayDialog(opt.tarSaydialog);
                WaitUserInput = true;
                InputCallBack.InputOptions setting = new InputCallBack.InputOptions();

                ClickMode origineMode = ClickMode.Disabled;
                switch (opt._clickMode)//返回舊有的對話模式
                {

                    case ClickMode.ClickAnywhere:
                        //origineMode=  sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickAnywhere);
                        // yield return sayDia.GetWriter().WaitForClick();
                        setting.parentPos = opt._toPosition;
                        SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                        yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(opt._clickMode, null, setting);

                        break;
                    case ClickMode.ClickOnDialog:
                        origineMode = sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickOnDialog);
                        yield return sayDia.GetWriter().DoWaitForInput();

                        break;
                    case ClickMode.ClickOnButton:

                        setting.parentPos = opt._clickPosition;
                        setting.touchSize = opt._clickButtonSize;
                        SayDialog.GetSayDialog().GetComponent<DialogInput>().CloseInputButtonArea();
                        yield return InputCallBack.GetInputCallBack().CreateDetectInputCB(opt._clickMode, null, setting);

                        break;

                    default:
                        origineMode = sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(ClickMode.ClickAnywhere);
                        break;
                }
                if (origineMode != ClickMode.Disabled)
                {
                    sayDia.GetWriter().GetComponent<DialogInput>().SetDialogInputModle(origineMode);
                }
                WaitUserInput = false;
            }
            if (opt._move)
            {
                Vector3 toPos = opt._toPosition.anchoredPosition3D + opt._offest;
                opt.CharaObj.GetComponent<RectTransform>().DOAnchorPos(toPos, opt.aTween.aEffectAniDuration);
                //   StartCoroutine(SpineTween.DoMoveTween(opt._rawImage.gameObject, toPos, opt._easeType, opt.aTween.aEffectAniDuration));
            }
            if (opt._scaleAni)
            {
                StartCoroutine(SpineTween.SpineScale(opt.CharaObj, opt._scale, opt._easeType, opt.aTween.aEffectAniDuration));
            }
            if (opt._fade)
            {
                var charaRender = stage.CharaRendererParent.GetComponent<CharaRenderer>();
                StartCoroutine(charaRender.FadeOut(opt, stage.CharaParent, "StoryContent",()=> { ClearCharaRecord(opt); }));
            }
            opt.SetMyAnimation(opt._animation);


            float waitTime = 0;

             if (opt._waitFadeFinish)
            {
                waitTime = opt.aTween.aFadeAniDuration+0.1f;

            }
            else if (opt._waitAnimationFinish)//不需要讓後續指令等待前面的指令
            {
                waitTime = opt.aTween.aAnimationPlayRoundTime;

            }

            yield return new WaitForSeconds(waitTime);//位移動畫或淡入
            FinishCommand(opt);

            /*
                        if (opt._move && opt._fade)
                        {
                            // waitTime=options.aTween.aFadeAniDuration>options.aTween.aMoveAniDuration?options.aTween.aFadeAniDuration:options.aTween.aMoveAniDuration;
                            waitTime = Mathf.Max(opt.aTween.aFadeAniDuration, opt.aTween.aEffectAniDuration);
                            yield return new WaitForSeconds(waitTime);//位移動畫或淡入
                            finishCommand();
                        }
                        else if (opt._move && !opt._fade)
                        {
                            waitTime = opt.aTween.aEffectAniDuration;
                            yield return new WaitForSeconds(waitTime);//位移動畫或淡入
                            finishCommand();
                        }
                        else if (opt._waitFadeFinish)
                        {
                            waitTime = opt.aTween.aFadeAniDuration;
                            yield return new WaitForSeconds(waitTime);//位移動畫或淡入
                            finishCommand();
                        }
                        else if (opt.aTween.aFadeAniDuration > 0)//不需要讓後續指令等待前面的指令
                        {
                            IEnumerator waitDelete()
                            {
                                yield return new WaitForSeconds(opt.aTween.aFadeAniDuration);
                                finishCommand();
                            }

                            StartCoroutine(waitDelete());
                        }
            */
        }

        //fade in out的時候切換成raw image  並且角色跑到淡出的圖層下

        public void ClearCharaRecord(SpineCharaAniOptions opt)
        {
           opt.CharaObj.GetComponent<SkeletonGraphic>().DOKill();
            /*   int num = 0;
           foreach (var obj in stage.SpineSkeletonGraphicOnStageList)
           {
               if (obj.gameObject.name == opt.CharaObj.name)
               {
                   break;
               }
               num++;
           }*/
            opt.CharaObj.SetCanvasGroup(0);
            opt.CharaObj.SetActive(false);
            //Debug.Log("執行清除");
            //opt.CharaObj.SetActive(false);
          //  Destroy(opt.CharaObj);
         //   Destroy(stage.SpineSkeletonGraphicOnStageList[num].gameObject);

          //  stage.SpineSkeletonGraphicOnStageList.RemoveAt(num);

        }

        /// <summary>
        /// Sets the dimmed state of a character on the stage.
        /// </summary>
        public virtual void SetDimmed(Character character, bool dimmedState)
        {
            if (character.State.dimmed == dimmedState)
            {
                return;
            }
            
            character.State.dimmed = dimmedState;

            Color targetColor = dimmedState ? stage.DimColor : Color.white;

            // LeanTween doesn't handle 0 duration properly
            float duration = (stage.FadeDuration > 0f) ? stage.FadeDuration : float.Epsilon;

            LeanTween.color(character.State.portraitImage.rectTransform, targetColor, duration).setEase(stage.FadeEaseType).setRecursive(false);


        }

        public virtual void JudgeCharaSetDimmed(string displayChara)
        {
            Debug.Log("inherit class write");
        }
        /// <summary>
        /// 設置
        /// </summary>
        /// <param name="character"></param>
        /// <param name="dimmedState"></param>
        /// <returns></returns>
        public virtual IEnumerator SetCharaDimmed(SkeletonGraphic character, bool dimmedState)
        {
            Color targetColor = dimmedState ? stage.DimColor : Color.white;
            //這裡的fadeDuration是拿stage的
            // LeanTween doesn't handle 0 duration properly
            float duration = (stage.FadeDuration > 0f) ? stage.FadeDuration : float.Epsilon;
            var c= character.DOColor(targetColor, duration).AsyncWaitForCompletion();
            yield return c;
           // LeanTween.color(character.State.portraitImage.rectTransform, targetColor, duration).setEase(stage.FadeEaseType).setRecursive(false);

        }



        public void SetAutoPlay(bool _switch)
        {
            if (_switch)
            {
                InAutoPlay = true;
                //因為進入自動化或skip,所以重新初始化觸碰功能,先刪除所有當前停留得觸碰
                InputCallBack.GetInputCallBack().InitInputList();
                if (WaitUserInput) {
                    aOptions._OnComplete();
                    WaitUserInput = false;
                }
            }
            else
            {
                InAutoPlay = false;
            }
        }
        /// <summary>
        /// 設定是否進入跳過階段
        /// </summary>
        /// <param name="_switch"></param>
        public void SetSkipPlay(bool _switch)
        {
            if (InSkipPlay)
            {
                InSkipPlay = true;
            }
            else
            {
                InSkipPlay = false;
            }

        }


    }
}
