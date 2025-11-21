using System;
using System.Collections.Generic;

namespace DQB2ProcessEditor
{
	internal class Backpack
	{
        public bool Lock { get; set; }
        public ProcessMemory.CarryType Type { get; private init; }
        public String BackupTime { get; private init; }
        public List<Item> Items { get; private init; }

        public Backpack(ProcessMemory.CarryType type, List<Item> items)
        {
            Type = type;
            DateTime dt = DateTime.Now;
            BackupTime = $"{dt.Year}/{dt.Month}/{dt.Day} {dt.Hour}:{dt.Minute}:{dt.Second}";
            if (type == ProcessMemory.CarryType.eBag)
            {
                BackupTime += " 👜";
            }
            Items = items;
        }
    }
}
