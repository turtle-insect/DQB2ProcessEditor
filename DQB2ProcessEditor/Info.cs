using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DQB2ProcessEditor
{
	internal class Info
	{
		private static readonly Info mThis = new Info();
		public ObservableCollection<String> ErrorLog { get; private set; } = new ObservableCollection<String>();
		public List<ItemInfo> AllItem { get; private set; } = new List<ItemInfo>(4000);
		public Dictionary<UInt16, BitmapImage> AllImage { get; private set; } = new Dictionary<UInt16, BitmapImage>(1500);

		public List<ItemCategory> ItemCategory { get; private set; } = new List<ItemCategory>(20);

		public List<ItemTemplate> ItemTemplate { get; private set; } = new List<ItemTemplate>(10);

		// Category -> <ID, Name>
		private Dictionary<UInt16, Dictionary<UInt16, String>> AllBlock = new Dictionary<UInt16, Dictionary<UInt16, String>>();

		private Info() { }

		public static Info GetInstance() { return mThis; }

		public void ItemLoad()
		{
			String filename = @"info\item.txt";
			if (!System.IO.File.Exists(filename)) return;

			AllItem.Clear();
			foreach (var line in System.IO.File.ReadAllLines(filename))
			{
				var items = SplitLine(line);
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

		public void ItemTemplateLoad()
		{
			String path = @"info\template";
			if (!System.IO.Directory.Exists(path)) return;

			ItemTemplate.Clear();
			foreach (var filename in System.IO.Directory.GetFiles(path))
			{
				var template = new ItemTemplate();
				template.Name = System.IO.Path.GetFileName(filename);

				foreach (var line in System.IO.File.ReadAllLines(filename))
				{
					var items = SplitLine(line);
					if (items.Length < 2) continue;

					var item = new Item();
					item.ID = Convert.ToUInt16(items[0]);
					item.Count = Convert.ToUInt16(items[1]);
					if (item.Count > 999) item.Count = 999;
					template.Items.Add(item);
				}

				if (template.Items.Count > 0)
				{
					ItemTemplate.Add(template);
				}
			}
		}

		public void ItemCategoryLoad()
		{
			String filename = @"info\item_category.txt";
			String dir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"info\image\category");
			if (!System.IO.File.Exists(filename)) return;

			ItemCategory.Clear();
			foreach (var line in System.IO.File.ReadAllLines(filename))
			{
				var items = SplitLine(line);
				if (items.Length < 2) continue;

				var category = new ItemCategory();
				category.ID = Convert.ToUInt16(items[0]);
				category.Name = items[1];
				category.Image = ImageLoad(System.IO.Path.Combine(dir, $"{items[0]}.png"));

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
			foreach (var line in System.IO.File.ReadAllLines(filename))
			{
				var items = SplitLine(line);
				if (items.Length != 3) continue;

				if (String.IsNullOrEmpty(items[0])) continue;
				if (String.IsNullOrEmpty(items[1])) continue;
				if (String.IsNullOrEmpty(items[2])) continue;

				UInt16 category = Convert.ToUInt16(items[0]);
				UInt16 blockID = Convert.ToUInt16(items[1]);
				String name = items[2];

				Dictionary<UInt16, String>? blocks = null;
				if (AllBlock.ContainsKey(category))
				{
					blocks = AllBlock[category];
				}
				else
				{
					blocks = new Dictionary<UInt16, String>();
					AllBlock.Add(category, blocks);
				}

				if (blocks.ContainsKey(blockID))
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

		public List<Item> BluePrintItemLoad(Byte[] buffer)
		{
			var items = new List<Item>();

			if (buffer.Length != 0x30008) return items;

			UInt32 size = BitConverter.ToUInt16(buffer, 0x30000);
			size *= BitConverter.ToUInt16(buffer, 0x30002);
			size *= BitConverter.ToUInt16(buffer, 0x30004);

			var blockDictionary = new Dictionary<UInt32, UInt32>();
			for (int i = 0; i < size; i++)
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

				// ブロックのIDはカテゴリとIDを逆で扱う.
				if (blockID == 0)
				{
					blockID = category;
					category = 0;
				}

				// 水中にある場合、カテゴリーが揺れるので補完する
				// 1780,2047のカテゴリから探すと良さそう
				var categoryList = new List<UInt16>() { category, 1780, 2047 };
				List<UInt16>? ids = null;
				foreach (var id in categoryList)
				{
					ids = Search(id, blockID);
					if (ids != null && ids.Count > 0) break;
				}

				// 無い物を調査出来る様にしておく
				// ある程度整理出来たら本処理は不要
				if (ids == null || ids.Count == 0)
				{
					String log = $"Unknown\nCategory = {category} ID = {blockID}";
					ErrorLog.Add(log);
					continue;
				}

				UInt32 count = blockDictionary[key];
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

		private String[] SplitLine(String line)
		{
			line = line.Replace("\n", "");
			line = line.Replace("\r", "");
			if (line.Length < 3) return new String[0];
			if (line[0] == '#') return new String[0];

			return line.Split('\t');
		}


		private BitmapImage ImageLoad(String filename)
		{
			var image = new BitmapImage();
			if (!System.IO.File.Exists(filename)) return image;

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
					if (info.Link)
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
