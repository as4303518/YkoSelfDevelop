using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YKO.Common.UI;
using System.Reflection;


namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(AlterScene))]
    public class AlterSceneEditor : CommandEditor
    {

        private SerializedProperty sceneNameProp;
        public override void OnEnable()
        {
            sceneNameProp = serializedObject.FindProperty("SceneName");

        }


        public override void DrawCommandGUI()
        {
            //轉場需要有陣列會更方便 先預設回 mypage
            List<string> strList = new List<string>();

            var fieldArr = typeof(SceneConst.SceneName).GetFields(ExportData.DefaultBindingFlags | BindingFlags.Static);

            foreach (var field in fieldArr)
            {
                strList.Add(field.GetValue(new SceneConst.SceneName()) as string);

            }
            
            StringField(sceneNameProp,
                         new GUIContent("FinishDefaultAnimation", "Animation representing character"),
                         new GUIContent("<None>"),
                          strList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

