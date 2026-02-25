using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using YKO.Common.UI;

namespace YKO.Main
{
    public class StoryManager : MonoBehaviour
    {//receive story directory

        [SerializeField] private string DataName;

        [SerializeField] private Flowchart mFlowchart;


        /// <summary>
        /// 設置mFlowcahrt的 DataName
        /// mFlowchart.OverrideSaveData();根據DataName 設置文檔
        /// </summary>

        public void Start()
        {
            StartCoroutine(Init());     

        }

        public IEnumerator Init()
        {

            yield return LoadStoryData();
            MainManager.Instance.HideLoading(true);
            mFlowchart.gameObject.SetActive(true);


        }

        public IEnumerator LoadStoryData()
        {


            yield return GameSceneManager.Instance.UnloadOtherSceneAsync();
       //  yield return mFlowchart.OverrideSaveData("prologue");


        }








    }
}
