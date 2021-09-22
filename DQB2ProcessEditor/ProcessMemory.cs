using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2ProcessEditor
{
	class ProcessMemory
	{
		private readonly Memory.Mem mMemory = new Memory.Mem();
		private UInt64 mBaseAddress;

		public bool CalcBaseAddress()
		{
			int pID = mMemory.GetProcIdFromName("DQB2");
			if (mMemory.OpenProcess(pID) == false) return false;

			// 主人公のアドレス取得からインベントリのアドレス取得
			// v1.7.3a
			// mov rax, [DQB2.exe + 0x137E490]
			// mov rcx, [rax + 60]

			var processList = System.Diagnostics.Process.GetProcessesByName("DQB2");
			if (processList == null || processList.Length == 0) return false;
			UInt64 address = (UInt64)processList[0].MainModule.BaseAddress;
			address += 0x137E490;
			Byte[] buffer = mMemory.ReadBytes(address.ToString("x"), 8);
			address = BitConverter.ToUInt64(buffer, 0);
			if (address == 0) return false;
			address += 0x60;
			buffer = mMemory.ReadBytes(address.ToString("x"), 8);
			address = BitConverter.ToUInt64(buffer, 0);
			if (address == 0) return false;

			mBaseAddress = address;
			return true;
		}

		public List<Item> ReadInventoryItem()
		{
			if (mBaseAddress == 0) return null;

			var inventory = new List<Item>();
			UInt64 address = mBaseAddress + 0xB88650;
			Byte[] buffer = mMemory.ReadBytes(address.ToString("x"), 15 * 2 * 2);
			for (int i = 0; i < 15; i++)
			{
				var item = new Item();
				item.ID = BitConverter.ToUInt16(buffer, i * 4);
				item.Count = BitConverter.ToUInt16(buffer, i * 4 + 2);
				inventory.Add(item);
			}
			return inventory;
		}

		public void WriteInventoryItem(int inventoryIndex, Item item)
		{
			if (mBaseAddress == 0) return;

			UInt64 address = mBaseAddress + 0xB88650 + (UInt64)inventoryIndex * 4;
			WriteMemory(address, BitConverter.GetBytes(item.ID));
			WriteMemory(address + 2, BitConverter.GetBytes(item.Count));
		}

		public void WriteInventory(List<Item> inventory)
		{
			for(int index = 0; index < inventory.Count; index++)
			{
				WriteInventoryItem(index, inventory[index]);
			}
		}

		private void WriteMemory(UInt64 address, Byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				mMemory.WriteMemory(address.ToString("x"), "byte", buffer[i].ToString("x"));
				address++;
			}
		}
	}
}
