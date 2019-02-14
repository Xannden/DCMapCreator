using System.Windows;

namespace CompendiumMapCreator.View
{
	/// <summary>
	/// Interaction logic for TitleWindow.xaml
	/// </summary>
	public partial class TitleWindow : Window
	{
		public string MapTitle
		{
			get; set;
		}

		public TitleWindow(string title)
		{
			this.InitializeComponent();

			this.MapTitle = title;
			this.DataContext = this;

			this.Box.Focus();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;

			this.Close();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;

			this.Close();
		}
	}
}