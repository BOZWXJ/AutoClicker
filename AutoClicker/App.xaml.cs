using Livet;
using Reactive.Bindings;
using Reactive.Bindings.Schedulers;
using System.Windows;

namespace AutoClicker
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			DispatcherHelper.UIDispatcher = Dispatcher;
			//AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			ReactivePropertyScheduler.SetDefault(new ReactivePropertyWpfScheduler(Dispatcher));
		}

		// Application level error handling
		//private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		//{
		//    //TODO: Logging
		//    MessageBox.Show(
		//        "Something errors were occurred.",
		//        "Error",
		//        MessageBoxButton.OK,
		//        MessageBoxImage.Error);
		//
		//    Environment.Exit(1);
		//}
	}
}
