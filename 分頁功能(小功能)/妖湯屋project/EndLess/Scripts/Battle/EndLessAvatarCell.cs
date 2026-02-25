using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;

namespace YKO.EndLess
{

    public class EndLessAvatarCell : MonoBehaviour
    {

        [SerializeField]
        private HeroIcon heroIcon;
        /// <summary>
        /// 血條
        /// </summary>
        [SerializeField]
        private Image hpBar;
        /// <summary>
        /// 資料
        /// </summary>
        public Proto_23910_Response.Partner aData;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="data"></param>
        public void Init(Proto_23910_Response.Partner data)
        {
            heroIcon.InitHero(data.bid, data.star)
                    .SetHeroLevel(data.lev);
            hpBar.fillAmount = data.hp_per / 100f;
        }



 
    }
}