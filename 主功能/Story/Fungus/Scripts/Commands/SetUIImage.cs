// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using YKO.Support.Expansion;
using YKO.Support;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
//using UnityEditor;

namespace Fungus
{
    public enum EffectType
    {
        None,
        FadeIn,
        FadeOut

    }

    /// <summary>
    /// Changes the Image property on a UI element.
    /// </summary>
    [CommandInfo("UI",
                 "Set UI Image",
                 "Changes the Image property of a list of UI Images.")]
    [AddComponentMenu("")]
    public class SetUIImage : Command
    {
        [Tooltip("List of UI Images to set the source image property on")]
        public List<Image> images = new List<Image>();

        [Tooltip("The sprite set on the source image property")]
        public Sprite sprite;

        public EffectType effectType;

        public bool waitUntilFinished;

        /// <summary>
        /// 判斷是否為Hcg
        /// </summary>
        private bool judgeHCG = false;

        //  [SerializeField] protected bool useMultLanguage;

        public float effectDuration;
        #region Public members

        public override void OnEnter()
        {

            StartCoroutine(StartExecuteCommand());
        }

        private IEnumerator StartExecuteCommand()
        {
            //多語言圖片 暫時註解
            var stage = ParentBlock.GetFlowchart().mStage;
            // var path = AssetDatabase.GetAssetPath(sprite);
            var path = "";
           yield return  JudgeSpriteOfSpeciftyPath(sprite.name, p => { path = p; }) ;
                
            judgeHCG = path.JudgeTargetPathIsHCG();
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                image.sprite = sprite;

                switch (effectType)
                {
                    case EffectType.None:
                        if (judgeHCG) stage.AddHCGActivePath(path);
                        break;
                    case EffectType.FadeIn:
                        if (judgeHCG) stage.AddHCGActivePath(path);
                        image.gameObject.SetCanvasGroup(0);
                        StartCoroutine(image.gameObject.eSetCanvasGroup(1, duration: effectDuration));
                        break;
                    case EffectType.FadeOut:
                        image.gameObject.SetCanvasGroup(1);
                        StartCoroutine(
                            image.gameObject.eSetCanvasGroup(0,
                            () => {
                                if (judgeHCG) stage.RemoveHCGActivePath(path);
                            },
                            duration: effectDuration));
                        break;

                }
            }

            if (waitUntilFinished)
            {
                yield return new WaitForSeconds(effectDuration);
            }


            Continue();


        }
        /// <summary>
        /// 優先判斷指定加載的圖片路徑
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IEnumerator JudgeSpriteOfSpeciftyPath(string _name,Action< string> cb)
        {

            foreach (var imgFormat in FungusResourcesPath.ImageFormat) 
            {

                var res = Addressables.LoadResourceLocationsAsync(string.Format(FungusResourcesPath.HCGAddressPath,"Image",_name,imgFormat), typeof(object));
                yield return res;
                
                if (res.Result.Count>0) {
                    cb(res.Result[0].PrimaryKey);
                    yield break;
                }
            }
            cb(null);

        }



        public override string GetSummary()
        {
            string summary = "";
            for (int i = 0; i < images.Count; i++)
            {
                var targetImage = images[i];
                if (targetImage == null)
                {
                    continue;
                }
                if (summary.Length > 0)
                {
                    summary += ", ";
                }
                summary += targetImage.name;
            }

            if (summary.Length == 0)
            {
                return "Error: No sprite selected";
            }

            return summary + " = " + sprite;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "images")
            {
                return true;
            }

            return false;
        }
        public override void AdjustCommandExecuteSpeedAcceleration(bool _switch)
        {
            if (_switch)
            {

                tempDuration = effectDuration;
                effectDuration = 0;

            }
            else
            {
                effectDuration = tempDuration;

            }

        }
        public override void OnCommandAdded(Block parentBlock)
        {
            // Add a default empty entry
            images.Add(null);
        }

        #endregion
    }
}
