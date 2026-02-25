using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Serialization;

namespace Fungus
{

    public enum DisplayEffect
    {
        None,
        Fade,
        Move
    }
    [Serializable]
    public class MenuImageOption: PropertyAttribute
    {
        public Block targetBlock;
        public Sprite _image=null;
        public Vector2 imageSize = Vector2.zero;
       // [FormerlySerializedAs("targetSequence")]
        public InputCallBack.InputOptions _options;


    }
    [CommandInfo("Narrative",
             "Menu Image",
             "Displays a button in a multiple Image choice menu")]
    [AddComponentMenu("")]

    public class MenuImage : Command
    {

        //private static List<MenuImageOption>

        [SerializeField ] public MenuImageOption Options;

        [SerializeField]public ClickAeraSetting clickAeraSetting;

        public override void GetConnectedBlocks (ref List<Block> connectedBlocks)
        {
            if (Options.targetBlock != null)
            {
                connectedBlocks.Add(Options.targetBlock);
            }
        }


        public override void OnEnter()
        {

            if (Options._options.parentPos == null)
            {
                return;
            }



            if (clickAeraSetting== ClickAeraSetting.Default)
            {
                Options._options.touchSize = Options._options.parentPos.sizeDelta;
            }

            InputCallBack.GetInputCallBack().CreateMenuImage(Options);

           Continue();
        
        }

        public void DrawTexture()
        {

            bool isThis = false;
            foreach (var com in Flowchart.GetInstance().SelectedCommands)
            {
                if (com == this)
                {
                    isThis = true;
                }

            }

            if (!isThis)
            {
                return;
            }

            if (!Options._options.parentPos)
            {
                //Debug.LogError("Not Have a ClickPosTarget");
                return;
            }

            RectTransform ClickPos = Options._options.parentPos;
            GameObject sp = new GameObject("displayRect", typeof(RectTransform));
            sp.transform.SetParent(ClickPos.parent, false);
            RectTransform newRect = sp.GetComponent<RectTransform>();

            newRect.position = ClickPos.position;
            newRect.pivot = ClickPos.pivot;

            if (clickAeraSetting == ClickAeraSetting.Default)
            {
                newRect.sizeDelta = ClickPos.sizeDelta;
            }
            else
            {
                newRect.sizeDelta = Options._options.touchSize;
            }

            DrawGizmoLine.DrawTexture(newRect);
            DestroyImmediate(sp);
        }

        public void OnDrawGizmos()
        {
            DrawTexture();
        }





    }



}
