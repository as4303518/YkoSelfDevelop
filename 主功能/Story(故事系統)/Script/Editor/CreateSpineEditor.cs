using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Fungus.EditorUtils
{

    [CustomEditor(typeof( CreateSpine))]
    public class CreateSpineEditor : CommandEditor
    {

        CreateSpine tar;


        private SerializedProperty skeletonGraphicProp;
        private SerializedProperty effectTypeProp;
        private SerializedProperty aAnimationProp;
        private SerializedProperty aFinishDefaultAnimation;
        private SerializedProperty spineOrderProp;
        private SerializedProperty sizeProp;
        private SerializedProperty loopProp;
        private SerializedProperty waitAnimationFinishProp;
        private SerializedProperty durationProp;
        private SerializedProperty cgBreakPointProp;

        public override void OnEnable()
        {
            tar= (CreateSpine)target;
            skeletonGraphicProp = serializedObject.FindProperty("skeletonGraphic");
            cgBreakPointProp= serializedObject.FindProperty("cgBreakPoint");
            effectTypeProp = serializedObject.FindProperty("effectType");
            aAnimationProp= serializedObject.FindProperty("aAnimation");
            aFinishDefaultAnimation= serializedObject.FindProperty("aFinishDefaultAnimation");
            spineOrderProp= serializedObject.FindProperty("spineOrder");
            sizeProp= serializedObject.FindProperty("size");
            loopProp= serializedObject.FindProperty("loop");
            waitAnimationFinishProp= serializedObject.FindProperty("waitAnimationFinish");
            durationProp= serializedObject.FindProperty("duration");
        }
        public override void DrawCommandGUI()
        {

            EditorGUILayout.PropertyField(skeletonGraphicProp);

            if (tar.mSkeleGraphic!=null) {

                EditorGUILayout.PropertyField(effectTypeProp);
                EditorGUILayout.PropertyField(cgBreakPointProp);
                if (tar.mEffectType != CreateSpine.EffectType.FadeOut)
                {
                    StringField(aAnimationProp,
                       new GUIContent("Animation"),
                       new GUIContent("None"),
                        tar.mSkeleGraphic.GetSkeletonStrings());

                    StringField(aFinishDefaultAnimation,
                       new GUIContent("a Finish Animation"),
                       new GUIContent("None"),
                       tar.mSkeleGraphic.GetSkeletonStrings());
                }

                if (tar.mEffectType!=CreateSpine.EffectType.None) 
                {
                    EditorGUILayout.PropertyField(durationProp);
                }

                if (tar.mEffectType != CreateSpine.EffectType.FadeOut )
                {
                    EditorGUILayout.PropertyField(spineOrderProp);
                    EditorGUILayout.PropertyField(sizeProp);

                    if (string.IsNullOrEmpty( tar.FinishDefaultAnimation)) {
                        EditorGUILayout.PropertyField(loopProp);
                    }

                    EditorGUILayout.PropertyField(waitAnimationFinishProp);
                }

            }

            serializedObject.ApplyModifiedProperties();


        }


    }

}
