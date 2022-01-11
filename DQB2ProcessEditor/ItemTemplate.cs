using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2ProcessEditor
{
	internal class ItemTemplate
	{
		public String Name { get; set; }
		public List<Item> Items { get; set; } = new List<Item>();
	}
}
