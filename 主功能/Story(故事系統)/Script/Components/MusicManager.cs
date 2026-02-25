// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

namespace Fungus
{
    /// <summary>
    /// Music manager which provides basic music and sound effect functionality.
    /// Music playback persists across scene loads.
    /// </summary>
    //[RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        /// <summary>
        /// 音樂
        /// </summary>
        protected AudioSource audioSourceMusic;
        /// <summary>
        /// 環境音
        /// </summary>
        protected AudioSource audioSourceAmbiance;
        /// <summary>
        /// 特效音
        /// </summary>
        protected AudioSource audioSourceSoundEffect;

        void Reset()
        {
            int audioSourceCount = this.GetComponents<AudioSource>().Length;
            for (int i = 0; i < 3 - audioSourceCount; i++)
                gameObject.AddComponent<AudioSource>();
            
        }

        protected virtual void Awake()
        {
            Reset();
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSourceMusic = audioSources[0];
            audioSourceAmbiance = audioSources[1];
            audioSourceSoundEffect = audioSources[2];
        }

        protected virtual void Start()
        {
            audioSourceMusic.playOnAwake = false;
            audioSourceMusic.loop = true;
        }

        #region Public members

        /// <summary>
        /// Plays game music using an audio clip.
        /// One music clip may be played at a time.
        /// </summary>
        public void PlayMusic(AudioClip musicClip, bool loop, float fadeDuration, float atTime,float volume )
        {
            if (audioSourceMusic == null || audioSourceMusic.clip == musicClip)
            {
                return;
            }

            if (Mathf.Approximately(fadeDuration, 0f))
            {
                audioSourceMusic.clip = musicClip;
                audioSourceMusic.loop = loop;
                audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                audioSourceMusic.volume = volume;
                audioSourceMusic.Play();
            }
            else
            {
                audioSourceMusic.volume = 0;
                audioSourceMusic.clip = musicClip;
                audioSourceMusic.loop = loop;
                audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                audioSourceMusic.Play();
                LeanTween.value(gameObject, 0, volume, fadeDuration)
                    .setOnUpdate((v) => {
                        // Fade out current music
                        audioSourceMusic.volume = v;
                    });
            }
        }

        private Coroutine saveSoundEffectCoro=null;

        /// <summary>
        /// Plays a sound effect once, at the specified volume.
        /// </summary>
        /// <param name="soundClip">The sound effect clip to play.</param>
        /// <param name="volume">The volume level of the sound effect.</param>
        public virtual void PlaySound(AudioClip soundClip, float volume,float atTime=0,float duration=0,float fadeSoundTime = 0.5f)
        {
            audioSourceSoundEffect.clip = soundClip;
            audioSourceSoundEffect.volume = volume;
            IEnumerator Over()
            {
                if (saveSoundEffectCoro!=null)
                {
                    StopCoroutine(saveSoundEffectCoro);
                }
                //當開始撥放時間比撥放總時長長  或者 開始時間加上其他設定時間比撥放總時長長
                if (atTime>soundClip.length|| atTime + duration + fadeSoundTime > soundClip.length)
                {
                    atTime = 0;
                }
                if (duration <= 0)
                { 
                    duration=soundClip.length;
                }
                else
                {
                    float remainTime = soundClip.length - atTime - duration;
                    if (remainTime < 0)
                    {
                        duration = soundClip.length - atTime - fadeSoundTime;
                    }
                    else
                    {
                        //持續時間不變
                    }
                }
                audioSourceSoundEffect.time = atTime;
                audioSourceSoundEffect.Play();
                yield return new WaitForSeconds(duration);
                // Tween.
                var tw = DOTween.To(() => audioSourceSoundEffect.volume, v => audioSourceSoundEffect.volume = v, 0, fadeSoundTime);
                yield return new WaitForSeconds(fadeSoundTime);
                audioSourceSoundEffect.Stop();
            }
            saveSoundEffectCoro = StartCoroutine(Over());

          //  audioSourceSoundEffect.PlayOneShot(soundClip, volume);
        }

        /// <summary>
        /// 在淡出中結束音樂
        /// </summary>
        /// <returns></returns>
        public IEnumerator StopSoundOfFade(float dur, Action cb = null)
        {
            yield return DOTween.To(
                () => audioSourceSoundEffect.volume
                , v => audioSourceSoundEffect.volume = v
                , 0, dur)
                .WaitForCompletion();
            if (audioSourceSoundEffect)
            {
                audioSourceSoundEffect?.Stop();
                audioSourceSoundEffect.clip = null;
                audioSourceSoundEffect.volume = 1;
            }
            cb?.Invoke();
        }



        /// <summary>
        /// Plays a sound effect with optional looping values, at the specified volume.
        /// </summary>
        /// <param name="soundClip">The sound effect clip to play.</param>
        /// <param name="loop">If the audioclip should loop or not.</param>
        /// <param name="volume">The volume level of the sound effect.</param>
        public virtual void PlayAmbianceSound(AudioClip clip, bool loop, float volume,float atTime,float fadeInDur)
        {

            if (audioSourceAmbiance == null || audioSourceAmbiance.clip == clip)
            {
                return;
            }

            if (atTime>clip.length|| atTime > clip.length)
            {
                atTime = 0;
            }

            if (Mathf.Approximately(fadeInDur, 0f))
            {
                audioSourceAmbiance.clip = clip;
                audioSourceAmbiance.loop = loop;
                audioSourceAmbiance.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                audioSourceAmbiance.Play();
            }
            else
            {
                audioSourceAmbiance.volume = 0;
                audioSourceAmbiance.clip = clip;
                audioSourceAmbiance.loop = loop;
                audioSourceAmbiance.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                audioSourceAmbiance.Play();
                DOTween.To(() => audioSourceAmbiance.volume, t => audioSourceAmbiance.volume = t, volume, fadeInDur);
            }

          /*  audioSourceAmbiance.loop = loop;
            audioSourceAmbiance.clip = soundClip;
            audioSourceAmbiance.volume = volume;
            audioSourceAmbiance.Play();*/
        }

        /// <summary>
        /// Shifts the game music pitch to required value over a period of time.
        /// </summary>
        /// <param name="pitch">The new music pitch value.</param>
        /// <param name="duration">The length of time in seconds needed to complete the pitch change.</param>
        /// <param name="onComplete">A delegate method to call when the pitch shift has completed.</param>
        public virtual void SetAudioPitch(float pitch, float duration, System.Action onComplete)
        {
            if (Mathf.Approximately(duration, 0f))
            {
                audioSourceMusic.pitch = pitch;
                audioSourceAmbiance.pitch = pitch;
                if (onComplete != null)
                {
                    onComplete();
                }
                return;
            }

            LeanTween.value(gameObject,
                audioSourceMusic.pitch,
                pitch,
                duration).setOnUpdate((p) =>
                {
                    audioSourceMusic.pitch = p;
                    audioSourceAmbiance.pitch = p;
                }).setOnComplete(() =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });
        }

        /// <summary>
        /// Fades the game music volume to required level over a period of time.
        /// </summary>
        /// <param name="volume">The new music volume value [0..1]</param>
        /// <param name="duration">The length of time in seconds needed to complete the volume change.</param>
        /// <param name="onComplete">Delegate function to call when fade completes.</param>
        public virtual void SetAudioVolume(float volume, float duration, System.Action onComplete)
        {
            if (Mathf.Approximately(duration, 0f))
            {
                if (onComplete != null)
                {
                    onComplete();
                }
                audioSourceMusic.volume = volume;
                audioSourceAmbiance.volume = volume;
                return;
            }

            LeanTween.value(gameObject,
                audioSourceMusic.volume,
                volume,
                duration).setOnUpdate((v) => {
                    audioSourceMusic.volume = v;
                    audioSourceAmbiance.volume = v;
                }).setOnComplete(() => {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });
        }
        /// <summary>
        /// 在淡出中結束音樂
        /// </summary>
        /// <returns></returns>
        public IEnumerator StopMusicOfFade(float dur,Action cb=null)
        {
            yield return DOTween.To(
                ()=>audioSourceMusic.volume
                ,v=> audioSourceMusic.volume=v
                , 0, dur)
                .WaitForCompletion();
            if (audioSourceMusic)
            {
                audioSourceMusic?.Stop();
                audioSourceMusic.clip = null;
                audioSourceMusic.volume = 1;
            }
            cb?.Invoke();
        }

        /*/// <summary>
        /// Stops playing game music.
        /// </summary>
        public virtual void StopMusic()
        {
            audioSourceMusic.Stop();
            audioSourceMusic.clip = null;
        }*/

        /// <summary>
        /// Stops playing game ambiance.
        /// </summary>
       /* public virtual void StopAmbiance()
        {
            audioSourceAmbiance.Stop();
            audioSourceAmbiance.clip = null;
        }*/

        /// <summary>
        /// 在淡出中結束氛圍音樂
        /// </summary>
        /// <returns></returns>
        public IEnumerator StopAmbianceOfFade(float dur, Action cb = null)
        {
            yield return DOTween.To(
                () => audioSourceAmbiance.volume
                , v => audioSourceAmbiance.volume = v
                , 0, dur)
                .WaitForCompletion();
            if (audioSourceAmbiance)
            {
                audioSourceAmbiance?.Stop();
                audioSourceAmbiance.clip = null;
                audioSourceAmbiance.volume = 1;
            }
            cb?.Invoke();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #endregion
    }
}