#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using Fungus;
using Unity.VisualScripting;
using System;

public class EffectTestToParticle : MonoBehaviour
{

    public ParticleSystem particle;
    public float setAlpha=0;
    public float duration=0;
    public int calcCount;

 

    public void SetParticleEffect()
    {

          var particleList=particle.GetComponentsInChildren<ParticleSystem>();
          Debug.Log("particleSys數量=>" + particleList.Length);
          foreach (var parEach in particleList)
          {
              var par = parEach.main;


              var parArr = new ParticleSystem.Particle[par.maxParticles];

              int num = parEach.GetParticles(parArr);

              for (int i = 0; i < num; i++)
              {
                  int a = i;
                  StartCoroutine(SetAlpha(parArr[a], setAlpha, duration, () => { parEach.SetParticles(parArr, num);  } ));
              }

          }

    }

    private IEnumerator SetAlpha(ParticleSystem.Particle particle,float endValue,float dur, Action update )
    {
        float subColorValue = ((endValue-particle.startColor.a) / dur)*Time.fixedDeltaTime ;
        float calcTime = dur;
        float calcAlpha = particle.startColor.a;

        particle.startColor =Color.black; 

        Debug.Log("要減=>"+ ((endValue - particle.startColor.a) / dur) +"時間delt=>"+ Time.fixedDeltaTime);
        Debug.Log("透明度=>"+calcAlpha+"減的數字=>"+subColorValue);
        int count = 0;
        while (calcTime>0) {
            calcAlpha +=subColorValue;
            count++;
            Debug.Log("剩餘透明度=>" + calcAlpha);
            Debug.Log("統計次數=>"+count);
            particle.startColor = new Color(particle.startColor.r, particle.startColor.g, particle.startColor.b, calcAlpha);
            calcTime -= Time.deltaTime;
            update();
            yield return null;
        }
        yield return null;
    }



}

[CustomEditor(typeof(EffectTestToParticle))]
public class EffectTestToParticleEditor : Editor
{

    EffectTestToParticle tar;
    private SerializedProperty particleProp;
    private SerializedProperty alphaProp;
    private SerializedProperty durationProp;

    public void OnEnable()
    {
        tar= target as EffectTestToParticle;
        particleProp = serializedObject.FindProperty("particle");
        alphaProp = serializedObject.FindProperty("setAlpha");
        durationProp = serializedObject.FindProperty("duration");
    }

    public override void OnInspectorGUI()
    {

        EditorGUILayout.PropertyField(particleProp);
        EditorGUILayout.PropertyField(alphaProp);
        EditorGUILayout.PropertyField(durationProp);
        if (GUILayout.Button("粒子系統")) {

            tar.SetParticleEffect();
        }

        serializedObject.ApplyModifiedProperties();
    }



}
#endif