using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;

namespace YKO.SevenDay
{

    public class SevenDayTipPopup : PopupBase
    {

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text descript;


        public void Init(string _title,string _descript)
        {
            base.Init();
           title.text = _title;
            descript.text = _descript;
        }


    }
}
