using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using YKO.Network;

namespace Fungus
{

    public enum SettingLanguage
    {
        EN,
        TW,
        CN,
        JP,
        KR,
    }
    /// <summary>
    /// 文本名稱類別
    /// </summary>
    public enum AssetTextType  
    {
        StoryText,
        CharaNumber,
        UINumber
    }

    public static class TranslateOfCsv
    {
        /// <summary>
        /// 儲存多語言的文字檔(加載第一次後可避免重複等待加載檔案的時間)
        /// </summary>
        public static Dictionary<string, string> saveTransLateTextFile = new Dictionary<string, string>();

        public static IEnumerator eGetSpecifyValueOfCsvFile(string key,AssetTextType fileName, SettingLanguage language, Action<string> cb)
        {

            yield return eGetSpecifyValueOfCsvFile(key, fileName.ToString(), language, cb);
        
        }
        public static IEnumerator eGetSpecifyValueOfCsvFile(string key,  SettingLanguage language, Action<string> cb)
        {
            string fileName = key.Split("_")[0];//ex: main_ch00_00_text00 main is fileName
            yield return eGetSpecifyValueOfCsvFile(key, fileName, language, cb);
        }

        /// <summary>
        /// 透過指定檔案名稱 加載多語言
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fileName"></param>
        /// <param name="language"></param>
        /// <param name="cb"></param>
        public static void GetSpecifyValueOfCsvFile(string key, AssetTextType fileName, SettingLanguage language, Action<string> cb)
        {
             GetSpecifyValueOfCsvFile(key, fileName.ToString(), language, cb);
        }

        /// <summary>
        /// 透過key 判斷檔案名稱 加載多語言
        /// </summary>
        /// <param name="key"></param>
        /// <param name="language"></param>
        /// <param name="cb"></param>
        public static void GetSpecifyValueOfCsvFile(string key, SettingLanguage language, Action<string> cb)
        {
            string fileName = key.Split("_")[0];//ex: main_ch00_00_text00 main is fileName
            GetSpecifyValueOfCsvFile(key, fileName, language, cb);
        }


        /// <summary>
        /// 透過語言編號與檔案名稱,獲得相對的多語言字串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fileName"></param>
        /// <param name="language"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static IEnumerator eGetSpecifyValueOfCsvFile(string value,string fileName,SettingLanguage language, Action<string> cb)
          {
                string CsvText = null;
                

                if (saveTransLateTextFile.ContainsKey(fileName))
                {
                    CsvText = saveTransLateTextFile[fileName];
                }
                else
                {
                yield return GetAssetText(fileName, data =>
                    {
                        if (data != null)
                        {
                            CsvText = data;
                        }
                    });
                }

            if (CsvText == null)
            {
                Debug.LogWarning("Not Have AssetText");
                cb(value);
                yield break;
            }

            cb(GetSentenceString(CsvText, value, language));


        }


        /// <summary>
        /// 透過語言編號與檔案名稱,獲得相對的多語言字串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fileName"></param>
        /// <param name="language"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static void GetSpecifyValueOfCsvFile(string value, string fileName, SettingLanguage language, Action<string> cb)
        {
            string CsvText = null;
            try
            {
                CsvText = saveTransLateTextFile[fileName];
            }
            catch 
            {

                Debug.LogWarning("不存在的文本檔案=>"+fileName);
            
            }


            if (CsvText == null)
            {
                Debug.LogWarning("Not Have AssetText");
                cb(value);
                return;
            }

            cb(GetSentenceString(CsvText, value, language));


        }


        /// <summary>
        /// 通過固定格式,從Csv.text獲取Sentence要求的值
        /// </summary>
        /// <param name="CsvText"></param>
        /// <param name="sentenceNum"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private static string GetSentenceString(string CsvText,string sentenceNum, SettingLanguage language)
        {
            List<string> csvList = CsvText.Split("\n").ToList();//分行
            List<string[]> textData = new List<string[]>();//分格

            foreach (string data in csvList)
            {
                textData.Add(data.Split("$"));
            }

            int languageInt = 0;

            foreach (string title in textData[0])
            {
                if (title.StartsWith("Text"))
                {
                    if (title.Remove(title.IndexOf("Text"), 4).Trim() == language.ToString())
                    {
                        break;
                    }
                }
                languageInt++;
            }


            string result = "";

            try {
                var lineResult = textData.Find(strArr => {
                    return  strArr[0].Trim() == sentenceNum.Trim();
                    });
                result = lineResult[languageInt];

            }
            catch
            {
                Debug.Log("找不到" + sentenceNum + "的" + language.ToString() + "翻譯句子");
                result = "Can Not Found Sentence Number=>" + sentenceNum + "With Language=>" + language.ToString();
            }

            return result.ConditionModification();
          //  return textData.Find(strArr => strArr[0] == sentenceNum)[languageInt];

        }

        /// <summary>
        /// 根據條件篩選文字
        /// </summary>
        public static string  ConditionModification(this string sentence) 
        {
            if (sentence.Contains("%NAME%")) {
                return sentence.Replace("%NAME%", MessageResponseData.Instance.UserData.name);
            }
            else
            {
                return sentence;
            }
        }


        /// <summary>
        /// 透過參數路徑從Resources回傳文本內容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static IEnumerator GetAssetText(string fileName, Action<string> cb=null)
        {
            if (saveTransLateTextFile.ContainsKey(fileName)) 
            {
                cb?.Invoke(saveTransLateTextFile[fileName]);
                yield break;
            }

            ResourceRequest CsvText = null;
            try
            {
                //###使用addressable
                CsvText = Resources.LoadAsync<TextAsset>(FungusResourcesPath.AssetTextPath+ fileName);

            }
            catch
            {
                Debug.LogError("No Find Text");
            }

            if (CsvText != null)
            {
                yield return new WaitUntil(() => CsvText.isDone);

                if ((CsvText.asset as TextAsset) != null)//空的一樣會加載
                {
                    if (!saveTransLateTextFile.ContainsKey(fileName))
                    {

                        saveTransLateTextFile.Add(fileName, (CsvText.asset as TextAsset).text);
                    }
                    if (cb != null)
                    {
                        cb((CsvText.asset as TextAsset).text);
                    }
                }
                else
                {
                    Debug.Log("Can Not Found Assettext From =>" + fileName);
                    if (cb != null)
                    {
                        cb(null);
                    }
                }

            }
            else
            {
                if (cb != null){
                    cb(null);
                }
            }
        }

    }
}
