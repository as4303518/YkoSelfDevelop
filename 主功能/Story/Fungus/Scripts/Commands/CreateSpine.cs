using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
//using UnityEditor;
using UnityEngine.AddressableAssets;
using System;


namespace Fungus
{
    [AddComponentMenu("")]
    [CommandInfo("Narrative", "CreateSpine", "Create Spine Object")]
    public class CreateSpine : Command
    {
        public enum EffectType
        {
            None,
            FadeIn,
            FadeOut,
            FadeWaitTime//動畫播放完畢關閉
        }

        [SerializeField] protected SkeletonGraphic skeletonGraphic;
        public SkeletonGraphic mSkeleGraphic {  get { return skeletonGraphic; }  }

        [SerializeField] protected EffectType effectType = EffectType.None;
        public EffectType mEffectType { get { return effectType; } }

        [SerializeField] protected string aAnimation;//要執行的動畫
        [SerializeField] protected string aFinishDefaultAnimation;//執行主動畫後的預設動畫 
        [SerializeField] protected int spineOrder = 0;
        [SerializeField] protected Vector2 size=Vector2.one;
        [SerializeField] protected bool loop;//是否持續
        [SerializeField] protected bool waitAnimationFinish;//是否持續
        [SerializeField] protected bool cgBreakPoint=false;//是否為cg斷點
        [SerializeField] protected float duration;//特效時間  在 FadeWaitTime是整體fade時間
        /// <summary>
        /// 判斷是否為Hcg
        /// </summary>
        public bool judgeHCG = false;

        private Stage stage=null;
        private SkeletonGraphic tarSkeleton = null;

        public string FinishDefaultAnimation { get { return aFinishDefaultAnimation; }set{ aFinishDefaultAnimation = value; } }

        private string path = "";

        public override void OnEnter()
        {
            StartCoroutine(StartExecuteCommand());


        }

        private IEnumerator StartExecuteCommand()
        {

            stage = ParentBlock.GetFlowchart().mStage;

            if (skeletonGraphic == null)
            {
                Debug.LogError("Not Have Choose Chara");
                yield break;
            }

           yield return JudgeSpriteOfSpeciftyPath(skeletonGraphic.name, p => { path = p; });

            judgeHCG = path.JudgeTargetPathIsHCG();


            tarSkeleton = stage.FindCGInstanceByName(skeletonGraphic.name);
            RectTransform rect = null;

            if (tarSkeleton == null)
            {
                var sp = Instantiate(skeletonGraphic);
                sp.transform.SetParent(stage.CharaParent, false);
                sp.name = skeletonGraphic.name;
                rect = sp.GetComponent<RectTransform>();
                rect.localScale = size;
                //rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
                tarSkeleton = sp;
                stage.SpineSkeletonGraphicCG.Add(tarSkeleton);

            }
            else
            {
                rect = tarSkeleton.GetComponent<RectTransform>();
            }

            if (spineOrder > 0)
            {
                Canvas canvas = null;
                skeletonGraphic.TryGetComponent<Canvas>(out canvas);
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = spineOrder;
                }
            }

            StartCoroutine(PlayCGAnimation());

        }

            private IEnumerator PlayCGAnimation()
        {
            switch (effectType)
            {
                case EffectType.None:
                    if (waitAnimationFinish)
                    {
                        yield return PlayAnimation();
                    }
                    else
                    {
                        StartCoroutine(PlayAnimation());
                    }
                    break;
                case EffectType.FadeIn:
                    yield return PlayEffect(tarSkeleton, effectType);
                    break;
                case EffectType.FadeOut:
                    yield return PlayEffect(tarSkeleton, effectType);
                    break;
                case EffectType.FadeWaitTime:
                    yield return PlayEffect(tarSkeleton, EffectType.FadeIn);
                    break;
            }

            if (effectType==EffectType.FadeWaitTime) {
                yield return PlayEffect(tarSkeleton, EffectType.FadeOut);
            }

            Continue();
        }



        private IEnumerator PlayEffect(SkeletonGraphic skele,EffectType type)
        {
            if (skele == null) {
                Debug.LogError("Not Have Chara");
                yield break;
            }
            var stage = ParentBlock.GetFlowchart().mStage;
            switch (type) 
            {
                case EffectType.FadeIn:
                    skele.color = new Color(1, 1, 1, 0);
                     skele.DOFade(1, duration);

                    if (judgeHCG) stage.AddHCGActivePath(path);

                    if (waitAnimationFinish)
                    {
                        yield return PlayAnimation();
                    }
                    else
                    {
                        StartCoroutine(PlayAnimation());
                    }
                    break;
                case EffectType.FadeOut:
                    yield return  skele.DOFade(0, duration)
                        .OnComplete(
                        () => {
                            if (judgeHCG)stage.RemoveHCGActivePath(path);
                        })
                        .WaitForCompletion();
                    stage.SpineSkeletonGraphicCG.Remove(skele);
                    Destroy(skele.gameObject);
                    break;
            }
        }

        /// <summary>
        /// 優先判斷指定加載的圖片路徑
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IEnumerator JudgeSpriteOfSpeciftyPath(string _name, Action<string> cb)
        {
                var res = Addressables.LoadResourceLocationsAsync(string.Format(FungusResourcesPath.HCGAddressPath, "Spine", _name, "prefab"), typeof(object));
                yield return res;

                if (res.Result.Count > 0)
                {
                    cb(res.Result[0].PrimaryKey);
                    yield break;
                }
            
            cb(null);

        }

        private IEnumerator PlayAnimation()
        {

            tarSkeleton.AnimationState.SetAnimation(0, aAnimation, loop);
             yield return new WaitForSeconds(tarSkeleton.GetAnimationTime(aAnimation));

            if (!string.IsNullOrEmpty( aFinishDefaultAnimation)) {
                tarSkeleton.AnimationState.SetAnimation(0, aFinishDefaultAnimation, true);
            }

        }
        public override bool CanNotSkipCommand()
        {
            return cgBreakPoint;
        }





    }




}
