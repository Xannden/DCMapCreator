using System.Drawing;
using System.Drawing.Text;

namespace CompendiumMapCreator.Format.Export
{
	public class Title : IDrawable
	{
		private readonly string title;
		private Size size;

		public Title(string title)
		{
			this.title = title;
		}

		public void Draw(Graphics g, Point p)
		{
			if (!string.IsNullOrEmpty(this.title))
			{
				using (Font titleFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 15))
				{
					StringFormat format = new StringFormat()
					{
						Alignment = StringAlignment.Center,
					};

					g.DrawString(this.title, titleFont, Brushes.White, new RectangleF(p, this.size), format);
				}

				g.DrawHorizontalLine(p.Y + this.size.Height - 2, p.X, p.X + this.size.Width);
			}
		}

		public Size Layout(int width, int height)
		{
			if (!string.IsNullOrEmpty(this.title))
			{
				this.size = new Size(width, 28);
			}

			return this.size;
		}
	}
}