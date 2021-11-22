using PInvoke;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace AutoClicker
{
	class KeyMouseHook : IDisposable
	{
		private User32.WindowsHookDelegate keyHookProc = null!;
		private User32.SafeHookHandle keyHookId = User32.SafeHookHandle.Null;

		private User32.WindowsHookDelegate mouseHookProc = null!;
		private User32.SafeHookHandle mouseHookId = User32.SafeHookHandle.Null;

		public void Dispose()
		{
			UnHook();
		}

		public void Hook(bool key, bool mouse)
		{
			if (key && keyHookId == User32.SafeHookHandle.Null) {
				keyHookProc = KeyHookProcedure;
				using var curProcess = Process.GetCurrentProcess();
				using ProcessModule curModule = curProcess.MainModule;
				keyHookId = User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, keyHookProc, Kernel32.GetModuleHandle(curModule?.ModuleName), 0);
			}
			if (mouse && mouseHookId == User32.SafeHookHandle.Null) {
				mouseHookProc = MouseHookProcedure;
				using var curProcess = Process.GetCurrentProcess();
				using ProcessModule curModule = curProcess.MainModule;
				mouseHookId = User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE_LL, mouseHookProc, Kernel32.GetModuleHandle(curModule?.ModuleName), 0);
			}
		}

		public void UnHook()
		{
			if (keyHookId != User32.SafeHookHandle.Null) {
				keyHookId.Close();
				keyHookId = User32.SafeHookHandle.Null;
			}
			if (mouseHookId != User32.SafeHookHandle.Null) {
				mouseHookId.Close();
				mouseHookId = User32.SafeHookHandle.Null;
			}
		}

		public int KeyHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0) {
				return User32.CallNextHookEx(keyHookId.DangerousGetHandle(), nCode, wParam, lParam);
			}
			bool cancel = false;
			Win32Api.KBDLLHOOKSTRUCT kb = Marshal.PtrToStructure<Win32Api.KBDLLHOOKSTRUCT>(lParam);
			switch ((User32.WindowMessage)wParam) {
			case User32.WindowMessage.WM_KEYDOWN:
			case User32.WindowMessage.WM_SYSKEYDOWN:
				if ((kb.dwFlags & Win32Api.KBDLLHOOKSTRUCTF.LLKHF_INJECTED) == 0) {
					cancel = OnKeyDownEvent(kb);
				}
				break;
			case User32.WindowMessage.WM_KEYUP:
			case User32.WindowMessage.WM_SYSKEYUP:
				if ((kb.dwFlags & Win32Api.KBDLLHOOKSTRUCTF.LLKHF_INJECTED) == 0) {
					cancel = OnKeyUpEvent(kb);
				}
				break;
			}
			if (cancel) {
				return (int)new IntPtr(0);
			} else {
				return User32.CallNextHookEx(keyHookId.DangerousGetHandle(), nCode, wParam, lParam);
			}
		}

		public int MouseHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0) {
				return User32.CallNextHookEx(keyHookId.DangerousGetHandle(), nCode, wParam, lParam);
			}
			Win32Api.MSLLHOOKSTRUCT s = Marshal.PtrToStructure<Win32Api.MSLLHOOKSTRUCT>(lParam);
			switch ((User32.WindowMessage)wParam) {
			case User32.WindowMessage.WM_MOUSEMOVE:
				break;
			case User32.WindowMessage.WM_LBUTTONDOWN:
				OnMouseDownEvent(MouseButton.Left, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_MBUTTONDOWN:
				OnMouseDownEvent(MouseButton.Middle, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_RBUTTONDOWN:
				OnMouseDownEvent(MouseButton.Right, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_XBUTTONDOWN:
				OnMouseDownEvent((s.mouseData >> 16) == 1 ? MouseButton.XButton1 : MouseButton.XButton2, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_LBUTTONUP:
				OnMouseUpEvent(MouseButton.Left, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_MBUTTONUP:
				OnMouseUpEvent(MouseButton.Middle, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_RBUTTONUP:
				OnMouseUpEvent(MouseButton.Right, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_XBUTTONUP:
				OnMouseUpEvent((s.mouseData >> 16) == 1 ? MouseButton.XButton1 : MouseButton.XButton2, s.pt.x, s.pt.y, 0, s.time);
				break;
			case User32.WindowMessage.WM_MOUSEWHEEL:
				OnMouseWheelEvent(MouseButton.Middle, s.pt.x, s.pt.y, (int)s.mouseData >> 16, s.time);
				break;
			}
			return User32.CallNextHookEx(keyHookId.DangerousGetHandle(), nCode, wParam, lParam);
		}

		public delegate void KeyEventHandler(object sender, KeyHookEventArgs e);
		public event KeyEventHandler KeyDownEvent;
		public event KeyEventHandler KeyUpEvent;

		private bool OnKeyDownEvent(Win32Api.KBDLLHOOKSTRUCT kb)
		{
			KeyHookEventArgs e = new(kb);
			KeyDownEvent?.Invoke(this, e);
			return e.Cancel;
		}

		private bool OnKeyUpEvent(Win32Api.KBDLLHOOKSTRUCT kb)
		{
			KeyHookEventArgs e = new(kb);
			KeyUpEvent?.Invoke(this, e);
			return e.Cancel;
		}

		public delegate void MouseEventHandler(object sender, MouseHookEventArgs e);
		public event MouseEventHandler MouseMoveEvent;
		public event MouseEventHandler MouseDownEvent;
		public event MouseEventHandler MouseUpEvent;
		public event MouseEventHandler MouseWheelEvent;
		private void OnMouseMoveEvent(MouseButton btn, int x, int y, int delta, uint time)
		{
			MouseMoveEvent?.Invoke(this, new MouseHookEventArgs(btn, x, y, delta, time));
		}
		private void OnMouseDownEvent(MouseButton btn, int x, int y, int delta, uint time)
		{
			MouseDownEvent?.Invoke(this, new MouseHookEventArgs(btn, x, y, delta, time));
		}
		private void OnMouseUpEvent(MouseButton btn, int x, int y, int delta, uint time)
		{
			MouseUpEvent?.Invoke(this, new MouseHookEventArgs(btn, x, y, delta, time));
		}
		private void OnMouseWheelEvent(MouseButton btn, int x, int y, int delta, uint time)
		{
			MouseWheelEvent?.Invoke(this, new MouseHookEventArgs(btn, x, y, delta, time));
		}
	}

	public class KeyHookEventArgs : CancelEventArgs
	{
		public Key VkCode { get; }
		public uint ScanCode { get; }
		public Win32Api.KBDLLHOOKSTRUCTF Flags { get; }
		public bool IsLLKHF_EXTENDED { get; }
		public uint Time { get; }

		public KeyHookEventArgs(Win32Api.KBDLLHOOKSTRUCT kb)
		{
			VkCode = KeyInterop.KeyFromVirtualKey((int)kb.wVk);
			ScanCode = kb.wScan;
			Flags = kb.dwFlags;
			IsLLKHF_EXTENDED = (Flags & Win32Api.KBDLLHOOKSTRUCTF.LLKHF_EXTENDED) != 0;
			Time = kb.time;
		}
	}

	public class MouseHookEventArgs : CancelEventArgs
	{
		public MouseButton Button { get; }
		public int X { get; }
		public int Y { get; }
		public int Delta { get; }
		public uint Time { get; }

		public MouseHookEventArgs(MouseButton btn, int x, int y, int delta, uint time)
		{
			Button = btn;
			X = x;
			Y = y;
			Delta = delta;
			Time = time;
		}
	}

}
