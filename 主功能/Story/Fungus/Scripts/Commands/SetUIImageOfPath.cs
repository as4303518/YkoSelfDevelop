
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using YKO.Support;
using YKO.Support.Expansion;

namespace Fungus
{
    /// <summary>
    /// Changes the Image property on a UI element.
    /// </summary>
    [CommandInfo("UI",
                 "Set UI Image (Path)",
                 "Changes the Image property of a list of UI Images.")]
    [AddComponentMenu("")]
    public class SetUIImageOfPath : Command
    {
        [Tooltip("List of UI Images to set the source image property on")]
        [SerializeField] public List<Image> images = new List<Image>();

        [SerializeField] public string path = "";
        [SerializeField] protected bool cgBreakPoint=false;//是否為cg斷點
        protected Sprite sprite=null;

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
        /// <summary>
        /// 執行command
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartExecuteCommand()
        {
            // FungusResourcesPath.AddressPath
            judgeHCG = path.JudgeTargetPathIsHCG();
            yield return  LoadAssetManager.LoadAsset<Sprite>(
                FungusResourcesPath.AddressPath+"Image/"+path,
                res => {
                    if (res!=null) {
                        sprite = res;
                    }
                    else
                    {
                        sprite = null;
                    }
                }
                );

            var stage = ParentBlock.GetFlowchart().mStage;
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
                        StartCoroutine(image.gameObject.eSetCanvasGroup(1, duration:effectDuration));
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
        /// 透過圖片名字獲取準確的圖片路徑
        /// </summary>
        /// <returns></returns>
        /*public string CatchAssetPathOfName(string tarName)
        {
            ///實際要找的名稱(避免單圖分割誤判
            string fullName = "";
            //是否為單圖分割
            bool isMultSprite = false;
            if (tarName.Contains("_")) 
            {
                fullName = tarName.Split("_")[0];
                isMultSprite = true;
            }
            else
            {
                fullName = tarName;
            }
            //尋找所有跟fullName 同樣的guid物件          後方string為篩選路徑   
          //  var guids = AssetDatabase.FindAssets(fullName, new[] { FungusResourcesPath.AddressPath + "Image" });



              //  var path = AssetDatabase.GUIDToAssetPath(guid);
                judgeHCG = JudgeTargetPathIsHCG(path);
                
                var pathTempSplit = path.Split(".")[0].Split("/"); // ex: Assets/Application/Story/Fungus/Resources/Image/CG/sdcg0011.png
                if (pathTempSplit[(pathTempSplit.Length - 1)].Equals(fullName))
                {
                    var nameSplit = tarName.Split("_");
                    if (isMultSprite) 
                    {
                        //第一個是本名 避免有兩個底線,所以必須把後面分隔的名稱逐個加 ex:abc_[abc_1]

                        for (int i=0;i<nameSplit.Length;i++)
                        {
                            var split = nameSplit[i];
                            if (i>0)
                            {
                                if (i>1) {//第一個底線是區分多圖加載的底線,非名稱
                                    path += "_";
                                }
                                path += split;
                            }
                        }
                    }
                    Debug.Log("加載的路徑=>" + path );
                    return path;
                }
            
        }*/
        /*/// <summary>
        /// 判斷目標路徑是否為嘿嘿嘿的內容
        /// </summary>
        /// <returns></returns>
        public bool JudgeTargetPathIsHCG(string path)
        {
            var pathSplit= path.Split(".")[0].Split("/");
            foreach (var split in pathSplit)
            {
                if (split.Equals(FungusResourcesPath.HCGFloderName)) 
                {
                    return true;
                }
            }
            return false;
        }*/
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
                return "Error: No Image selected";
            }

            return summary + " = " + path;
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

        /// <summary>
        /// 指定可停止跳過功能的路徑
        /// </summary>
        private List<string> canNotSkipPath = new List<string>()
        {
            "CG",
            "HCG",
        };
        /// <summary>
        /// 判斷圖片是否可停止跳過功能
        /// </summary>
        /// <returns></returns>
        public override bool CanNotSkipCommand()
        {
            /*bool isCG = false;

            foreach (string limitPath in canNotSkipPath)
            {
                if (path.StartsWith(limitPath))
                {
                    isCG = true;
                }
            }*/

            return cgBreakPoint;
        }


        #endregion
    }
}

