using System.Reflection;
using System.Windows;

namespace CompendiumMapCreator.View
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml.
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}

		public string Version => Assembly.GetAssembly(typeof(AboutWindow)).GetName().Version.ToString();
	}
}