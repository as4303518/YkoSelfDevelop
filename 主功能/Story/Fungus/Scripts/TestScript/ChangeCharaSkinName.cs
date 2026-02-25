
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fungus;
using UnityEngine.UI;
using YKO.Support.Expansion;
using System.IO;
using UnityEngine.AddressableAssets;
using YKO.Mypage;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;


public class ChangeCharaSkinName : MonoBehaviour
{

    public string filePath;

    public GameObject[] targets;


    public void ExecuteWork()
    {
        //ModificationPrefabSetting();
        //ModificationPrefabParam();
      //  CreateFlowchartPos();
        ModificationCGSetting();
        //Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutineOwnerless(Test());
        //  Test();
    }

    /// <summary>
    /// 修改prefab的配置(新增物件,更動子母物件狀況,而非只修改參數)
    /// </summary>
    public void ModificationPrefabSetting()
    {
        targets = Resources.LoadAll<GameObject>(FungusResourcesPath.StoryFlowchartPrefabsPath);
        foreach (var target in targets)
        {

            if (target.GetComponent<Flowchart>().mStage.safeMask == null)
            {
                var prePath = AssetDatabase.GetAssetPath(target);
                var preTar = PrefabUtility.InstantiatePrefab(target) as GameObject;
                var flow = preTar.GetComponent<Flowchart>();
                var tarCanvas = flow.mStage.transform.Find("Canvas");
                var sp = new GameObject("SafeMask", typeof(Image), typeof(Canvas), typeof(CanvasGroup));

                sp.GetComponent<Image>().color = Color.black;

                var can = sp.GetComponent<Canvas>();
                can.overrideSorting = true;
                can.sortingLayerName = "Default";
                can.sortingOrder = 8;

                sp.SetCanvasGroup(0);

                sp.transform.SetParent(tarCanvas);
                flow.mStage.safeMask = sp;
                sp.SetLayer("StoryContent");
                sp.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(preTar, prePath);
                DestroyImmediate(preTar);
                Debug.Log("附值");
            }
            //flow.mStage.GetEffectImage.GetComponent<RectTransform>().sizeDelta = new Vector2(2000, 2000);
            EditorUtility.SetDirty(target);

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 修改prefab的參數(新增物件,更動子母物件狀況,而非只修改參數)
    /// </summary>
    public void ModificationPrefabParam()
    {
        ModificationCommandImageOfPath();
    }
    /// <summary>
    /// 修改安全模式顯示尺寸
    /// </summary>
    private  void ModificationSafeRecttransform()
    {
        foreach (var target in targets)
        {
            var flow = target.GetComponent<Flowchart>();
            var rect = flow.mStage.safeMask.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = new Vector2(2500, 2500);

            EditorUtility.SetDirty(target);

        }

    }
    /// <summary>
    /// 修改(Command) Set UI Image為Set UI Image Of Path
    /// </summary>
    private void ModificationSetUIImageToConvertPathVersion() {

        foreach (var target in targets)
        {
            var prePath = AssetDatabase.GetAssetPath(target);
            var preTar = PrefabUtility.InstantiatePrefab(target) as GameObject;
            var flow = preTar.GetComponent<Flowchart>();
            var blocks = flow.GetComponents<Block>();
            foreach (var block in blocks)
            {
                List<Command> comList = new List<Command>();
                foreach (var com in block.CommandList)
                {
                    if (com == null)
                    {
                        continue;
                    }
                    if (com.GetType() == typeof(SetUIImage))
                    {
                        var orCom = com as SetUIImage;
                        var newCom = block.AddComponent<SetUIImageOfPath>();
                        newCom.images = orCom.images;
                        newCom.effectDuration = orCom.effectDuration;
                        newCom.effectType = orCom.effectType;
                        newCom.waitUntilFinished = orCom.waitUntilFinished;
                        if (orCom.sprite != null)
                        {
                            newCom.path = AssetDatabase.GetAssetPath(orCom.sprite);
                        }
                        else
                        {
                            newCom.path = "";
                        }
                        //    block.CommandList.Insert(block.CommandList.IndexOf(com), newCom);
                        comList.Add(newCom);
                    }
                    else
                    {
                        comList.Add(com);
                    }
                }
                block.CommandList.Clear();
                block.CommandList.AddRange(comList);
            }
            PrefabUtility.SaveAsPrefabAsset(preTar, prePath);
            DestroyImmediate(preTar);
            EditorUtility.SetDirty(target);
        }





    }
    /// <summary>
    /// 修改Set UI Image Of Path的Path判定
    /// </summary>
    private void ModificationCommandImageOfPath()
    {
        targets = Resources.LoadAll<GameObject>(FungusResourcesPath.StoryFlowchartPrefabsPath);

        foreach (var target in targets)
        {
            var tarComs = target.GetComponents<SetUIImageOfPath>();
            foreach (var com in tarComs)
            {
                if (!string.IsNullOrWhiteSpace(com.path))
                {
                    Debug.Log("變換成=>"+ com.path.TrimStart(FungusResourcesPath.AddressPath + "Image/"));
                    com.path = com.path.TrimStart(FungusResourcesPath.AddressPath + "Image/");
                }
            }
            EditorUtility.SetDirty(target);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
    /// <summary>
    /// 位置生成與重新判斷上下高度的詳細靜態資訊
    /// vector為 top bottom
    /// </summary>
    private Dictionary<string ,Vector2> createPosInfo=
    new Dictionary<string, Vector2>() 
     {

        { "Middle 0.9",new Vector2(-220,220)},
        { "Middle 1",new Vector2(-100,100)},
        { "Middle 1.1",new Vector2(150,-150)},
        { "Middle 1.2",new Vector2(350,-350)},
        { "Middle 1.3",new Vector2(550,-550)},
        { "Middle 1.4",new Vector2(750,-750)},
        { "Middle 1.5",new Vector2(950,-950)},
        { "Middle 1.8",new Vector2(1650,-1650)},
        { "Left",new Vector2(-100,100)},
        { "Left 1.3",new Vector2(550,-550)},
        { "Left 1.5",new Vector2(950,-950)},
        { "Left 1.8",new Vector2(1650,-1650)},
        { "Right",new Vector2(-100,100)},
        { "Right 1.3",new Vector2(550,-550)},
        { "Right 1.5",new Vector2(950,-950)},
        { "Right 1.8",new Vector2(1650,-1650)},
    };

    public GameObject alienWidthCanvas = null;

    /// <summary>
    /// 新增flowchart pos  : Left 1.3 ...etc
    /// </summary>
    private void CreateFlowchartPos()
    {
        Debug.Log("開始生成");
        //需要先從名字判斷是否有該pos
        //有就抓  沒就生成 並且更改名字
        targets = Resources.LoadAll<GameObject>(FungusResourcesPath.StoryFlowchartPrefabsPath);
        foreach (var target in targets)
        {
            var prePath = AssetDatabase.GetAssetPath(target);
            var preTar = PrefabUtility.InstantiatePrefab(target) as GameObject;
            var flow = preTar.GetComponent<Flowchart>();

            //新增對齊寬的canvas
            
            /*var canvas=flow.mStage.GetType().GetField("alienWidthCanvas", ExportData.DefaultBindingFlags).GetValue(flow.mStage);
            var sdCg = (canvas as Canvas).gameObject.transform.Find("SD CG");
            var panel = sdCg.Find("SD CG panel ");
            panel.name = "SD CG panel";
           panel.GetComponent<Image>().raycastTarget=false;
           sdCg.Find("SD CG image").GetComponent<Image>().raycastTarget = false;*/


               int indexPos = 0;//pos在母物件上的排序順位
               foreach (var pos in createPosInfo)
               {
                   RectTransform tarPos = null;
                   for (int i=0;i<flow.mStage.PositionsParent.childCount;i++)
                   {
                       var child = flow.mStage.PositionsParent.GetChild(i);
                       if (child.name==pos.Key) 
                       {
                          tarPos=child.GetComponent<RectTransform>();
                        tarPos.anchorMin = new Vector2(0.5f, 0);
                        tarPos.anchorMax = new Vector2(0.5f, 1);
                        tarPos.pivot = new Vector2(0.5f, 0);
                        tarPos.offsetMax = new Vector2(tarPos.offsetMax.x, -pos.Value.x);//top
                        tarPos.offsetMin = new Vector2(tarPos.offsetMin.x, pos.Value.y);//bottom
                        if (pos.Key.StartsWith("Left"))
                        {
                            tarPos.localPosition = new Vector3(-250, tarPos.localPosition.y, 0);
                        }
                        if (pos.Key.StartsWith("Right"))
                        {
                            tarPos.localPosition = new Vector3(250, tarPos.localPosition.y, 0);
                        }
                        if (pos.Key.StartsWith("Middle"))
                        {
                            tarPos.localPosition = new Vector3(0, tarPos.localPosition.y, 0);
                        }

                    }
                   }
                   if (tarPos==null) 
                   { 
                       tarPos=new GameObject(pos.Key,typeof(RectTransform),typeof(CanvasRenderer)).GetComponent<RectTransform>();
                       tarPos.SetParent(flow.mStage.PositionsParent);
                       tarPos.localScale = Vector3.one;
                       tarPos.anchorMin = new Vector2(0.5f, 0);
                       tarPos.anchorMax = new Vector2(0.5f, 1);
                       tarPos.pivot = new Vector2(0.5f, 0);
                       tarPos.offsetMax = new Vector2(tarPos.offsetMax.x, -pos.Value.x);//top
                       tarPos.offsetMin = new Vector2(tarPos.offsetMin.x, pos.Value.y);//bottom

                       if (pos.Key.StartsWith("Left")) 
                       {
                           tarPos.localPosition = new Vector3(-250, tarPos.localPosition.y, 0);
                       }
                       if (pos.Key.StartsWith("Right"))
                       {
                           tarPos.localPosition = new Vector3(250, tarPos.localPosition.y, 0);
                       }
                       if (pos.Key.StartsWith("Middle"))
                       {
                           tarPos.localPosition = new Vector3(0, tarPos.localPosition.y, 0);
                       }

                       tarPos.sizeDelta = new Vector2(400,tarPos.sizeDelta.y);
                       //生成

                   }
                   tarPos.SetSiblingIndex(indexPos);
                   indexPos++;
                   //設定tarpos
               }
            PrefabUtility.SaveAsPrefabAsset(preTar, prePath);
            DestroyImmediate(preTar);
            EditorUtility.SetDirty(target);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


    }

    private void ModificationCGSetting()
    {
        targets = Resources.LoadAll<GameObject>(FungusResourcesPath.StoryFlowchartPrefabsPath);
        foreach (var target in targets)
        {
            var prePath = AssetDatabase.GetAssetPath(target);
            var preTar = PrefabUtility.InstantiatePrefab(target) as GameObject;
            var flow = preTar.GetComponent<Flowchart>();
            var canvas = (flow.mStage.GetType().GetField("alienWidthCanvas", ExportData.DefaultBindingFlags).GetValue(flow.mStage) as Canvas) ;

            var cgImg=canvas.transform.Find("SD CG").Find("SD CG image");
            cgImg.GetComponent<Image>().color = Color.white;
            cgImg.GetComponent<RectTransform>().sizeDelta = new Vector2(1080,844);
            DestroyImmediate(canvas.transform.Find("SD CG").Find("SD CG panel").gameObject);

            PrefabUtility.SaveAsPrefabAsset(preTar, prePath);
            DestroyImmediate(preTar);
            EditorUtility.SetDirty(target);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();




    }


    public string[] sprPathList;
    public string tarName = "";
    public Sprite tarSprite = null;
    public IEnumerator Test()
    {
        Debug.Log("執行");
        var res = Addressables.LoadResourceLocationsAsync(tarName);
        yield return res;
        List<string> paths = new List<string>();
        foreach (var path in res.Result)
        {
            
            paths.Add(path.PrimaryKey);

                var load=Addressables.LoadAssetAsync<Sprite>(path.PrimaryKey);
                yield return load;
                tarSprite = load.Result;
            
        }

        sprPathList = paths.ToArray();
    }
    /*
    public void Test()
    {

        var guids= AssetDatabase.FindAssets(tarName, new[] { FungusResourcesPath.AddressPath+"Image" });

        List<string> paths= new List<string>();

        foreach (var guid in guids) 
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            paths.Add(path);
            var pathTempSplit = path.Split(".")[0].Split("/");// ex: Assets/Application/Story/Fungus/Resources/Image/CG/sdcg0011.png
            if (pathTempSplit[(pathTempSplit.Length-1)].Equals(tarName))
            { 
                 tarSprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }

        sprPathList = paths.ToArray();
    }*/

    /* [MenuItem("Tools/Fungus/PrefabsVaule")]
     public static void ExecuteWork()
     {
         var filePath = "FlowchartPrefab";
         var res= Resources.LoadAll<GameObject>(filePath);

         Debug.Log("找到物件數量=>"+res.Length);

         foreach (var obj in res)
         {
             var flow = obj.GetComponent<Flowchart>();
             var spineAni=flow.GetComponents<SpineCharaAni>();
             Debug.Log("目前處理=>"+flow.DataName);
             foreach (var ani in spineAni)
             {
                 var charaAllSkinName = ani.aTarget.aSkeletonGraphic.GetSkinStrings();
                 if (charaAllSkinName.Contains("lv1"))
                 {
                     ani.aInitialSkinName = "lv1";
                 }
                 else
                 {
                     ani.aInitialSkinName = "default";
                 }

                EditorUtility.SetDirty(obj);
            }
         }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("測試執行");

     }*/








}
[CustomEditor(typeof(ChangeCharaSkinName))]
public class ChangeCharaSkinNameEditor : Editor
{
    private ChangeCharaSkinName tar;
    public void OnEnable()
    {
        tar = target as ChangeCharaSkinName;
    }
    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        if (GUILayout.Button("start"))
        {
            tar.ExecuteWork();

        }


    }










}
#endif