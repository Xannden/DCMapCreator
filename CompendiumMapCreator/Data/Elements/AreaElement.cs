using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CompendiumMapCreator.Data
{
	public abstract class AreaElement : Element
	{
		public int AreaWidth { get; }

		public int AreaHeight { get; }

		public Color Color { get; }

		protected AreaElement(int areaWidth, int areaHeight, Color color, IconType type) : base(CreateImage(areaWidth, areaHeight, color), type)
		{
			this.AreaWidth = areaWidth;
			this.AreaHeight = areaHeight;
			this.Color = color;
		}

		private static Image CreateImage(int width, int height, Color color)
		{
			MemoryStream stream = new MemoryStream();

			using (Bitmap temp = new Bitmap(width, height))
			{
				using (Graphics g = Graphics.FromImage(temp))
				{
					g.FillRectangle(new SolidBrush(color), 0, 0, width, height);
				}

				temp.Save(stream, ImageFormat.Png);
			}

			return new Image(stream);
		}
	}
}