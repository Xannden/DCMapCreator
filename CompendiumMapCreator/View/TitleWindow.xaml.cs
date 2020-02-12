using System;
using System.Windows;

namespace CompendiumMapCreator.View
{
	/// <summary>
	/// Interaction logic for TitleWindow.xaml.
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
		}

		protected override void OnActivated(EventArgs e)
		{
			this.Box.Focus();
			this.Box.SelectAll();

			base.OnActivated(e);
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