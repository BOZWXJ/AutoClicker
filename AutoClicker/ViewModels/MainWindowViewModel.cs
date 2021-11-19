using AutoClicker.Models;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace AutoClicker.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
		private Model model;
		public ReactivePropertySlim<string> Target { get; }
		public ReactivePropertySlim<int> X { get; }
		public ReactivePropertySlim<int> Y { get; }
		public ReactivePropertySlim<int> Interval { get; }
		public ReactivePropertySlim<bool> SelectWindowBusy { get; }
		public ReactivePropertySlim<bool> AutoClickBusy { get; }

		public ReadOnlyReactiveProperty<bool> TextBoxEnable { get; }
		public ReactiveProperty<bool> ToggleButtonIsChecked { get; }
		public ReadOnlyReactiveProperty<bool> ToggleButtonEnable { get; }

		public ReactiveCommand SelectWindow { get; } = new ReactiveCommand();
		public ReactiveCommand AutoClickStartStop { get; } = new ReactiveCommand();
		public ReactiveCommand WindowClose { get; } = new ReactiveCommand();

		public MainWindowViewModel()
		{
			model = new Model();
			Target = model.ToReactivePropertySlimAsSynchronized(p => p.Target);
			X = model.ToReactivePropertySlimAsSynchronized(p => p.X);
			Y = model.ToReactivePropertySlimAsSynchronized(p => p.Y);
			Interval = model.ToReactivePropertySlimAsSynchronized(p => p.Interval);
			SelectWindowBusy = model.ToReactivePropertySlimAsSynchronized(p => p.SelectWindowBusy);
			AutoClickBusy = model.ToReactivePropertySlimAsSynchronized(p => p.AutoClickBusy);

			TextBoxEnable = SelectWindowBusy.CombineLatest(AutoClickBusy, (x, y) => !(x | y)).ToReadOnlyReactiveProperty();
			ToggleButtonIsChecked = AutoClickBusy.ToReactivePropertyAsSynchronized(p => p.Value, (IObservable<bool> ox) => ox.Where(p => !p).Select(p => p), (IObservable<bool> ox) => ox.Where(p => false).Select(p => p));
			ToggleButtonEnable = SelectWindowBusy.Inverse().ToReadOnlyReactiveProperty();

			// イベント
			SelectWindow.Subscribe(model.SelectWindow);
			AutoClickStartStop.Subscribe(p => {
				if (model.AutoClickBusy) {
					model.AutoClickStop();
				} else {
					model.AutoClickStart();
				}
			});
			WindowClose.Subscribe(p => {
				if (model.AutoClickBusy) {
					model.AutoClickStop();
				}
				model.Save();
			});
		}

		public void Initialize()
		{
		}

	}
}
