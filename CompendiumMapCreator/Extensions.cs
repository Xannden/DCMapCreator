using System.Collections.Generic;
using System.Drawing;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator
{
	public static class Extensions
	{
		public static void DrawVerticalLine(this Graphics g, int x, int y0, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x - 1, y0, x - 1, y1);
			g.DrawLine(new Pen(Color.White, 1f), x, y0, x, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x + 1, y0, x + 1, y1);
		}

		public static void DrawHorizontalLine(this Graphics g, int y, int x0, int x1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y - 1, x1, y - 1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y, x1, y);
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y + 1, x1, y + 1);
		}

		public static List<LabelElementVM> GetLabels(this IList<ElementVM> elements)
		{
			List<LabelElementVM> labels = new List<LabelElementVM>();

			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i] is LabelElementVM l && !string.IsNullOrEmpty(l.Text))
				{
					labels.Add(l);
				}
			}

			if (labels.Count == 0)
			{
				return null;
			}

			labels.Sort((lhs, rhs) => lhs.Number.CompareTo(rhs.Number));

			return labels;
		}

		public static Point OffsetBy(this Point p, Point o)
		{
			return new Point(p.X + o.X, p.Y + o.Y);
		}

		public static Point OffsetBy(this Point p, int x, int y)
		{
			return new Point(p.X + x, p.Y + y);
		}

		public static Point TopLeft(this Rectangle r)
		{
			return new Point(r.Left, r.Top);
		}

		public static Point TopRight(this Rectangle r)
		{
			return new Point(r.Right, r.Top);
		}

		public static Point BottomLeft(this Rectangle r)
		{
			return new Point(r.Left, r.Bottom);
		}

		public static Point BottomRight(this Rectangle r)
		{
			return new Point(r.Right, r.Bottom);
		}

		public static RectangleF OffsetBy(this Rectangle r, int xOffset = 0, int yOffset = 0, int widthOffset = 0, int heightOffset = 0)
		{
			return new RectangleF(r.X + xOffset, r.Y + yOffset, r.Width + widthOffset, r.Height + heightOffset);
		}

		public static Rectangle OffsetBy(this Rectangle r, Point p)
		{
			return new Rectangle(r.X + p.X, r.Y + p.Y, r.Width, r.Height);
		}

		public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color c) => System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
	}
}