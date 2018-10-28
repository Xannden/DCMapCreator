using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Element
	{
		public Element(int x, int y, IconType type)
		{
			this.X = x;
			this.Y = y;
			this.Type = type;
			this.Image = this.Type.GetImage();
		}

		public int X { get; set; }

		public int Y { get; set; }

		public int Width => this.Image.PixelWidth;

		public int Height => this.Image.PixelHeight;

		public BitmapImage Image { get; }

		public IconType Type { get; }
	}
}