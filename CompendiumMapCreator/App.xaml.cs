using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using CompendiumMapCreator.Data;
using Xannden.Update;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for App.xaml.
	/// </summary>
	public partial class App : Application
	{
		public static Config Config { get; } = new Config();

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			if (!Debugger.IsAttached)
			{
				File.AppendAllText("errorLog.txt", e.Exception.ToString());

				MessageBox.Show("An unexpected error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				e.Handled = true;
			}
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Updater updater = new Updater("https://raw.githubusercontent.com/Xannden/DCMapCreator/master/UpdateInfo.json", Assembly.GetExecutingAssembly());

			if (updater.NeedUpdate())
			{
				MessageBoxResult result = MessageBox.Show("There is an update available.\n\nDo you want to update?", "DDO Compendium Map Creator", MessageBoxButton.YesNo, MessageBoxImage.Information);

				if (result == MessageBoxResult.Yes)
				{
					updater.RunUpdate();

					this.Shutdown();
					return;
				}
			}

			Config.Init();

			if (File.Exists("errorLog.txt"))
			{
				File.Delete("errorLog.txt");
			}

			this.StartupUri = new Uri("View/MainWindow.xaml", UriKind.RelativeOrAbsolute);
		}
	}
}