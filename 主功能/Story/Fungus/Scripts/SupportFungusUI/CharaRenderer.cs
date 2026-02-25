using Fungus;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Support.Expansion;

public class CharaRenderer : MonoBehaviour
{

    /// <summary>
    /// 反射渲染的圖片prefab
    /// </summary>
    [SerializeField]
    private Texture targetTexturePrefab = null;
    /// <summary>
    /// 漸層圖片
    /// </summary>
    [SerializeField]
    private GameObject fadeTexture = null;
    [HideInInspector]
    public bool isTween = false;

    /*
    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public void CreateRawImage(Transform parent)
    {
        fadeTexture = new GameObject("fadeRawImage", typeof(RectTransform), typeof(RawImage), typeof(CanvasRenderer));
        fadeTexture.transform.SetParent(parent);
        var raw = fadeTexture.GetComponent<RawImage>();
        raw.texture = targetTexturePrefab;
        raw.SetNativeSize();
        var rawRect = fadeTexture.GetComponent<RectTransform>();
        rawRect.localPosition = Vector3.zero;
    }*/

    /// <summary>
    /// 淡出
    /// </summary>
    /// <param name="target"></param>
    /// <param name="endTweenBackParent"></param>
    /// <param name="tweenTime"></param>
    /// <param name="origineLayerName"></param>
    /// <returns></returns>
    public IEnumerator FadeOut(SpineCharaAniOptions opt, Transform endTweenBackParent, string origineLayerName,Action cb=null)
    {
        yield return Fade(false, opt, endTweenBackParent, origineLayerName,cb);

    }
    /// <summary>
    /// 淡入
    /// </summary>
    /// <param name="target"></param>
    /// <param name="endTweenBackParent"></param>
    /// <param name="tweenTime"></param>
    /// <param name="origineLayerName"></param>
    /// <returns></returns>
    public IEnumerator FadeIn(SpineCharaAniOptions opt, Transform endTweenBackParent, string origineLayerName)
    {
       yield return Fade(true, opt, endTweenBackParent,  origineLayerName);

    }
    /// <summary>
    /// 淡入淡出特效
    /// </summary>
    /// <param name="_fadeIn"></param>
    /// <param name="target"></param>
    /// <param name="endTweenBackParent"></param>
    /// <param name="tweenTime"></param>
    /// <param name="origineLayerName"></param>
    /// <returns></returns>
    private IEnumerator Fade(bool _fadeIn, SpineCharaAniOptions opt, Transform endTweenBackParent,string origineLayerName,Action cb=null)
    {
        float origineNum = 0;
        float targetNum = 0;

        var target = opt.CharaObj.transform;
        var tweenTime = opt.aTween.aFadeAniDuration;



        if (_fadeIn)
        {
            targetNum = 1;
        }
        else
        {
            origineNum = 1;
        }
        //opt._spineOrder

        target.gameObject.SetLayer("StoryFade");

        if (isTween)
        {
            Debug.Log("還在tween");



            fadeTexture.SetCanvasToOrder(opt._spineOrder, opt.orderName,this);
            target.SetParent(transform);

            yield return new WaitUntil(() => !isTween);
            
            if (target != null)
            {
                target.SetParent(endTweenBackParent);
                target.gameObject.SetLayer(origineLayerName);
            }
            if (cb!=null)
            {
                cb();
            }
        }
        else
        {
            Debug.Log("沒在tween");
            isTween = true;
            fadeTexture.SetCanvasToOrder(opt._spineOrder, opt.orderName,this);

            target.SetParent(transform);
            fadeTexture.SetCanvasGroup(origineNum);
            fadeTexture.SetActive(true);
            yield return fadeTexture.eSetCanvasGroup(targetNum,
                () => {
                    isTween = false;
                    fadeTexture.SetActive(false);
                    target.SetParent(endTweenBackParent);
                    target.gameObject.SetLayer(origineLayerName);
                    if (cb != null)
                    {
                        cb();
                    }
                },
                0, tweenTime);
        }


    }



    //生成rawimage,抓



    /*
    /// <summary>
    /// 顯示的圖片
    /// </summary>
    private RawImage mRawImg = null;
    /// <summary>
    /// 顯示的圖片(public
    /// </summary>
    public RawImage MyRawImg { get { return mRawImg; } }
    */

    /// <summary>
    /// 實際操作的角色
    /// </summary>
    // private GameObject charaObj = null;
    /// <summary>
    /// 實際操作的角色(public
    /// </summary>
    /*  public GameObject  CharaObj{ get { return charaObj; } }

      public void Init(SpineCharaAniOptions _opt,Stage stage)
      {
          opt = _opt;

          charaObj = Instantiate(opt._SpineCharaPrefab.gameObject);
          charaObj.name = opt._charaName;
          charaObj.transform.SetParent(transform);

          charaObj.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

          opt.CharaObj = charaObj;
          RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

          GameObject rawObj = new GameObject(
              opt._SpineCharaPrefab.name+"_RawImg",
              typeof(RectTransform),
              typeof(RawImage),
              typeof(CanvasRenderer)
           );

          rawObj.layer = LayerMask.NameToLayer("StoryContent");
          //mRawImg = rawObj.GetComponent<RawImage>();
          //mRawImg.raycastTarget = false;
        //  mRawImg.texture = renderTexture;
          mCamera.targetTexture = renderTexture;

          //opt._rawImage = mRawImg;

          rawObj.transform.SetParent( stage.CharaParent,false);
          var rectRaw = rawObj.GetComponent<RectTransform>();
          rectRaw.sizeDelta = new Vector2(Screen.width, Screen.height);
          rectRaw.anchorMin = Vector2.zero;
          rectRaw.anchorMax = Vector2.one;
          rectRaw.offsetMin = Vector2.zero;
          rectRaw.offsetMax = Vector2.zero;

      }

      public void Init(Character chara, Transform parent)
      {
          charaObj = Instantiate(chara.aSkeletonGraphic.gameObject);
          charaObj.name = chara.name;
          charaObj.transform.SetParent(transform,false);
          charaObj.transform.localScale = Vector3.one;
          charaObj.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

          RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

          GameObject rawObj = new GameObject(
              chara.name + "_RawImg",
              typeof(RectTransform),
              typeof(RawImage),
              typeof(CanvasRenderer)
           );

          //mRawImg = rawObj.GetComponent<RawImage>();
        //  mRawImg.texture = renderTexture;
          mCamera.targetTexture = renderTexture;
          rawObj.transform.SetParent(parent, false);
          var rectRaw = rawObj.GetComponent<RectTransform>();
          rectRaw.sizeDelta = new Vector2(Screen.width, Screen.height);
          rectRaw.anchorMin = Vector2.zero;
          rectRaw.anchorMax = Vector2.one;


      }*/




}
