using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using YKO.Common.UI;
using YKO.Common;
using YKO.Network;

namespace YKO.Casino
{
    public class CasinoWatchRatePopup : PopupBase
    {
        private class LocalizeKey
        {
            public const string Title = "查看機率";
            public const string SubTitle1 = "十方賭坊";
            public const string SubTitle2 = "高級賭坊";
        }

        [SerializeField]
        private Text titleText;
        [SerializeField]
        private Text subTitleText;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Transform cellParent;
        [SerializeField]
        private GameObject txtCell;

        // Start is called before the first frame update
        void Start()
        {
            txtCell.SetActive(false);
            titleText.text = LocalizeKey.Title;
        }

        public void Init(int type) {
            base.Init();

            subTitleText.text = type == 2 ? LocalizeKey.SubTitle2 : LocalizeKey.SubTitle1;
            closeButton.OnClickAsObservable().Subscribe(_ => ClosePopup()).AddTo(this);

            var lev = MessageResponseData.Instance.UserData.lev;

            var a = JsonObjectTool.ObjectToListList<float>(LoadResource.Instance.DialData.data_magnificat_list[type.ToString()]
                                    .First(i => (lev >= i.Value.min&&lev<=i.Value.max)).Value.award);


            foreach (var show in JsonObjectTool.ObjectToListList<float>(LoadResource.Instance.DialData.data_magnificat_list[type.ToString()]
                                    .First(i => MessageResponseData.Instance.UserData.lev > i.Value.min).Value.award)) {
                var cell = Instantiate(txtCell, cellParent);
                EasySet(cell, show[0], show[1]);
                cell.SetActive(true);
            }


        }

        private void EasySet(GameObject cell, float bid, float rate) {
            cell.transform.Find("item").GetComponent<Text>().text = LocaleManager.Instance.GetLocalizedString(LocaleTableEnum.ResourceData, LoadResource.Instance.GetItemName<string>((uint)bid, "name"));
            cell.transform.Find("rate").GetComponent<Text>().text = rate + "%";
        }
    }
}