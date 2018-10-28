using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}

		public ViewModel.MainWindow ViewModel => (ViewModel.MainWindow)this.DataContext;

		public void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.Space) && this.ViewModel.IconType != IconType.None)
			{
				BitmapImage image = this.ViewModel.IconType.GetImage();

				Vector absulute = e.GetPosition(this.Zoom) - new System.Windows.Point((image.Width * this.Zoom.Scale) / 2, (image.Height * this.Zoom.Scale) / 2);

				int x = (int)((absulute.X - this.Zoom.ViewportPositionX) / this.Zoom.Scale);
				int y = (int)((absulute.Y - this.Zoom.ViewportPositionY) / this.Zoom.Scale);

				this.ViewModel.AddElement(new Element(x, y, this.ViewModel.IconType));
			}
		}

		public void Window_KeyDown(object sender, KeyEventArgs e) => this.ViewModel.Window_KeyDown(e);

		public void LoadImage(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel.LoadImage())
			{
				this.Zoom.Center();
			}
		}

		public void SaveImage(object sender, RoutedEventArgs e) => this.ViewModel.SaveImage();

		private void SaveProject_Click(object sender, RoutedEventArgs e) => this.ViewModel.SaveProject();

		private void LoadProject_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadProject();
			this.Zoom.Center();
		}
	}
}