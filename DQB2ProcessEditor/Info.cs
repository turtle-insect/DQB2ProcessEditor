using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace DQB2ProcessEditor
{
	class Info
	{
		private List<ItemInfo> AllItem = new List<ItemInfo>();
		public ObservableCollection<ItemInfo> FilterItem { get; private set; } = new ObservableCollection<ItemInfo>();

		public void Filter(String filter)
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

		public void Load()
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
	}
}
