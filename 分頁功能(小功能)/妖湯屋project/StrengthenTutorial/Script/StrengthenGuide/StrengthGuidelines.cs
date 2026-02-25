using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YKO.Casino;
using YKO.Common.Data;
using YKO.Common.Sound;
using YKO.Common.UI;
using YKO.DataModel;
using YKO.Network;
using YKO.Support;
using YKO.Support.Expansion;
using static UnityEngine.Rendering.DebugUI;

namespace YKO.StrengthGuide
{

    /// <summary>
    /// 強化指引頁面class
    /// </summary>
    public class StrengthGuidelines : MonoBehaviour
    {

        private class LocalizeKey
        {
            public static readonly string StrengthManualProgress = "實力強化進度";


        }

        /// <summary>
        /// 指向提示圖片
        /// </summary>
        [SerializeField] private GameObject directionTipImg;
        /// <summary>
        /// 出戰陣容陣列
        /// </summary>
        [SerializeField] private List<HeroIcon> AvatarList;
        /// <summary>
        /// 當前選定的角色頭像
        /// </summary>
        private HeroIcon speciftyAvatarCell = null;

        public HeroIcon SpeciftyAvatarCell { get { return speciftyAvatarCell; } }

        /// <summary>
        /// 進度條說明
        /// </summary>
        [SerializeField] private Text percentRateDescript;

        /// <summary>
        /// 總共強化完成趴數
        /// </summary>
        [SerializeField] private Image percentRateImg;

        /// <summary>
        /// 頭像卡群母物件
        /// </summary>
        [SerializeField] private Transform heroIconListParent;
        /// <summary>
        /// 進度條卡群母物件
        /// </summary>
        [SerializeField] private Transform progressRateCellParent;
        /// <summary>
        /// 進度卡群頁面(分割顯示用
        /// </summary>
        [SerializeField] private GameObject ProgressInfoPage;
        /// <summary>
        /// 當前選擇的角色名稱
        /// </summary>
        [SerializeField] private Text charaNameText;

        [SerializeField] private Text strengthProgressText;


        #region variable
        /// <summary>
        /// 進度條卡陣列
        /// </summary>
        private List<StrengthProgressRateCell> progressRateCellList = new List<StrengthProgressRateCell>();

        /// <summary>
        /// 第一次開啟?
        /// </summary>
        private bool firstOpen = false;
        /// <summary>
        /// 富頁面的資料加載(因為沒有綁定完加必須等待加載完才能點選UI,所以必須記錄coroutine 以隨時暫停正在執行的序列
        /// </summary>
        private Coroutine updateCharaStrengthData = null;

        #endregion


        #region  prefab or Data
        /// <summary>
        /// 進度條卡prefab
        /// </summary>
        [SerializeField] private GameObject progressRateCellPrefab;

        /// <summary>
        /// 進度條卡prefab
        /// </summary>
        [SerializeField] private GameObject heroIconPrefab;
        /// <summary>
        /// 主管理員
        /// </summary>
         private StrengthenTutorialManager manager;

        #endregion


        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init(StrengthenTutorialManager  _manager)
        {
            manager = _manager;
            if (!firstOpen)
            {
                yield return UpdateUI();
            }

        }


        /// <summary>
        /// 設置UI
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateUI()
        {
            if (speciftyAvatarCell == null)
            {
                var charaList = BattleManager.Instance.TeamList[YKO.CONST.PartnerConst.Fun_Form.Drama].PosInfos;
                int num = 0;
                foreach (var chara in charaList)
                {
                    if (chara.id>0) {
                        yield return manager.NetManager.HandleProto11070(chara.id);
                        
                        var sp = Instantiate(heroIconPrefab);

                        sp.transform.SetParent(heroIconListParent, false);

                        HeroIcon cell = sp.GetComponent<HeroIcon>();
                        var heroData = BattleManager.Instance.HeroList[chara.id];
                        var tempNum = num;

                     //   Debug.Log("角色id=>"+chara.id);
                     //   Debug.Log("角色的bid=>" + BattleManager.Instance.GetBID(chara.id));
                        
                        cell.Init(heroData);
                        cell.ButtonClick = () => {
                         //   charaNameText.text = heroData.name;
                            ClickAvatarCell(tempNum); 
                        
                        };



                        AvatarList.Add(cell);
                        num++;
                    }
                }
                //等待回傳的11070符合發出去的數量
                // yield return new WaitUntil(() => func.manager.NetManager.Proto11070List.Count >= charaList.Length);

                if (charaList.Length>0) //有角色
                    ClickAvatarCell(0); //更新頭像


                firstOpen = true;


            }
        }

        /// <summary>
        /// 更新下方的進度卡頁面
        /// </summary>
        /// <returns></returns>
        private void UpdateInfoPage(int num)
        {
            
            progressRateCellParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var tarProto = manager.NetManager.Proto11070List[num];
            strengthProgressText.text = LocalizeKey.StrengthManualProgress;
           // percentRateDescript.text = "<b>[角色id=>" +num + "]</b>實力強化進度";

            if (progressRateCellList.Count > 0)
            {
                foreach (var cell in progressRateCellList)
                {
                    Destroy(cell.gameObject);
                }
                progressRateCellList.Clear();
            }
            float allPercent = 0;
            foreach (var data in manager.FixedData.data_stronger_two.Values) 
            {

                var sp = Instantiate(progressRateCellPrefab);
                sp.transform.SetParent(progressRateCellParent, false);

                // foreach func.manager.NetManager.Proto11070List

                var _allProgressRate = tarProto.stronger_partner_score.ToList().Find(v => v.id_2 == data.sort).val;
                var _curProgressRate = tarProto.partner_score.ToList().Find(v => v.id_2 == data.sort).val;

               StartCoroutine( sp.GetComponent<StrengthProgressRateCell>().Init(new StrengthProgressRateCell.StrengthProgressRateCellData()
                { 
                   manager=this,
                    fixedData=data,
                    allProgressRate= tarProto.stronger_partner_score.ToList().Find(v => v.id_2 == data.sort).val,
                    curProgressRate= tarProto.partner_score.ToList().Find(v => v.id_2 == data.sort).val,
                    playExitAnim=manager.GoToSpeciftyScene

                }));
                progressRateCellList.Add(sp.GetComponent<StrengthProgressRateCell>());
                allPercent += (float)_curProgressRate / (float)_allProgressRate;

            }


            allPercent =MathF.Round( allPercent / manager.FixedData.data_stronger_two.Count,2);

            //percentRateDescript.text=integer.ToString()+"%";
           // percentRateImg.fillAmount = allPercent;

            DOTween.To(() => 0, value => {
                percentRateDescript.text = MathF.Ceiling( value *(float)100)  .ToString()+"%";
                percentRateImg.fillAmount = value;
            }, allPercent, 0.2f * Mathf.Ceil(allPercent * 5));

            //根據加強數量去生成progressRateCellPrefab
            //加入到progressRateCellList
          //  yield return ProgressInfoPage.eSetCanvasGroup(1,null,0,0.5f);
            updateCharaStrengthData = null;

        }

        /// <summary>
        /// 點擊頭像
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private void ClickAvatarCell(int num) {
            Debug.Log("點擊頭像號碼=>"+num);
            if (updateCharaStrengthData!=null) {
                StopCoroutine(updateCharaStrengthData);
                updateCharaStrengthData = null;
            }
                speciftyAvatarCell = AvatarList[num];
                charaNameText.text = speciftyAvatarCell.Data.name;
            var avaRect = speciftyAvatarCell.GetComponent<RectTransform>();
                 LayoutRebuilder.ForceRebuildLayoutImmediate(heroIconListParent.GetComponent<RectTransform>());
                var rect = directionTipImg.gameObject.GetComponent<RectTransform>();
                rect.position = new Vector3(avaRect.position.x, rect.position.y, rect.position.z);
                UpdateInfoPage(num);

            
        }



    }

}


