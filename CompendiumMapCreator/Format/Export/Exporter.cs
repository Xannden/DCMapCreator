using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace CompendiumMapCreator.Format.Export
{
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
					map.Draw(g, new Point());

					result.Save(fileName, ImageFormat.Png);
				}
			}
		}
	}
}