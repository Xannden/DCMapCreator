using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Point start;
		private Point origin;
		private Element element;

		public MainWindow()
		{
			this.InitializeComponent();
		}

		public ViewModel.MainWindow ViewModel => (ViewModel.MainWindow)this.DataContext;

		public void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (((this.Zoom.Child as Grid)?.Children[1]).IsMouseCaptured)
			{
				((this.Zoom.Child as Grid)?.Children[1]).ReleaseMouseCapture();
				this.element = null;
			}
			else if (!Keyboard.IsKeyDown(Key.Space) && this.ViewModel.IconType != IconType.None)
			{
				BitmapImage image = this.ViewModel.IconType.GetImage();

				Vector absulute = e.GetPosition(this.Zoom) - new Point((image.Width * this.Zoom.Scale) / 2, (image.Height * this.Zoom.Scale) / 2);

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

		private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.Space) && this.ViewModel.IconType == IconType.None)
			{
				Point absulute = e.GetPosition(this.Zoom);

				int x = (int)((absulute.X - this.Zoom.ViewportPositionX) / this.Zoom.Scale);
				int y = (int)((absulute.Y - this.Zoom.ViewportPositionY) / this.Zoom.Scale);

				Element element = this.ViewModel.Select(new Point(x, y));

				if (element != null)
				{
					this.element = element;
					this.start = e.GetPosition(this.Zoom);
					this.origin = new Point(element.X, element.Y);
					((this.Zoom.Child as Grid)?.Children[1])?.CaptureMouse();
				}
			}
		}

		private void Border_MouseMove(object sender, MouseEventArgs e)
		{
			if (((this.Zoom.Child as Grid)?.Children[1]).IsMouseCaptured)
			{
				Vector v = this.start - (e.GetPosition(this.Zoom));

				v = new Vector(v.X / this.Zoom.Scale, v.Y / this.Zoom.Scale);

				this.element.X = (int)(this.origin.X - v.X);
				this.element.Y = (int)(this.origin.Y - v.Y);
			}
		}
	}
}