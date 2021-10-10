using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DQB2ProcessEditor
{
    class Info
	{
		private List<ItemInfo> AllItem = new List<ItemInfo>();
		// Category => List<Block>
		private Dictionary<UInt16, List<BlockInfo>> AllBlock = new Dictionary<UInt16, List<BlockInfo>>();
		public ObservableCollection<ItemInfo> FilterItem { get; private set; } = new ObservableCollection<ItemInfo>();

		public void ItemFilter(String filter)
		{
			FilterItem.Clear();
			foreach (var info in AllItem)
			{
				if (String.IsNullOrEmpty(filter) || info.Name.IndexOf(filter) >= 0)
				{
					FilterItem.Add(info);
				}
			}
		}

		public void ItemLoad()
		{
			String filename = @"info\item.txt";
			if (!System.IO.File.Exists(filename)) return;

			AllItem.Clear();
			foreach (var text in System.IO.File.ReadAllLines(filename))
			{
				var line = text.Replace("\n", "");
				line = line.Replace("\r", "");
				if (line.Length < 3) continue;
				if (line[0] == '#') continue;

				var items = line.Split('\t');
				if (items.Length != 2) continue;

				var info = new ItemInfo() { ID = Convert.ToUInt16(items[0]), Name = items[1] };
				if (info.ID == 0) continue;

				AllItem.Add(info);
			}
		}

		public void BlockLoad()
		{
			String filename = @"info\block.txt";
			if (!System.IO.File.Exists(filename)) return;

			AllBlock.Clear();
			foreach (var text in System.IO.File.ReadAllLines(filename))
			{
				var line = text.Replace("\n", "");
				line = line.Replace("\r", "");
				if (line.Length < 3) continue;
				if (line[0] == '#') continue;

				var items = line.Split('\t');
				if (items.Length != 3) continue;

				if (String.IsNullOrEmpty(items[0])) continue;
				UInt16 category = Convert.ToUInt16(items[0]);

				var info = new BlockInfo();
				if (String.IsNullOrEmpty(items[1])) continue;
				info.ID = Convert.ToUInt16(items[1]);
				info.Name = items[2];
				if (String.IsNullOrEmpty(info.Name)) continue;

				List<BlockInfo> infos = null; 
				if (AllBlock.ContainsKey(category))
				{
					infos = AllBlock[category];
				}
				else
				{
					infos = new List<BlockInfo>();
					AllBlock.Add(category, infos);
				}
				infos.Add(info);
			}

			foreach (var key in AllBlock.Keys)
			{
				AllBlock[key].Sort();
			}
		}

		public List<Item> BluePrintLoad(String filename)
        {
			var items = new List<Item>();
			if (!System.IO.File.Exists(filename)) return items;

			Byte[] buffer = System.IO.File.ReadAllBytes(filename);
			if (buffer.Length != 0x30008) return items;

			UInt32 count = BitConverter.ToUInt16(buffer, 0x30000);
			count *= BitConverter.ToUInt16(buffer, 0x30002);
			count *= BitConverter.ToUInt16(buffer, 0x30004);

			var blockDictionary = new Dictionary<UInt32, UInt16>();
			for (int i = 0; i < count; i++)
			{
				UInt32 key = BitConverter.ToUInt32(buffer, i * 6 + 0);
				if (key == 0) continue;

				if (!blockDictionary.ContainsKey(key))
				{
					blockDictionary.Add(key, 0);
				}
				blockDictionary[key]++;
			}

			foreach (var key in blockDictionary.Keys)
			{
				Byte[] value = BitConverter.GetBytes(key);
				UInt16 id = BitConverter.ToUInt16(value, 0);
				UInt16 category = BitConverter.ToUInt16(value, 2);
				if(id == 0)
                {
					id = category;
					category = 0;
                }
				var info = Search(category, id);
				if (info == null) continue;

				items.Add(new Item() { ID = info.ID, Count = blockDictionary[key] });
			}

			return items;
		}

		private ItemInfo Search(UInt16 category, UInt16 id)
		{
			if (!AllBlock.ContainsKey(category)) return null;

			var infos = AllBlock[category];
			if(id == 3)
            {
				category = 0;
            }
			int min = 0;
			int max = infos.Count;
			String name = "";
			for (; min < max;)
			{
				int mid = (min + max) / 2;
				if (infos[mid].ID == id)
				{
					name = infos[mid].Name;
					break;
				}
				else if (infos[mid].ID > id) max = mid;
				else min = mid + 1;
			}
			if (String.IsNullOrEmpty(name)) return null;

			foreach (var item in AllItem)
			{
				if (name == item.Name) return item;
			}

			return null;
		}
	}
}
