using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YKO.Common.UI;

namespace YKO.MallShop
{
    public class MallRulePopup : PopupBase
    {
        private class LocalizeKey
        {
            public const string titleString = "規則";
        }

        [SerializeField]
        private Button btnClose = default;

        [SerializeField]
        private Text title = default;

        [SerializeField]
        private Text rule = default;

        public override void Init()
        {
            base.Init();
        }

        public void Init(string ruleData)
        {
            Init();

            RegisterEvent();

            title.text = LocalizeKey.titleString;

            rule.text = ruleData;
        }

        private void RegisterEvent()
        {
            btnClose.OnClickAsObservable().Subscribe(_ => OnButtonClose()).AddTo(this);
        }

        private void OnButtonClose()
        {
            ClosePopup();
        }
    }
}
