using System;
using System.Collections.Generic;
using System.Drawing;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Format.Export
{
	public class FullMap : IDrawable
	{
		private readonly Title title;
		private readonly Legend legend;
		private readonly Map map;
		private readonly InfoList infoList;
		private Point titlePos;
		private Point legendPos;
		private Point mapPos;
		private Point infoListPos;
		private Size size;

		public FullMap(Font font, Image image, IList<ElementVM> elements, bool addLegend, string title)
		{
			this.title = new Title(title);
			this.legend = new Legend(font, elements, addLegend);
			this.map = new Map(image, elements);
			this.infoList = new InfoList(elements.GetLabels(), font);
		}

		public void Draw(Graphics g, Point p)
		{
			g.FillRectangle(Brushes.Black, new RectangleF(default, this.size));

			this.title.Draw(g, this.titlePos.OffsetBy(p));
			this.legend.Draw(g, this.legendPos.OffsetBy(p));
			this.infoList.Draw(g, this.infoListPos.OffsetBy(p));
			this.map.Draw(g, this.mapPos.OffsetBy(p));
		}

		public Size Layout(int width, int height)
		{
			this.titlePos = new Point(0, 0);

			Size legendSize = this.legend.Layout(width, height);

			Size mapSize = this.map.Layout(width, height);

			int totalWidth = legendSize.Width + mapSize.Width;

			Size titleSize = this.title.Layout(totalWidth, height);

			this.legendPos = new Point(0, titleSize.Height);
			this.mapPos = new Point(legendSize.Width, titleSize.Height);

			int totalHeight = titleSize.Height;

			Size infoListSize;

			if (legendSize.Height > mapSize.Height)
			{
				this.infoListPos = new Point(legendSize.Width, this.mapPos.Y + mapSize.Height);

				infoListSize = this.infoList.Layout(mapSize.Width, height);

				totalHeight += Math.Max(legendSize.Height, infoListSize.Height + mapSize.Height);
			}
			else
			{
				this.infoListPos = new Point(0, this.mapPos.Y + mapSize.Height);

				infoListSize = this.infoList.Layout(totalWidth, height);

				totalHeight += mapSize.Height + infoListSize.Height;
			}

			this.size = new Size(totalWidth, totalHeight);

			return this.size;
		}
	}
}