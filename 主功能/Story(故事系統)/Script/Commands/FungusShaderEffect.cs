using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Fungus
{
    [CommandInfo("Effect",
             "ShaderEffect",
             "Display Shader Effect")]
    [AddComponentMenu("")]
    public class FungusShaderEffect : Command
    {

 
        public ShaderEffectType effectType;
        public Ease ease;
        public float fadeDuration;

        [Range(0,0.1f)]
        public float strength;
        public Color targetColor=Color.white;
        public float scanningLineCount=4;
        public float scanningLineSpeed=2;
        [Range(0, 1f)]
        public float scanningLineStrength=0.1f;


        public bool canSetStartValue=false;
        public bool canSetStartColor = false;

        public float startValue = 0;
        public Color startColor= Color.white;
        public bool closeObj=false;

        

        // Start is called before the first frame update
        public override void OnEnter()
        {
            StartCoroutine(ExecuteEffect());
        }

        private IEnumerator ExecuteEffect()
        {
            Stage stage = ParentBlock.GetFlowchart().mStage;

            string materialName = effectType.ToString();
            Material newMal = null;
            if (stage.GetEffectImage.material.name != materialName)
            {

                var resqu = Resources.LoadAsync<Material>(FungusResourcesPath.ShaderEffectPath + materialName + "/" + materialName + "Material");
                yield return new WaitUntil(() => resqu.isDone);
                newMal = Instantiate((resqu.asset as Material));
                newMal.name = materialName;
                stage.GetEffectImage.material = newMal;
            }
            else
            {
                newMal = stage.GetEffectImage.material;
            }
            stage.GetEffectImage.gameObject.SetActive(true);//wait for the  material to be Configuration completed

            switch (effectType)
            {
                case ShaderEffectType.GaussBlur:

                    yield return SetMaterialPropertyValue(newMal, MaterialValueName.Strength, strength);

                    break;
                case ShaderEffectType.AdjustColor:

                    yield return SetMaterialPropertyColor(newMal, MaterialValueName.Color, targetColor);

                    break;
                case ShaderEffectType.BlurAndAdjustColor:

                    StartCoroutine(SetMaterialPropertyValue(newMal, MaterialValueName.Strength, strength));
                    yield return SetMaterialPropertyColor(newMal, MaterialValueName.Color, targetColor);

                    break;
                case ShaderEffectType.Retro:

                    newMal.SetFloat(MaterialValueName.ScanningLineSpeed, scanningLineSpeed);
                    newMal.SetFloat(MaterialValueName.ScanningLineCount, scanningLineCount);
                    newMal.SetFloat(MaterialValueName.ScanningLineStrength, scanningLineStrength);

                    yield return SetMaterialPropertyColor(newMal, MaterialValueName.Color, targetColor);

                    break;
                case ShaderEffectType.RetroAndBlur:

                    newMal.SetFloat(MaterialValueName.ScanningLineSpeed, scanningLineSpeed);
                    newMal.SetFloat(MaterialValueName.ScanningLineCount, scanningLineCount);
                    newMal.SetFloat(MaterialValueName.ScanningLineStrength, scanningLineStrength);
                    StartCoroutine(SetMaterialPropertyValue(newMal, MaterialValueName.Strength, strength));
                    yield return SetMaterialPropertyColor(newMal, MaterialValueName.Color, targetColor);

                    break;
            }

            if (closeObj) {
                stage.GetEffectImage.gameObject.SetActive(false);
            }

            Continue();
        }



        private IEnumerator SetMaterialPropertyValue(Material mal,string propertyName,float value)
        {

            if (canSetStartValue)
            {
                mal.SetFloat(propertyName, startValue);
            }

            yield return mal.DOFloat(value, propertyName, fadeDuration).SetEase(ease).WaitForCompletion();

        }

        private IEnumerator SetMaterialPropertyColor(Material mal, string propertyName, Color color)
        {

            if (canSetStartColor)
            {
                mal.SetColor(propertyName, startColor);
            }

            yield return mal.DOColor(color, propertyName, fadeDuration).SetEase(ease).WaitForCompletion();

        }

        public enum ShaderEffectType
        {
            GaussBlur,//高斯模糊
            AdjustColor,//調整全部螢幕顏色
            BlurAndAdjustColor,//高斯模糊加調整顏色
            Retro,//復古效果,調整顏色
            RetroAndBlur//復古效果,調整顏色,高斯模糊


        }

        public static class MaterialValueName
        {
            public static readonly string Strength="_Strength";
            public static readonly string Offset="_Offset";
            public static readonly string Color="_Color";
            public static readonly string ScanningLineCount = "_ScanningLineCount";
            public static readonly string ScanningLineSpeed = "_ScanningLineSpeed";
            public static readonly string ScanningLineStrength = "_ScanningLineStrength";

        }

    }



}
