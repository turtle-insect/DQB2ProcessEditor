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
			CreateBlock();
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
			Info.ItemFilter(ItemNameFilter);
		}

		public bool ImportBluePrint(String filename)
        {
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			var append = Info.BluePrintLoad(filename);
			if (append == null || append.Count == 0) return false;

			int index = 0;
			var items = pm.ReadItem(ProcessMemory.CarryType.eBag);
			foreach(var item in items)
            {
				if(item.ID == 0 && item.Count == 0)
                {
					item.ID = append[index].ID;
					item.Count = append[index].Count;
					index++;
					if (index >= append.Count) break;
				}
			}
			pm.WriteItems(ProcessMemory.CarryType.eBag, items);
			return true;
		}

		private void CreateItem()
		{
			Info.ItemLoad();
			Info.ItemFilter("");
		}

		private void CreateBlock()
        {
			Info.BlockLoad();
        }
	}
}
