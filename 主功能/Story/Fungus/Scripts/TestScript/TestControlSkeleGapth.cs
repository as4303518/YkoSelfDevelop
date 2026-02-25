#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEditor;

public class TestControlSkeleGapth : MonoBehaviour
{
    // Start is called before the first frame update

    public SkeletonGraphic chara;
    public int aniLayer;
    public string animationName;

    public string transName = "";
  public void ChangeAnimation()
    {

    
        //Debug.Log("執行動畫" + animationName);
        //  chara.AnimationState.SetAnimation(aniLayer, animationName, true);
        //"talk_start"
    }

    public void CloseCharaAni()
    {
      //  Debug.Log("關閉動畫為預設");
      //  chara.AnimationState.SetEmptyAnimation(aniLayer, 1);

    }

}



[CustomEditor(typeof(TestControlSkeleGapth))]
public class TestControlSkeleGapthEditor:Editor
{

    private TestControlSkeleGapth tar;
    private SerializedProperty animationNameProp;
    private SerializedProperty skeletonGraphicProp;
    private SerializedProperty aniLayerProp;
    private SerializedProperty transProp;

    public void OnEnable()
    {
        tar=target as TestControlSkeleGapth;
        animationNameProp = serializedObject.FindProperty("animationName");
        skeletonGraphicProp= serializedObject.FindProperty("chara");
        aniLayerProp = serializedObject.FindProperty("aniLayer");
        transProp = serializedObject.FindProperty("transName");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(skeletonGraphicProp);
        EditorGUILayout.PropertyField(aniLayerProp);
        EditorGUILayout.PropertyField(transProp);
        if (tar.chara != null)
        {
            List<string> animations = tar.chara.GetAllAnimationsStringName();
            animations.Insert(0, "<None>");

            List<GUIContent> labels = new List<GUIContent>();
            int nowNum = 0;

            for (int i=0;i<animations.Count;i++) {
                string animationsString = animations[i];
                labels.Add(new GUIContent(animationsString));
                if (animationNameProp.stringValue == animationsString)
                {
                    nowNum=i;
                }
            }

 
            

           int chooseNum=EditorGUILayout.Popup(new GUIContent("SkinChoose"), nowNum, labels.ToArray());

    
           animationNameProp.stringValue = animations[chooseNum];

        }
            
        if (GUILayout.Button("Click Change Animation")) {
        
        tar.ChangeAnimation();
        }

        if (GUILayout.Button("Click Stop Animation"))
        {

            tar.CloseCharaAni();
        }
        serializedObject.ApplyModifiedProperties();
    }


}
#endif