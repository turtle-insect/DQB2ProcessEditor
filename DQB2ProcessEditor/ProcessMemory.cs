using System;
using System.Collections.Generic;

namespace DQB2ProcessEditor
{
    class ProcessMemory
	{
		public enum CarryType
		{
			eInventory,
			eBag,
		}
		Dictionary<CarryType, Carry> Carrys = new Dictionary<CarryType, Carry>()
		{
			{CarryType.eInventory,  new Carry(){ Distance = 0xB88650, ItemCount = 15 } },
			{CarryType.eBag,  new Carry(){ Distance = 0xB8DF74, ItemCount = 420 } },
		};
		private readonly Memory.Mem mMemory = new Memory.Mem();
		private UInt64 mBaseAddress;

		public bool CalcPlayerAddress()
		{
			String ProcName = Properties.Settings.Default.ProcessName;
			int pID = mMemory.GetProcIdFromName(ProcName);
			if (mMemory.OpenProcess(pID) == false) return false;

			// 主人公のアドレス取得からインベントリのアドレス取得
			// v1.7.3a
			// mov rax, [DQB2.exe + 0x137E490]
			// mov rcx, [rax + 60]

			Byte[] buffer = mMemory.ReadBytes(ProcName + ".exe+0x137E490,0x60", 8);
			UInt64 address = BitConverter.ToUInt64(buffer, 0);
			if (address == 0) return false;

			mBaseAddress = address;
			return true;
		}

		public bool WritePlayerJumpPower(float power)
		{
			String ProcName = Properties.Settings.Default.ProcessName;
			int pID = mMemory.GetProcIdFromName(ProcName);
			if (mMemory.OpenProcess(pID) == false) return false;

			// DQB2.exe+1AA049

			return mMemory.WriteMemory(ProcName + ".exe+0x133A8B8,0x58,0x10,0x2A4", "float", power.ToString());
		}

		public List<Item> ReadItem(CarryType type)
		{
			if (mBaseAddress == 0) return null;

			var Items = new List<Item>();
			var carry = Carrys[type];
			UInt64 address = mBaseAddress + carry.Distance;
			Byte[] buffer = mMemory.ReadBytes(address.ToString("x"), carry.ItemCount * 2 * 2);
			for (int i = 0; i < carry.ItemCount; i++)
			{
				var item = new Item();
				item.ID = BitConverter.ToUInt16(buffer, i * 4);
				item.Count = BitConverter.ToUInt16(buffer, i * 4 + 2);
				Items.Add(item);
			}
			return Items;
		}

		public void WriteItem(CarryType type, int inventoryIndex, Item item)
		{
			if (mBaseAddress == 0) return;

			var carry = Carrys[type];
			UInt64 address = mBaseAddress + carry.Distance + (UInt64)inventoryIndex * 4;
			mMemory.WriteBytes((address + 0).ToString("x"), BitConverter.GetBytes(item.ID));
			mMemory.WriteBytes((address + 2).ToString("x"), BitConverter.GetBytes(item.Count));
		}

		public void WriteItems(CarryType type, List<Item> Items)
		{
			for (int index = 0; index < Items.Count; index++)
			{
				WriteItem(type, index, Items[index]);
			}
		}

		public void ClearItem(CarryType type)
		{
			var carry = Carrys[type];
			Byte[] buffer = new Byte[carry.ItemCount * 4];
			UInt64 address = mBaseAddress + carry.Distance;
			mMemory.WriteBytes(address.ToString("x"), buffer);
		}

		public void ClearItem(int page)
		{
			var carry = Carrys[CarryType.eBag];
			Byte[] buffer = new Byte[60 * 4];
			UInt64 address = mBaseAddress + carry.Distance + (UInt64)page * 240;
			mMemory.WriteBytes(address.ToString("x"), buffer);
		}
	}
}
