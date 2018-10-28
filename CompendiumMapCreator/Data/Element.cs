using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Element : INotifyPropertyChanged
	{
		private bool isSelected;
		private int x;
		private int y;

		public Element(int x, int y, IconType type)
		{
			this.X = x;
			this.Y = y;
			this.Type = type;
			this.Image = this.Type.GetImage();
		}

		public int X
		{
			get => this.x;
			set
			{
				this.x = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.X)));
			}
		}

		public int Y
		{
			get => this.y;
			set
			{
				this.y = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Y)));
			}
		}

		public int Width => this.Image.PixelWidth;

		public int Height => this.Image.PixelHeight;

		public BitmapImage Image { get; }

		public IconType Type { get; }

		public bool IsSelected
		{
			get => this.isSelected;
			set
			{
				this.isSelected = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool Contains(Point point) => this.X < point.X && (this.X + this.Width) >= point.X && this.Y < point.Y && (this.Y + this.Width) >= point.Y;
	}
}