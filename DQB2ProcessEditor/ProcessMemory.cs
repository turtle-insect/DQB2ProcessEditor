using System;
using System.Collections.Generic;

namespace DQB2ProcessEditor
{
	internal class ProcessMemory
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

		public bool CalcPlayerAddress(ProcessInfo info)
		{
			int pID = mMemory.GetProcIdFromName(info.Name);
			if (mMemory.OpenProcess(pID) == false) return false;

			// 主人公のアドレス取得からインベントリのアドレス取得
			// v1.7.3a
			// mov rax, [DQB2.exe + 0x137E490]
			// mov rcx, [rax + 60]

			Byte[] buffer = mMemory.ReadBytes(info.Name + $".exe+{info.Address},0x60", 8);
			mBaseAddress = BitConverter.ToUInt64(buffer, 0);
			if (mBaseAddress == 0) return false;
			return true;
		}

		public List<Item> ReadItem(CarryType type)
		{
			var Items = new List<Item>();
			if (mBaseAddress == 0) return Items;
			
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

		public UInt64 CalcBluePrintAddress(int index)
		{
			// DQB2.exe + 4A194A - 48 8B 05 3FCBED00     - mov rax,[DQB2.exe+137E490] { (23A253CE500) }
			// DQB2.exe + 4A1951 - 4C 8B 50 60 - mov r10,[rax+60]
			// DQB2.exe + 4A1955 - 49 81 C2 30701600 - add r10,00167030 { 1470512 }
			// DQB2.exe + 4A1965 - 48 69 C8 08000300 - imul rcx,rax,00030008 { 196616 }
			//		rax: 4
			// DQB2.exe + 4A196C - 4C 03 D1 - add r10,rcx
			UInt64 address = mBaseAddress + 0x167030;
			address += (UInt64)(index) * 0x30008;
			return address;
		}

		public void WriteBluePrint(int index, ref Byte[] buffer)
		{
			UInt64 address = CalcBluePrintAddress(index);
			mMemory.WriteBytes(address.ToString("x"), buffer);
		}

		public Byte[] ReadBluePrint(int index)
		{
			UInt64 address = CalcBluePrintAddress(index);
			return mMemory.ReadBytes(address.ToString("x"), 0x30008);
		}

		public void ClearBluePrint(int index)
		{
			UInt64 address = CalcBluePrintAddress(index);
			mMemory.WriteBytes(address.ToString("x"), new Byte[0x30008]);
		}
	}
}
