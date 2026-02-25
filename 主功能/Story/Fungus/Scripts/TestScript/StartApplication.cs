#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

public class StartApplication : MonoBehaviour
{


     [MenuItem("Tools/StartMain", false, 1)]
  //  [RuntimeInitializeOnLoadMethod]
    static void StartMainScene()
    {
        /*   if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("Boot"))
           {
               EditorSceneManager.OpenScene("Assets/Application/Boot/Scenes/BootScene.unity");
           }*/
        Debug.Log("這裡有執行");
        EditorSceneManager.OpenScene("Assets/Application/Boot/Scenes/BootScene.unity");
        //EditorApplication.ExecuteMenuItem("Edit/Play");

        // EditorApplication.isPlaying = true;
    }




    [MenuItem("Tools/StartMain", true, 1)]
    static bool ValidStartMainScene()
    {
        return !Application.isPlaying;
    }
}
#endif
