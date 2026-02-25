using Fungus.EditorUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fungus;
using PlasticGui.Configuration;

[CustomEditor(typeof(FungusShaderEffect))]
public class FungusShaderEffectEditor : CommandEditor
{

    private FungusShaderEffect tar;

    private SerializedProperty effectTypeProp;
    private SerializedProperty easeProp;
    private SerializedProperty fadeDurationProp;

    private SerializedProperty strengthProp;
    private SerializedProperty targetColorProp;
    private SerializedProperty scanningLineCountProp;
    private SerializedProperty scanningLineSpeedProp;
    private SerializedProperty scanningLineStrengthProp;

    private SerializedProperty canSetStartValueProp;
    private SerializedProperty canSetStartColorProp;

    private SerializedProperty startValueProp;
    private SerializedProperty startColorProp;

    private SerializedProperty closeObjProp;


    public override void OnEnable()
    {
        tar=target as FungusShaderEffect;

        effectTypeProp=serializedObject.FindProperty("effectType");
        easeProp = serializedObject.FindProperty("ease");
        fadeDurationProp = serializedObject.FindProperty("fadeDuration");

        strengthProp = serializedObject.FindProperty("strength");
        targetColorProp = serializedObject.FindProperty("targetColor");
        scanningLineCountProp = serializedObject.FindProperty("scanningLineCount");
        scanningLineSpeedProp = serializedObject.FindProperty("scanningLineSpeed");
        scanningLineStrengthProp = serializedObject.FindProperty("scanningLineStrength");

        canSetStartValueProp = serializedObject.FindProperty("canSetStartValue");
        canSetStartColorProp = serializedObject.FindProperty("canSetStartColor");

        startValueProp = serializedObject.FindProperty("startValue");
        startColorProp = serializedObject.FindProperty("startColor");

        closeObjProp = serializedObject.FindProperty("closeObj");

    }

    public override void DrawCommandGUI()
    {
        EditorGUILayout.PropertyField(effectTypeProp);
        EditorGUILayout.PropertyField(easeProp);
        EditorGUILayout.PropertyField(fadeDurationProp);

        switch (tar.effectType) {
            case FungusShaderEffect.ShaderEffectType.GaussBlur:
                EditorGUILayout.LabelField("StrengthSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(strengthProp);
                EditorGUILayout.PropertyField(canSetStartValueProp);

                if (tar.canSetStartValue) 
                {
                    EditorGUILayout.PropertyField(startValueProp);
                }

                break;
            case FungusShaderEffect.ShaderEffectType.AdjustColor:
                EditorGUILayout.LabelField("ColorSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(targetColorProp);
                EditorGUILayout.PropertyField(canSetStartColorProp);

                if (tar.canSetStartColor)
                {
                    EditorGUILayout.PropertyField(startColorProp);
                }
                DisplayColorWheel();
                break;
            case FungusShaderEffect.ShaderEffectType.BlurAndAdjustColor:
                EditorGUILayout.LabelField("StrengthSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(strengthProp);
                EditorGUILayout.PropertyField(canSetStartValueProp);
                if (tar.canSetStartValue)
                {
                    EditorGUILayout.PropertyField(startValueProp);
                }

                EditorGUILayout.LabelField("ColorSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(targetColorProp);
                EditorGUILayout.PropertyField(canSetStartColorProp);

                if (tar.canSetStartColor)
                {
                    EditorGUILayout.PropertyField(startColorProp);
                }
                DisplayColorWheel();
                break;
            case FungusShaderEffect.ShaderEffectType.Retro:
                EditorGUILayout.LabelField("ColorSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(targetColorProp);
                EditorGUILayout.PropertyField(canSetStartColorProp);

                if (tar.canSetStartColor)
                {
                    EditorGUILayout.PropertyField(startColorProp);
                }
                DisplayColorWheel();
                DisplayScanningNoise();
                break;
            case FungusShaderEffect.ShaderEffectType.RetroAndBlur:
                EditorGUILayout.LabelField("StrengthSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(strengthProp);
                EditorGUILayout.PropertyField(canSetStartValueProp);


                if (tar.canSetStartValue)
                {
                    EditorGUILayout.PropertyField(startValueProp);
                }

                EditorGUILayout.LabelField("ColorSetting", TitleStyle, GUILayout.Height(30));
                EditorGUILayout.PropertyField(targetColorProp);
                EditorGUILayout.PropertyField(canSetStartColorProp);


                if (tar.canSetStartColor)
                {
                    EditorGUILayout.PropertyField(startColorProp);
                }
                DisplayColorWheel();
                DisplayScanningNoise();
                break;

        }
        
        EditorGUILayout.PropertyField(closeObjProp);

        serializedObject.ApplyModifiedProperties();


    }
    /// <summary>
    /// 開啟特效色盤
    /// </summary>
    private void DisplayColorWheel()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.ColorField(new GUIContent(""), new Color(1, 0.9f, 0.7f, 1), false, true, false, GUILayout.Width(18));
        EditorGUILayout.ColorField(new GUIContent(""), Color.white, false, true, false, GUILayout.Width(18));

        Flowchart flow=tar.ParentBlock.GetFlowchart();

        foreach (var color in flow.ShaderColorWheel) {

            EditorGUILayout.ColorField(new GUIContent(""), color, false, true, false, GUILayout.Width(18));
        }

        EditorGUILayout.EndHorizontal();
    }
    private void DisplayScanningNoise()
    {
        EditorGUILayout.LabelField("ScanningNoiseSetting", TitleStyle, GUILayout.Height(30));
        EditorGUILayout.PropertyField(scanningLineCountProp);
        EditorGUILayout.PropertyField(scanningLineSpeedProp);
        EditorGUILayout.PropertyField(scanningLineStrengthProp);

    }


}
