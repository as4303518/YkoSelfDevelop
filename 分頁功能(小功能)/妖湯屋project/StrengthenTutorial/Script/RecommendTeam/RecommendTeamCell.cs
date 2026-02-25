using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.Font;
using YKO.Common.UI;
using YKO.DataModel;
using YKO.Support.Expansion;

namespace YKO.StrengthGuide
{

    /// <summary>
    /// 團隊卡
    /// </summary>
    public class RecommendTeamCell : MonoBehaviour
    {
        public class LocalizeKey
        {
            public static readonly string SearchAnalytics = "查看分析";

            public static readonly string CloseExpand = "收起";
        }




        // Start is called before the first frame update
        /// <summary>
        /// 團隊名稱
        /// </summary>
        [SerializeField] private Text teamNameText;
        /// <summary>
        /// 頭像群組母物件
        /// </summary>
        [SerializeField]
        private Transform avatarGroupParent;

        /// <summary>
        /// 擴展按鈕
        /// </summary>
        [SerializeField]
        private Button expandBtn;


        /// <summary>
        /// 擴展圖標
        /// </summary>
        [SerializeField]
        private Image expandIcon;
        /// <summary>
        /// 擴張提示文字
        /// </summary>
        [SerializeField] private Text expandTipText;
        /// <summary>
        /// 分隔線
        /// </summary>
        [SerializeField] private GameObject separateLine;
        /// <summary>
        /// 擴展描述文字
        /// </summary>
        [SerializeField]
        private Text expandDescriptText;

        #region variable
        /// <summary>
        /// 頭像陣列
        /// </summary>
        private List<GameObject> avatarObjList = new List<GameObject>();

        private  StrongerData.Recommand aData;

        /// <summary>
        /// 當前擴展狀態
        /// </summary>
        private bool isExpand = false;

        #endregion

        #region prefab& sprite
        /// <summary>
        /// 角色頭像
        /// </summary>
        [SerializeField] private GameObject heroIconPrefab = null;

        /// <summary>
        /// 顯示點擊擴張的箭頭
        /// </summary>
        public Sprite expandArrowSprite;
        /// <summary>
        /// 顯示點擊收起的箭頭
        /// </summary>
        public Sprite unExpandArrowSprite;


        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public void Init(StrongerData.Recommand _data,Action _finishLoading)
        {

            aData = _data;
           UpdateUI();
            SetButtonSetting();
            _finishLoading();

        }
        /// <summary>
        /// 設定按鈕設定
        /// </summary>
        private void SetButtonSetting()
        {
            expandBtn.OnClickAsObservable().Subscribe(_ => ClickExpandBtn());
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {

            expandDescriptText.text =ConvertForShinyOfLight.ConvertStringToUnityEditorDisplay( aData.desc);
            teamNameText.text = aData.name;

            var list = JsonConvert.DeserializeObject<int[][]>(aData.hero_list.ToString());

            foreach (var chara_bid in list[0])
            {
                Debug.Log("數字=>"+chara_bid);
                var sp = Instantiate(heroIconPrefab);
                sp.transform.SetParent(avatarGroupParent, false);


                var hero = sp.GetComponent<HeroIcon>();
                hero.InitHero((uint)chara_bid);
                //  hero.ButtonClick=

                /*
                sp.GetComponent<HeroIcon>().Init(
                    new AvatarCell.AvatarCellFunc() { 
                    //ClickAvatar
                    },
                    new AvatarCell.AvatarCellData() 
                    { 
                    num=chara_bid,
                    partner_bid=chara_bid.ToString(),
                    }
                    );
                */

                //###to do: 透過chara_bid獲取角色資訊,生成頭像並在點擊頭像後彈出角色資訊
                avatarObjList.Add(sp);
            }
        }
        /// <summary>
        /// 紀錄擴張或收縮動畫的執行時間
        /// </summary>
        private float expandTweenTime = 0;

        /// <summary>
        /// 擴展內容
        /// </summary>
        private void ClickExpandBtn()
        {
            isExpand = !isExpand;
            SetExpandSetting(isExpand);

        }

        /// <summary>
        /// 點擊擴展按鈕
        /// </summary>
        /// <returns></returns>
        private void SetExpandSetting(bool expand)
        {
            var rect = GetComponent<RectTransform>();
            rect.DOKill();
            expandTipText.DOKill();
            if (expandTweenTime<=0)
            {
                expandTweenTime = 0.2f;

            }
            else
            {
                expandTweenTime = 0.2f - expandTweenTime;
            }

            if (expand)
            {
                expandIcon.sprite = unExpandArrowSprite;
                expandTipText.text = LocalizeKey.CloseExpand;
                separateLine.SetActive(true);
                expandDescriptText.gameObject.SetActive(true);
                StartCoroutine( separateLine.eSetCanvasGroup(1, null, 0, expandTweenTime));
                StartCoroutine(expandDescriptText.gameObject.eSetCanvasGroup(1, null, 0, expandTweenTime));

                expandTipText.DOColor(new Color(0.5f, 0.5f, 0.5f), expandTweenTime);

                var descTextRect = expandDescriptText.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(descTextRect);
                rect.DOSizeDelta(new Vector2(1000, 430 + descTextRect.sizeDelta.y), expandTweenTime)
                .SetEase(Ease.Linear)
                .OnUpdate(() => { expandTweenTime -= Time.deltaTime; });

            }
            else
            {
                expandIcon.sprite = expandArrowSprite;
                expandTipText.text = LocalizeKey.SearchAnalytics;

                StartCoroutine(separateLine.eSetCanvasGroup(
                    0,
                    () => { separateLine.SetActive(false); }, 
                    0, 
                    expandTweenTime
                    ));

                StartCoroutine(expandDescriptText.gameObject.eSetCanvasGroup(
                    0, 
                    () => { expandDescriptText.gameObject.SetActive(false); },
                    0,
                    expandTweenTime
                    ));
                expandTipText.DOColor(new Color(0.85f, 0.17f, 0.21f), expandTweenTime);
                rect.DOSizeDelta(new Vector2(1000, 300), expandTweenTime)
                .SetEase(Ease.Linear)
                .OnUpdate(() => { expandTweenTime -= Time.deltaTime; });

            }

        }

        


    }
}