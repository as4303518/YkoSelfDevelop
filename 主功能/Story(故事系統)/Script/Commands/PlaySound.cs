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
                 "Play Sound",
                 "Plays a once-off sound effect. Multiple sound effects can be played at the same time.")]
    [AddComponentMenu("")]
    public class PlaySound : Command
    {
        [Tooltip("Sound effect clip to play")]
        [SerializeField] protected AudioClip soundClip;

        [Range(0,1)]
        [Tooltip("Volume level of the sound effect")]
        [SerializeField] protected float volume = 1;

        [Tooltip("Wait until the sound has finished playing before continuing execution.")]
        [SerializeField] protected bool waitUntilFinished;

        /// <summary>
        /// 撥放開始時間
        /// </summary>
        [SerializeField] protected float atTime;
        /// <summary>
        /// 撥放持續時間
        /// </summary>
        [SerializeField]protected float durationTime;

        /// <summary>
        /// 淡出持續時間
        /// </summary>
        [SerializeField] protected float fadeOutSoundTime=0.5f;

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
            musicManager.PlaySound(soundClip, settingVolume*volume,atTime,durationTime, fadeOutSoundTime);

            if (waitUntilFinished)
            {
                Invoke("DoWait", soundClip.length);
            }
            else
            {
                Continue();
            }
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
