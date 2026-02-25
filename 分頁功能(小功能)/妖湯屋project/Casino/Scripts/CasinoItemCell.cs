using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using YKO.Common.UI;
using Unity.VisualScripting;
using YKO.Support;
using YKO.Common;

namespace YKO.Casino
{
    public class CasinoItemCell : MonoBehaviour
    {
        [SerializeField]
        private GameObject itemIcon;

        private ItemIcon curItemIcon=null;

        [SerializeField]
		private Button mainButton;
		[SerializeField]
		private GameObject checkImage;
		/// <summary>
		/// 領取獎勵
		/// </summary>
		[SerializeField]
		private GameObject receiveRewardImg;

		public GameObject ReceiveRewardImg { get { return receiveRewardImg; } }

        [SerializeField]
		public uint luckVal;


		public Action Callback { get; private set; }

		private void Start()
		{
			itemIcon.SetActive(false);
			mainButton.OnClickAsObservable().Subscribe(_ => OnMainButton()).AddTo(this);

		}

		public void Init(uint bid, uint amount,long _lucky_val, Action callback)
		{
			for (int i = 1; i < itemIcon.transform.parent.childCount; i++) 
			{
				Destroy(itemIcon.transform.parent.GetChild(i).gameObject);
			}
			luckVal = (uint)_lucky_val;
            var sp = Instantiate(itemIcon, itemIcon.transform.parent);
			curItemIcon = sp.GetComponent<ItemIcon>();
			var rar = LoadResource.Instance.GetItemName<long>(bid, "quality");
            curItemIcon
			.Init(
				bid,
				amount: amount,
				isShowLongPanel:false,
				rarity:(uint)rar

			 );
            curItemIcon.gameObject.SetActive(true);
			Callback = callback;
		}

		public void SetItemSelected(bool enable) 
		{
			checkImage.SetActive(enable);
        }
		/// <summary>
		/// 設置道具與背景圖反灰
		/// </summary>
		/// <returns></returns>
		public IEnumerator ReverseGray(bool enable)
		{

				if (curItemIcon==null)
				{
					yield break;
				}
			yield return curItemIcon.ReverseGray(enable);

			/*if (enable)
            {
                //Assets/Application/Story/Fungus/Resources/ShaderEffect/AdjustColor/AdjustColorMaterial.mat
				yield return LoadAssetManager.LoadAsset<Material>
					(
					FungusResourcesPath.ShaderEffectAddressPath + "AdjustColor/AdjustColorMaterial.mat",
					res => {
                        var mat = Instantiate(res);
                        mat.SetColor("_Color", new Color(0.7f, 0.7f, 0.7f, 1));
                        curItemIcon.GetComponent<ItemIcon>().Icon.material = mat;
                        curItemIcon.GetComponent<Image>().material = mat;
                       }
					);
			}
			else
			{
                Debug.Log("沒有material2");
                	if (curItemIcon.GetComponent<ItemIcon>().Icon.material!=null) 
					{
						Destroy(curItemIcon.GetComponent<ItemIcon>().Icon.material);
						curItemIcon.GetComponent<ItemIcon>().Icon.material = null;
						curItemIcon.GetComponent<Image>().material = null;
					}
            }*/

        }

		/// <summary>
		/// 可以領獎勵提示
		/// </summary>
		/// <param name="enable"></param>
		public void SetReceiveReward(bool enable)
		{
            receiveRewardImg.SetActive(enable);
        }

        private void OnMainButton() 
		{
			if (checkImage.activeSelf) 
			{ 
				
			}
			else Callback?.Invoke();
		}
	}
}