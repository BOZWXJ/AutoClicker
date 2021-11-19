using PInvoke;
using System;
using System.Runtime.InteropServices;

namespace AutoClicker
{
	public static class Win32Api
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct KBDLLHOOKSTRUCT
		{
			/// <summary>
			/// 仮想キーコード 1-255
			/// </summary>
			public uint wVk;
			/// <summary>
			/// ハードウェアスキャンコード
			/// </summary>
			public uint wScan;
			/// <summary>
			/// 拡張キーフラグ、イベント挿入フラグ、コンテキストコード、および遷移状態フラグ。
			/// LLKHF_EXTENDED (KF_EXTENDED >> 8) 拡張キーフラグ
			/// LLKHF_LOWER_IL_INJECTED 0x00000002 イベント挿入フラグ（より低い整合性レベルのプロセス）
			/// LLKHF_INJECTED 0x00000010 イベント挿入フラグ（任意のプロセス）
			/// LLKHF_ALTDOWN (KF_ALTDOWN >> 8) Test the context code.
			/// LLKHF_UP (KF_UP >> 8) Test the transition-state flag.
			/// </summary>
			public KBDLLHOOKSTRUCTF dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		[Flags]
		public enum KBDLLHOOKSTRUCTF : uint
		{
			LLKHF_EXTENDED = 0x00000001,
			LLKHF_INJECTED = 0x00000010,
			LLKHF_ALTDOWN = 0x00000020,
			LLKHF_UP = 0x00000080,
			LLKHF_LOWER_IL_INJECTED = 0x00000002
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSLLHOOKSTRUCT
		{
			/// <summary>
			/// スクリーン座標
			/// </summary>
			public POINT pt;
			/// <summary>
			/// WM_MOUSEWHEEL の場合、上位ワードがホイールの回転量
			/// WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, or WM_NCXBUTTONDBLCLK の場合、上位ワードがXBUTTON
			/// </summary>
			public uint mouseData;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		public static IntPtr MakeLParam(int low, int high)
		{
			return new IntPtr((long)(((ulong)high << 16) | ((ulong)low & ~(0xffff_ffff_ffff_ffff << 16))));
			//return new IntPtr((long)(((ulong)high << (IntPtr.Size * 4)) | ((ulong)low & ~(0xffff_ffff_ffff_ffff << (IntPtr.Size * 4)))));
		}

	}
}
