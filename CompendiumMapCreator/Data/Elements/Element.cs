using System.ComponentModel;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public class Element : INotifyPropertyChanged
	{
		private int x;
		private int y;
		private double opacity = 1;
		private Image image;

		public Element(IconType type)
		{
			this.X = 0;
			this.Y = 0;
			this.Type = type;
			this.image = Image.GetImageFromResources(type.GetImageFile());
		}

		protected Element(Image image, IconType type)
		{
			this.X = 0;
			this.Y = 0;
			this.Type = type;
			this.image = image;
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

		public int Width => this.Image.Width;

		public int Height => this.Image.Height;

		public Image Image
		{
			get => this.image;
			set
			{
				this.image = value;

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Image)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Width)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Height)));
			}
		}

		public IconType Type
		{
			get;
		}

		public double Opacity
		{
			get => this.opacity;

			set
			{
				this.opacity = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Opacity)));
			}
		}

		public bool IsCopy
		{
			get; set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool Contains(ImagePoint point) => this.X <= point.X && (this.X + this.Width) > point.X && this.Y <= point.Y && (this.Y + this.Height) > point.Y;
	}
}