using System.Windows;

namespace CompendiumMapCreator.Data
{
	public struct WindowPoint
	{
		public int X { get; }

		public int Y { get; }

		public WindowPoint(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public ImagePoint ToImage(double scale, int x, int y) => new ImagePoint((int)((this.X - x) / scale), (int)((this.Y - y) / scale));
	}

	public static class WindowPointExtensions
	{
		public static WindowPoint AsWindow(this Point p) => new WindowPoint((int)p.X, (int)p.Y);

		public static ImagePoint ToImage(this Point p, double scale, int x, int y) => p.AsWindow().ToImage(scale, x, y);
	}
}