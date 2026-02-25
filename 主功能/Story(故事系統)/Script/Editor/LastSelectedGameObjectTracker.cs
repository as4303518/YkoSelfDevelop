#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Fungus.EditorUtils
{
    /// <summary>
    /// 為了方便在顯示Block的選項時,不會因為物件在Hierarchy上是靜態,而抓不到下拉選項中該顯示的block
    /// 例如:SetToSns,Call,Menu等多選項的Command
    /// </summary>
    [InitializeOnLoad]
    public static class LastSelectedGameObjectTracker 
    {
       public static GameObject lastSelectedObject;

        // 靜態建構函式，當編輯器載入時自動執行
        static LastSelectedGameObjectTracker()
        {
            // 監聽選擇更改事件
            Selection.selectionChanged += OnSelectionChanged;
        }
        private static void OnSelectionChanged()
        {
            // 更新最後選中的物件
            if (Selection.activeGameObject!=null)
            {
                lastSelectedObject = Selection.activeGameObject;
            }

            if (lastSelectedObject != null)
            {
               // Debug.Log("最後選中的遊戲物件是: " + lastSelectedObject.name);
            }
            else
            {

              //  Debug.Log("沒有選中的遊戲物件。");
            }

        }
    }
}
#endif