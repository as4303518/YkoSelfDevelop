// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using YKO.Common.UI;
using Spine.Unity;
using YKO.Support.Expansion;
using YKO.CONST;
using DG.Tweening;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace Fungus
{
    /// <summary>
    /// Define a set of screen positions where character sprites can be displayed and controls portraits.
    /// editor模式下可以跑
    /// </summary>
    [ExecuteInEditMode]  
    public class Stage : PortraitController  //hierarchy裡面的 script
    {
        [Tooltip("Canvas object containing the stage positions.")]
        [SerializeField] protected Canvas portraitCanvas;//alien expand 

        /// <summary>
        /// 用於模糊特效使用  因為模糊特效會常態性搭配黑白特效或復古特效等,如果都在同一個圖層下渲染效果會疊加,
        /// 並且UI Layer顯示只認Canvas組件的Layer,所以必須另外開一個Canvas去渲染特效
        /// </summary>
        [SerializeField] protected Canvas effectCanvas;//alien expand 
        /// <summary>
        /// 專門對齊寬的Canvas
        /// </summary>
        [SerializeField] protected Canvas alienWidthCanvas;//alien expand 

        /// <summary>
        /// 裝故事彈窗的Canvas(對其擴展,而非高
        /// </summary>
        [SerializeField] protected Canvas popupCanvas;//alien expand 

        public Canvas PopupCanvas { set { popupCanvas = value; } get { return popupCanvas; } }

        public RawImage GetEffectImage;

        public GameObject safeMask;
        /// <summary>
        /// 透過flowchart useSafeMode來判斷安全模式開關 而不是透過設定判斷的 scene
        /// </summary>
        private string[] useFlowchartSwitchSafeScene=new string[]
        { 
        "DevelopVersion",
        "FungusStory"
        };

        public Transform CharaParent = null;


        protected Transform audiosParent = null;

        public Transform AudiosParent
        {
            get
            {
                if (!audiosParent)
                {
                    SetAudiosParent();
                }
                return audiosParent;
            }
        }

        public Transform PositionsParent = null;

        public Transform ViewParent = null;

        public Transform LastBackGroundParent = null;

        public Transform BackGroundParent = null;

        public Transform ForeImageParent = null;

        public Transform ForeEffectParent = null;
        /// <summary>
        /// fade的母物件
        /// </summary>
        public Transform CharaRendererParent = null;

        private GameObject tipPopupPrefab = null;

        private GameObject tipPopup = null;

        [Tooltip("Dim portraits when a character is not speaking.")]
        [SerializeField] protected bool dimPortraits;

        [Tooltip("Choose a dimColor")]
        [SerializeField] protected Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("Duration for fading character portraits in / out.")]
        [SerializeField] protected float fadeDuration = 0.5f;

        [Tooltip("Duration for moving characters to a new position")]
        [SerializeField] protected float moveDuration = 1f;

        [Tooltip("Ease type for the fade tween.")]
        [SerializeField] protected LeanTweenType fadeEaseType;

        [Tooltip("Constant offset to apply to portrait position.")]
        [SerializeField] protected Vector2 shiftOffset;

        [Tooltip("The position object where characters appear by default.")]
        [SerializeField] protected RectTransform defaultPosition;

        [Tooltip("List of stage position rect transforms in the stage.")]
        [SerializeField] protected List<RectTransform> positions;
        [Tooltip("List of stage position rect transforms in the stage.")]
        [SerializeField] protected List<string> RecordActiveHCGOfPath = new List<string>();
        //protected List<CharaRenderer> spineCharaOnStageList = new List<CharaRenderer>();


        protected static List<Stage> activeStages = new List<Stage>();

        private bool exe = true;

        private Flowchart mFlowchart = null;

        public Flowchart MyFlowChart { get { return mFlowchart; } set { mFlowchart = value; } }

        protected  void OnEnable()
        {
            
            if (!ConfirmParentComponent()) {
                return;
            }

            if (gameObject.name == "_CommandCopyBuffer"|| transform.parent.name == "_CommandCopyBuffer")
            {
                return;
            }

            if (!activeStages.Contains(this))
            {
                activeStages.Add(this);
            }
            if (portraitCanvas==null) {
                if (transform.Find("Canvas").GetComponent<Canvas>()) {
                    portraitCanvas = transform.Find("Canvas").GetComponent<Canvas>();
                }
                else
                {
                    CreateCanavas();
                }

            }
            

#if UNITY_EDITOR
            

            EditorCoroutineUtility.StartCoroutineOwnerless(SetCnavasCamera(portraitCanvas,true));
            EditorCoroutineUtility.StartCoroutineOwnerless(SetCnavasCamera(effectCanvas));
            EditorCoroutineUtility.StartCoroutineOwnerless(SetCnavasCamera(alienWidthCanvas, setCameraScalerValue: 0));
            EditorCoroutineUtility.StartCoroutineOwnerless(DetectPosStatus());
#endif
        }


        public IEnumerator DetectPosStatus()
        {
            while (exe) {
                if (PositionsParent.childCount!=positions.Count) {
                    positions.Clear();

                    for (int i=0;i<PositionsParent.childCount;i++) {
                          var pos = PositionsParent.GetChild(i);

                        RectTransform getRect = null;

                        if (pos.TryGetComponent<RectTransform>(out getRect)) {
                            positions.Add(getRect);
                        }
                    
                    }
                
                }

                yield return null;
            }
        }
        /// <summary>
        /// 獲得 stage 位置
        /// </summary>
        /// <returns></returns>
        public List<RectTransform> GetPositions() {

            List<RectTransform> rectList = new List<RectTransform>();
            if (PositionsParent.childCount>0)
            {
                for (int i = 0; i < PositionsParent.childCount; i++)
                {
                    var pos = PositionsParent.GetChild(i);
                    RectTransform getRect = null;
                    if (pos.TryGetComponent<RectTransform>(out getRect))
                    {
                        rectList.Add(getRect);
                    }
                }
            }
            return rectList;
        }

        protected virtual void OnDisable()
        {
            exe = false;
            StopAllCoroutines();
            activeStages.Remove(this);

        }

        public void Start()
        {
            StartCoroutine( SetCnavasCamera(portraitCanvas,true));
            StartCoroutine(SetCnavasCamera(effectCanvas));
            StartCoroutine(SetCnavasCamera(alienWidthCanvas,setCameraScalerValue:0));
            StartCoroutine( LoadPrefab());
            //CreateFadeRenderParent();
           // CreateParent("CharaRendererParent");
            if (Application.isPlaying && portraitCanvas != null)
            {
                portraitCanvas.gameObject.SetActive(true);
            }
           // StartCoroutine(SetSafeModeCanvas());
        }
        /// <summary>
        /// 因為canvas unity 設定關係 程式碼許多情況下無法設值為true 只有手動可以 所以強制設置為true 
        /// </summary>
        /// <returns></returns>
        public IEnumerator SetSafeModeCanvas()
        {
            while(true){

                safeMask.GetComponent<Canvas>().overrideSorting = true;
                yield return null;
            }
        }

        private IEnumerator LoadPrefab()
        {
            if (tipPopupPrefab == null)
            {
                var resqu = Resources.LoadAsync<GameObject>(FungusResourcesPath.PrefabsPath + "ClickTipPopup");
                yield return new WaitUntil(() => resqu.isDone);
                tipPopupPrefab = resqu.asset as GameObject;
            }
        }

        #region Public members

        /// <summary>
        /// Gets the list of active stages.
        /// </summary>
        public static List<Stage> ActiveStages { get { return activeStages; } }

        /// <summary>
        /// Returns the currently active stage.
        /// </summary>
        public static Stage GetActiveStage()
        {
            if (Stage.activeStages == null ||
                Stage.activeStages.Count == 0)
            {
                return null;
            }

            return Stage.activeStages[0];
        }

        /// <summary>
        /// Canvas object containing the stage positions.
        /// </summary>
        public virtual Canvas PortraitCanvas { get { return portraitCanvas; } }

        /// <summary>
        /// Dim portraits when a character is not speaking.
        /// </summary>
        public virtual bool DimPortraits { get { return dimPortraits; } set { dimPortraits = value; } }

        /// <summary>
        /// Choose a dimColor.
        /// </summary>
        public virtual Color DimColor { get { return dimColor; } set { dimColor = value; } }

        /// <summary>
        /// Duration for fading character portraits in / out.
        /// </summary>
        public virtual float FadeDuration { get { return fadeDuration; } set { fadeDuration = value; } }

        /// <summary>
        /// Duration for moving characters to a new position.
        /// </summary>
        public virtual float MoveDuration { get { return moveDuration; } set { moveDuration = value; } }

        /// <summary>
        /// Ease type for the fade tween.
        /// </summary>
        public virtual LeanTweenType FadeEaseType { get { return fadeEaseType; }set { fadeEaseType = value; } }

        /// <summary>
        /// Constant offset to apply to portrait position.
        /// </summary>
        public virtual Vector2 ShiftOffset { get { return shiftOffset; }set { shiftOffset = value; } }

        /// <summary>
        /// The position object where characters appear by default.
        /// </summary>
        public virtual RectTransform DefaultPosition { get { return defaultPosition; }set { defaultPosition = value; } }

        /// <summary>
        /// List of stage position rect transforms in the stage.
        /// </summary>
        public virtual List<RectTransform> Positions { get { return positions; } }
        /// <summary>
        /// spine CG 群組
        /// </summary>
        private List<SkeletonGraphic> spineSkeletonGraphicCG = new List<SkeletonGraphic>();
        /// <summary>
        /// spine CG 群組
        /// </summary>
        public List<SkeletonGraphic> SpineSkeletonGraphicCG { get { return spineSkeletonGraphicCG; } }

        //  public virtual List<CharaRenderer> SpineCharaOnStageList { get { return spineCharaOnStageList; } }
        protected List<SkeletonGraphic> spineSkeletonGraphicOnStageList = new List<SkeletonGraphic>();
        public List<SkeletonGraphic> SpineSkeletonGraphicOnStageList { get { return spineSkeletonGraphicOnStageList; } }

        /// <summary>
        /// 設置spine角色明暗(目前對話中的角色與動畫
        /// </summary>
        /// <param name="charaName"></param>
        public override void JudgeCharaSetDimmed(string charaName)
        {
            if (DimPortraits) 
            {

                foreach (var opt in spineSkeletonGraphicOnStageList) {

                    if (opt.name!=charaName) {

                        StartCoroutine(SetCharaDimmed(opt.GetComponent<SkeletonGraphic>(), true));
                    }
                    else
                    {
                        StartCoroutine(SetCharaDimmed(opt.GetComponent<SkeletonGraphic>(), false));
                    }

                }
            }

        }

        /// <summary>
        /// 尋找spine角色
        /// </summary>
        /// <param name="charaName"></param>
        /// <returns></returns>
        public SkeletonGraphic FindSkeletonGraphicInstanceByName(string charaName)
        {
            SkeletonGraphic target = null;
            foreach (var skele in spineSkeletonGraphicOnStageList) {

                if (skele.name==charaName) {
                    target = skele;
                }
            }
            return target;
        }
        /// <summary>
        /// 尋找spine CG
        /// </summary>
        /// <param name="charaName"></param>
        /// <returns></returns>
        public SkeletonGraphic FindCGInstanceByName(string charaName)
        {
            SkeletonGraphic target = null;
            foreach (var skele in spineSkeletonGraphicCG)
            {

                if (skele.name == charaName)
                {
                    target = skele;
                }
            }
            return target;
        }

        /// <summary>
        /// Searches the stage's named positions
        /// If none matches the string provided, give a warning and return a new RectTransform
        /// </summary>
        public RectTransform GetPosition(string positionString)
        {
            if (string.IsNullOrEmpty(positionString))
            {
                return null;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                if ( String.Compare(positions[i].name, positionString, true) == 0 )
                {
                    return positions[i];
                }
            }
            return null;
        }



        public View GetView(string rectName)
        {

            if (rectName.Equals(null))
            {
                return null;
            }

            for (int i=0;i<ViewParent.childCount;i++) {
                View view = ViewParent.GetChild(i).GetComponent<View>();
                if (String.Compare(view.name,rectName,true)==0) {
                    return view;
                }
            }
            return null;
        }
#if UNITY_EDITOR
        public Image GetImage(string imageName, ImageOrderParent imageOrder)
        {

            if (imageName.Equals(null)) {
                return null;
            }

            Debug.Log("獲取的母物件=>" + imageOrder.ToString());

            switch (imageOrder) {
                case ImageOrderParent.LastBackGround:
                    for (int i = 0; i < LastBackGroundParent.childCount; i++)
                    {
                        Image img = LastBackGroundParent.GetChild(i).GetComponent<Image>();
                        if (String.Compare(img.name, imageName, true) == 0)
                        {
                            return img;
                        }
                    }
                    break;
                case ImageOrderParent.BackGround:
                    for (int i = 0; i < BackGroundParent.childCount; i++)
                    {
                        Image img = BackGroundParent.GetChild(i).GetComponent<Image>();
                        if (String.Compare(img.name, imageName, true) == 0)
                        {
                            return img;
                        }
                    }
                    break;
                case ImageOrderParent.Images:
                    for (int i = 0; i < ForeImageParent.childCount; i++)
                    {
                        Image img = ForeImageParent.GetChild(i).GetComponent<Image>();
                        if (String.Compare(img.name, imageName, true) == 0)
                        {
                            return img;
                        }
                    }
                    break;
                case ImageOrderParent.ForeGround:
                    for (int i = 0; i < ForeEffectParent.childCount; i++)
                    {
                        Image img = ForeEffectParent.GetChild(i).GetComponent<Image>();
                        if (String.Compare(img.name, imageName, true) == 0)
                        {
                            return img;
                        }
                    }
                    break;
        }

            return null;

        }

#endif
        public SpriteRenderer GetSpriteRenderer(string imageName)
        {
            
            if (imageName.Equals(null))
            {
                return null;
            }
            for (int i = 0; i < ForeImageParent.childCount; i++)
            {
                SpriteRenderer sprite = ForeImageParent.GetChild(i).GetComponent<SpriteRenderer>();
                if (String.Compare(sprite.name, imageName, true) == 0)
                {
                    return sprite;
                }
            }
            return null;
        }

        public GameObject FindEffectObject(string effectName)
        {


            for (int i=0;i<ForeEffectParent.childCount;i++) 
            {

            var child=ForeEffectParent.GetChild(i);

                if (effectName==child.name)
                {
                    return child.gameObject;
                }
             }

            return transform.Find("EffectCanvas")?.gameObject;

        }

        public void CloseOtherRaycastTarget(RectTransform tarRect)
        {
            foreach (var rect in positions) 
            {
                if (rect==tarRect) 
                {
                }
                else
                {
                    if (rect.GetComponent<Image>())
                    {
                        rect.GetComponent<Image>().raycastTarget = false;
                    }
                }
            }
        }

        public void OpenAllPositionRaycastTarget()
        {

            foreach (var rect in positions)
            {
                if (rect.GetComponent<Image>()) 
                {
                    rect.GetComponent<Image>().raycastTarget = true;
                }
            }


        }

        public void ClearData()
        {

            foreach (var pos in positions) { 
            DestroyImmediate(pos.gameObject);
            }
            positions.Clear();

            List<GameObject> childs = new List<GameObject>();

            for (int i=0;i<ViewParent.childCount;i++) {
            var child = ViewParent.GetChild(i);
                childs.Add(child.gameObject);
            }

            for (int i = 0; i <LastBackGroundParent.childCount; i++)
            {
                var child = LastBackGroundParent.GetChild(i);
                childs.Add(child.gameObject);
            }

            for (int i = 0; i < BackGroundParent.childCount; i++)
            {
                var child = BackGroundParent.GetChild(i);
                childs.Add(child.gameObject);
            }
            for (int i = 0; i < ForeImageParent.childCount; i++)
            {
                var child = ForeImageParent.GetChild(i);
                childs.Add(child.gameObject);
            }
            for (int i = 0; i < ForeEffectParent.childCount; i++)
            {
                var child = ForeEffectParent.GetChild(i);
                childs.Add(child.gameObject);
            }

            for (int i = 0; i <AudiosParent.childCount; i++)
            {
                var child = AudiosParent.GetChild(i);
                childs.Add(child.gameObject);
            }
            foreach (GameObject obj in childs) {
                DestroyImmediate(obj);
            }
            childs.Clear();

        }

        public IEnumerator SetCnavasCamera(Canvas canvas ,bool setMainCmaera=false,float setCameraScalerValue=1)
        {
            if (canvas==null) 
            {
                yield break;
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            UICanvasBase.CheckIsHaveUICameraBase(canvas.gameObject,TargetCameraType.StoryCamera);


            if (canvas.worldCamera==null&&setMainCmaera) {
                if (Camera.main!=null)
                {
                    canvas.worldCamera = Camera.main;
                }
                else
                {
                    Debug.Log("main camera is null");
                }
            }
            var porCan = canvas.GetComponent<CanvasScaler>();
           porCan.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            porCan.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            porCan.matchWidthOrHeight = setCameraScalerValue;

            IEnumerator AdjustCameraMode()
            {
                yield return new WaitForSeconds(0.01f);
                canvas.renderMode = RenderMode.WorldSpace;

            }

            yield return AdjustCameraMode();

            //    LayoutRebuilder.ForceRebuildLayoutImmediate
        }
    


        public void SetAudiosParent()
        {
            if (!audiosParent)
            {
                var AudPar = gameObject.transform.parent.Find("Audios");
                if (AudPar)
                {
                    audiosParent = AudPar;
                }
                else
                {
                    GameObject sp = new GameObject("Audios");
                    sp.transform.SetParent(transform.parent);
                    sp.transform.position = Vector3.zero;
                    audiosParent = sp.transform;
                }
            }
        }
        /*/// <summary>
        /// 生成寄放角色Fade渲染模組的母物件(為了因應角色隱形時,關節圖層不會顯現
        /// 母物件會控制底下子物件的layer 即便根子物件的layer不同,會以母物件有canvas的為主
        /// </summary>
        private void CreateFadeRenderParent()
        {
            if (CharaRendererParent==null&&Application.isPlaying) 
            {
                Debug.Log("生成母物件");
                var sp = Instantiate(charaRendererParentPrefab, transform);
                sp.name = "CharaRenderParent";
                sp.transform.localPosition = Vector3.zero;
                CharaRendererParent = sp.transform;
                CharaRendererParent.GetComponent<CharaRenderer>().Init(effectCanvas.transform);
            }



            var field=GetType().GetField(parentName,ExportData.DefaultBindingFlags);


            if ((field.GetValue(this) as Transform)==null) 
            {
                if (transform.Find(parentName)!=null)
                {
                    field.SetValue(this, transform.Find(parentName));
                }
                else
                {
                    GameObject sp = new GameObject(parentName, typeof(RectTransform));
                    sp.transform.SetParent(transform);
                    sp.transform.localScale = Vector3.one;
                    sp.transform.localPosition = Vector3.zero;
                    var rect = sp.GetComponent<RectTransform>();
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.offsetMin = Vector2.zero;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    field.SetValue(this, sp.transform);
                }
            }
        }*/

        /// <summary>
        /// 增加活耀的hcg物件路徑,以此方法紀錄場上是否有hcg
        /// </summary>
        /// <param name="path"></param>
        public void AddHCGActivePath(string path)
        {
            Debug.Log("加入hcgList?=>"+path+"安全模式狀況=>" + SceneManager.GetActiveScene().name);
            RecordActiveHCGOfPath.Add(path);


            //玩家是否調用flowchart上的開關設定
            bool useFlowchartSafeModeSwitch = false;
            //是否啟用安全模式
            bool enableSafeMode = false;

            //判斷是否為測試用scene(測試用就已flowchart上的開關為主
            foreach (var sceneName in useFlowchartSwitchSafeScene) 
            {
                if (sceneName==SceneManager.GetActiveScene().name) 
                {
                    useFlowchartSafeModeSwitch = true;
                }
            }

            if (useFlowchartSafeModeSwitch) 
            {
                if (mFlowchart.useSafeMode)
                {
                    enableSafeMode = true;
                }

            }
            else
            {
                if (PlayerPrefs.GetInt(PlayerPrefsKeys.SAFE_MODE) > 0 )
                {
                    enableSafeMode = true;
                }
            }
            if (enableSafeMode)
            {
                safeMask.gameObject.SetActive(true);
                safeMask.GetComponent<Canvas>().overrideSorting = true;
                safeMask.SetCanvasGroup(1);
            }
        }
        /// <summary>
        /// 增加活耀的hcg物件路徑,以此方法紀錄場上是否有hcg
        /// </summary>
        /// <param name="path"></param>
        public void RemoveHCGActivePath(string path)
        {
            if (RecordActiveHCGOfPath.Contains(path))
            {
                RecordActiveHCGOfPath.Remove(path);
                if (RecordActiveHCGOfPath.Count <= 0)
                {

                    StartCoroutine(safeMask.eSetCanvasGroup(0, () => { safeMask.gameObject.SetActive(false); }));
                }
            }
        }

        public void CreateImageTip(Image image=null)
        {
            if (tipPopup == null)
            {
                tipPopup = Instantiate(tipPopupPrefab);
                tipPopup.transform.SetParent(ForeEffectParent, false);
                tipPopup.GetComponent<ClickNextTipPopup>().Init(ClickNextTipPopup.EffectType.Transfer,image);
            }
        }
        public void CreateTextTip(string str)
        {
            if (tipPopup == null)
            {
                tipPopup = Instantiate(tipPopupPrefab);
                tipPopup.transform.SetParent(ForeEffectParent, false);
                tipPopup.GetComponent<ClickNextTipPopup>().Init(ClickNextTipPopup.EffectType.Transfer, str);
            }
        }

        public void DestoryTip()
        {
            if (tipPopup != null)
            {
                Destroy(tipPopup);
            }
        }

        public void CreateCanavas()
        {

            GameObject sp = new GameObject("Canvas", 
                typeof(RectTransform), typeof(Canvas),typeof(GraphicRaycaster),
                typeof(CanvasScaler),typeof(CanvasGroup)
                );
            portraitCanvas=sp.GetComponent<Canvas>();
        }

        public bool ConfirmParentComponent()
        {
            if (gameObject.transform.parent==null) {
                return false;
            }
            else
             {
                Flowchart flow = null;

                if (transform.parent.TryGetComponent<Flowchart>(out flow)) {

                        if (flow.name!= "_CommandCopyBuffer") {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                }
                else
                {
                    return false;
                }
            }

        }




        #endregion
    }
}

