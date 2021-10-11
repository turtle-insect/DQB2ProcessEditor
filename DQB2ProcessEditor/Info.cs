using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DQB2ProcessEditor
{
    class Info
	{
		private List<ItemInfo> AllItem = new List<ItemInfo>();
		// Category -> <ID, Name>
		private Dictionary<UInt16, Dictionary<UInt16, String>> AllBlock = new Dictionary<UInt16, Dictionary<UInt16, String>>();
		public ObservableCollection<ItemInfo> FilterItem { get; private set; } = new ObservableCollection<ItemInfo>();
		public ObservableCollection<String> ErrorLog { get; private set; } = new ObservableCollection<String>();

		public void ItemFilter(String filter)
		{
			FilterItem.Clear();
			String originalFilter = filter;
			String hiraganaFilter = ToHiragana(filter);

			foreach (var info in AllItem)
			{
				if (String.IsNullOrEmpty(filter) ||
					info.Name.IndexOf(filter) >= 0 ||
					ToHiragana(info.Name).IndexOf(hiraganaFilter) >= 0)
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
				if (items.Length != 4) continue;

				var info = new ItemInfo();
				info.ID = Convert.ToUInt16(items[0]);
				info.Name = items[1];
				info.Rare = Convert.ToUInt16(items[2]);
				info.Link = items[3] == "True";
				if (info.ID == 0) continue;

				AllItem.Add(info);
			}
		}

		public void BlockLoad()
		{
			String filename = @"info\block.txt";
			if (!System.IO.File.Exists(filename)) return;

			AllBlock.Clear();
			ErrorLog.Clear();
			foreach (var text in System.IO.File.ReadAllLines(filename))
			{
				var line = text.Replace("\n", "");
				line = line.Replace("\r", "");
				if (line.Length < 3) continue;
				if (line[0] == '#') continue;

				var items = line.Split('\t');
				if (items.Length != 3) continue;

				if (String.IsNullOrEmpty(items[0])) continue;
				if (String.IsNullOrEmpty(items[1])) continue;
				if (String.IsNullOrEmpty(items[2])) continue;

				UInt16 category = Convert.ToUInt16(items[0]);
				UInt16 blockID = Convert.ToUInt16(items[1]);
				String name = items[2];

				Dictionary<UInt16, String> blocks = null; 
				if (AllBlock.ContainsKey(category))
				{
					blocks = AllBlock[category];
				}
				else
				{
					blocks = new Dictionary<UInt16, String>();
					AllBlock.Add(category, blocks);
				}

				if(blocks.ContainsKey(blockID))
                {
					// カテゴリ、IDの重複
					// ありえないケース
					// 正しい値が調査出来るまで発生する
					// Log出しのみする
					String log = $"Duplicate\nCategory = {category} ID = {blockID}\n{blocks[blockID]}\n{name}";
					ErrorLog.Add(log);
					continue;
				}
				
				blocks.Add(blockID, name);
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

			var blockDictionary = new Dictionary<UInt32, UInt32>();
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
				UInt16 blockID = BitConverter.ToUInt16(value, 0);
				UInt16 category = BitConverter.ToUInt16(value, 2);

				// ブロックのIDはカテゴリとIDを逆として扱う.
				if(blockID == 0)
                {
					blockID = category;
					category = 0;
                }
				var ids = Search(category, blockID);
				if (ids == null || ids.Count == 0)
				{
					String log = $"Unknown\nCategory = {category} ID = {blockID}";
					ErrorLog.Add(log);
					continue;
				}

				count = blockDictionary[key];
				foreach (var id in ids)
				{
					for (int i = 0; i < count / 999; i++)
					{
						items.Add(new Item() { ID = id, Count = 999 });
					}
					count %= 999;
					if (count != 0)
					{
						items.Add(new Item() { ID = id, Count = (UInt16)count });
					}
				}
			}

			return items;
		}

		private List<UInt16> Search(UInt16 category, UInt16 id)
		{
			List<UInt16> ids = new List<UInt16>();
			if (!AllBlock.ContainsKey(category)) return ids;

			var blocks = AllBlock[category];
			if (!blocks.ContainsKey(id)) return ids;
			String name = blocks[id];
			if (String.IsNullOrEmpty(name)) return ids;

			foreach (var info in AllItem)
			{
				if (name == info.Name)
				{
					if(info.Link)
                    {
						ids.Clear();
						ids.Add(info.ID);
						break;
					}
					ids.Add(info.ID);
				}
			}

			return ids;
		}

		private String ToHiragana(String value)
		{
			if (value == null) return null;
			return new String(value.Select(c => (c >= 'ァ' && c <= 'ヶ') ? (char)(c + 'ぁ' - 'ァ') : c).ToArray());
		}
	}
}
