using System.ComponentModel;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public class Element : INotifyPropertyChanged
	{
		private Image image;
		private double opacity = 1;
		private bool optional;
		private int x;
		private int y;

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

		public event PropertyChangedEventHandler PropertyChanged;

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

		public bool IsCopy
		{
			get; set;
		}

		public bool IsOptional
		{
			get => this.optional;

			set
			{
				this.optional = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsOptional)));
			}
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

		public virtual string ToolTip => null;

		public IconType Type
		{
			get;
		}

		public int Width => this.Image.Width;

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

		public bool Contains(ImagePoint point) => this.X <= point.X && (this.X + this.Width) > point.X && this.Y <= point.Y && (this.Y + this.Height) > point.Y;

		public ImagePoint Position()
		{
			return new ImagePoint(this.X, this.Y);
		}

		protected void OnPropertyChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}