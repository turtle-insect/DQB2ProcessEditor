using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace DQB2ProcessEditor
{
    class ViewModel
	{
		public Info Info { get; private set; } = Info.GetInstance();
		public String ItemNameFilter { get; set; }
		public UInt16 ItemCategoryFilter { get; set; }
		public ProcessMemory.CarryType CarryType { get; set; }
		public int ClearBagPageIndex { get; set; }
		public int BluePrintIndex { get; set; }
		public ObservableCollection<ItemInfo> FilterItems { get; private set; } = new ObservableCollection<ItemInfo>();
		public ObservableCollection<Backpack> Backpacks { get; private set; } = new ObservableCollection<Backpack>();


		public ViewModel()
		{
			LoadInfo();
		}

		public void LoadInfo()
        {
			CreateItem();
			CreateImage();
			CreateBlock();
		}

		public bool InjectionItem(IEnumerable iterator)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			// アイテムの情報取得.
			// 存在しないところを探す.
			var items = pm.ReadItem(CarryType);
			if (items == null) return false;
			int itemIndex = 0;
			foreach(var item in iterator)
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

			if (Backpacks.Count > 50)
			{
				for(int i = 50; i >= 0; i++)
                {
					if(Backpacks[i].Lock == false)
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
			if(Info.ItemCategory.Count > ItemCategoryFilter)
			{
				category = Info.ItemCategory[ItemCategoryFilter].ID;
			}

			foreach (var info in Info.AllItem)
			{
				if (category == 0 || category == info.Category)
				{
					if (String.IsNullOrEmpty(ItemNameFilter) ||
						info.Name.IndexOf(ItemNameFilter) >= 0 ||
						info.Ruby.IndexOf(hiraganaFilter) >= 0)
					{
						FilterItems.Add(info);
					}
				}
			}
		}

		public bool ImportBluePrint(String filename)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			if (!System.IO.File.Exists(filename)) return false;
			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			if (buffer.Length != 0x30008) return false;

			pm.WriteBluePrint(BluePrintIndex, ref buffer);
			return true;
		}

		public bool ExportBluePrint(String filename)
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			Byte[] buffer = pm.ReadBluePrint(BluePrintIndex);
			System.IO.File.WriteAllBytes(filename, buffer);
			return true;
		}

		public bool ClearBluePrint()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			pm.ClearBluePrint(BluePrintIndex);
			return true;
		}

		public bool ImportBluePrintItem(String filename)
        {
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			var append = Info.BluePrintItemLoad(filename);
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

		public bool BackpaktoBag(Backpack backpack)
        {
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress()) return false;

			pm.WriteItems(backpack.Type, backpack.Items);
			return true;
		}

		private void CreateItem()
		{
			Info.ItemLoad();
			Info.ItemCategoryLoad();
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
			if (value == null) return null;
			return new String(value.Select(c => (c >= 'ァ' && c <= 'ヶ') ? (char)(c + 'ぁ' - 'ァ') : c).ToArray());
		}
	}
}
