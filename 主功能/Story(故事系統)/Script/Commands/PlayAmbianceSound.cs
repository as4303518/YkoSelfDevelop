// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using System;
using UnityEngine;
using static YKO.Common.Sound.SoundController;

namespace Fungus
{
    /// <summary>
    /// Plays a once-off sound effect. Multiple sound effects can be played at the same time.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Ambiance Sound",
                 "Plays a background sound to be overlayed on top of the music. Only one Ambiance can be played at a time.")]
    [AddComponentMenu("")]
    public class PlayAmbianceSound : Command
    {
        [Tooltip("Sound effect clip to play")]
        [SerializeField]
        protected AudioClip soundClip;

        [Range(0, 1)]
        [Tooltip("Volume level of the sound effect")]
        [SerializeField]
        protected float volume = 1;
        
        [Tooltip("Sound effect clip to play")]
        [SerializeField]
        protected bool loop;

        /// <summary>
        /// 撥放開始時間點
        /// </summary>
        [Tooltip("Length of time to fade out previous playing music.")]
        [SerializeField] protected float atTime = 1f;


        /// <summary>
        /// 淡入撥放
        /// </summary>
        [Tooltip("Length of time to fade out previous playing music.")]
        [SerializeField] protected float fadeDuration = 1f;


        protected virtual void DoWait()
        {
            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            if (soundClip == null)
            {
                Continue();
                return;
            }

            var musicManager = FungusManager.Instance.MusicManager;
            var settingVolume = PlayerPrefs.GetFloat(Enum.GetName(typeof(SoundType), SoundType.SE), 1.0f);
            musicManager.PlayAmbianceSound(soundClip, loop, settingVolume*volume,atTime,fadeDuration);
            Continue();
        }

        public override string GetSummary()
        {
            if (soundClip == null)
            {
                return "Error: No sound clip selected";
            }

            return soundClip.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}
