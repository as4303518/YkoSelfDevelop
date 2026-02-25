using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace YKO.EndLess
{
    public class EndLessBuffCell : MonoBehaviour
    {
        public class LocalizeKey
        {
            public static readonly string Select = "選擇";
        }

        /// <summary>
        /// 增益圖片
        /// </summary>
        [SerializeField]
        private Image buffImg;
        /// <summary>
        /// 增益效果描述文字
        /// </summary>
        [SerializeField]
        private Text buffEffectDescriptText;
        /// <summary>
        /// 選擇按鈕
        /// </summary>
        [SerializeField]
        private Button selectBtn;
        /// <summary>
        /// 選擇按鈕
        /// </summary>
        [SerializeField]
        private Text selectBtnText;

        /// <summary>
        /// 資料
        /// </summary>
        private EndLessBuffCellFunc aData;


        public void Init(EndLessBuffCellFunc data)
        {
            aData = data;
            UpdateUI();
            Register();

        }
        private void Register()
        {
            selectBtn.OnClickAsObservable().Subscribe(_ => aData.clickSelectCB());
        }
        private void UpdateUI()
        {
            buffImg.sprite = aData.buffSprite;
            buffEffectDescriptText.text = aData.buffEffect;
            selectBtnText.text = LocalizeKey.Select;
        }

        public class EndLessBuffCellFunc
        {
            /// <summary>
            /// buff的效果
            /// </summary>
            public string buffEffect;
            /// <summary>
            /// buff顯示的圖片
            /// </summary>
            public Sprite buffSprite;
            /// <summary>
            /// 選擇的buff
            /// </summary>
            public Action clickSelectCB;

        }


    }
}
