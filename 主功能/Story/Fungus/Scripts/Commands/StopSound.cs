using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fungus
{
    [CommandInfo("Audio","Stop Sound","Stop Effect Sound")]
    [AddComponentMenu("")]
    public class StopSound :Command
    {
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
                StartCoroutine(musicManager.StopSoundOfFade(fadeDuration, () => { Continue(); }));
            }
            else
            {
                StartCoroutine(musicManager.StopSoundOfFade(fadeDuration));
                Continue();
            }
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}
