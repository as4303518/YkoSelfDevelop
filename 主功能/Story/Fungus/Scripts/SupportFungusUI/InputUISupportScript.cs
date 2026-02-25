using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace Fungus{

    public static class InputUISupportScript
    {

        

        public static GameObject CreateButtonArea(InputCallBack.InputOptions opt,Action clickAction,Color color)
        {
            RectTransform sp = new GameObject("Touch Area", typeof(RectTransform), typeof(Button),typeof(Image)).GetComponent<RectTransform>();

            sp.sizeDelta = opt.touchSize;

            if (opt.SetLocalPos)
            {
                //Prevent full-screen buttons from being affected by the offest of the parent object
                if (opt.parentPos.GetComponentInParent<Canvas>()) {
                    sp.SetParent(opt.parentPos.GetComponentInParent<Canvas>().transform);
                }
                else
                {
                    sp.SetParent(opt.parentPos);
                }

                sp.localPosition = Vector3.zero;
                sp.SetParent(opt.parentPos);
            }
            else
            {
                sp.SetParent(opt.parentPos);
                sp.anchoredPosition = Vector2.zero;
                sp.localPosition = new Vector3(sp.localPosition.x, sp.localPosition.y, 0);
               // sp.localPosition = Vector2.zero;
                    
            }

            sp.rotation = opt.parentPos.rotation;
            sp.localScale = Vector3.one;
            Image spImage = sp.GetComponent<Image>();
            spImage.color = color;
                
          
            
            
            sp.GetComponent<Button>().onClick.AddListener(() => {
                clickAction();
            });



            return sp.gameObject;
        }

        public static GameObject CreateButtonArea(InputCallBack.InputOptions opt, Action clickAction)
        {
            
            return CreateButtonArea(opt,clickAction, new Color(1,1,1,0));
        }



        private static Regex m_richRegex = new Regex("<.*?>");
        public static string RemoveAllRichText(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return m_richRegex.Replace(str, string.Empty);
        }



    }






}



