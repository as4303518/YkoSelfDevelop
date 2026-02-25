using UnityEngine;
using YKO.Common.UI;
using YKO.Story;

namespace Fungus
{
    [CommandInfo("Scene",
             "AlterScene",
             "Transfer to new scene.")]
    [AddComponentMenu("")]
    
    public class AlterScene : Command
    {
        public string SceneName = "";

        public override void OnEnter()
        {
            Continue();

            //轉場需要有陣列會更方便 先預設回 mypage

            //   StartCoroutine(StoryScene.Instance.OnCommandChangeScene(SceneConst.SceneName.MypageScene));
            if (string.IsNullOrEmpty(SceneName)) {
                StartCoroutine(StoryScene.Instance.OnCommandChangeScene(StoryScene.Instance.GetStorySceneParam.transitionsScene));
            }
            else
            {
                StartCoroutine(StoryScene.Instance.OnCommandChangeScene(SceneName));
            }

        }
    }
}

