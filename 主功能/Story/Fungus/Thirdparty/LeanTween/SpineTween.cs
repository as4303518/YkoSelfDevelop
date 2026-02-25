using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine.Unity;
using UnityEngine.UI;



public static class SpineTween
{
    public static IEnumerator UIFadeIn(GameObject image,float duration)
    {
        CanvasGroup canvas = null;
        if (image.TryGetComponent<CanvasGroup>(out canvas)) {


        }
        else
        {
            canvas=image.gameObject.AddComponent<CanvasGroup>();
        }
        canvas.alpha = 0;
        LeanTween.alphaCanvas(canvas, 1, duration);
        yield return new WaitForSeconds(duration);
            
    }

    public static IEnumerator UIFadeOut(GameObject image, float duration)
    {
        CanvasGroup canvas = null;
        if (image.TryGetComponent<CanvasGroup>(out canvas))
        {


        }
        else
        {
            canvas = image.gameObject.AddComponent<CanvasGroup>();
        }
        canvas.alpha = 1;
        LeanTween.alphaCanvas(canvas, 0, duration);
        yield return new WaitForSeconds(duration);

    }

    public static IEnumerator SpineSkeletonGraphicFadeIn(GameObject obj, float dur)
    {
        SkeletonGraphic data = null;
        if (obj.TryGetComponent<SkeletonGraphic>(out data))
        {

            Color origineC = data.color;
            float addColorA = 0;
            float offValue = data.color.a / dur * Time.deltaTime;

            data.color = new Color(origineC.r, origineC.g, origineC.b, 0);

            while (addColorA < origineC.a)
            {

                if (data != null)
                {
                    addColorA = addColorA + offValue > 1 ? 1 : addColorA + offValue;

                    data.color = new Color(origineC.r, origineC.g, origineC.b, addColorA);

                    yield return null;
                }
                else
                {
                    yield break;
                }

            }
            data.color = origineC;


        }
        else
        {
            Debug.Log("Get Script On Fail");
            yield break;
        }
    }

    public static IEnumerator SpineSkeletonGraphicFadeOut(GameObject obj, float dur)
    {
        SkeletonGraphic data = null;

        if (obj.TryGetComponent<SkeletonGraphic>(out data))
        {

            Color origineC = data.color;
            float addColorA = origineC.a;
            float offValue = data.color.a / dur * Time.fixedDeltaTime;

            while (addColorA > 0)
            {

                if (data != null)
                {
                    addColorA = addColorA - offValue < 0 ? 0 : addColorA - offValue;

                    data.color = new Color(origineC.r, origineC.g, origineC.b, addColorA);

                    yield return null;
                }
                else
                {
                    yield break;
                }
            }
            data.color = new Color(origineC.r, origineC.g, origineC.b, 0);


        }
        else
        {
            Debug.Log("Get Script On Fail");
            yield break;
        }
    }
    public static IEnumerator DoMoveTween(GameObject obj,Vector3 toPos,LeanTweenType ease=LeanTweenType.linear,float duration=0)
    {
        LeanTween.moveLocal(obj, toPos, duration)
            .setEase(ease);
        yield return new WaitForSeconds(duration);

    }

    public static IEnumerator SpineScale(GameObject obj, Vector3 toScale, LeanTweenType ease = LeanTweenType.linear, float duration = 0)
    {
        Debug.Log("設定的size=>"+toScale);
        if (duration<=0) 
        {
             obj.transform.localScale = toScale;
            yield break;
        }
        LeanTween.scale(obj, toScale, duration)
            .setEase(ease);
        yield return new WaitForSeconds(duration);
    }


}







