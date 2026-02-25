using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fungus
{


    [CommandInfo("Sns",
          "EndSns",
          "Close Sns Setting")]
    public class EndSns : Command
    {
        [BlockList]
        public Block targetBlock=null;
        public override void OnEnter()
        {
            Flowchart flow=ParentBlock.GetFlowchart();

            Transform snsWindow = flow.mStoryControl.SnsWindow.transform;

            if (snsWindow!=null) 
            {
                StartCoroutine(
                    snsWindow.GetComponent<SnsManager>().EndSnsWindow(()=> {
                    Continue();
                   // StartCoroutine(flow.mStoryControl.ShowTopUI());
                }));
            } 
            else
            {
                Continue();
            }

        }

        public override void Continue()
        {
            base.Continue();
            if (targetBlock != null)
            {
                targetBlock.GetFlowchart().StartCoroutine(targetBlock.Execute());
            }
        }

        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (targetBlock!=null) { 
            connectedBlocks.Add(targetBlock);
            }

        }

     /*   public override bool CanNotSkipCommand()
        {
            return true;
        }*/


    }


}
