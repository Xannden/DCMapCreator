using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for ZoomControl.xaml
	/// </summary>
	public partial class ZoomControl : UserControl, INotifyPropertyChanged
	{
		private double scale = 1;
		private Point start;
		private Point origin;
		private int viewportPositionX;
		private int viewportPositionY;

		public double Scale
		{
			get => this.scale;
			private set
			{
				if (this.CC.ActualHeight * value < 10 || this.CC.ActualWidth * value < 10)
				{
					return;
				}

				this.scale = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Scale)));
			}
		}

		public int ViewportPositionX
		{
			get => this.viewportPositionX;
			private set
			{
				this.viewportPositionX = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ViewportPositionX)));
			}
		}

		public int ViewportPositionY
		{
			get => this.viewportPositionY;
			private set
			{
				this.viewportPositionY = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ViewportPositionY)));
			}
		}

		public object Child
		{
			get => this.GetValue(ChildProperty);
			set => this.SetValue(ChildProperty, value);
		}

		public int ChildWidth
		{
			get => (int)this.GetValue(ChildWidthProperty);
			set => this.SetValue(ChildWidthProperty, value);
		}

		public int ChildHeight
		{
			get => (int)this.GetValue(ChildHeightProperty);
			set => this.SetValue(ChildHeightProperty, value);
		}

		public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(object), typeof(ZoomControl));
		public static readonly DependencyProperty ChildWidthProperty = DependencyProperty.Register(nameof(ChildWidth), typeof(int), typeof(ZoomControl));
		public static readonly DependencyProperty ChildHeightProperty = DependencyProperty.Register(nameof(ChildHeight), typeof(int), typeof(ZoomControl));

		public event PropertyChangedEventHandler PropertyChanged;

		public ZoomControl()
		{
			this.InitializeComponent();
		}

		public void Center()
		{
			this.ViewportPositionX = (int)((this.Grid.ActualWidth / 2) - (this.ChildWidth * this.Scale / 2));
			this.ViewportPositionY = (int)((this.Grid.ActualHeight / 2) - (this.ChildHeight * this.Scale / 2));
		}

		private void Grid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				Point relative = e.GetPosition(this.CC);
				double absuluteX = (relative.X * this.Scale) + this.ViewportPositionX;
				double absuluteY = (relative.Y * this.Scale) + this.ViewportPositionY;

				this.Scale += (e.Delta * 0.001) + (e.Delta > 0 ? this.Scale * 0.05 : -(this.Scale * 0.05));

				this.ViewportPositionX = (int)(absuluteX - (relative.X * this.Scale));
				this.ViewportPositionY = (int)(absuluteY - (relative.Y * this.Scale));
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift))
			{
				this.ViewportPositionX += (int)(e.Delta * 0.5);
			}
			else
			{
				this.ViewportPositionY += (int)(e.Delta * 0.5);
			}
		}

		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.Space))
			{
				TranslateTransform tt = (this.CC.RenderTransform as TransformGroup)?.Children.First(c => c is TranslateTransform) as TranslateTransform;

				this.start = e.GetPosition(this.Grid);
				this.origin = new Point(tt.X, tt.Y);
				this.Grid.CaptureMouse();
			}
		}

		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this.Grid.ReleaseMouseCapture();

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.Grid.IsMouseCaptured)
			{
				Vector v = this.start - e.GetPosition(this.Grid);
				this.ViewportPositionX = (int)(this.origin.X - v.X);
				this.ViewportPositionY = (int)(this.origin.Y - v.Y);
			}
		}
	}
}