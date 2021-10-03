using System;

namespace DQB2ProcessEditor
{
    class ViewModel
	{
		public Info Info { get; set; } = new Info();
		public String ItemNameFilter { get; set; }
		public ProcessMemory.CarryType CarryType { get; set; }
		public int ClearBagPageIndex { get; set; }
		public float PlayerJumpPower { get; set; } = 1.5F;


		public ViewModel()
		{
			CreateItem();
		}

		public bool InjectionItem(UInt16 itemID)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			for (int index = 0; index < items.Count; index++)
			{
				if(items[index].ID == 0)
				{
					items[index].ID = itemID;
					items[index].Count = Properties.Settings.Default.ItemCount;
					pm.WriteItem(CarryType, index, items[index]);
					return true;
				}
			}

			// 設定によって強制的に上書き.
			if (CarryType == ProcessMemory.CarryType.eBag || Properties.Settings.Default.ItemForceWrite == false)
			{
				return false;
			}

			var item = new Item();
			item.ID = itemID;
			item.Count = Properties.Settings.Default.ItemCount;
			pm.WriteItem(CarryType, Properties.Settings.Default.InventoryIndex, item);
			return true;
		}

		public bool InjectionAllItem()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			int filterIndex = 0;
			for (int index = 0; index < items.Count && filterIndex < Info.FilterItem.Count; index++)
			{
				if (items[index].ID == 0)
				{
					items[index].ID = Info.FilterItem[filterIndex].ID;
					items[index].Count = Properties.Settings.Default.ItemCount;
					filterIndex++;
				}
			}

			pm.WriteItems(CarryType, items);
			return true;
		}

		public bool WriteInventoryItemCount()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			// アイテムの情報取得.
			// 存在しているところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			for (int index = 0; index < items.Count; index++)
			{
				if (items[index].ID != 0)
				{
					items[index].Count = Properties.Settings.Default.ItemCount;
				}
			}

			pm.WriteItems(CarryType, items);
			return true;
		}

		public bool WritePlayerJumpPower()
        {
			var pm = new ProcessMemory();
			return pm.WritePlayerJumpPower(PlayerJumpPower);
		}

		public bool ClearItem()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			pm.ClearItem(ClearBagPageIndex);
			return true;
		}

		public bool ClearItem(ProcessMemory.CarryType type)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			pm.ClearItem(type);
			return true;
		}

		public void FilterItem()
		{
			Info.Filter(ItemNameFilter);
		}

		private void CreateItem()
		{
			Info.Load();
			Info.Filter("");
		}
	}
}
