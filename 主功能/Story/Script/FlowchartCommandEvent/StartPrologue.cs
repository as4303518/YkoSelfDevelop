using UnityEngine;
using YKO.Story;

namespace Fungus
{
    [CommandInfo("Scene",
                 "StartPrologue",
                 "Start Prologue event.")]
    [AddComponentMenu("")]

    public class StartPrologue : Command
    {
        public override void OnEnter()
        {
            FungusManager fungusManager = FungusManager.Instance;

            if (fungusManager != null) // 避免屏蔽screen
            {
                FungusManager.ClearFungusInstance();
            }

            Continue();

            //StartCoroutine(StoryScene.Instance.OnCommandStartPrologue());
        }
    }
}

