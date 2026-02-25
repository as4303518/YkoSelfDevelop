// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Collections;
using YKO.AttractionSystem;
using YKO.Common.UI;
using YKO.Story;
using System;

namespace Fungus
{
    /// <summary>
    /// Displays a button in a multiple choice menu.
    /// </summary>
    [CommandInfo("Narrative",
                 "Menu",
                 "Displays a button in a multiple choice menu")]
    [AddComponentMenu("")]//不會讓腳本顯示在addCompent裡面
    public class Menu : Command, ILocalizable, IBlockCaller
    {
        [Tooltip("Text to display on the menu button")]
        [SerializeField] protected string text = "Option Text";
        [TextArea(5,10)]
        [Tooltip("Notes about the option text for other authors, localization, etc.")]
        [SerializeField] protected string description = "";

        [FormerlySerializedAs("targetSequence")]
        [Tooltip("Block to execute when this option is selected")]
        [SerializeField] protected Block targetBlock;

        [Tooltip("Hide this option if the target block has been executed previously")]
        [SerializeField] protected bool hideIfVisited;

        [Tooltip("If false, the menu option will be displayed but will not be selectable")]
        [SerializeField] protected BooleanData interactable = new BooleanData(true);

        [Tooltip("A custom Menu Dialog to use to display this menu. All subsequent Menu commands will use this dialog.")]
        [SerializeField] protected MenuDialog setMenuDialog;

        [Tooltip("If true, this option will be passed to the Menu Dialogue but marked as hidden, this can be used to hide options while maintaining a Menu Shuffle.")]
        [SerializeField] protected BooleanData hideThisOption = new BooleanData(false);
        /// <summary>
        /// 劇情的群組ID
        /// </summary>
        [SerializeField] protected int storyGroupID = 0;

        [SerializeField]protected int mOrder;

        #region Public members

        public string Text { get { return text; } set { text = value; } }

        public string TargetBlockName
        {
            get { return targetBlock.BlockName; }
            set
            {
                Flowchart flow = GetComponent<Flowchart>();
                targetBlock = flow.FindBlock(value);
            }
        }
        public string Descrption { get { return description; } set { description = value; } }
        public MenuDialog SetMenuDialog { get { return setMenuDialog; } set { setMenuDialog = value; } }



        public override void OnEnter()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            
            if (setMenuDialog != null)
            {
                // Override the active menu dialog
                MenuDialog.ActiveMenuDialog = setMenuDialog;
            }

            bool hideOption = (hideIfVisited && targetBlock != null && targetBlock.GetExecutionCount() > 0) || hideThisOption.Value;

            var menuDialog = MenuDialog.GetMenuDialog();

            if (menuDialog != null)
            {
                menuDialog.CanChoose = true;
                if (!menuDialog.IsActive())
                {
                    menuDialog.SetActive(true);
                }

                string displayText = "";

                var flowchart = GetFlowchart();

                if (flowchart.useAssetText)
                {
                    yield return TranslateOfCsv.eGetSpecifyValueOfCsvFile(
                        text,
                        flowchart.mLanguage,
                        str => { displayText = str; }
                        );
                }
                else
                {
                    displayText = text;
                }


                displayText = flowchart.SubstituteVariables(displayText);

                menuDialog.AddOption(displayText, interactable, hideOption, targetBlock, 
                    menuIndex => 
                    {
                    mOrder = (menuIndex+1);
                    },
                    () => {
                        void RequestAttractionDataOfGroupID()
                        {

                            if (GameSceneManager.Instance == null) return; 

                            StoryScene.StorySceneParam sceneParam = GameSceneManager.Instance.GetSceneParam() as StoryScene.StorySceneParam;
                            if (sceneParam != null)
                            {
                                switch (sceneParam.origineScene)
                                {
                                    case SceneConst.SceneName.AttractionSystemScene:
                                        storyGroupID = AttractionSystemManager.Instance.GetCurrentLoveTalkingId();
                                        Debug.Log($">>> 劇情ID = {storyGroupID}");

                                        if (storyGroupID > 0)
                                        {
                                            // 花月籠屋劇情分歧選項
                                            AttractionSystemManager.Instance.RequestToFinishTalking((uint)storyGroupID, (uint)mOrder);

                                            Debug.Log($">>> 花月籠屋: {storyGroupID} / {mOrder}");
                                        }

                                        break;
                                    default:
                                        if (storyGroupID > 0)
                                        {
                                            StoryScene.Instance.SendProto13050Resquest(storyGroupID, mOrder);
                                        }

                                        break;
                                }
                            }
                        }

#if UNITY_EDITOR
                            RequestAttractionDataOfGroupID();
#else
                        try
                        {
                            RequestAttractionDataOfGroupID();
                         }
                        catch(Exception ex)
                        {
                            Debug.Log("錯誤=>"+ex);
                        }
#endif
                    }
                );
            }
            Continue();
        }


        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (targetBlock != null)
            {
                connectedBlocks.Add(targetBlock);
            }
        }

        public override string GetSummary()
        {
            if (targetBlock == null)
            {
                return "Error: No target block selected";
            }

            if (text == "")
            {
                return "Error: No button text selected";
            }

            return text + " : " + targetBlock.BlockName;
        }

        public override bool CanNotSkipCommand()
        {
            return true;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return interactable.booleanRef == variable || hideThisOption.booleanRef == variable ||
                base.HasReference(variable);
        }

        public bool MayCallBlock(Block block)
        {
            return block == targetBlock;
        }

#endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return text;
        }

        public virtual void SetStandardText(string standardText)
        {
            text = standardText;
        }
        public virtual string GetDescription()
        {
            return description;
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual string GetStringId()
        {
            // String id for Menu commands is MENU.<Localization Id>.<Command id>
            return "MENU." + GetFlowchartLocalizationId() + "." + itemId;
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            var f = GetFlowchart();

            f.DetermineSubstituteVariables(text, referencedVariables);
        }
#endif
#endregion Editor caches
    }
}