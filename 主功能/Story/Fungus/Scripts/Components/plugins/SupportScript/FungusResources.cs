using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Fungus {
    public static class FungusResources //獲取 fungus prefab資源的腳本
    {
        /// <summary>
        /// 獲得角色
        /// </summary>
        /// <param name="charaName"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        public static IEnumerator GetCharacter(string charaName,Action<Character>finish) {
            Debug.Log("角色名=>" + charaName);
            ResourceRequest request = Resources.LoadAsync<GameObject>(FungusResourcesPath.Chara2DPortraits+charaName);
            yield return request;
            try
            {
                finish((request.asset as GameObject).GetComponent<Character>());
            }
            catch
            {
                Debug.LogError("找不到角色=>"+ FungusResourcesPath.Chara2DPortraits + charaName);
            }
      
        }
        /// <summary>
        /// 獲得音頻
        /// </summary>
        /// <param name="audioClipName"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        public static IEnumerator GetAudioClip(string audioClipName,Action<AudioClip>finish)
        {
            ResourceRequest request = Resources.LoadAsync<AudioClip>(FungusResourcesPath.CharaVoice + audioClipName);
            yield return request;


            try
            {
                finish((request.asset as AudioClip));
            }
            catch
            {
                Debug.LogError("找不到音頻=>" + audioClipName);
            }


        }

        /// <summary>
        /// 判斷目標路徑是否為嘿嘿嘿的內容
        /// </summary>
        /// <returns></returns>
        public static bool JudgeTargetPathIsHCG(this string path)
        {
            Debug.Log("判斷路徑=>"+path);
            if (string.IsNullOrWhiteSpace( path))
            {
                return false;
            }
            var pathSplit = path.Split(".")[0].Split("/");
            foreach (var split in pathSplit)
            {
                if (split.Equals(FungusResourcesPath.HCGFloderName))
                {
                    return true;
                }
            }
            return false;
        }

        /*   public static IEnumerator GetCharaSpine(string charaName, Action<CharaSpine> finish)
           {

               ResourceRequest request = Resources.LoadAsync<GameObject>(FungusResourcesPath.SpineChara + charaName);
               yield return request;

               finish( (request.asset as GameObject).GetComponent<CharaSpine>() );

           }*/


    }


}