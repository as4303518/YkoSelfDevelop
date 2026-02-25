using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace YKO.StrengthGuide
{

    public class CommonQuestion : MonoBehaviour
    {

        /// <summary>
        /// 問題卡母物件
        /// </summary>
        [SerializeField] private Transform questionCellParent;

        #region  Prefab
        /// <summary>
        /// 問題表格預製物
        /// </summary>
        [SerializeField] private GameObject questionCellPrefab;
        #endregion

        #region param
        /// <summary>
        /// 問題卡物件陣列
        /// </summary>
        private List<GameObject> questionCellList = new List<GameObject>();

        private StrengthenTutorialManager manager;

        private bool firstOpen = false;

        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_manager"></param>
        /// <returns></returns>
        public IEnumerator Init(StrengthenTutorialManager _manager)
        {
            manager = _manager;
            if (!firstOpen) {
                UpdateUI();
                yield return null;
            }


        }
        /// <summary>
        /// 更新UI顯示
        /// </summary>
        private void UpdateUI()
        {
            foreach (var problemData in manager.FixedData.data_problem.Values)
            {
                var sp = Instantiate(questionCellPrefab);
                sp.transform.SetParent(questionCellParent, false);
                sp.GetComponent<QuestionCell>().Init(problemData);
                questionCellList.Add(sp);
            }
            firstOpen = true;

        }

    }
}