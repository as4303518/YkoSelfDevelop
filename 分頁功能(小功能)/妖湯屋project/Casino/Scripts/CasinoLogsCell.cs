using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace YKO.Casino
{
    public class CasinoLogsCell : MonoBehaviour
    {

        private class LocalizeKey
        {
            public const string Get = "獲得";
        }
        /// <summary>
        /// 玩家名稱
        /// </summary>
        [SerializeField]
        private Text playerNameText;
        /// <summary>
        /// 獲得的key text
        /// </summary>
        [SerializeField]
        private Text getKeyText;
        /// <summary>
        /// 道具名稱
        /// </summary>
        [SerializeField]
        private Text itemNameText;

        private CasinoLogsCellData aData;

        public void Init(CasinoLogsCellData _data)
        {
            aData = _data;
            UpdateUI();
        }

        private void UpdateUI()
        {
            playerNameText.text = aData.playerName;
            itemNameText.text = aData.itemName;
            getKeyText.text = LocalizeKey.Get;

        }
        public class CasinoLogsCellData
        {
            /// <summary>
            /// 玩家名稱
            /// </summary>
            public string playerName;
            /// <summary>
            /// 道具名稱
            /// </summary>
            public string itemName;

            

        }

    }
}