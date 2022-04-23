using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DQB2ProcessEditor
{
	internal class ViewModel
	{
		public Info Info { get; private set; } = Info.GetInstance();
		public List<ProcessInfo> processInfos { get; private set; } = new List<ProcessInfo>();
		public bool ItemFilterTile { get; set; } = false;
		public String ItemNameFilter { get; set; } = "";
		public UInt16 ItemCategoryFilter { get; set; }
		public ProcessMemory.CarryType CarryType { get; set; }
		public int ClearBagPageIndex { get; set; }
		public ObservableCollection<ItemInfo> FilterItems { get; private set; } = new ObservableCollection<ItemInfo>();
		public ObservableCollection<Backpack> Backpacks { get; private set; } = new ObservableCollection<Backpack>();


		public ViewModel()
		{
			LoadInfo();

			processInfos.Add(new ProcessInfo() { Name = "DQB2", Address = "0x137E490" });
			processInfos.Add(new ProcessInfo() { Name = "DQB2_EU", Address = "0x13AF558" });
			processInfos.Add(new ProcessInfo() { Name = "DQB2_AS", Address = "0x139D3F8" });
		}

		public void LoadInfo()
		{
			CreateItem();
			CreateImage();
			CreateBlock();
		}

		public bool InjectionItemInfo(IEnumerable iterator)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			int itemIndex = 0;
			foreach (var item in iterator)
			{
				if (itemIndex >= items.Count) break;

				var info = item as ItemInfo;
				if (info == null) continue;

				for (; itemIndex < items.Count; itemIndex++)
				{
					if (items[itemIndex].ID == 0)
					{

						items[itemIndex].ID = info.ID;
						items[itemIndex].Count = Properties.Settings.Default.ItemCount;
						itemIndex++;
						break;
					}
				}
			}

			pm.WriteItems(CarryType, items);
			return true;
		}

		public bool InjectionItem(List<Item> TemplateItems)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			int itemIndex = 0;
			foreach (var item in TemplateItems)
			{
				if (itemIndex >= items.Count) break;


				for (; itemIndex < items.Count; itemIndex++)
				{
					if (items[itemIndex].ID == 0)
					{

						items[itemIndex].ID = item.ID;
						items[itemIndex].Count = item.Count;
						itemIndex++;
						break;
					}
				}
			}

			pm.WriteItems(CarryType, items);
			return true;
		}

		public bool WriteItemCount()
		{
			return WriteItemCount(CarryType);
		}

		public bool WriteInventoryItemCount()
		{
			return WriteItemCount(ProcessMemory.CarryType.eInventory);
		}

		public bool WriteBagItemCount()
		{
			return WriteItemCount(ProcessMemory.CarryType.eBag);
		}

		private bool WriteItemCount(ProcessMemory.CarryType type)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			// アイテムの情報取得.
			// 存在しているところを探す.
			var items = pm.ReadItem(type);
			if (items == null) return false;
			for (int index = 0; index < items.Count; index++)
			{
				if (items[index].ID != 0)
				{
					items[index].Count = Properties.Settings.Default.ItemCount;
				}
			}

			pm.WriteItems(type, items);
			return true;
		}

		public bool ClearItem()
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			pm.ClearItem(ClearBagPageIndex);
			return true;
		}

		public bool ClearItem(ProcessMemory.CarryType type)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			if (Backpacks.Count > 50)
			{
				for (int i = 50; i >= 0; i++)
				{
					if (Backpacks[i].Lock == false)
					{
						Backpacks.RemoveAt(i);
						break;
					}
				}
			}
			Backpacks.Insert(0, new Backpack(type, pm.ReadItem(type)));

			pm.ClearItem(type);
			return true;
		}

		public void FilterItem()
		{
			FilterItems.Clear();
			String originalFilter = ItemNameFilter;
			String hiraganaFilter = ToHiragana(ItemNameFilter);
			UInt16 category = 0;
			if (Info.ItemCategory.Count > ItemCategoryFilter)
			{
				category = Info.ItemCategory[ItemCategoryFilter].ID;
			}

			foreach (var info in Info.AllItem)
			{
				if (category == 0 || category == info.Category)
				{
					if (String.IsNullOrEmpty(ItemNameFilter) ||
						info.Name?.IndexOf(ItemNameFilter) >= 0 ||
						info.Ruby?.IndexOf(hiraganaFilter) >= 0)
					{
						FilterItems.Add(info);
					}
				}
			}
		}

		public bool ImportBluePrint(String filename, int index)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			if (!System.IO.File.Exists(filename)) return false;
			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			if (buffer.Length != 0x30008) return false;

			pm.WriteBluePrint(index, ref buffer);
			return true;
		}

		public bool ExportBluePrint(String filename, int index)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			Byte[] buffer = pm.ReadBluePrint(index);
			System.IO.File.WriteAllBytes(filename, buffer);
			return true;
		}

		public bool ClearBluePrint(int index)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			pm.ClearBluePrint(index);
			return true;
		}

		public bool ImportBluePrintItem(int index)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			Byte[] buffer = pm.ReadBluePrint(index);
			return ImportBluePrintItem(buffer);
		}

		public bool ImportBluePrintItem(String filename)
		{
			if (!System.IO.File.Exists(filename)) return false;
			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			return ImportBluePrintItem(buffer);
		}

		private bool ImportBluePrintItem(Byte[] buffer)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			var append = Info.BluePrintItemLoad(buffer);
			if (append == null || append.Count == 0) return false;

			int index = 0;
			var items = pm.ReadItem(ProcessMemory.CarryType.eBag);
			foreach (var item in items)
			{
				if (item.ID == 0 && item.Count == 0)
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

		public bool BackpaktoBag(Backpack backpack)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			pm.WriteItems(backpack.Type, backpack.Items);
			return true;
		}

		private ProcessMemory? CreateProcessMemory()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress(processInfos[Properties.Settings.Default.ProcessIndex])) return null;

			return pm;
		}

		private void CreateItem()
		{
			Info.ItemLoad();
			Info.ItemCategoryLoad();
			Info.ItemTemplateLoad();
			FilterItem();
		}

		private void CreateImage()
		{
			Info.ItemImageLoad();
		}

		private void CreateBlock()
		{
			Info.BlockLoad();
		}

		private String ToHiragana(String value)
		{
			return new String(value.Select(c => (c >= 'ァ' && c <= 'ヶ') ? (char)(c + 'ぁ' - 'ァ') : c).ToArray());
		}
	}
}

