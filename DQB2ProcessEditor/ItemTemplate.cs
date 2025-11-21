using System;
using System.Collections.Generic;

namespace DQB2ProcessEditor
{
	internal class ItemTemplate
	{
		public String? Name { get; set; }
		public List<Item> Items { get; set; } = new();
	}
}
