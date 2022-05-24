using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DQB2ProcessEditor
{
	internal class KeyboardHook
	{
		public delegate void KeyEventHandler(int keyCode);
		public event KeyEventHandler? KeyDownEvent;

		private const int WH_KEYBOARD_LL = 0x000D;
		private const int WM_KEYDOWN = 0x0100;

		private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		private IntPtr mHhook = IntPtr.Zero;
		private HookProc? mProc;

		public void Hook()
		{
			UnHook();

			mProc = new HookProc(KeyboardProc);
			using (var process = Process.GetCurrentProcess())
			{
				using (var module = process?.MainModule)
				{
					IntPtr address = module?.BaseAddress ?? IntPtr.Zero;
					if (address == IntPtr.Zero) return;
					mHhook = SetWindowsHookEx(WH_KEYBOARD_LL, mProc, address, 0);
				}
			}
		}

		public void UnHook()
		{
			if (mHhook == IntPtr.Zero) return;

			UnhookWindowsHookEx(mHhook);
			mHhook = IntPtr.Zero;
		}

		private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (wParam == (IntPtr)WM_KEYDOWN)
			{
				int keyCode = Marshal.ReadInt32(lParam);
				KeyDownEvent?.Invoke(keyCode);
			}
			return CallNextHookEx(mHhook, nCode, wParam, lParam);
		}
	}
}
