// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using DG.Tweening;
using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Music", 
                 "Stops the currently playing game music.")]
    [AddComponentMenu("")]
    public class StopMusic : Command
    {
        /// <summary>
        /// 是否等待command執行結束,再進行下一步
        /// </summary>
        public bool waitMusicOver = false;

        /// <summary>
        /// 淡出過場時間
        /// </summary>
        public float fadeDuration = 0;


        #region Public members

        public override void OnEnter()
        {
            var musicManager = FungusManager.Instance.MusicManager;
            if (waitMusicOver)
            {
                StartCoroutine(musicManager.StopMusicOfFade(fadeDuration, () => { Continue(); }));
            }
            else
            {
                StartCoroutine(musicManager.StopMusicOfFade(fadeDuration));
                Continue();
            }
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
        #endregion
    }
}