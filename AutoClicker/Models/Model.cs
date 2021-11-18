using AutoClicker.Properties;
using Livet;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClicker.Models
{
	public class Model : NotificationObject, IDisposable
	{
		KeyMouseHook hook;

		public Model()
		{
			Target = Settings.Default.Target;
			X = Settings.Default.X;
			Y = Settings.Default.Y;
			Interval = Settings.Default.Interval;

			hook = new KeyMouseHook();
			hook.MouseDownEvent += Hook_MouseDownEvent;
			hook.Hook(false, true);
		}

		public void Dispose()
		{
			hook.Dispose();
		}

		public void Save()
		{
			Settings.Default.Target = Target;
			Settings.Default.X = X;
			Settings.Default.Y = Y;
			Settings.Default.Interval = Interval;
			Settings.Default.Save();
		}

		public void SelectWindow()
		{
			SelectWindowBusy = true;
		}

		private void Hook_MouseDownEvent(object sender, MouseHookEventArgs e)
		{
			if (SelectWindowBusy) {
				// Window 取得
				IntPtr hWnd = User32.WindowFromPoint(new POINT() { x = e.X, y = e.Y });
				hWnd = User32.GetAncestor(hWnd, User32.GetAncestorFlags.GA_ROOT);
				string str = User32.GetWindowText(hWnd);
				if (string.IsNullOrEmpty(str)) {
					str = User32.GetClassName(hWnd);
				}
				RECT rect;
				User32.GetWindowRect(hWnd, out rect);
				Target = str;
				X = e.X - rect.left;
				Y = e.Y - rect.top;
				SelectWindowBusy = false;
			}
		}

		Task task;
		public void AutoClickStart()
		{
			AutoClickBusy = true;
			task = Task.Run(AutoClickMethod);
		}

		public void AutoClickStop()
		{
			AutoClickBusy = false;
			task.Wait();
			task = null;
		}

		private async void AutoClickMethod()
		{
			IntPtr hWnd = IntPtr.Zero;
			foreach (var proc in Process.GetProcesses()) {
				if (proc.MainWindowTitle.Contains(Target)) {
					hWnd = proc.MainWindowHandle;
				}
			}
			if (hWnd == IntPtr.Zero) {
				AutoClickBusy = false;
				return;
			}
			while (AutoClickBusy) {
				TestCount++;
				User32.SendMessage(hWnd, User32.WindowMessage.WM_LBUTTONDOWN, new IntPtr(1), Win32Api.MakeLParam(X, Y));
				User32.SendMessage(hWnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), Win32Api.MakeLParam(X, Y));
				await Task.Delay(Interval).ConfigureAwait(false);
			}
		}


		private int _TestCount; // debug:
		public int TestCount
		{
			get => _TestCount;
			set => RaisePropertyChangedIfSet(ref _TestCount, value);
		}

		private string _Target;
		public string Target
		{
			get => _Target;
			set => RaisePropertyChangedIfSet(ref _Target, value);
		}

		private int _X;
		public int X
		{
			get => _X;
			set => RaisePropertyChangedIfSet(ref _X, value);
		}

		private int _Y;
		public int Y
		{
			get => _Y;
			set => RaisePropertyChangedIfSet(ref _Y, value);
		}

		private int _Interval;
		public int Interval
		{
			get => _Interval;
			set => RaisePropertyChangedIfSet(ref _Interval, value);
		}

		private bool _SelectWindowBusy;
		public bool SelectWindowBusy
		{
			get => _SelectWindowBusy;
			set => RaisePropertyChangedIfSet(ref _SelectWindowBusy, value);
		}

		private bool _AutoClickBusy;
		public bool AutoClickBusy
		{
			get => _AutoClickBusy;
			set => RaisePropertyChangedIfSet(ref _AutoClickBusy, value);
		}

	}
}
