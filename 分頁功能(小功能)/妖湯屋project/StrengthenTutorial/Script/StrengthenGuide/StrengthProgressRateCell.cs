using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using YKO.DataModel;
using YKO.Support;
using UniRx;
using DG.Tweening;
using YKO.Common.UI;
using YKO.Network;
using YKO.HeroDetail;
using YKO.Guild;
using YKO.Main;

namespace YKO.StrengthGuide
{


    /// <summary>
    /// 進度卡物件
    /// </summary>
    public class StrengthProgressRateCell : MonoBehaviour
    {

        #region 
        /// <summary>
        /// 標題文字
        /// </summary>
        [SerializeField] private Text titleText;
        /// <summary>
        /// 顯示圖檔
        /// </summary>
        [SerializeField] private Image iconImg;
        /// <summary>
        /// 前往目的地的按鈕(強化指南只向的目的地
        /// </summary>
        [SerializeField] private Button destinationBtn;
        /// <summary>
        /// 強化方式說明文案
        /// </summary>
        [SerializeField] private Text descriptText;
        /// <summary>
        /// 完成率
        /// </summary>
        [SerializeField] private Text completeRateText;

        /// <summary>
        /// 進度條
        /// </summary>
        [SerializeField] private Image percentBar;

        [SerializeField]private List<Sprite>IconSpriteList=new List<Sprite>();

        #endregion



        #region param


        /// <summary>
        /// 進度子物件條的方法
        /// </summary>
        private StrengthProgressRateCellData aData;




        #endregion
        public IEnumerator Init(StrengthProgressRateCellData _Data)
        {
            aData = _Data;

            if (aData.fixedData.sort == 4)//妖湯屋的英雄進階沒有顯示
            {
                gameObject.SetActive(false);
            }
            else
            {
                SetButtonSetting();
                yield return UpdateUI();
            }


        }

        private void SetButtonSetting()
        {
            destinationBtn.OnClickAsObservable().Subscribe(_ => ClickdestinationBtn());
        }

        private IEnumerator UpdateUI()
        {
            var curPercent = (float)aData.curProgressRate / (float)aData.allProgressRate;
            titleText.text =LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData.fixedData.name);
            descriptText.text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, aData.fixedData.desc);
            // percentBar.fillAmount = curPercent;//做出percent移動的效果
            // completeRateText.text =Math.Round (curPercent*100,2)+"%";


            //未放置圖片至resources
            /* yield return LoadAssetManager.LoadAsset<Sprite>(""+aData.fixedData.icon, res => {
                iconImg.sprite = res;
            });*/
            iconImg.sprite = IconSpriteList[((int)aData.fixedData.sort-1)];



            DOTween.To(() => 0,
                value =>
                {
                    percentBar.fillAmount = value;
                    completeRateText.text = Mathf.Floor(value * 100).ToString() + "%";
                },
                curPercent, (float)(0.2f * Mathf.Ceil(curPercent * 5))).SetEase(Ease.Linear);
            //加載圖片
            yield return null;
        }
        /// <summary>
        /// 點擊按鈕前往目的地
        /// </summary>
        /// <returns></returns>
        private void ClickdestinationBtn()
        {
            //  根據aData.fixedData.evt_type偵測目的地
            switch (aData.fixedData.sort)
            {
                case 1://英雄等级

                    aData.playExitAnim(
                        SceneConst.SceneName.HeroDetailScene,
                        new HeroDetailScene.SceneParameter()
                        {
                            heroData = aData.manager.SpeciftyAvatarCell.Data
                        }
                            );

                    break;
                case 2://装备穿戴
                    Debug.Log("前往装备穿戴頁面");
                    aData.playExitAnim(
                        SceneConst.SceneName.HeroDetailScene,
                        new HeroDetailScene.SceneParameter()
                        {
                            heroData = aData.manager.SpeciftyAvatarCell.Data
                        }
                            );

                    break;
                case 3://英雄星级
                    Debug.Log("前往英雄星级頁面");
                    aData.playExitAnim(
                        SceneConst.SceneName.HeroDetailScene,
                        new HeroDetailScene.SceneParameter()
                        {
                            heroData = aData.manager.SpeciftyAvatarCell.Data
                        }
                            );

                    break;
                case 4://英雄进阶
                    Debug.Log("前往英雄进阶頁面");
                    aData.playExitAnim(
                        SceneConst.SceneName.HeroDetailScene,
                        new HeroDetailScene.SceneParameter()
                        {
                            heroData = aData.manager.SpeciftyAvatarCell.Data
                        }
                            );

                    break;
                case 5://神器培养
                    Debug.Log("前往神器培养頁面");
                    aData.playExitAnim(
                        SceneConst.SceneName.StrangthenScene,
                        null
                            );


                    break;
                case 6://公会技能
                    Debug.Log("前往公会技能頁面");

                    if (MessageResponseData.Instance.UserData.lev >= GuildManager.Instance.GetSystemUnlockedMinLevel())
                    {
                        aData.playExitAnim(
                    SceneConst.SceneName.GuildScene,
                        new GuildScene.SceneParameter()
                    {
                        hasJoined = MessageResponseData.Instance.UserData.gname == "" ? false : true
                    }
                        );
                    }
                    else
                    {
                        MainManager.Instance.ShowCommonPopup("", $"{GuildManager.Instance.GetSystemUnlockedCondition()}{GuildManager.Instance.GetSystemUnlockedMinLevel()}");
                    }

                    break;
                case 7://符文培养
                    Debug.Log("前往符文培养頁面");
                    aData.playExitAnim(
                SceneConst.SceneName.HeroDetailScene,
                new HeroDetailScene.SceneParameter()
                {
                    heroData = aData.manager.SpeciftyAvatarCell.Data
                }
                    );

                    break;
            }


        }


        /// <summary>
        /// 進度卡資料
        /// </summary>
        public class StrengthProgressRateCellData
        {
            /// <summary>
            /// UI顯示的靜態資料
            /// </summary>
            public StrongerData.Stronger_Two fixedData;

            public StrengthGuidelines manager;

            /// <summary>
            /// 全部的進度
            /// </summary>
            public uint allProgressRate;
            /// <summary>
            /// 當前進度
            /// </summary>
            public uint curProgressRate;

            #region func
            /// <summary>
            /// 含主畫面Tween的結束動畫
            /// </summary>
            public Action<string, object> playExitAnim = null;

            #endregion


        }



    }
}