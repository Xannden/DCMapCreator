using System;
using System.Collections.Generic;
using System.Drawing;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Format.Export
{
	public class Map : IDrawable
	{
		private readonly Image image;
		private readonly IList<ElementVM> elements;
		private Point offset;
		private Size size;

		public Map(Image image, IList<ElementVM> elements)
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
				this.DrawElement(g, this.elements[i], position);
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

		private void DrawElement(Graphics g, ElementVM e, Point offset)
		{
			Point p = new Point(e.X + offset.X, e.Y + offset.Y);

			if (e is AreaElementVM a)
			{
				Brush pen = new SolidBrush(a.Color);

				g.FillRectangle(pen, new Rectangle(p, new Size(a.Width, a.Height)));
			}
			else
			{
				Bitmap image = new Bitmap(e.Image.DrawingImage);

				// TODO: Make this work with any size of icon
				switch (e.Rotation)
				{
					case 90:
					case -270:
						p = new Point(e.X + offset.X + 2, e.Y + offset.Y - 2);
						image.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;

					case 180:
					case -180:
						image.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;

					case 270:
					case -90:
						p = new Point(e.X + offset.X + 2, e.Y + offset.Y - 2);
						image.RotateFlip(RotateFlipType.Rotate270FlipNone);
						break;
				}

				g.DrawImage(image, p);
			}

			if (e.Optional)
			{
				Rectangle rect = new Rectangle(p, new Size(e.Width, e.Height));

				using SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0xC0, 0xC0, 0xC0));
				using Pen pen = new Pen(brush, 1);

				// TopLeft
				Point topLeft = rect.TopLeft();
				g.DrawLine(pen, topLeft.OffsetBy(-1, -1), topLeft.OffsetBy(2, -1));
				g.DrawLine(pen, topLeft.OffsetBy(-1, -1), topLeft.OffsetBy(-1, 2));

				// TopRight
				Point topRight = rect.TopRight();
				g.DrawLine(pen, topRight.OffsetBy(-3, -1), topRight.OffsetBy(0, -1));
				g.DrawLine(pen, topRight.OffsetBy(0, -1), topRight.OffsetBy(0, 2));

				// BottomLeft
				Point bottomLeft = rect.BottomLeft();
				g.DrawLine(pen, bottomLeft.OffsetBy(-1, 0), bottomLeft.OffsetBy(2, 0));
				g.DrawLine(pen, bottomLeft.OffsetBy(-1, 0), bottomLeft.OffsetBy(-1, -3));

				// BottomRight
				Point bottomRight = rect.BottomRight();
				g.DrawLine(pen, bottomRight.OffsetBy(-3, 0), bottomRight.OffsetBy(0, 0));
				g.DrawLine(pen, bottomRight.OffsetBy(-0, 0), bottomRight.OffsetBy(0, -3));
			}
		}
	}
}