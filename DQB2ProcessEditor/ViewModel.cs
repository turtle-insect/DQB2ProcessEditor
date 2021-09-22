using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2ProcessEditor
{
	class ViewModel
	{
		public Info Info { get; set; } = new Info();
		public String ItemNameFilter { get; set; }
		

		public ViewModel()
		{
			CreateItem();
		}

		public bool InjectionItem(UInt16 itemID)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcBaseAddress()) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var inventory = pm.ReadInventoryItem();
			if (inventory == null) return false;
			for (int index = 0; index < inventory.Count; index++)
			{
				if(inventory[index].ID == 0)
				{
					inventory[index].ID = itemID;
					inventory[index].Count = Properties.Settings.Default.ItemCount;
					pm.WriteInventoryItem(index, inventory[index]);
					return true;
				}
			}

			// 設定によって強制的に上書き.
			if (Properties.Settings.Default.ItemForceWrite == false)
			{
				return false;
			}

			var item = new Item();
			item.ID = itemID;
			item.Count = Properties.Settings.Default.ItemCount;
			pm.WriteInventoryItem(Properties.Settings.Default.InventoryIndex, item);
			return true;
		}

		public bool InjectionAllItem()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcBaseAddress()) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var inventory = pm.ReadInventoryItem();
			if (inventory == null) return false;
			int filterIndex = 0;
			for (int index = 0; index < inventory.Count && filterIndex < Info.FilterItem.Count; index++)
			{
				if (inventory[index].ID == 0)
				{
					inventory[index].ID = Info.FilterItem[filterIndex].ID;
					inventory[index].Count = Properties.Settings.Default.ItemCount;
					filterIndex++;
				}
			}

			pm.WriteInventory(inventory);
			return true;
		}

		public bool WriteInventoryItemCount()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcBaseAddress()) return false;

			// アイテムの情報取得.
			// 存在しているところを探す.
			var inventory = pm.ReadInventoryItem();
			if (inventory == null) return false;
			for (int index = 0; index < inventory.Count; index++)
			{
				if (inventory[index].ID != 0)
				{
					inventory[index].Count = Properties.Settings.Default.ItemCount;
				}
			}

			pm.WriteInventory(inventory);
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
