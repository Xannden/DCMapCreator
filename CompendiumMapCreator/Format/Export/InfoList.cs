using System;
using System.Collections.Generic;
using System.Drawing;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Export
{
	public class InfoList : IDrawable
	{
		private readonly List<Label> labels;
		private readonly Font font;
		private Rectangle[] positions;
		private Size size;

		public InfoList(List<Label> labels, Font font)
		{
			this.labels = labels;
			this.font = font;
		}

		public void Draw(Graphics g, Point p)
		{
			if (this.labels == null)
			{
				return;
			}

			StringFormat format = new StringFormat()
			{
				FormatFlags = StringFormatFlags.LineLimit,
				Trimming = StringTrimming.None,
			};

			p.Offset(0, 3);

			for (int i = 0; i < this.labels.Count; i++)
			{
				Image icon = this.labels[i].Image;

				Rectangle area = this.positions[i].OffsetBy(p);

				g.DrawImage(icon.DrawingImage, area.X + (9 - (icon.Width / 2)), area.Y + (7 - (icon.Height / 2)));

				g.DrawString(this.labels[i].Text, this.font, Brushes.White, area.OffsetBy(xOffset: 16, widthOffset: -16), format);
			}

			g.DrawVerticalLine(p.X - 2, p.Y, p.Y + this.size.Height);
			g.DrawHorizontalLine(p.Y - 2, p.X - 1, p.X + this.size.Width);
		}

		public Size Layout(int width, int height)
		{
			if (this.labels == null)
			{
				return default;
			}

			// Calculate the number of columns
			int columns = width / 200;

			if (columns > this.labels.Count)
			{
				columns = this.labels.Count;
			}

			// Make sure there is always at least 1 column
			columns = Math.Max(columns, 1);

			float columnWidth = width / columns;

			this.positions = new Rectangle[this.labels.Count];

			StringFormat format = new StringFormat()
			{
				FormatFlags = StringFormatFlags.LineLimit,
				Trimming = StringTrimming.None,
			};

			int x = 0;
			int y = 0;
			int totalHeight = 0;

			using (Graphics g = Graphics.FromImage(new Bitmap(1, 1)))
			{
				for (int i = 0; i < this.labels.Count; i++)
				{
					int r = i / columns;
					int c = i % columns;

					int rowHeight = 20;

					if (c == 0 && i != 0)
					{
						y += rowHeight;
						x = 0;
						rowHeight = 20;
					}

					SizeF size = g.MeasureString(this.labels[i].Text, this.font, new SizeF(columnWidth - 16, 1000), format);

					this.positions[i] = new Rectangle(x, y, (int)columnWidth, 0);

					rowHeight = Math.Max((int)Math.Ceiling(size.Height), rowHeight);

					x += (int)columnWidth;

					if (c == 0)
					{
						totalHeight += rowHeight;
					}
				}
			}

			this.size = new Size(width, totalHeight);

			return this.size;
		}
	}
}