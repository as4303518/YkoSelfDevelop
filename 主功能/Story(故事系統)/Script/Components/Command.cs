// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace Fungus
{   
    /// <summary>
    /// Attribute class for Fungus commands.
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandInfoAttribute : Attribute//出現在+號後  的選項
    {
        /// <summary>
        /// Metadata atribute for the Command class. 
        /// </summary>
        /// <param name="category">The category to place this command in.</param>
        /// <param name="commandName">The display name of the command.</param>
        /// <param name="helpText">Help information to display in the inspector.</param>
        /// <param name="priority">If two command classes have the same name, the one with highest priority is listed. Negative priority removess the command from the list.</param>///
        public CommandInfoAttribute(string category, string commandName, string helpText, int priority = 0)
        {
            this.Category = category;
            this.CommandName = commandName;
            this.HelpText = helpText;
            this.Priority = priority;
        }

        public string Category { get; set; }
        public string CommandName { get; set; }
        public string HelpText { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Base class for Commands. Commands can be added to Blocks to create an execution sequence.
    /// </summary>
    /// 



    [Serializable]
    public abstract class  Command : MonoBehaviour, IVariableReference
    {

        [FormerlySerializedAs("commandId")]
        [HideInInspector]
        [SerializeField] protected int itemId = -1; // Invalid flowchart item id

        [HideInInspector]
        [SerializeField] protected int indentLevel;

        protected string errorMessage = "";



        #region Editor caches
#if UNITY_EDITOR
        //
        protected List<Variable> referencedVariables = new List<Variable>();
        //public List<string> defStr = new List<string>() { "gh", "hj" };
        //public List<List<string>> aaaa = new List<List<string>>() {new List<string>() { "gh", "hj" }};

        //public string[] bbbb=new string[0];

        //used by var list adapter to highlight variables 
        public bool IsVariableReferenced(Variable variable)
        {
            return referencedVariables.Contains(variable) || HasReference(variable);
        }

        /// <summary>
        /// Called by OnValidate
        /// 
        /// Child classes to specialise to add variable references to referencedVariables, either directly or
        /// via the use of Flowchart.DetermineSubstituteVariables
        /// </summary>
        protected virtual void RefreshVariableCache()
        {
            referencedVariables.Clear();
        }
#endif
        #endregion Editor caches

        #region Public members

        /// <summary>
        /// Unique identifier for this command.
        /// Unique for this Flowchart.
        /// </summary>
        public virtual int ItemId { get { return itemId; } set { itemId = value; } }

        /// <summary>
        /// Error message to display in the command inspector.
        /// </summary>
        public virtual string ErrorMessage { get { return errorMessage; } }

        /// <summary>
        /// Indent depth of the current commands.
        /// Commands are indented inside If, While, etc. sections.
        /// </summary>
        public virtual int IndentLevel { get { return indentLevel; } set { indentLevel = value; } }

        /// <summary>
        /// Index of the command in the parent block's command list.
        /// </summary>
        public virtual int CommandIndex { get; set; }

        /// <summary>
        /// Set to true by the parent block while the command is executing.
        /// </summary>
        public virtual bool IsExecuting { get; set; }

        /// <summary>
        /// Timer used to control appearance of executing icon in inspector.
        /// </summary>
        public virtual float ExecutingIconTimer { get; set; }

        /// <summary>
        /// Reference to the Block object that this command belongs to.
        /// This reference is only populated at runtime and in the editor when the 
        /// block is selected.
        /// </summary>
        public virtual Block ParentBlock { get; set; }

        /// <summary>
        /// Returns the Flowchart that this command belongs to.
        /// </summary>
        public virtual Flowchart GetFlowchart()
        {
            var flowchart = GetComponent<Flowchart>();
            if (flowchart == null &&
                transform.parent != null)
            {
                flowchart = transform.parent.GetComponent<Flowchart>();
            }
            return flowchart;
        }


        protected float tempDuration = 0;
        /// <summary>
        /// 修改command執行速度,為了因應skip功能(需要在短時間內執行到節點 need Execute to node in short time
        /// </summary>
        /// <param name="_switch">開關</param>
        public virtual void AdjustCommandExecuteSpeedAcceleration(bool _switch) 
        {
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        public virtual void Execute()
        {
            OnEnter();
        }

        /// <summary>
        /// End execution of this command and continue execution at the next command.
        /// </summary>
        public virtual void Continue()//動畫播放完畢
        {
            // This is a noop if the Block has already been stopped

            if (IsExecuting)
            {
                Continue(CommandIndex + 1);
            }
        }

        /// <summary>
        /// End execution of this command and continue execution at a specific command index.
        /// </summary>
        /// <param name="nextCommandIndex">Next command index.</param>
        public virtual void Continue(int nextCommandIndex)
        {
            Flowchart fc=GetFlowchart();

         //   ReduceSkipStackFrequency(fc);
            if (fc._storyEnabled) {
                OnExit();
                if (ParentBlock != null)
                {
                    ParentBlock.JumpToCommandIndex = nextCommandIndex;
                }
            }

        }
        

        public Type GetNextCommandType()
        {
            if (ParentBlock.CommandList.Count > (CommandIndex + 1))
            {
                return ParentBlock.CommandList[CommandIndex + 1].GetType();
            }
            else
            {
                return null;
            }
        }

        public Command GetNextCommand()
        {
            if (ParentBlock.CommandList.Count > (CommandIndex + 1))
            {
                return ParentBlock.CommandList[CommandIndex + 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Stops the parent Block executing.
        /// </summary>
        public virtual void StopParentBlock()
        {
            OnExit();
            if (ParentBlock != null)
            {
                ParentBlock.Stop();
            }
        }

        /// <summary>
        /// Called when the parent block has been requested to stop executing, and
        /// this command is the currently executing command.
        /// Use this callback to terminate any asynchronous operations and 
        /// cleanup state so that the command is ready to execute again later on.
        /// </summary>
        public virtual void OnStopExecuting()
        {}

        /// <summary>
        /// Called when the new command is added to a block in the editor.
        /// </summary>
        public virtual void OnCommandAdded(Block parentBlock)
        {}

        /// <summary>
        /// Called when the command is deleted from a block in the editor.
        /// </summary>
        public virtual void OnCommandRemoved(Block parentBlock)
        {}

        /// <summary>
        /// Called when this command starts execution.
        /// </summary>
        public virtual void OnEnter()
        {}
        /// <summary>
        ///是否是不能夠跳過的command 
        ///false 可以直接跳過 true 停止跳過
        /// </summary>
        /// <returns></returns>
        public virtual bool CanNotSkipCommand() {
            return false;
        }

        /// <summary>
        /// Called when this command ends execution.
        /// </summary>
        public virtual void OnExit()
        {}

        /// <summary>
        /// Called when this command is reset. This happens when the Reset command is used.
        /// </summary>
        public virtual void OnReset()
        {}

        /// <summary>
        /// Populates a list with the Blocks that this command references.
        /// </summary>
        public virtual void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {}

        /// <summary>
        /// Returns true if this command references the variable.
        /// Used to highlight variables in the variable list when a command is selected.
        /// </summary>
        public virtual bool HasReference(Variable variable)
        {
            return false;
        }

        public virtual string GetLocationIdentifier()
        {
            return ParentBlock.GetFlowchart().GetName() + ":" + ParentBlock.BlockName + "." + this.GetType().Name + "#" + CommandIndex.ToString(); 
        }

        /// <summary>
        /// Called by unity when script is loaded or its data changed by editor
        /// </summary>
        public virtual void OnValidate()
        {
#if UNITY_EDITOR
            RefreshVariableCache();
#endif
        }

        /// <summary>
        /// Returns the summary text to display in the command inspector.
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        /// <summary>
        /// Returns the searchable content for searches on the flowchart window.
        /// </summary>
        public virtual string GetSearchableContent()
        {
            return GetSummary();
        }

        /// <summary>
        /// Returns the help text to display for this command.
        /// </summary>
        public virtual string GetHelpText()
        {
            return "";
        }

        /// <summary>
        /// Return true if this command opens a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool OpenBlock()
        {
            return false;
        }

        /// <summary>
        /// Return true if this command closes a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool CloseBlock()
        {
            return false;
        }

        /// <summary>
        /// Return the color for the command background in inspector.
        /// </summary>
        /// <returns>The button color.</returns>
        public virtual Color GetButtonColor()
        {
            return Color.white;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed in the inspector. 
        /// This is useful for hiding certain properties based on the value of another property.
        /// </summary>
        public virtual bool IsPropertyVisible(string propertyName)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed as a reorderable list in the inspector.
        /// This only applies for array properties and has no effect for non-array properties.
        /// </summary>
        public virtual bool IsReorderableArray(string propertyName)
        {
            return false;
        }

        /// <summary>
        /// Returns the localization id for the Flowchart that contains this command.
        /// </summary>
        public virtual string GetFlowchartLocalizationId()
        {
            // If no localization id has been set then use the Flowchart name
            var flowchart = GetFlowchart();
            if (flowchart == null)
            {
                return "";
            }

            string localizationId = GetFlowchart().LocalizationId;
            if (localizationId.Length == 0)
            {
                localizationId = flowchart.GetName();            
            }

            return localizationId;
        }

       // public  abstract

        public virtual void GetSaveDataToValue(string valueName,object valueData)//設置不可序列又客製化的值 ex sprite,audioclip etc
        {





        }


        #endregion
    }
}