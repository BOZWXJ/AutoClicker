using AutoClicker.Properties;
using Livet;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
			RefreshTitleList();

			hook = new KeyMouseHook();
			hook.MouseDownEvent += Hook_MouseDownEvent;
			hook.Hook(false, true);
		}

		public void Dispose()
		{
			AutoClickStop();
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

		public void SelectPosition()
		{
			SelectPositionBusy = true;
		}

		private void Hook_MouseDownEvent(object sender, MouseHookEventArgs e)
		{
			if (SelectWindowBusy | SelectPositionBusy) {
				IntPtr hWnd = User32.WindowFromPoint(new POINT() { x = e.X, y = e.Y });
				hWnd = User32.GetAncestor(hWnd, User32.GetAncestorFlags.GA_ROOT);
				string str = User32.GetWindowText(hWnd);
				if (SelectWindowBusy) {
					Target = str;
				}
				if (str.Contains(Target)) {
					// ウィンドウのクライアント座標
					User32.GetWindowRect(hWnd, out RECT rect);
					X = e.X - rect.left;
					Y = e.Y - rect.top;
				}
				SelectWindowBusy = false;
				SelectPositionBusy = false;
			}
		}

		CancellationTokenSource tokenSource;
		Task task;
		public void AutoClickStart()
		{
			AutoClickBusy = true;
			tokenSource = new CancellationTokenSource();
			task = Task.Run(() => AutoClickMethod(tokenSource.Token), tokenSource.Token);
		}

		public void AutoClickStop()
		{
			tokenSource?.Cancel();
			task?.Wait();
			task = null;
			tokenSource = null;
			AutoClickBusy = false;
		}

		private async void AutoClickMethod(CancellationToken token)
		{
			IntPtr hWnd = IntPtr.Zero;
			// MainWindowTitle に Target 文字列を含むウィンドウハンドル
			foreach (var proc in Process.GetProcesses()) {
				if (proc.MainWindowTitle.Contains(Target)) {
					hWnd = proc.MainWindowHandle;
				}
			}
			if (hWnd == IntPtr.Zero) {
				AutoClickBusy = false;
				return;
			}
			// ウィンドウ位置
			RECT wRect;
			User32.GetWindowRect(hWnd, out wRect);
			// コントロールのウィンドウハンドル
			hWnd = User32.WindowFromPoint(new POINT() { x = X + wRect.left, y = Y + wRect.top });
			// コントロールの位置
			RECT cRect;
			User32.GetWindowRect(hWnd, out cRect);
			// ウィンドウのクライアント座標からコントロールのクライアント座標へ変換
			int x = X + wRect.left - cRect.left;
			int y = Y + wRect.top - cRect.top;
			IntPtr lParam = Win32Api.MakeLParam(x, y);
			IntPtr wParam = new IntPtr(0);

			while (!token.IsCancellationRequested) {
				User32.SendMessage(hWnd, User32.WindowMessage.WM_MOUSEMOVE, wParam, lParam);
				User32.SendMessage(hWnd, User32.WindowMessage.WM_LBUTTONDOWN, wParam, lParam);
				User32.SendMessage(hWnd, User32.WindowMessage.WM_MOUSEMOVE, wParam, lParam);
				User32.SendMessage(hWnd, User32.WindowMessage.WM_LBUTTONUP, wParam, lParam);
				try {
					await Task.Delay(Interval, token).ConfigureAwait(false);
				} catch (TaskCanceledException) { }
			}
		}

		public void RefreshTitleList()
		{
			TitleList.Clear();
			foreach (var proc in Process.GetProcesses()) {
				if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle)) {
					TitleList.Add(proc.MainWindowTitle);
				}
			}
		}

		private ObservableCollection<string> _TitleList = new ObservableCollection<string>();
		public ObservableCollection<string> TitleList
		{
			get => _TitleList;
			set => RaisePropertyChangedIfSet(ref _TitleList, value);
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

		private bool _SelectPositionBusy;
		public bool SelectPositionBusy
		{
			get => _SelectPositionBusy;
			set => RaisePropertyChangedIfSet(ref _SelectPositionBusy, value);
		}

		private bool _AutoClickBusy;
		public bool AutoClickBusy
		{
			get => _AutoClickBusy;
			set => RaisePropertyChangedIfSet(ref _AutoClickBusy, value);
		}

	}
}
