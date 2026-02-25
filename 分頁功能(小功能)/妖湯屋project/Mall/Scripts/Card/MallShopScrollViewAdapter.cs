using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.CustomAdapters.GridView;
using Com.ForbiddenByte.OSA.DataHelpers;
using UnityEngine;

namespace YKO.MallShop
{
    [Serializable]
    public class MallShopGridParams : GridParams
    {
		public Action<ShopMallProductData> CellCallback { get; private set; }

		public void SetCheckTeamFunc(Action<ShopMallProductData> callback)
		{
			CellCallback = callback;
		}
	}

    public class MallShopCellViewsHolder : CellViewsHolder
	{
		public MallSellItemCell _cellCtrl;

		/// <inheritdoc/>
		public override void CollectViews()
		{
			base.CollectViews();

			_cellCtrl = views.GetComponentInChildren<MallSellItemCell>();
		}

		public void UpdateViews(MallShopCellModel modle, MallShopGridParams param)
		{
			_cellCtrl.Init(param.CellCallback);
			_cellCtrl.UpdateData(modle.data);
		}

		//public void UpdateViewsScale(BasicModel model)
		//{
		//	views.localScale = Vector3.one * model.ExpandedRealAmount;
		//}
	}

	public class MallShopCellModel
    {
        public ShopMallProductData data;
		public int camp_type { get { return data.camp_type; } }
	}

    public class MallShopScrollViewAdapter : GridAdapter<MallShopGridParams, MallShopCellViewsHolder>
    {
		FilterableDataHelper<MallShopCellModel> Data;

		#region Filter
		//private List<long> filterableItems = new List<long>();
		private long filterableItems = 0;
		public void addFilterIndex(int index)
		{
			//filterableItems.Clear();
			//filterableItems.Add(index);
			filterableItems = index;

			rebuildPredicate();
		}

		//public void removeFilterIndex(int index)
		//{
		//	filterableItems.Remove(index);

		//	rebuildPredicate();
		//}

		private void rebuildPredicate()
		{
			Predicate<MallShopCellModel> target;
			if (filterableItems == 0)
			{
				target = (t) => true;
			}
			else
			{
				target = (t) => filterableItems == t.camp_type;
			}

			Data.FilteringCriteria = target;
		}
		#endregion

		#region OSA implementation
		/// <inheritdoc/>
		protected override void Start()
		{
			Data = new FilterableDataHelper<MallShopCellModel>(this);

			base.Start();
		}

		/// <seealso cref="GridAdapter{TParams, TCellVH}.Refresh(bool, bool)"/>
		public override void Refresh(bool contentPanelEndEdgeStationary = false /*ignored*/, bool keepVelocity = false)
		{
			_CellsCount = Data.Count;
			base.Refresh(false, keepVelocity);
		}

		protected override void OnCellViewsHolderCreated(MallShopCellViewsHolder cellVH, CellGroupViewsHolder<MallShopCellViewsHolder> cellGroup)
		{
			base.OnCellViewsHolderCreated(cellVH, cellGroup);

			// Set listeners for the Toggle in each cell. Will call OnCellToggled() when the toggled state changes
			// Set this adapter as listener for the OnItemLongClicked event
			//cellVH.toggle.onValueChanged.AddListener(_ => OnCellToggled(cellVH));
			//cellVH.longClickableComponent.longClickListener = this;
		}

		/// <summary> Called when a cell becomes visible </summary>
		/// <param name="viewsHolder"> use viewsHolder.ItemIndexto find your corresponding model and feed data into its views</param>
		/// <see cref="GridAdapter{TParams, TCellVH}.UpdateCellViewsHolder(TCellVH)"/>
		protected override void UpdateCellViewsHolder(MallShopCellViewsHolder viewsHolder)
		{
			var model = Data[viewsHolder.ItemIndex];
			viewsHolder.UpdateViews(model, _Params);

			//UpdateSelectionState(viewsHolder, model);
			//viewsHolder.UpdateViewsScale(model);
		}

		/// <summary>Data can only be modified using this method, because and intermediary conversion step is needed before being able to actually display it</summary>
		/// <param name="cellList"></param>
		public void SetData(List<ShopMallProductData> cellList)
		{
			//var cellsPerRow = _Params.CurrentUsedNumCellsPerGroup;
			//List<SouvenirCellModel> cellsList;
			//GridWithCategoriesUtil.ConvertCategoriesToListOfItemModels(cellsPerRow, _Categories, out cellsList);
			//_LastKnownNumberOfCellsPerGroup = cellsPerRow;
			Data.ResetItems(cellList.Select(i => new MallShopCellModel() { data = i }).ToList());

			// Since we don't use a DataHelper to notify OSA for us, we do it manually
			ResetItems(Data.Count, false, true); // the last 2 params are not important. Can be omitted if you want
		}
		#endregion

		#region Extra

		#endregion
	}
}