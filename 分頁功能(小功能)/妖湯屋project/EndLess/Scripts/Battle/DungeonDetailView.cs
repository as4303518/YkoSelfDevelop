using DG.Tweening;
using Newtonsoft.Json;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common;
using YKO.Common.Data;
using YKO.Common.UI;
using YKO.CONST;
using YKO.Main;
using YKO.Network;
using static FormData;



namespace YKO.EndLess
{


    public class DungeonDetailView : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup = default;
        [SerializeField]
        private GameObject panel = default;

        [SerializeField]
        private Button btnFilter = default;
        [SerializeField]
        private Button btnEditTeam = default;
        [SerializeField]
        private Button btnEnterBattle = default;

        [SerializeField]
        public Sprite[] formationSprites = default;
        [SerializeField]
        public Image formationImage = default;

        [SerializeField]
        private Text txtStageIndex = default;
        [SerializeField]
        private Text txtStageName = default;
        [SerializeField]
        private Text txtCurrentPower = default;

        [SerializeField]
        private GameObject[] teamPosList = default;
        [SerializeField]
        private Sprite[] teamPosSprites = default;
        [SerializeField]
        private HeroIcon heroIcon = default;

        private bool _isShow;

        private DungeonDetailViewFunc aData;
        /// <summary>
        /// 無盡顯示隊伍屬性
        /// </summary>
        private PartnerConst.Fun_Form type;


        public void Init(DungeonDetailViewFunc data)
        {
            aData = data;
            // canvasGroup.alpha = 0;
            type = aData.endLessView.GetForm();
            
            UpdateUI();
            RegisterEvent();
            ShowView();
        }



        private void RegisterEvent()
        {

            btnFilter.onClick.RemoveAllListeners();
            btnEditTeam.onClick.RemoveAllListeners();
            btnEnterBattle.onClick.RemoveAllListeners();

            btnFilter.OnClickAsObservable().Where(_ => _isShow == true).Subscribe(_ => CloseView());
            btnEditTeam.OnClickAsObservable().Where(_ => _isShow == true).Subscribe(_ => OnEditTeamButton());
            btnEnterBattle.OnClickAsObservable().Where(_ => _isShow == true).Subscribe(_ => OnEnterBattleButton());
        }

        public void UpdateUI()
        {
            // txtStageIndex.text = GuildDungeonController.Instance.GetChapterName(challengingInfo.currentChapterId) + "：";
            // txtStageName.text = GuildDungeonController.Instance.GetChapterDesc(challengingInfo.currentChapterId);

            txtStageIndex.text = aData.stageIndex;
            txtStageName.text = aData.stageName;

            txtCurrentPower.text = MessageResponseData.Instance.UserData.power.ToString();

            // 先清除隊伍九宮格
            foreach (GameObject teamPos in teamPosList)
            {
                teamPos.GetComponent<Image>().sprite = teamPosSprites[0];

                for (int i = 0; i < teamPos.transform.childCount; i++)
                {
                    Destroy(teamPos.transform.GetChild(i).gameObject);
                }
            }

            FormData formData = BattleManager.Instance.TeamList[type];
            
            formationImage.sprite = formationSprites[formData.Form - 1];
            

            // 根據隊伍英雄位置擺放英雄圖示
            foreach (var info in formData.PosInfos)
            {
                int nineIndex = LoadResource.Instance.FormationData.data_form_data[formData.Form.ToString()].Pos[info.pos - 1][1];
                
                if (BattleManager.Instance.GetBID(info.id) != 0)
                {
                    HeroIcon _heroIcon = Instantiate(heroIcon, teamPosList[nineIndex].transform);
                    var hero = BattleManager.Instance.GetFilterHeroList()[info.id];
                    _heroIcon.Init(hero);
                }
                else
                {
                    teamPosList[nineIndex].GetComponent<Image>().sprite = teamPosSprites[1];
                }
            }

            //生成好友腳色在隊伍顯示上
            if (BattleManager.Instance.extraEditTeamData.ContainsKey(type)) 
            {
                var posInfos = BattleManager.Instance.extraEditTeamData[type];
                Debug.Log("顯示的英雄資訊=>" + JsonConvert.SerializeObject(posInfos));
                foreach (var posInfo in posInfos) {

                    int nineIndex = LoadResource.Instance.FormationData.data_form_data[formData.Form.ToString()].Pos[posInfo.pos - 1][1];
                    HeroIcon _heroIcon = Instantiate(heroIcon, teamPosList[nineIndex].transform);

                    var meg = BattleManager.Instance.GetRespose<Proto_23906_Response>(23906);
                    var tar = meg.list.FirstOrDefault(v => v.id == posInfo.id);

                    HeroData heroData = new HeroData();
                    heroData.UpdateData(tar);   
                    heroData.owner_id = tar.rid;
                    heroData.camp_type = LoadResource.Instance.PartnerData.data_partner_base[heroData.bid.ToString()].camp_type;
                    _heroIcon.Init(heroData);
                }

            }




        }

        public void ShowView()
        {
            canvasGroup.alpha = 0;
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -1624, 0);
            gameObject.SetActive(true);

            Sequence sequence = DOTween.Sequence().SetAutoKill();
            sequence.Append(canvasGroup.DOFade(1, 0.2f))
                    .Join(panel.GetComponent<RectTransform>().DOAnchorPosY(-240, 0.2f))
                    .OnComplete(() => _isShow = true);
        }

        public void CloseView()
        {
            _isShow = false;

            Sequence sequence = DOTween.Sequence().SetAutoKill();
            sequence.Append(canvasGroup.DOFade(0, 0.2f))
                    .Join(panel.GetComponent<RectTransform>().DOAnchorPosY(-1624, 0.2f))
                    .OnComplete(() => { gameObject.SetActive(false); });
        }

        private void OnEnterBattleButton()
        {
            List<FormData.PosInfo> posInfo = new List<PosInfo>();

            var battleInfo = BattleManager.Instance.TeamList[type];
            var userData = MessageResponseData.Instance.UserData;
            for (int i = 0; i < battleInfo.PosInfos.Length; i++)
            {
                var tarPos = battleInfo.PosInfos[i];
                posInfo.Add(new PosInfo() { rid=userData.rid, pos= tarPos.pos, id= tarPos.id ,srv_id=userData.srv_id});
            }

            switch (type)
            {
                case PartnerConst.Fun_Form.EndLess:
                case PartnerConst.Fun_Form.EndLessWater:
                case PartnerConst.Fun_Form.EndLessFire:
                case PartnerConst.Fun_Form.EndLessWind:
                case PartnerConst.Fun_Form.EndLessLightDark:
                    var proto23906 = BattleManager.Instance.GetRespose<Proto_23906_Response>(23906);
                    if (BattleManager.Instance.extraEditTeamData.ContainsKey(type)) {
                        foreach (var data in BattleManager.Instance.extraEditTeamData[type])
                        {
                            var srv = proto23906.list.FirstOrDefault(v => v.rid == data.rid).srv_id;
                            posInfo.Add(new PosInfo() { rid = data.rid, pos = data.pos, id = data.id, srv_id = srv });
                        }
                    }

                    break;
            }



            MainManager.Instance.ShowLoading();
            aData.startChallengeCB(posInfo ,battleInfo.Form ,battleInfo.Hallow );

            //進入戰鬥,傳入戰鬥數據


            /*   BattleManager.Instance.GuildDungeonBattleStart(
                    _info.currentBossId,
                    BattleManager.Instance.TeamList[type].Hallow,
                    BattleManager.Instance.TeamList[type].Form,
                    posInfo,
                    _dungeonView,
                    () => _dungeonView.PlayExitAnim(() =>
                    {
                        GameSceneManager.Instance.AddScene(SceneConst.SceneName.BattleScene,
                            new Battle.BattleScene.SceneParameter()
                            {
                                id = _info.currentChapterId
                            });
                    }
                    )
                    );*/


        }


        /// <summary>
        /// 進入組隊頁
        /// </summary>
        private void OnEditTeamButton()
        {
            bool isAttribute = type == PartnerConst.Fun_Form.EndLess ? false : true;
            GameSceneManager.Instance.SetCurSceneRecord(new EndLessView.SceneParameter() { openDungeonDetailPanel = true , enterAttributePanel=isAttribute });

            //MainManager.Instance.ShowLoading(true);
            aData.endLessView.PlayExitAnim(() =>
            {
                GameSceneManager.Instance.AddScene(SceneConst.SceneName.EditTeamScene,
                    param: new EditTeam.EditTeamView.SceneParameter()
                    {
                        types = new List<PartnerConst.Fun_Form>() { type }
                    });
            });
        }

        /// <summary>
        /// 戰鬥組隊狀況畫面資料
        /// </summary>
        public class DungeonDetailViewFunc
        {
            /// <summary>
            /// 關卡名稱
            /// </summary>
            public string stageIndex;
            /// <summary>
            /// 關卡名稱
            /// </summary>
            public string stageName;

            

            /// <summary>
            /// 開始挑戰的方法
            /// </summary>
            public Action<List<PosInfo>, UInt16 , uint > startChallengeCB;
            /// <summary>
            /// 顯示view
            /// </summary>
            public EndLessView endLessView;

        }
    }
}
