using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;


/*public enum SnsType // 
{
    None,
    Message,
    Reply,
    Image,
    ReplyImage


}*/

namespace Fungus
{

    [CommandInfo("Sns",
              "StartSns",
              "Open Sns Setting")]
    public class StartSns : Command
    {
        public string DialogWindowName;
        /// <summary>
        /// 話題名稱
        /// </summary>
        public string TopicLabel;

        public List<CharaSnsSetting> DialogChara = new List<CharaSnsSetting>();

        public List< SnsMessage> HistorySns=new List<SnsMessage>();

        private static StartSns instance = null;
        public static StartSns GetInstance()
        {
            if (instance == null)
            {
                instance = GetStartSns();
            }
            return instance;
        }

        private static StartSns GetStartSns()
        {
            return GameObject.FindFirstObjectByType<StartSns>();
        }

        public override void OnEnter()
        {
          StartCoroutine( StartSnsDialog());

            //Continue();

        }

        private IEnumerator StartSnsDialog() {
            SnsManager snsManager = null;
            bool isHaveEffect = true;

            //to do  sns鈴聲通知

            if (Flowchart.GetInstance().mStoryControl.SnsWindow==null)
            {
                yield return CreateSnsWindow(_sns => { snsManager = _sns; });
            }
            else
            {
               snsManager= Flowchart.GetInstance().mStoryControl.SnsWindow;//也許也可以甚麼都不運行
                isHaveEffect = false;
            }

            foreach (var sns in HistorySns) {
                var flowchart = ParentBlock.GetFlowchart();
                sns.useAssetText = flowchart.useAssetText;
                sns.mLanguage = flowchart.mLanguage;
            }
            string labelName = "";

            yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
            TopicLabel,
            ParentBlock.GetFlowchart().mLanguage,
            str => {
                Debug.Log("null狀態=>"+str);
                labelName = string.IsNullOrWhiteSpace(str)?TopicLabel:str;
            }
            );


            yield return snsManager.Init(new SnsManagerFunc(
                ParentBlock.GetFlowchart(),
                labelName,
                DialogWindowName,
                DialogChara,
                HistorySns,
                Continue
                ),
                isHaveEffect
                );
        }


        private IEnumerator CreateSnsWindow(Action<SnsManager> cb)
        {
            ResourceRequest  resReq = Resources.LoadAsync<GameObject>(FungusResourcesPath.PrefabsPath + "SnsSystem");
            yield return resReq;

            GameObject sp = Instantiate((GameObject)resReq.asset);
            sp.name = "SnsSystem";
            SnsManager snsManager= sp.GetComponent<SnsManager>();
            Flowchart flow = ParentBlock.GetFlowchart();

            flow.mStoryControl.SnsWindow =snsManager;
            sp.transform.SetParent(flow.mStage.PopupCanvas.transform,false);
           // StartCoroutine(flow.mStoryControl.HideDialogAndTopUI());

            cb(snsManager);
        }


        public void SetCharaValueForAllHistory() {//
            //Debug.Log("顯示的數量=>" + DialogChara.Count);
            List<CharaSnsSetting> setCharas = new List<CharaSnsSetting>();
            foreach (var chara in DialogChara)
            {
                  CharaSnsSetting ch = new CharaSnsSetting() {
                    mFungusChara = chara.mFungusChara,
                    mCharaRole = chara.mCharaRole,
                    mDirection = chara.mDirection
                };

                setCharas.Add(ch);
            }


            if (DialogChara.Count>0&&HistorySns.Count>0) {

                for (int i=0; i<HistorySns.Count ;i++) {

                    HistorySns[i].mChara.Charas = setCharas;
                }

            }

        }

        /* public void SetCharaValueForAllHistory()
         {
             Debug.Log("執行測試");
             if (str.Count<=0||strArr.Count<=0) {
                 return;
             }
             List<string> test = new List<string>(str);


                 for (int i=0;i<strArr.Count;i++) {

                     strArr[i].strList = test;

                 }

         }*/

        public override bool CanNotSkipCommand()
        {
            return true;
        }


    }


}
