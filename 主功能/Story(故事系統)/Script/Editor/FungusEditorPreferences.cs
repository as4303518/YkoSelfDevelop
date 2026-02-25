// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fungus
{
    namespace EditorUtils
    {
        /// <summary>
        /// Shows Fungus section in the Edit->Preferences in unity allows you to configure Fungus behaviour
        /// 
        /// ref https://docs.unity3d.com/ScriptReference/PreferenceItem.html
        /// </summary>
        [InitializeOnLoad]
        public static class FungusEditorPreferences//在project設定可以看到fungus選項
        {
            // Have we loaded the prefs yet
            private static bool prefsLoaded = false;
            private const string HIDE_MUSH_KEY = "hideMushroomInHierarchy";
            private const string USE_LEGACY_MENUS = "useLegacyMenus";
            private const string USE_GRID_SNAP = "useGridSnap";

            public static bool hideMushroomInHierarchy;
            public static bool useLegacyMenus;
            public static bool useGridSnap;

            static FungusEditorPreferences()
            {
                LoadOnScriptLoad();
            }

#if UNITY_2019_1_OR_NEWER
            [SettingsProvider]//生成fungus在projectsetting裡的選項
            public static SettingsProvider CreateFungusSettingsProvider()
            {
                // First parameter is the path in the Settings window.
                // Second parameter is the scope of this setting: it only appears in the Project Settings window.
                var provider = new SettingsProvider("Project/Fungus", SettingsScope.Project) //將fungus選項加入project Setting
                {
                    // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                    guiHandler = (searchContext) => PreferencesGUI()

                    // // Populate the search keywords to enable smart search filtering and label highlighting:
                    // keywords = new HashSet<string>(new[] { "Number", "Some String" })
                };

                return provider;
            }

#else
            [PreferenceItem("Fungus")]
#endif
            private static void PreferencesGUI()//*在編輯器上紀錄數值
            {
                // Load the preferences
                if (!prefsLoaded)
                {
                    LoadOnScriptLoad();
                }

                // Preferences GUI
                hideMushroomInHierarchy = EditorGUILayout.Toggle("Hide Mushroom Flowchart Icon", hideMushroomInHierarchy);
                useLegacyMenus = EditorGUILayout.Toggle(new GUIContent("Legacy Menus", "Force Legacy menus for Event, Add Variable and Add Command menus"), useLegacyMenus);
                useGridSnap = EditorGUILayout.Toggle(new GUIContent("Grid Snap", "Align and Snap block positions and widths in the flowchart window to the grid"), useGridSnap);

                EditorGUILayout.Space();
                //ideally if any are null, but typically it is all or nothing that have broken links due to version changes or moving files external to Unity
                if (FungusEditorResources.Add == null)
                {
                    EditorGUILayout.HelpBox("FungusEditorResources need to be regenerated!", MessageType.Error);
                }

                //Fungus 圖像資源
                if (GUILayout.Button(new GUIContent("Select Fungus Editor Resources SO", "If Fungus icons are not showing correctly you may need to reassign the references in the FungusEditorResources. Button below will locate it.")))
                {
                    var ids = AssetDatabase.FindAssets("t:FungusEditorResources");//*獲取組件
                    if (ids.Length > 0)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(ids[0]);
                        var asset = AssetDatabase.LoadAssetAtPath<FungusEditorResources>(p);
                        Selection.activeObject = asset;//*自動把你的選擇定格在該物件
                    }
                    else
                    {
                        Debug.LogError("No FungusEditorResources found!");
                    }
                }

                if (GUILayout.Button("Open Changelog (version info)"))
                {
                    //From project path down, look for our Fungus\Docs\ChangeLog.txt
                    var projectPath = System.IO.Directory.GetParent(Application.dataPath);
                    var fileMacthes = System.IO.Directory.GetFiles(projectPath.FullName, "CHANGELOG.txt", System.IO.SearchOption.AllDirectories);

                    fileMacthes = fileMacthes.Where((x) =>
                    {
                        var fileFolder = System.IO.Directory.GetParent(x);
                        return fileFolder.Name == "Docs" && fileFolder.Parent.Name == "Fungus";
                    }).ToArray();

                    if (fileMacthes == null || fileMacthes.Length == 0)
                    {
                        Debug.LogWarning("Cannot locate Fungus\\Docs\\CHANGELONG.txt");
                    }
                    else
                    {
                        Application.OpenURL(fileMacthes[0]);
                    }
                }

                // Save the preferences
                if (GUI.changed)//將數值儲存在editor(編輯器)裡面,跟playerPrefs有異曲同工之妙
                {
                    EditorPrefs.SetBool(HIDE_MUSH_KEY, hideMushroomInHierarchy);
                    EditorPrefs.SetBool(USE_LEGACY_MENUS, useLegacyMenus);
                    EditorPrefs.SetBool(USE_GRID_SNAP, useGridSnap);
                }
            }

            public static void LoadOnScriptLoad()//將editor(編輯器)裡面的數值取出,跟playerPrefs有異曲同工之妙
            {
                hideMushroomInHierarchy = EditorPrefs.GetBool(HIDE_MUSH_KEY, false);
                useLegacyMenus = EditorPrefs.GetBool(USE_LEGACY_MENUS, false);
                useGridSnap = EditorPrefs.GetBool(USE_GRID_SNAP, false);
                prefsLoaded = true;
            }
        }
    }
}