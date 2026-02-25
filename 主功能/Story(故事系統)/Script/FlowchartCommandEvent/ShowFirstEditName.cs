using UnityEngine;
using YKO.Network;
using YKO.Story;

namespace Fungus
{
    [CommandInfo("Scene",
                 "ShowFirstEditName",
                 "Show FirstEditName popup.")]
    [AddComponentMenu("")]

    public class ShowFirstEditName : Command
    {
        public override void OnEnter()
        {
            FungusManager fungusManager = FungusManager.Instance;

          /*  if (fungusManager != null) // 避免屏蔽screen
            {
                FungusManager.ClearFungusInstance();
            }*/

            // sex為2代表尚未取名
            if (MessageResponseData.Instance.ProtoResponse10301.sex == 2)
            {
                StartCoroutine(StoryScene.Instance.OnCommandShowFirstEditName(Continue));
            }
            else
            {
                Continue();
            }

        }

        public override bool CanNotSkipCommand()
        {
            if (MessageResponseData.Instance.ProtoResponse10301.sex < 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}

