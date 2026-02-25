#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using YKO.Common.Font;
using Fungus;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public class Test : MonoBehaviour
{

    /*public class TestA
    {
        public int num=0;

        public string propertyName = "";



    }*/

    public struct TestA
    {
        public int num;

        public string propertyName;
    }
    [SerializeField]
    private Transform tarObj;

    [SerializeField]
    private Transform rootObj;
    //public List<string> strLIst=new List<string>();

    public void Init()
    {
        rootObj = tarObj.parent;
        Debug.Log("根目錄=>"+ (tarObj.parent==null));
        

        // var Aa = strLIst.First(v => v == "aa");
        //Debug.Log("aa=>"+Aa);
        //var ss = new SpineCharaAni();
        // var ss = new Say();
        /* Debug.Log("測試000=>" + ss.ToString());
         Debug.Log("測試1=>"+ss.GetType().Name);
         Debug.Log("測試2=>" + ss.GetType().FullName);

         Debug.Log("測試4=>" + ss.GetType().BaseType);

         Debug.Log("測試6=>" + GetType().GetField("ss", ExportData.DefaultBindingFlags).FieldType.Name);

         Debug.Log("測試7=>" + GetType().GetField("ss", ExportData.DefaultBindingFlags).GetValue(this).GetType());*/

        //Vector3 ww = new Vector3();
        //      Debug.Log("顯示時間=>"+DateTime.Now);
        //     Debug.Log("顯示時間月=>" + DateTime.Now.Month);
        //     Debug.Log("顯示時間ticks=>" + DateTime.Now.Ticks);

        /* Debug.Log("直接顯示=>"+ee);
         Debug.Log("字串=>"+ee.ToString() );
         Debug.Log("數字=>" +int.Parse(ee));
         Debug.Log("數字=>" + (int)ee);
         Debug.Log("數字再字串=>" + ((int)ee).ToString());*/
        /*   var time = UnixTime.FromUnixTime(second);

           TimeSpan t = DateTime.Now-UnixTime.UNIX_EPOCH;
           TimeSpan t2 = DateTime.UtcNow - UnixTime.UNIX_EPOCH;

           Debug.Log("計算秒數=>" + UnixTime.CalcTimeOfDayAtSecond((uint)second,true));*/
        //    Debug.Log("顯示timeStamp1=>" + t.TotalSeconds);
        //Debug.Log("顯示timeStamp2=>" + t2.TotalSeconds);



        // string str = ConvertForShinyOfLight.ConvertStringToUnityEditorDisplay(prompt);
        //Debug.Log("轉換=>"+prompt.Substring(startIndex));


    }

    public enum TestEnum
    {
        aa = 1,
        bb = 20,
        cc = 300,
        dd = 4000
    }





}

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public Test tar;
    //private SerializedProperty listProp;

    public void OnEnable()
    {
        tar = target as Test;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("startTest"))
        {
            tar.Init();

        }
    }
}
#endif

