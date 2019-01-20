using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CompendiumMapCreator.Data
{
	public class Trap : Element
	{
		public int TrapWidth { get; set; }

		public int TrapHeight { get; set; }

		public Trap(int x, int y, int width, int height) : base(null, IconType.Trap)
		{
			this.X = x;
			this.Y = y;
			this.TrapWidth = width;
			this.TrapHeight = height;
			this.Image = this.CreateImage();
		}

		private Image CreateImage()
		{
			MemoryStream stream = new MemoryStream();

			using (Bitmap temp = new Bitmap(this.TrapWidth, this.TrapHeight))
			{
				using (Graphics g = Graphics.FromImage(temp))
				{
					g.FillRectangle(new SolidBrush(Color.FromArgb(255, 47, 158, 00)), 0, 0, this.TrapWidth, this.TrapHeight);
				}

				temp.Save(stream, ImageFormat.Png);
			}

			return new Image(stream);
		}
	}
}