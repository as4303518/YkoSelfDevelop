using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class RegexTest : MonoBehaviour
{
    // Start is called before the first frame update
    public string str;

    public string filterText= "<assets=";
    public void Init()
    {

        /*
            int numCount = 0;
            Regex numMatch = new Regex(@"^[0-9]+$");

            while (numMatch.IsMatch(tempStr[numCount].ToString()))
            {
                numCount++;
            }
            Debug.Log("數字數量是=>" + numCount);
        */
        var sorts = SortingLayer.layers;
        foreach (var layer in sorts)
        {
            Debug.Log("圖層名稱=>"+layer.name);

        }

        string tempStr = str;
        
        if (tempStr.Contains(filterText))
        {
            var split = tempStr.Split(filterText);

            for (int i = 0; i < (split.Length-1); i++)
            {
                var pos = tempStr.IndexOf(filterText);
                int numCount = 0;
                Regex numMatch = new Regex(@"^[0-9]+$");

                while ((pos + 8 + numCount) < tempStr.Length&& numMatch.IsMatch(tempStr[pos + 8 + numCount].ToString()))
                {
                    numCount++;
                }
                Debug.Log("數字數量是=>" + numCount);
                tempStr=tempStr.Remove(pos +8+ numCount, 2);
                tempStr=tempStr.Remove(pos, 8);
                Debug.Log("輸出結果=>"+tempStr);

            }

        }
        else
        {
            Debug.Log("不包含");
        }

    }


}

#if UNITY_EDITOR
[CustomEditor(typeof(RegexTest))]
public class RegexTestEditor : Editor
{
    private RegexTest tar;

    public void OnEnable()
    {
        tar=target as RegexTest;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("start")) 
        {
            tar.Init();
        }
    }
}
#endif