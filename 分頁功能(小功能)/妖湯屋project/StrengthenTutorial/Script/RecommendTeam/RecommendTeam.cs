using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using YKO.DataModel;

namespace YKO.StrengthGuide
{


    /// <summary>
    /// 
    /// 推薦隊伍的主script
    /// </summary>
    public class RecommendTeam : MonoBehaviour
    {

        /// <summary>
        /// 推薦隊伍陣列(根據個別隊伍分類
        /// </summary>
        private List<GameObject> teamCellList = new List<GameObject>();



        #region prefab or parent 
        /// <summary>
        /// 隊伍推薦母物件
        /// </summary>
        [SerializeField] private Transform teamCellParent;
        /// <summary>
        /// 推薦隊伍子物件
        /// </summary>
        [SerializeField] private GameObject teamCellPrefab;

        

        #endregion

        #region  data 

        private StrengthenTutorialManager manager;

        private bool firstOpen = false;
        #endregion

        /// <summary>
        /// 更新當前的顯示
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init(StrengthenTutorialManager _manager)
        {
            manager = _manager;
            if (!firstOpen) {
                yield return UpdateUI();
            }

        }

        private IEnumerator UpdateUI()
        {
            int completeCount = 0;
            var fixedDatas = manager.FixedData.data_recommand.Values.ToList();

            fixedDatas.Sort((a, b) => 
            {
                if (a.sort<=b.sort) {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            );

            foreach (var teamInfo in fixedDatas) {

                var sp = Instantiate(teamCellPrefab);
                sp.transform.SetParent(teamCellParent,false);

                sp.GetComponent<RecommendTeamCell>()
                    .Init(
                    teamInfo,
                    () => { completeCount++; });

                teamCellList.Add(sp);


            }
            yield return new WaitUntil(() =>completeCount>= manager.FixedData.data_recommand.Count);
            firstOpen= true;

        }

    }
}
