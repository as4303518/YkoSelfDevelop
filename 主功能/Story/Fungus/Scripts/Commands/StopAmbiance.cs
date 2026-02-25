// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Ambiance", 
                 "Stops the currently playing game ambiance.")]
    [AddComponentMenu("")]
    public class StopAmbiance : Command
    {
        #region Public members

        /// <summary>
        /// 是否等待command執行結束,再進行下一步
        /// </summary>
        public bool waitMusicOver = false;
        /// <summary>
        /// 淡出過場時間
        /// </summary>
        public float fadeDuration = 0;

        public override void OnEnter()
        {
            var musicManager = FungusManager.Instance.MusicManager;
            
            if (waitMusicOver)
            {
                StartCoroutine(musicManager.StopAmbianceOfFade(fadeDuration, () => { Continue(); }));
            }
            else
            {
                StartCoroutine(musicManager.StopAmbianceOfFade(fadeDuration));
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