// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using System.IO;
using UnityEngine.Localization.Settings;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;



namespace Fungus
{

    /// <summary>
    /// Visual scripting controller for the Flowchart programming language.
    /// Flowchart objects may be edited visually using the Flowchart editor window.
    /// </summary>
    [ExecuteInEditMode]

    public class Flowchart : MonoBehaviour, ISubstitutionHandler
    {
        public const string SubstituteVariableRegexString = "{\\$.*?}";
        
        public SettingLanguage mLanguage;

        public bool useAssetText=false;

        public bool useSafeMode = false;

        /// <summary>
        /// 不透過storyScene  腳本開始故事的scene(直接將prefab放在場上即可加載
        /// </summary>
        private string[] AutoStartFlowchartStoryScene = new string[]
        {
            "DevelopVersion",
             "FungusStory"
        };

        [HideInInspector]
        [SerializeField] protected int version = 0; // Default to 0 to always trigger an update for older versions of Fungus.

        [HideInInspector]
        [SerializeField] protected Vector2 scrollPos;

        [HideInInspector]
        [SerializeField] protected Vector2 variablesScrollPos;

        [HideInInspector]
        [SerializeField] protected bool variablesExpanded = true;

        [HideInInspector]
        [SerializeField] protected float blockViewHeight = 400;

        [HideInInspector]
        [SerializeField] protected float zoom = 1f;

        [HideInInspector]
        [SerializeField] protected List<Block> selectedBlocks = new List<Block>();

        [HideInInspector]
        [SerializeField] protected List<Command> selectedCommands = new List<Command>();

        [HideInInspector]
        [SerializeField] protected List<Variable> variables = new List<Variable>();

        [SerializeField] public bool _storyEnabled = true;//

        [TextArea(3, 5)]
        [Tooltip("Description text displayed in the Flowchart editor window")]
        [SerializeField] protected string description = "";

        [SerializeField] public TextAsset flowchartOverrideTextFileToJson=null;
        [SerializeField] public TextAsset flowchartOverrideTextFileToCsv = null;

        [SerializeField] protected string dataName = "";

        public Stage mStage=null;
       
        [Range(0f, 5f)]
        [Tooltip("Adds a pause after each execution step to make it easier to visualise program flow. Editor only, has no effect in platform builds.")]
        [SerializeField] protected float stepPause = 0f;

        [Tooltip("Use command color when displaying the command list in the Fungus Editor window")]
        [SerializeField] protected bool colorCommands = true;

        [Tooltip("Hides the Flowchart block and command components in the inspector. Deselect to inspect the block and command components that make up the Flowchart.")]
        [SerializeField] protected bool hideComponents = true;

        [Tooltip("Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.")]
        [SerializeField] protected bool saveSelection = true;

        [Tooltip("Unique identifier for this flowchart in localized string keys. If no id is specified then the name of the Flowchart object will be used.")]
        [SerializeField] protected string localizationId = "";

        [Tooltip("Display line numbers in the command list in the Block inspector.")]
        [SerializeField] protected bool showLineNumbers = false;

        [Tooltip("List of commands to hide in the Add Command menu. Use this to restrict the set of commands available when editing a Flowchart.")]
        [SerializeField] protected List<string> hideCommands = new List<string>();

      //  [SerializeField] public List<string> StopSkipCommands = new List<string> { "Menu", "ShowFirstEditName" ,"StartSns","SetToSns","EndSns"};

        [Tooltip("Lua Environment to be used by default for all Execute Lua commands in this Flowchart")]
        [SerializeField] protected LuaEnvironment luaEnvironment;

        [Tooltip("The ExecuteLua command adds a global Lua variable with this name bound to the flowchart prior to executing.")]
        [SerializeField] protected string luaBindingName = "flowchart";

        [SerializeField] protected List<Color> shaderColorWheel = new List<Color>();
        public List<Color> ShaderColorWheel { get { return shaderColorWheel; } }
        public StoryControl mStoryControl = null;

        [SerializeField] protected static List<Flowchart> cachedFlowcharts = new List<Flowchart>();

        protected static bool eventSystemPresent;

        protected StringSubstituter stringSubstituer;

#if UNITY_EDITOR
        public bool SelectedCommandsStale { get; set; }
#endif

#if UNITY_5_4_OR_NEWER
#else
        protected virtual void OnLevelWasLoaded(int level) 
        {
            LevelWasLoaded();
        }
#endif

        protected virtual void LevelWasLoaded()
        {
            // Reset the flag for checking for an event system as there may not be one in the newly loaded scene.
            eventSystemPresent = false;
        }

        protected virtual void Start()
        {
            CheckStorySettingWindow();
            CheckEventSystem();
            if (Application.isEditor)
            {
                StartCoroutine(JudgeAutoLoadScene());
            }
        }
        /// <summary>
        /// 判斷是否為自動加載的scene
        /// </summary>
        /// <returns></returns>
        private IEnumerator JudgeAutoLoadScene()
        {
            bool isStartStoryScene = false;

            //判斷是否為測試用scene(測試用就已flowchart上的開關為主
            foreach (var sceneName in AutoStartFlowchartStoryScene)
            {
                if (sceneName == SceneManager.GetActiveScene().name)
                {
                    isStartStoryScene = true;
                }
            }
            if (isStartStoryScene)
            {
                yield return LoadAssetText();
                StartShowStory();
            }

        }


        // There must be an Event System in the scene for Say and Menu input to work.
        // This method will automatically instantiate one if none exists.
        protected virtual void CheckEventSystem()
        {
            _storyEnabled =true;
            if (eventSystemPresent)
            {
                return;
            }

            eventSystemPresent = true;
        }
        /// <summary>
        /// 檢查是否有故事控制器,如果沒,則生成出來
        /// </summary>
        private void CheckStorySettingWindow()
        {
            SetLanguage();
            if ( gameObject.name == "GlobalVariables") {
                return;
            }
            if (!Application.isPlaying) {
                return;
            }
            if (mStoryControl == null ) 
            {
                GameObject sp = Instantiate(Resources.Load<GameObject>("Prefabs/PenguinPrefab/StorySettingWindow"));
                sp.transform.SetParent(transform);
                sp.name = "StorySettingWindow";
                mStoryControl = sp.GetComponent<StoryControl>();
            }
            else
            {
                mStoryControl.gameObject.SetActive(true);
            }
            mStoryControl.FindCameraRender(this);

        }

        private void SetLanguage()
        {
            var type=LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            switch (type)
            {
                case 0:
                    mLanguage = SettingLanguage.TW;
                    break;
                case 1:
                    mLanguage = SettingLanguage.CN;
                    break;
                case 2:
                    mLanguage = SettingLanguage.EN;
                    break;
                case 3:
                    mLanguage = SettingLanguage.JP;
                    break;
                case 4:
                    mLanguage = SettingLanguage.KR;
                    break;
            }
        }


        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            LevelWasLoaded();
        }

        protected virtual void OnEnable()
        {
            if (gameObject.name == "_CommandCopyBuffer")
            {
                return;
            }
            if (!cachedFlowcharts.Contains(this))
            {
                cachedFlowcharts.Add(this);
                //TODO these pairs could be replaced by something static that manages all active flowcharts
#if UNITY_5_4_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
#endif
            }

            CheckItemIds();
            CleanupComponents();
            UpdateVersion();

            StringSubstituter.RegisterHandler(this);

            if (mStage==null&& gameObject.name != "GlobalVariables") 
            {
                Stage stage=null;
                
                bool HaveStage = false;
                if (gameObject.transform.Find("Stage"))
                {
                    if (gameObject.transform.Find("Stage").TryGetComponent<Stage>(out stage))
                    {
                        HaveStage = true;
                        mStage = stage;
                    }
                }

                IEnumerator CreateStage  (Action<ResourceRequest> complete) 
                {
                    var stagePrefab=Resources.LoadAsync<GameObject>("Prefabs/Stage");
                    yield return new WaitUntil(() => stagePrefab.isDone);
                    complete(stagePrefab);
                };

                if (!HaveStage) {
                    StartCoroutine(CreateStage(complete => {
                        GameObject sp = Instantiate(complete.asset as GameObject);
                        sp.transform.SetParent(transform);
                        sp.transform.SetSiblingIndex(0);
                    }));
                }
            }
            if (gameObject.name != "GlobalVariables") 
            {
                mStage.MyFlowChart = this;
            }

        }

        protected virtual void OnDisable()
        {
            cachedFlowcharts.Remove(this);

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
#endif

            StringSubstituter.UnregisterHandler(this);
        }
        /// <summary>
        /// 開始故事
        /// </summary>
        public void StartShowStory()
        {
            var blocks = GetComponents<Block>();

           /* foreach (var block in blocks)
            {
                Debug.Log("block名稱=>"+block+"_eve的類型" + block._EventHandler);
            }*/

            try
            {
                Block tarBlock = null;
                foreach (var block in GetComponents<Block>()) 
                {
                    if (block._EventHandler != null&& block._EventHandler.GetType() == typeof(GameStarted))
                    {
                        tarBlock= block;
                    }
                }

                /*var tarBlock = GetComponents<Block>().First(v => {
                     Debug.Log("block1名稱=>" + v.BlockName);
                     if (v._EventHandler!=null)
                     {
                         Debug.Log("block1名稱=>" + v + "_eve的類型" + v._EventHandler.GetType());
                         return v._EventHandler.GetType() == typeof(GameStarted);
                     }
                     });*/
                if (tarBlock!=null)
                {
                    StartCoroutine((tarBlock._EventHandler as GameStarted).GameStartCoroutine());
                }
                else
                {
                    Debug.LogWarning("尚未設置GameStarted至Block裡面");
                }
            }
            catch
            {
                Debug.LogWarning("尚未設置GameStarted至Block裡面");
            }
        }
        /// <summary>
        /// 獲取此次故事多語翻譯的文本檔案
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadAssetText() 
        {
            var sayComList = GetComponents<Say>();
            var fileName = "";
            if (sayComList.Length>0)
            {
                fileName=sayComList[0].StoryText.Split("_")[0];
            }
            Debug.Log("###文本檔案名稱=>"+fileName);
            yield return TranslateOfCsv.GetAssetText(fileName);

        }


        protected virtual void UpdateVersion()
        {
            if (version == FungusConstants.CurrentVersion)
            {
                // No need to update
                return;
            }

            // Tell all components that implement IUpdateable to update to the new version
            var components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                IUpdateable u = component as IUpdateable;
                if (u != null)
                {
                    u.UpdateToVersion(version, FungusConstants.CurrentVersion);
                }
            }

            version = FungusConstants.CurrentVersion;
        }

        protected virtual void CheckItemIds()
        {
            // Make sure item ids are unique and monotonically increasing.
            // This should always be the case, but some legacy Flowcharts may have issues.
            List<int> usedIds = new List<int>();
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.ItemId == -1 || usedIds.Contains(block.ItemId))
                {
                    block.ItemId = NextItemId();
                }
                usedIds.Add(block.ItemId);
            }

            var commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                if (command.ItemId == -1 || usedIds.Contains(command.ItemId))
                {
                    command.ItemId = NextItemId();
                }
                usedIds.Add(command.ItemId);
            }
        }

        protected virtual void CleanupComponents()
        {
            // Delete any unreferenced components which shouldn't exist any more
            // Unreferenced components don't have any effect on the flowchart behavior, but
            // they waste memory so should be cleared out periodically.

            // Remove any null entries in the variables list
            // It shouldn't happen but it seemed to occur for a user on the forum 
            variables.RemoveAll(item => item == null);

            if (selectedBlocks == null) selectedBlocks = new List<Block>();
            if (selectedCommands == null) selectedCommands = new List<Command>();

            selectedBlocks.RemoveAll(item => item == null);
            selectedCommands.RemoveAll(item => item == null);

            var allVariables = GetComponents<Variable>();
            for (int i = 0; i < allVariables.Length; i++)
            {
                var variable = allVariables[i];
                if (!variables.Contains(variable))
                {
                    DestroyImmediate(variable);
                }
            }

            var blocks = GetComponents<Block>();
            var commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    var block = blocks[j];
                    if (block.CommandList.Contains(command))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(command);
                }
            }

            var eventHandlers = GetComponents<EventHandler>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                var eventHandler = eventHandlers[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    var block = blocks[j];
                    if (block._EventHandler == eventHandler)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(eventHandler);
                }
            }
        }

        public void DestroyObj(UnityEngine.Object tar)
        {
            DestroyImmediate(tar);
        }

        protected virtual Block CreateBlockComponent(GameObject parent)
        {
            Block block = parent.AddComponent<Block>();
            return block;
        }

        #region Public members

        /// <summary>
        /// Cached list of flowchart objects in the scene for fast lookup.
        /// </summary>
        public static List<Flowchart> CachedFlowcharts { get { return cachedFlowcharts; } }

        public static Flowchart GetInstance()
        {
            Flowchart tar = null;

            for (int i=0;i<cachedFlowcharts.Count;i++) {
                Flowchart tTar = cachedFlowcharts[i];
                if (tar == null)
                {
                    tar = tTar;
                    continue;
                }
                if (tar.transform.childCount<tTar.transform.childCount) {
                    tar = tTar;
                }

            }



            if (cachedFlowcharts.Count>0) {
                return tar;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Sends a message to all Flowchart objects in the current scene.
        /// Any block with a matching MessageReceived event handler will start executing.
        /// </summary>
        public static void BroadcastFungusMessage(string messageName)
        {
            var eventHandlers = UnityEngine.Object.FindObjectsOfType<MessageReceived>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                var eventHandler = eventHandlers[i];
                eventHandler.OnSendFungusMessage(messageName);
            }
        }

        /// <summary>
        /// Scroll position of Flowchart editor window.
        /// </summary>
        public virtual Vector2 ScrollPos { get { return scrollPos; } set { scrollPos = value; } }

        /// <summary>
        /// Scroll position of Flowchart variables window.
        /// </summary>
        public virtual Vector2 VariablesScrollPos { get { return variablesScrollPos; } set { variablesScrollPos = value; } }

        /// <summary>
        /// Show the variables pane.
        /// </summary>
        public virtual bool VariablesExpanded { get { return variablesExpanded; } set { variablesExpanded = value; } }

        /// <summary>
        /// Height of command block view in inspector.  commandsBlock詳細資料的高度
        /// </summary>
        public virtual float BlockViewHeight { get { return blockViewHeight; } set { blockViewHeight = value; } }

        /// <summary>
        /// Zoom level of Flowchart editor window.
        /// </summary>
        public virtual float Zoom { get { return zoom; } set { zoom = value; } }


        /// <summary>
        /// Current actively selected block in the Flowchart editor.
        /// </summary>
        public virtual Block SelectedBlock
        {
            get
            {
                if (selectedBlocks == null || selectedBlocks.Count == 0)
                    return null;

                return selectedBlocks[0];
            }
            set
            {
                ClearSelectedBlocks();
                AddSelectedBlock(value);
            }
        }

        public virtual List<Block> SelectedBlocks { get { return selectedBlocks; } set { selectedBlocks = value; } }

        /// <summary>
        /// Currently selected command in the Flowchart editor.
        /// </summary>
        public virtual List<Command> SelectedCommands { get { return selectedCommands; } }

        /// <summary>
        /// The list of variables that can be accessed by the Flowchart.
        /// </summary>
        public virtual List<Variable> Variables { get { return variables; } }

        public virtual int VariableCount { get { return variables.Count; } }

        /// <summary>
        /// Description text displayed in the Flowchart editor window
        /// </summary>
        public virtual string Description { get { return description; } }

        public virtual string DataName { get { return dataName; } }

        public virtual bool HideComponents { get { return hideComponents; } }

        public virtual List<string> HideCommands { get { return hideCommands; } }


        /// <summary>
        /// Slow down execution in the editor to make it easier to visualise program flow.
        /// </summary>
        public virtual float StepPause { get { return stepPause; } }

        /// <summary>
        /// Use command color when displaying the command list in the inspector.
        /// </summary>
        public virtual bool ColorCommands { get { return colorCommands; } }

        /// <summary>
        /// Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.
        /// </summary>
        public virtual bool SaveSelection { get { return saveSelection; } }

        /// <summary>
        /// Unique identifier for identifying this flowchart in localized string keys.
        /// </summary>
        public virtual string LocalizationId { get { return localizationId; } }

        /// <summary>
        /// Display line numbers in the command list in the Block inspector.
        /// </summary>
        public virtual bool ShowLineNumbers { get { return showLineNumbers; } }

        /// <summary>
        /// Lua Environment to be used by default for all Execute Lua commands in this Flowchart.
        /// </summary>
        public virtual LuaEnvironment LuaEnv { get { return luaEnvironment; } }

        /// <summary>
        /// The ExecuteLua command adds a global Lua variable with this name bound to the flowchart prior to executing.
        /// </summary>
        public virtual string LuaBindingName { get { return luaBindingName; } }

        /// <summary>
        /// Position in the center of all blocks in the flowchart. 所有block平均的中心點
        /// </summary>
        public virtual Vector2 CenterPosition { set; get; }

        /// <summary>
        /// Variable to track flowchart's version so components can update to new versions.
        /// </summary>
        public int Version { set { version = value; } }

        /// <summary>
        /// Returns true if the Flowchart gameobject is active.
        /// </summary>
        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Returns the Flowchart gameobject name.
        /// </summary>
        public string GetName()
        {
            return gameObject.name;
        }

        /// <summary>
        /// Returns the next id to assign to a new flowchart item.
        /// Item ids increase monotically so they are guaranteed to
        /// be unique within a Flowchart.
        /// </summary>
        public int NextItemId()
        {
            int maxId = -1;
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                maxId = Math.Max(maxId, block.ItemId);
            }

            var commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                maxId = Math.Max(maxId, command.ItemId);
            }
            return maxId + 1;
        }

        /// <summary>
        /// Create a new block node which you can then add commands to.
        /// </summary>
        public virtual Block CreateBlock(Vector2 position)
        {
            
            Block b = CreateBlockComponent(gameObject);
            b._NodeRect = new Rect(position.x, position.y, 0, 0);
            b.BlockName = GetUniqueBlockKey(b.BlockName, b);
            b.ItemId = NextItemId();

            return b;
        }

        /// <summary>
        /// Returns the named Block in the flowchart, or null if not found.
        /// </summary>
        public virtual Block FindBlock(string blockName)
        {
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.BlockName == blockName)
                {
                    return block;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks availability of the block in the Flowchart.
        /// You can use this method in a UI event. e.g. to test availability block, before handle it.
        public virtual bool HasBlock(string blockName)
        {
            var block = FindBlock(blockName);
            return block != null;
        }

        /// <summary>
        /// Executes the block if it is available in the Flowchart.
        /// You can use this method in a UI event. e.g. to try executing block without confidence in its existence.
        public virtual bool ExecuteIfHasBlock(string blockName)
        {
            if (HasBlock(blockName))
            {
                ExecuteBlock(blockName);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Execute a child block in the Flowchart.
        /// You can use this method in a UI event. e.g. to handle a button click.
        public virtual void ExecuteBlock(string blockName)
        {
            var block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (!ExecuteBlock(block))
            {
                Debug.LogWarning("Block " + blockName + " failed to execute");
            }
        }

        /// <summary>
        /// Stops an executing Block in the Flowchart.
        /// </summary>
        public virtual void StopBlock(string blockName)
        {
            var block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (block.IsExecuting())
            {
                block.Stop();
            }
        }

        /// <summary>
        /// Execute a child block in the flowchart.
        /// The block must be in an idle state to be executed.
        /// This version provides extra options to control how the block is executed.
        /// Returns true if the Block started execution.            
        /// </summary>
        public virtual bool ExecuteBlock(Block block, int commandIndex = 0, Action onComplete = null)//執行block
        {
            if (block == null)
            {
                Debug.LogError("Block must not be null");
                return false;
            }

            if (((Block)block).gameObject != gameObject)
            {
                Debug.LogError("Block must belong to the same gameobject as this Flowchart");
                return false;
            }

            // Can't restart a running block, have to wait until it's idle again
            if (block.IsExecuting())//已經執行
            {
                Debug.LogWarning(block.BlockName + " cannot be called/executed, it is already running.");
                return false;
            }
            // Start executing the Block as a new coroutine
            StartCoroutine(block.Execute(commandIndex, onComplete));
            // block.StartExecution();

            return true;
        }

        /// <summary>
        /// Stop all executing Blocks in this Flowchart.
        /// </summary>
        public virtual void StopAllBlocks()
        {
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    block.Stop();
                }
            }
        }

        /// <summary>
        /// Sends a message to this Flowchart only.
        /// Any block with a matching MessageReceived event handler will start executing.
        /// </summary>
        public virtual void SendFungusMessage(string messageName)
        {
            var eventHandlers = GetComponents<MessageReceived>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                var eventHandler = eventHandlers[i];
                eventHandler.OnSendFungusMessage(messageName);
            }
        }

        /// <summary>
        /// Returns a new variable key that is guaranteed not to clash with any existing variable in the list.
        /// </summary>
        public virtual string GetUniqueVariableKey(string originalKey, Variable ignoreVariable = null)
        {
            int suffix = 0;
            string baseKey = originalKey;

            // Only letters and digits allowed
            char[] arr = baseKey.Where(c => (char.IsLetterOrDigit(c) || c == '_')).ToArray();
            baseKey = new string(arr);

            // No leading digits allowed
            baseKey = baseKey.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = "Var";
            }

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < variables.Count; i++)
                {
                    var variable = variables[i];
                    if (variable == null || variable == ignoreVariable || variable.Key == null)
                    {
                        continue;
                    }
                    if (variable.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Returns a new Block key that is guaranteed not to clash with any existing Block in the Flowchart.
        /// </summary>
        public virtual string GetUniqueBlockKey(string originalKey, Block ignoreBlock = null)
        {
            int suffix = 0;
            string baseKey = originalKey.Trim();

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = FungusConstants.DefaultBlockName;
            }

            var blocks = GetComponents<Block>();

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < blocks.Length; i++)
                {
                    var block = blocks[i];
                    if (block == ignoreBlock || block.BlockName == null)
                    {
                        continue;
                    }
                    if (block.BlockName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Returns a new Label key that is guaranteed not to clash with any existing Label in the Block.
        /// </summary>
        public virtual string GetUniqueLabelKey(string originalKey, Label ignoreLabel)
        {
            int suffix = 0;
            string baseKey = originalKey.Trim();

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = "New Label";
            }

            var block = ignoreLabel.ParentBlock;

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                var commandList = block.CommandList;
                for (int i = 0; i < commandList.Count; i++)
                {
                    var command = commandList[i];
                    Label label = command as Label;
                    if (label == null || label == ignoreLabel)
                    {
                        continue;
                    }
                    if (label.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You will need to cast the returned variable to the correct sub-type.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable("MyBool") as BooleanVariable;
        /// boolVar.Value = false;
        /// </summary>
        public Variable GetVariable(string key)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                if (variable != null && variable.Key == key)
                {
                    return variable;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the variable with the specified key, or null if the key is not found.
        /// You can then access the variable's value using the Value property. e.g.
        /// BooleanVariable boolVar = flowchart.GetVariable<BooleanVariable>("MyBool");
        /// boolVar.Value = false;
        /// </summary>
        public T GetVariable<T>(string key) where T : Variable
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                if (variable != null && variable.Key == key)
                {
                    return variable as T;
                }
            }

            Debug.LogWarning("Variable " + key + " not found.");
            return null;
        }

        /// <summary>
        /// Returns a list of variables matching the specified type.
        /// </summary>
        public virtual List<T> GetVariables<T>() where T : Variable
        {
            var varsFound = new List<T>();

            for (int i = 0; i < Variables.Count; i++)
            {
                var currentVar = Variables[i];
                if (currentVar is T)
                    varsFound.Add(currentVar as T);
            }

            return varsFound;
        }

        /// <summary>
        /// Register a new variable with the Flowchart at runtime. 
        /// The variable should be added as a component on the Flowchart game object.
        /// </summary>
        public void SetVariable<T>(string key, T newvariable) where T : Variable
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var v = variables[i];
                if (v != null && v.Key == key)
                {
                    T variable = v as T;
                    if (variable != null)
                    {
                        variable = newvariable;
                        return;
                    }
                }
            }

            Debug.LogWarning("Variable " + key + " not found.");
        }

        /// <summary>
        /// Checks if a given variable exists in the flowchart.
        /// </summary>
        public virtual bool HasVariable(string key)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var v = variables[i];
                if (v != null && v.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the list of variable names in the Flowchart.
        /// </summary>
        public virtual string[] GetVariableNames()
        {
            var vList = new string[variables.Count];

            for (int i = 0; i < variables.Count; i++)
            {
                var v = variables[i];
                if (v != null)
                {
                    vList[i] = v.Key;
                }
            }
            return vList;
        }

        /// <summary>
        /// Gets a list of all variables with public scope in this Flowchart.
        /// </summary>
        public virtual List<Variable> GetPublicVariables()
        {
            var publicVariables = new List<Variable>();
            for (int i = 0; i < variables.Count; i++)
            {
                var v = variables[i];
                if (v != null && v.Scope == VariableScope.Public)
                {
                    publicVariables.Add(v);
                }
            }

            return publicVariables;
        }

        /// <summary>
        /// Gets the value of a boolean variable.
        /// Returns false if the variable key does not exist.
        /// </summary>
        public virtual bool GetBooleanVariable(string key)
        {
            var variable = GetVariable<BooleanVariable>(key);
            if (variable != null)
            {
                return GetVariable<BooleanVariable>(key).Value;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the value of a boolean variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetBooleanVariable(string key, bool value)
        {
            var variable = GetVariable<BooleanVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of an integer variable.
        /// Returns 0 if the variable key does not exist.
        /// </summary>
        public virtual int GetIntegerVariable(string key)
        {
            var variable = GetVariable<IntegerVariable>(key);
            if (variable != null)
            {
                return GetVariable<IntegerVariable>(key).Value;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Sets the value of an integer variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetIntegerVariable(string key, int value)
        {
            var variable = GetVariable<IntegerVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a float variable.
        /// Returns 0 if the variable key does not exist.
        /// </summary>
        public virtual float GetFloatVariable(string key)
        {
            var variable = GetVariable<FloatVariable>(key);
            if (variable != null)
            {
                return GetVariable<FloatVariable>(key).Value;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Sets the value of a float variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetFloatVariable(string key, float value)
        {
            var variable = GetVariable<FloatVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a string variable.
        /// Returns the empty string if the variable key does not exist.
        /// </summary>
        public virtual string GetStringVariable(string key)
        {
            var variable = GetVariable<StringVariable>(key);
            if (variable != null)
            {
                return GetVariable<StringVariable>(key).Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Sets the value of a string variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetStringVariable(string key, string value)
        {
            var variable = GetVariable<StringVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a GameObject variable.
        /// Returns null if the variable key does not exist.
        /// </summary>
        public virtual GameObject GetGameObjectVariable(string key)
        {
            var variable = GetVariable<GameObjectVariable>(key);

            if (variable != null)
            {
                return GetVariable<GameObjectVariable>(key).Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the value of a GameObject variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetGameObjectVariable(string key, GameObject value)
        {
            var variable = GetVariable<GameObjectVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Gets the value of a Transform variable.
        /// Returns null if the variable key does not exist.
        /// </summary>
        public virtual Transform GetTransformVariable(string key)
        {
            var variable = GetVariable<TransformVariable>(key);

            if (variable != null)
            {
                return GetVariable<TransformVariable>(key).Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the value of a Transform variable.
        /// The variable must already be added to the list of variables for this Flowchart.
        /// </summary>
        public virtual void SetTransformVariable(string key, Transform value)
        {
            var variable = GetVariable<TransformVariable>(key);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Set the block objects to be hidden or visible depending on the hideComponents property.
        /// </summary>
        public virtual void UpdateHideFlags()
        {
            if (hideComponents)
            {
                var blocks = GetComponents<Block>();
                for (int i = 0; i < blocks.Length; i++)
                {
                    var block = blocks[i];
                    block.hideFlags = HideFlags.HideInInspector;
                    if (block.gameObject != gameObject)
                    {
                        block.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                var commands = GetComponents<Command>();
                for (int i = 0; i < commands.Length; i++)
                {
                    var command = commands[i];
                    command.hideFlags = HideFlags.HideInInspector;
                }

                var eventHandlers = GetComponents<EventHandler>();
                for (int i = 0; i < eventHandlers.Length; i++)
                {
                    var eventHandler = eventHandlers[i];
                    eventHandler.hideFlags = HideFlags.HideInInspector;
                }
            }
            else
            {
                var monoBehaviours = GetComponents<MonoBehaviour>();
                for (int i = 0; i < monoBehaviours.Length; i++)
                {
                    var monoBehaviour = monoBehaviours[i];
                    if (monoBehaviour == null)
                    {
                        continue;
                    }
                    monoBehaviour.hideFlags = HideFlags.None;
                    monoBehaviour.gameObject.hideFlags = HideFlags.None;
                }
            }
        }
        /// <summary>
        /// Clears the list of selected commands.
        /// </summary>
        public virtual void ClearSelectedCommands()
        {
            selectedCommands.Clear();
#if UNITY_EDITOR
            SelectedCommandsStale = true;
#endif
        }

        /// <summary>
        /// Adds a command to the list of selected commands.
        /// </summary>
        public virtual void AddSelectedCommand(Command command)
        {
            if (!selectedCommands.Contains(command))
            {
                selectedCommands.Add(command);
#if UNITY_EDITOR
                SelectedCommandsStale = true;
#endif
            }
        }

        /// <summary>
        /// Clears the list of selected blocks.
        /// </summary>
        public virtual void ClearSelectedBlocks()
        {
            if (selectedBlocks == null)
            {
                selectedBlocks = new List<Block>();
            }

            for (int i = 0; i < selectedBlocks.Count; i++)
            {
                var item = selectedBlocks[i];

                if (item != null)
                {
                    item.IsSelected = false;
                }
            }
            selectedBlocks.Clear();
        }

        /// <summary>
        /// Adds a block to the list of selected blocks.
        /// </summary>
        public virtual void AddSelectedBlock(Block block)
        {
            if (!selectedBlocks.Contains(block))
            {
                block.IsSelected = true;
                selectedBlocks.Add(block);
            }
        }

        public virtual bool DeselectBlock(Block block)
        {
            if (selectedBlocks.Contains(block))
            {
                DeselectBlockNoCheck(block);
                return true;
            }
            return false;
        }

        public virtual void DeselectBlockNoCheck(Block b)
        {
            b.IsSelected = false;
            selectedBlocks.Remove(b);
        }

        public void UpdateSelectedCache()
        {
            selectedBlocks.Clear();
            var res = gameObject.GetComponents<Block>();
            selectedBlocks = res.Where(x => x.IsSelected).ToList();
        }

        public void ReverseUpdateSelectedCache()
        {
            for (int i = 0; i < selectedBlocks.Count; i++)
            {
                if (selectedBlocks[i] != null)
                {
                    selectedBlocks[i].IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Reset the commands and variables in the Flowchart.
        /// </summary>
        public virtual void Reset(bool resetCommands, bool resetVariables)
        {
            if (resetCommands)
            {
                var commands = GetComponents<Command>();
                for (int i = 0; i < commands.Length; i++)
                {
                    var command = commands[i];
                    command.OnReset();
                }
            }

            if (resetVariables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    var variable = variables[i];
                    variable.OnReset();
                }
            }
        }

        /// <summary>
        /// Override this in a Flowchart subclass to filter which commands are shown in the Add Command list.
        /// </summary>
        public virtual bool IsCommandSupported(CommandInfoAttribute commandInfo)
        {
            for (int i = 0; i < hideCommands.Count; i++)
            {
                // Match on category or command name (case insensitive)
                var key = hideCommands[i];
                if (String.Compare(commandInfo.Category, key, StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(commandInfo.CommandName, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if there are any executing blocks in this Flowchart.
        /// </summary>
        public virtual bool HasExecutingBlocks()
        {
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns a list of all executing blocks in this Flowchart.
        /// </summary>
        public virtual List<Block> GetExecutingBlocks()
        {
            var executingBlocks = new List<Block>();
            var blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.IsExecuting())
                {
                    executingBlocks.Add(block);
                }
            }

            return executingBlocks;
        }

        /// <summary>
        /// Substitute variables in the input text with the format {$VarName}
        /// This will first match with private variables in this Flowchart, and then
        /// with public variables in all Flowcharts in the scene (and any component
        /// in the scene that implements StringSubstituter.ISubstitutionHandler).
        /// </summary>
        public virtual string SubstituteVariables(string input)//input 匯入文本
        {
            if (stringSubstituer == null)
            {
                stringSubstituer = new StringSubstituter();
            }

            // Use the string builder from StringSubstituter for efficiency.
            StringBuilder sb = stringSubstituer._StringBuilder;
            sb.Length = 0;
            sb.Append(input);

            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool changed = false;

            // Match the regular expression pattern against a text string.
            var results = r.Matches(input);
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching private variables in this Flowchart first
                for (int j = 0; j < variables.Count; j++)
                {
                    var variable = variables[j];
                    if (variable == null)
                        continue;
                    if (variable.Scope == VariableScope.Private && variable.Key == key)
                    {
                        string value = variable.ToString();
                        sb.Replace(match.Value, value);
                        changed = true;
                    }
                }
            }

            // Now do all other substitutions in the scene
            changed |= stringSubstituer.SubstituteStrings(sb);

            if (changed)
            {

                return sb.ToString();
            }
            else
            {
                return input;
            }
        }

        public virtual void DetermineSubstituteVariables(string str, List<Variable> vars)
        {
            Regex r = new Regex(Flowchart.SubstituteVariableRegexString);

            // Match the regular expression pattern against a text string.
            var results = r.Matches(str);
            for (int i = 0; i < results.Count; i++)
            {
                var match = results[i];
                var v = GetVariable(match.Value.Substring(2, match.Value.Length - 3));
                if (v != null)
                {
                    vars.Add(v);
                }
            }
        }
        #endregion
#if UNITY_EDITOR
        #region ExportToFile

        public IEnumerator ExportFlowchartToCsv()
        {
            //找資料夾位置
            string[] filePaths = AssetDatabase.FindAssets("FlowChartDataToCsv");
            string FilePath = "";

            if (filePaths.Length != 1)
            {
                FilePath = FungusResourcesPath.FlowchartSaveData;
            }
            else
            {
                //清除掉assets字串 方便轉移平台(手機)時 好找到實際位置 ex:Assets/Application/Story/Fungus/FlowChartDataToCsv 
                FilePath = AssetDatabase.GUIDToAssetPath(filePaths[0]).Remove(0, 6) + "/";
            }


            string assetPath = Application.dataPath + FilePath;

            if (!Directory.Exists(assetPath))
            {
                Directory.CreateDirectory(assetPath);

            }
            
            int fileNum = 0;

            string fileName = dataName;
            string filePath = "";

            if (!File.Exists(assetPath+ fileName+".csv"))
            {
                filePath = assetPath + fileName;

            }
            else
            {
                bool haveName = false;
                string fileDefaultName = dataName;

                while (!haveName) {
                    fileName = fileDefaultName + "_" + fileNum.ToString();

                    if (!File.Exists(assetPath + fileName+".csv")) {

                        Debug.Log("偵測檔案名稱=>" + assetPath + fileName);
                        Debug.Log("檔案是否存在?=>" + File.Exists(assetPath + fileName));

                        haveName = true;
                        filePath = assetPath + fileName;
                    }
                    fileNum++;
                }
            }

            yield return JsonToExcel.ExportToCsv(filePath,ConvertLabelToExportCsv());

            AssetDatabase.Refresh();
            flowchartOverrideTextFileToCsv= AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/"+FilePath+fileName + ".csv");


        }

        /// <summary>
        /// 以檔案覆蓋當前儲存
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetCsvDataOverrideFlowchart()
        {
            yield return JsonToExcel.FileCsvOverrideFlowChart(flowchartOverrideTextFileToCsv.text,ConvertLabelToExportCsv());
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        /// <summary>
        ///   透過csv生成block與相應的command
        /// </summary>
        /// <returns></returns>
        public IEnumerator CreateBlockOfCsvToFlowchart()
        {

            yield return JsonToExcel.CreateBlockOfCsvToFlowchart(flowchartOverrideTextFileToCsv.text, this);
        }
        /// <summary>
        /// 轉換當前flowchart的Command指令 標題與數據順序
        /// </summary>
        /// <returns></returns>
        private JsonToExcel.ConvertToCsvCondtions ConvertLabelToExportCsv()
        {

            List<string> titleType = new List<string>();//篩選出現過的 command ( 紀錄順序

           // List<string> totalTitle = new List<string>(); //儲存 command 有紀錄的 title
            List<string> titleData = new List<string>();//儲存 command 有紀錄的 title,存完後會clear,因為要避免重複賦值 

            List <Command> list = new List<Command>();

            List<JsonToExcel.ExportFormatCalc> formatList = new List<JsonToExcel.ExportFormatCalc>();

            Block[] blockArr = GetComponents<Block>();
            Array.Sort(blockArr);//block按造順序排列

            titleType.Clear();

            foreach (Block b in blockArr)
            {
                foreach (Command com in b.CommandList)
                {
                    //包含在序列化的list裡面
                    if (JsonToExcel.CommandExportToValueSettingList.ContainsKey(com.GetType()))
                    {
                        com.ParentBlock = b;
                        list.Add(com);


                        if (!titleType.Contains(com.GetType().Name))
                        {

                            titleType.Add(com.GetType().Name);

                            foreach (var csv in JsonToExcel.CommandExportToValueSettingList[com.GetType()])
                            {
                                //避免參數名稱相同 導致分不清是哪個command指令的
                                titleData.Add(com.GetType().Name + "_class_" + csv.fieldNameDisplayToCsv);
                            }

                            //     var refDic=titleData.ToDictionary(x => x.Key, x => x.Value);
                            var refList = titleData.ToList();

                            formatList.Add(new JsonToExcel.ExportFormatCalc(com.GetType().Name, refList));
                            titleData.Clear();
                        }
                    }
                }

           }
            
            return new JsonToExcel.ConvertToCsvCondtions(this,list, formatList);
        }


        private static readonly string FileDataExportFormat = ".json";

      /*  public void ClickCreateSaveData()
        {
           StartCoroutine(CreateSaveData());
        }


        /// <summary>
        /// 生成存檔
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateSaveData()
        {
#if UNITY_EDITOR
            if (dataName==""||dataName== null) {
                dataName = "UnNameData";
            }

            string[] filePaths = AssetDatabase.FindAssets("FlowchartStoryData");
            string FilePath = "";

            if (filePaths.Length!=1) 
            {
                FilePath =  FungusResourcesPath.FlowchartSaveData;
            }
            else
            {
                FilePath = AssetDatabase.GUIDToAssetPath(filePaths[0]).Remove(0,6)+"/";
            }

            string assetPath = Application.dataPath + FilePath;

            if (!Directory.Exists(assetPath))
            {
                Directory.CreateDirectory(assetPath);

            }

            int fileNo = 1;

            ExportData saveData = new ExportData(this);

            if (!File.Exists(assetPath + dataName+FileDataExportFormat)) 
            {
                yield return File.WriteAllTextAsync(assetPath + dataName + FileDataExportFormat, JsonUtility.ToJson(saveData));


                AssetDatabase.Refresh();
                flowchartOverrideTextFileToJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + FilePath + dataName  + FileDataExportFormat);

            }
            else
            {
                bool HaveName = true;
                while (HaveName) 
                {
                    if (!File.Exists(assetPath + dataName+ "_"+fileNo.ToString()+FileDataExportFormat  )) {
                        HaveName = false;

                        yield return File.WriteAllTextAsync(assetPath + dataName + "_" + fileNo.ToString() + FileDataExportFormat, JsonUtility.ToJson(saveData));
                        AssetDatabase.Refresh();
                        flowchartOverrideTextFileToJson=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + FilePath + dataName+"_"+fileNo.ToString() + FileDataExportFormat);

                    }
                    else
                    {
                        fileNo++;
                    }
                }
            }
#else
            yield return null;
#endif

        }
        */
        /// <summary>
        /// 覆蓋存檔
        /// </summary>
        /// <param name="_dataName"></param>
        /// <returns></returns>
        public IEnumerator OverrideSaveData(string _dataName=null)
        {
#if UNITY_EDITOR
            if (_dataName!=null) {
            dataName = _dataName;
            }

            string[] filePaths = AssetDatabase.FindAssets("FlowchartStoryData");
            string FilePath = "";

            if (filePaths.Length != 1)
            {
                FilePath =  FungusResourcesPath.FlowchartSaveData;
            }
            else
            {
                FilePath = AssetDatabase.GUIDToAssetPath(filePaths[0]).Remove(0, 6)+"/";
            }

            string assetPath = Application.dataPath + FilePath;
            //清理flowchart資料 刪除stage 多餘pos

            if (!Directory.Exists(assetPath))
            {
                Directory.CreateDirectory(assetPath);
            }

            ExportData data = new ExportData();

            if (flowchartOverrideTextFileToJson != null)
            {
                data = JsonUtility.FromJson<ExportData>(flowchartOverrideTextFileToJson.text);
            }
            else if (File.Exists(assetPath + dataName + FileDataExportFormat))
            {
                
                data = JsonUtility.FromJson<ExportData>(File.ReadAllText(assetPath + dataName + FileDataExportFormat));

                flowchartOverrideTextFileToJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + FilePath + dataName + FileDataExportFormat);

            }
            else
            {
                Debug.Log("not Have File");
            }

            yield return data.SetDataInfoToGame(this);
#else
            yield return null;
#endif
        }

        public void SetDataNameText() {
            dataName = flowchartOverrideTextFileToJson.name;
        }

        /// <summary>
        /// 完全清空flowchart內的參數賦值等
        /// </summary>
        public void InitData()
        {
            hideCommands.Clear();
            variables.Clear();
            hideComponents = true;

            Component[] components = gameObject.GetComponents<Component>();

            foreach (var com in components)
            {

                if (com.GetType().BaseType == typeof(EventHandler))
                {
                    DestroyImmediate(com);
                }
            }

            foreach (var com in components)
            {

                if (com.GetType() != typeof(Flowchart) && com.GetType() != typeof(Transform))
                {
                    DestroyImmediate(com);
                }
            }
        }

        public void SetOverrideData(FlowChartSaveData saveData)
        {
            InitData();
            description = saveData.description;
            dataName = saveData.dataName;
            colorCommands = saveData.colorCommands;
            hideComponents = saveData.hideComponents;
            stepPause = saveData.stepPause;
            saveSelection = saveData.saveSelection;
            localizationId = saveData.localizationId;
            showLineNumbers = saveData.showLineNumbers;
            hideCommands = new List<string>(saveData.hideCommands);
            luaBindingName = saveData.luaBindingName;



        }
        #endregion
#endif
        #region IStringSubstituter implementation

        /// <summary>
        /// Implementation of StringSubstituter.ISubstitutionHandler which matches any public variable in the Flowchart.
        /// To perform full variable substitution with all substitution handlers in the scene, you should
        /// use the SubstituteVariables() method instead.
        /// </summary>
        [MoonSharp.Interpreter.MoonSharpHidden]
        public virtual bool SubstituteStrings(StringBuilder input)
        {
            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool modified = false;

            // Match the regular expression pattern against a text string.
            var results = r.Matches(input.ToString());
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching public variables in this Flowchart
                for (int j = 0; j < variables.Count; j++)
                {
                    var variable = variables[j];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.Scope == VariableScope.Public && variable.Key == key)
                    {
                        string value = variable.ToString();
                        input.Replace(match.Value, value);
                        modified = true;
                    }
                }
            }

            return modified;
        }

        #endregion

    }

    public class BlockListAttribute : PropertyAttribute
    {


    }


}
