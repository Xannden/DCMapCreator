using System.Windows;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("An unexpected error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

			e.Handled = true;
		}
	}
}