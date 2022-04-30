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
		public List<ProcessInfo> ProcessInfos { get; private set; } = new List<ProcessInfo>();
		public bool ItemFilterTile { get; set; } = false;
		public UInt16 ImportTemplateItemIndex { get; set; }
		public ProcessMemory.CarryType CarryType { get; set; }
		public int ClearBagPageIndex { get; set; }
		public ObservableCollection<ItemInfo> FilterItems { get; private set; } = new ObservableCollection<ItemInfo>();
		public ObservableCollection<Backpack> Backpacks { get; private set; } = new ObservableCollection<Backpack>();

		private String mItemNameFilter = "";
		public String ItemNameFilter
		{
			get => mItemNameFilter;
			set
			{
				mItemNameFilter = value;
				FilterItem();
			}
		}
		private UInt16 mItemCategoryIndex;
		public UInt16 ItemCategoryIndex
		{
			get => mItemCategoryIndex;
			set
			{
				mItemCategoryIndex = value;
				FilterItem();
			}
		}

		public CommandAction ImportItemCommand { get; private set; }
		public CommandAction ImportTemplateItemCommand { get; private set; }
		public CommandAction WriteItemCountCommand { get; private set; }
		public CommandAction ClearItemCommand { get; private set; }
		public CommandAction ClearBagPageCommand { get; private set; }
		public CommandAction ImportBluePrintCommand { get; private set; }
		public CommandAction ExportBluePrintCommand { get; private set; }
		public CommandAction ClearBluePrintCommand { get; private set; }
		public CommandAction ClearBluePrintAllCommand { get; private set; }
		public CommandAction ImportBluePrintItemMemoryCommand { get; private set; }
		public CommandAction ImportBluePrintItemFileCommand { get; private set; }
		public CommandAction ReloadCommand { get; private set; }


		public ViewModel()
		{
			Reload(null);

			ProcessInfos.Add(new ProcessInfo() { Name = "DQB2", Address = "0x137E490" });
			ProcessInfos.Add(new ProcessInfo() { Name = "DQB2_EU", Address = "0x13AF558" });
			ProcessInfos.Add(new ProcessInfo() { Name = "DQB2_AS", Address = "0x139D3F8" });

			ImportItemCommand = new CommandAction(ImportItem);
			ImportTemplateItemCommand = new CommandAction(ImportTemplateItem);
			WriteItemCountCommand = new CommandAction(WriteItemCount);
			ClearItemCommand = new CommandAction(ClearItem);
			ClearBagPageCommand = new CommandAction(ClearBagPage);
			ImportBluePrintCommand = new CommandAction(ImportBluePrint);
			ExportBluePrintCommand = new CommandAction(ExportBluePrint);
			ClearBluePrintCommand = new CommandAction(ClearBluePrint);
			ClearBluePrintAllCommand = new CommandAction(ClearBluePrintAll);
			ImportBluePrintItemMemoryCommand = new CommandAction(ImportBluePrintItemMemory);
			ImportBluePrintItemFileCommand = new CommandAction(ImportBluePrintItemFile);
			ReloadCommand = new CommandAction(Reload);
		}

		public void WriteInventoryItemCount()
		{
			WriteItemCount(ProcessMemory.CarryType.eInventory);
		}

		public void WriteBagItemCount()
		{
			WriteItemCount(ProcessMemory.CarryType.eBag);
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

		public bool BackpaktoBag(Backpack backpack)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return false;

			pm.WriteItems(backpack.Type, backpack.Items);
			return true;
		}

		private void FilterItem()
		{
			FilterItems.Clear();
			String originalFilter = ItemNameFilter;
			String hiraganaFilter = ToHiragana(ItemNameFilter);
			UInt16 category = 0;
			if (Info.ItemCategory.Count > ItemCategoryIndex)
			{
				category = Info.ItemCategory[ItemCategoryIndex].ID;
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

		private bool ImportBluePrintItem(Byte[] buffer)
		{
			if (buffer.Length != 0x30008) return false;

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

		private int ParamIndex(object? parameter)
		{
			String value = (String?)parameter ?? "0";
			return int.Parse(value);
		}

		private void Reload(object? parameter)
		{
			CreateItem();
			CreateImage();
			CreateBlock();
		}

		private void ImportItem(object? parameter)
		{
			InjectionItemInfo(FilterItems);
		}

		private void ImportBluePrintItemFile(object? parameter)
		{
			var dlg = new Microsoft.Win32.OpenFileDialog();
			if (dlg.ShowDialog() == false) return;

			String filename = dlg.FileName;
			if (!System.IO.File.Exists(filename)) return;
			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			ImportBluePrintItem(buffer);
		}

		private void ImportBluePrintItemMemory(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			int index = ParamIndex(parameter);
			Byte[] buffer = pm.ReadBluePrint(index);
			ImportBluePrintItem(buffer);
		}

		private void ClearBluePrintAll(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			pm.ClearBluePrint(0);

			for (int index = 4; index < 8; index++)
			{
				pm.ClearBluePrint(index);
			}
		}

		private void ClearBluePrint(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			int index = ParamIndex(parameter);
			pm.ClearBluePrint(index);
		}

		private void ExportBluePrint(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			var dlg = new Microsoft.Win32.SaveFileDialog();
			if (dlg.ShowDialog() == false) return;

			int index = ParamIndex(parameter);
			System.IO.File.WriteAllBytes(dlg.FileName, pm.ReadBluePrint(index));
		}

		private void ImportBluePrint(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			var dlg = new Microsoft.Win32.OpenFileDialog();
			if (dlg.ShowDialog() == false) return;

			int index = ParamIndex(parameter);
			String filename = dlg.FileName;

			if (!System.IO.File.Exists(filename)) return;
			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			if (buffer.Length != 0x30008) return;

			pm.WriteBluePrint(index, ref buffer);
		}

		private void ClearBagPage(object? parameter)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			pm.ClearItem(ClearBagPageIndex);
		}

		private void ClearItem(object? parameter)
		{
			int value = ParamIndex(parameter);
			var type = (ProcessMemory.CarryType)value;

			BackupItem(type);

			var pm = CreateProcessMemory();
			if (pm == null) return;
			pm.ClearItem(type);
		}

		private void WriteItemCount(ProcessMemory.CarryType type)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

			// アイテムの情報取得.
			// 存在しているところを探す.
			var items = pm.ReadItem(type);
			if (items == null) return;
			for (int index = 0; index < items.Count; index++)
			{
				if (items[index].ID != 0)
				{
					items[index].Count = Properties.Settings.Default.ItemCount;
				}
			}

			pm.WriteItems(type, items);
		}

		private void WriteItemCount(object? parameter)
		{
			WriteItemCount(CarryType);
		}

		private void ImportTemplateItem(object? parameter)
		{
			InjectionItem(Info.ItemTemplate[ImportTemplateItemIndex].Items);
		}

		private ProcessMemory? CreateProcessMemory()
		{
			var pm = new ProcessMemory();
			if (!pm.CalcPlayerAddress(ProcessInfos[Properties.Settings.Default.ProcessIndex])) return null;

			return pm;
		}

		private void BackupItem(ProcessMemory.CarryType type)
		{
			var pm = CreateProcessMemory();
			if (pm == null) return;

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

