using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using YKO.Support;

public class ClickNextTipPopup : MonoBehaviour
{
    public enum EffectType
    {
        Flashing,
        Transfer
    }

    [SerializeField]private Image tipImage;
    [SerializeField] private Text tipText;

    public void Init(EffectType effectType,Image image=null) 
    {
        if (image!=null) {
        tipImage = image;
        }
        tipText.gameObject.SetActive(false);
        tipImage.gameObject.SetActive(true);

        AccrodObjectSetEffect(tipImage.gameObject, effectType);
    }

    public void Init(EffectType effectType,string textStr)
    {
        tipText.gameObject.SetActive(true);
        tipImage.gameObject.SetActive(false);
        tipText.text = textStr;
        AccrodObjectSetEffect( tipText.gameObject, effectType);
    }


    public void AccrodObjectSetEffect(GameObject obj,EffectType effectType,float dur=1)
    {
        switch (effectType)
        {
            case EffectType.Flashing:
                TweenManager.UIFlash(obj,dur);
                break;
            case EffectType.Transfer:
                TweenManager.UIMoveUpAndDown(obj, dur);
                break;
        }

    }
    
    public void StartAni()
    {


    }

    public void EndAni()
    {
        tipImage.GetComponent<RectTransform>().DOKill();
        tipText.DOKill();
    }

    public void OnDestroy()
    {
        EndAni();
    }


}
