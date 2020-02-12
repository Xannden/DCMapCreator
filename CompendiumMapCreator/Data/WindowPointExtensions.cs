using System.Windows;
using CompendiumMapCreator.Controls;

namespace CompendiumMapCreator.Data
{
	public static class WindowPointExtensions
	{
		public static WindowPoint AsWindow(this Point p) => new WindowPoint((int)p.X, (int)p.Y);

		public static ImagePoint ToImage(this Point p, ZoomControl zoom) => p.AsWindow().ToImage(zoom.Scale, zoom.ViewportPositionX, zoom.ViewportPositionY);
	}
}