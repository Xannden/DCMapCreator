using System;
using System.Collections.Generic;
using System.Drawing;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class Layout
	{
		public Position Title { get; private set; }

		public Position Legend { get; private set; }

		public Position Map { get; }

		public Position Info { get; private set; }

		public Position[] Labels { get; private set; }

		public Layout(System.Drawing.Image map)
		{
			this.Map = new Position()
			{
				X = 0,
				Y = 0,
				Width = map.Width,
				Height = map.Height,
			};
		}

		public void AddTitle()
		{
			this.Title = new Position()
			{
				X = 0,
				Y = 0,
				Height = 28,
				Width = this.Map.Width,
			};
		}

		public void AddLegend(int legendHeight)
		{
			this.Legend = new Position()
			{
				X = 0,
				Y = 0,
				Width = 150,
				Height = legendHeight,
			};
		}

		public void AddInfo()
		{
			this.Info = new Position();
		}

		public void Finish(IList<Element> elements, Font font)
		{
			if (this.Title != null)
			{
				if (this.Legend != null)
				{
					this.Legend.Y = this.Title.Height;
					this.Title.Width += this.Legend.Width;
				}

				this.Map.Y = this.Title.Height;
			}

			if (this.Legend != null)
			{
				this.Map.X = this.Legend.Width;
			}

			if (this.Info != null)
			{
				if ((this.Legend?.Height ?? 0) > this.Map.Height)
				{
					this.Info.X = this.Legend.Width;
					this.Info.Width = this.Map.Width;
				}
				else
				{
					this.Info.X = 0;
					this.Info.Width = this.Map.Width + (this.Legend?.Width ?? 0);
				}

				this.Info.Y = this.Map.Y + this.Map.Height + 3;

				this.Labels = this.LayoutInfoList(elements, font);
			}
		}

		public int Width()
		{
			return (this.Legend?.Width ?? 0) + this.Map.Width;
		}

		public int Height()
		{
			int height = this.Title?.Height ?? 0;

			if (this.Legend?.Height > this.Map.Height + (this.Info?.Height ?? 0))
			{
				height += this.Legend.Height;
			}
			else
			{
				height += this.Map.Height + (this.Info?.Height ?? 0);
			}

			return height;
		}

		private Position[] LayoutInfoList(IList<Element> elements, Font font)
		{
			List<Label> labels = elements.GetLabels();

			//Calculate the number of columns
			int columns = this.Info.Width / 200;

			if (columns > labels.Count)
			{
				columns = labels.Count;
			}

			//Make sure there is always at least 1 column
			columns = Math.Max(columns, 1);

			int rows = (int)Math.Ceiling(labels.Count / (float)columns);

			float columnWidth = this.Info.Width / columns;

			Position[] positions = new Position[labels.Count];

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
				for (int i = 0; i < labels.Count; i++)
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

					SizeF size = g.MeasureString(labels[i].Text, font, new SizeF(columnWidth - 16, 1000), format);

					positions[i] = new Position()
					{
						X = x,
						Y = y,
						Width = (int)columnWidth,
					};

					rowHeight = Math.Max((int)Math.Ceiling(size.Height), rowHeight);

					x += (int)columnWidth;

					if (c == 0)
					{
						totalHeight += rowHeight;
					}
				}
			}

			this.Info.Height = totalHeight;

			return positions;
		}
	}

	public class Position
	{
		public int X { get; set; }

		public int Y { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public RectangleF ToRectF(int xOffset = 0, int yOffset = 0, int widthOffset = 0, int heightOffset = 0)
		{
			return new RectangleF(this.X + xOffset, this.Y + yOffset, this.Width + widthOffset, this.Height + heightOffset);
		}
	}
}