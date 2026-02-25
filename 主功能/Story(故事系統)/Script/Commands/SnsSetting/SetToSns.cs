using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;



namespace Fungus
{

    [CommandInfo("Sns",
       "SetToSns",
       "Set Sns Setting")]
    public class SetToSns : Command
    {

        [SerializeField]public SnsMessage sns=new SnsMessage();


       /* public void InitSnsData() { 
        sns=new SnsMessage();
        }*/

        public override void OnEnter()
        {
            if (ParentBlock.GetFlowchart().mStoryControl.isSkipPlay)
            {
                if (sns.mMessageType._snsType== SnsType.Reply|| sns.mMessageType._snsType == SnsType.OneClickReply) 
                {
                    
                    foreach (var reply in sns.mMessageType._replyMessage) 
                    {
                        if (reply.targetBlock!=null) 
                        {
                            sns.curTargetBlock = reply.targetBlock;
                            break;
                        }
                    }
                }


                if (sns.curTargetBlock != null)
                {
                    sns.curTargetBlock.GetFlowchart().StartCoroutine(sns.curTargetBlock.Execute());
                }
                Continue();
            }
            else
            {
                SnsManager snsWindow = Flowchart.GetInstance().mStoryControl.SnsWindow;
                var flowchart = ParentBlock.GetFlowchart();
                sns.useAssetText = flowchart.useAssetText;
                sns.mLanguage = flowchart.mLanguage;
                if (snsWindow != null)
                {
                    if (sns.mMessageType._snsType == SnsType.Reply || sns.mMessageType._snsType == SnsType.OneClickReply) {
                        foreach (var reply in sns.mMessageType._replyMessage)
                        {
                            if (string.IsNullOrWhiteSpace( reply.introduction)) 
                            {
                                reply.introduction = "企劃未填上選項,請補上簡短回覆";
                            }
                            if (string.IsNullOrWhiteSpace(reply.message))
                            {
                                reply.message = "企劃未填上選項,請補上完整回覆";
                            }
                        }
                    }
                    StartCoroutine(snsWindow.SetDialogue(sns, () =>
                    {
                        if (sns.curTargetBlock != null)
                        {
                            sns.curTargetBlock.GetFlowchart().StartCoroutine(sns.curTargetBlock.Execute());
                        }
                        Continue();
                    }));
                }
                else
                {
                    if (sns.curTargetBlock != null)
                    {
                        sns.curTargetBlock.GetFlowchart().StartCoroutine(sns.curTargetBlock.Execute());
                    }
                    Continue();
                    
                }
            }
        }

        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (sns == null)
            {
                return;
            }
            if (sns.mMessageType == null)
            {
                return;
            }
            if (sns.mMessageType._replyMessage==null) {
                return;
            }
            if (sns.mMessageType._snsType!=SnsType.Reply) {
                return;
            }

            if (sns.mMessageType._replyMessage.Length>0)
            {
                foreach (var reply in sns.mMessageType._replyMessage) 
                {
                    if (reply.targetBlock!=null) {
                        connectedBlocks.Add(reply.targetBlock);
                    }
                }
            }
        }
        public override string GetSummary()
        {
            var displayTip = "";
            if (sns!=null)
            {
                displayTip+= sns.mMessageType._snsType.ToString();
                if (!string.IsNullOrWhiteSpace( sns.mMessageType._message)&& sns.mMessageType._snsType== SnsType.Message)
                {
                    displayTip += "/"+sns.mMessageType._message;
                }
            }

            return displayTip;
        }
        /* public override bool CanNotSkipCommand()
         {
             return true;
         }*/

    }

}
