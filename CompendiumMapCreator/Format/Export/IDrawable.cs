using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace CompendiumMapCreator.Format.Export
{
	public interface IDrawable
	{
		Size Layout(int width, int height);

		void Draw(Graphics g, Point p);
	}

	public static class Exporter
	{
		public static void Run(string fileName, Image image, IList<Element> elements, bool addLegend, string title)
		{
			using (Font font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 8))
			{
				FullMap map = new FullMap(font, image, elements, addLegend, title);

				Size size = map.Layout(int.MaxValue, int.MaxValue);

				using (Bitmap result = new Bitmap(size.Width, size.Height))
				using (Graphics g = Graphics.FromImage(result))
				{
					map.Draw(g);

					result.Save(fileName, ImageFormat.Png);
				}
			}
		}
	}

	public class FullMap
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

		public FullMap(Font font, Image image, IList<Element> elements, bool addLegend, string title)
		{
			this.title = new Title(title);
			this.legend = new Legend(font, elements, addLegend);
			this.map = new Map(image, elements);
			this.infoList = new InfoList(elements.GetLabels(), font);
		}

		public void Draw(Graphics g)
		{
			g.FillRectangle(Brushes.Black, new RectangleF(new PointF(), this.size));

			this.title.Draw(g, this.titlePos);
			this.legend.Draw(g, this.legendPos);
			this.infoList.Draw(g, this.infoListPos);
			this.map.Draw(g, this.mapPos);
		}

		public Size Layout(int width, int height)
		{
			Size legendSize = this.legend.Layout(width, height);

			Size mapSize = this.map.Layout(width, height);

			int totalWidth = legendSize.Width + mapSize.Width;

			Size titleSize = this.title.Layout(totalWidth, height);

			this.titlePos = new Point(0, 0);
			this.legendPos = new Point(0, titleSize.Height);
			this.mapPos = new Point(legendSize.Width, titleSize.Height);

			int totalHeight = titleSize.Height;

			Size infoListSize;

			if (legendSize.Height > mapSize.Height)
			{
				infoListSize = this.infoList.Layout(mapSize.Width, height);
				this.infoListPos = new Point(legendSize.Width, this.mapPos.Y + mapSize.Height);

				totalHeight += Math.Max(legendSize.Height, infoListSize.Height + mapSize.Height);
			}
			else
			{
				infoListSize = this.infoList.Layout(totalWidth, height);
				this.infoListPos = new Point(0, this.mapPos.Y + mapSize.Height);

				totalHeight += mapSize.Height + infoListSize.Height;
			}

			this.size = new Size(totalWidth, totalHeight);

			return this.size;
		}
	}
}