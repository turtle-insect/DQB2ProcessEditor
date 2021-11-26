using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace DQB2ProcessEditor
{
    class Info
	{
		private static readonly Info mThis = new Info();
		public List<ItemInfo> AllItem { get; private set; } = new List<ItemInfo>(4000);
		public Dictionary<UInt16, BitmapImage> AllImage = new Dictionary<UInt16, BitmapImage>(1500);

		public List<ItemCategory> ItemCategory { get; private set; } = new List<ItemCategory>(20);

		// Category -> <ID, Name>
		private Dictionary<UInt16, Dictionary<UInt16, String>> AllBlock = new Dictionary<UInt16, Dictionary<UInt16, String>>();
		public ObservableCollection<String> ErrorLog { get; private set; } = new ObservableCollection<String>();

		private Info() { }

		public static Info GetInstance() { return mThis; }

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
				if (items.Length < 7) continue;

				var info = new ItemInfo();
				info.ID = Convert.ToUInt16(items[0]);
				if (info.ID == 0) continue;

				info.Name = items[1];
				info.Ruby = items[2];
				info.Rare = Convert.ToUInt16(items[3]);
				info.Link = items[4] == "TRUE";
				if (!String.IsNullOrEmpty(items[5]))
				{
					info.Image = Convert.ToUInt16(items[5]);
				}
				if (!String.IsNullOrEmpty(items[6]))
				{
					info.Category = Convert.ToUInt16(items[6]);
				}

				AllItem.Add(info);
			}
		}

		public void ItemCategoryLoad()
		{
			String filename = @"info\item_category.txt";
			String dir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"info\image\category");
			if (!System.IO.File.Exists(filename)) return;

			ItemCategory.Clear();
			foreach (var text in System.IO.File.ReadAllLines(filename))
			{
				var line = text.Replace("\n", "");
				line = line.Replace("\r", "");
				if (line.Length < 3) continue;
				if (line[0] == '#') continue;

				var items = line.Split('\t');
				if (items.Length < 2) continue;

				var category = new ItemCategory();
				category.ID = Convert.ToUInt16(items[0]);
				category.Name = items[1];
				category.Image = ImageLoad(System.IO.Path.Combine(dir, items[0] + ".png"));

				ItemCategory.Add(category);
			}
		}

		public void ItemImageLoad()
		{
			String dir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"info\image\item");
			if (!System.IO.Directory.Exists(dir)) return;

			AllImage.Clear();
			Parallel.ForEach(System.IO.Directory.GetFiles(dir), file =>
			{
				UInt16 index;
				if (UInt16.TryParse(System.IO.Path.GetFileNameWithoutExtension(file), out index))
				{
					AllImage.Add(index, ImageLoad(file));
				}

			});
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

		public List<Item> BluePrintItemLoad(String filename)
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

		private BitmapImage ImageLoad(String filename)
		{
			if(!System.IO.File.Exists(filename)) return null;

			var image = new BitmapImage();
			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = new Uri(filename);
			image.EndInit();
			image.Freeze();
			return image;
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
	}
}
