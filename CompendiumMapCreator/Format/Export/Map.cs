using System;
using System.Collections.Generic;
using System.Drawing;

namespace CompendiumMapCreator.Format.Export
{
	public class Map : IDrawable
	{
		private readonly Image image;
		private readonly IList<Element> elements;
		private Point offset;
		private Size size;

		public Map(Image image, IList<Element> elements)
		{
			this.image = image;
			this.elements = elements;
		}

		public void Draw(Graphics g, Point p)
		{
			Point position = p.OffsetBy(this.offset);

			g.DrawImage(this.image.DrawingImage, position);

			for (int i = 0; i < this.elements.Count; i++)
			{
				g.DrawImage(this.elements[i].Image.DrawingImage, this.elements[i].X + position.X, this.elements[i].Y + position.Y);
			}

			g.DrawVerticalLine(p.X - 2, p.Y, p.Y + this.size.Height);
		}

		public Size Layout(int width, int height)
		{
			(int x, int y) min = (0, 0);
			(int x, int y) max = (this.image.Width, this.image.Height);

			for (int i = 0; i < this.elements.Count; i++)
			{
				min.x = Math.Min(min.x, this.elements[i].X);
				min.y = Math.Min(min.y, this.elements[i].Y);
				max.x = Math.Max(max.x, this.elements[i].X + this.elements[i].Width);
				max.y = Math.Max(max.y, this.elements[i].Y + this.elements[i].Height);
			}

			Rectangle bounding = Rectangle.FromLTRB(min.x, min.y, max.x, max.y);

			this.offset = new Point(Math.Abs(min.x) + 5, Math.Abs(min.y) + 5);

			this.size = new Size(bounding.Width + 10, bounding.Height + 10);

			return this.size;
		}
	}
}