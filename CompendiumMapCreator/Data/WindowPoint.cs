using System;
using CompendiumMapCreator.Controls;

namespace CompendiumMapCreator.Data
{
	public struct WindowPoint
	{
		public int X
		{
			get;
		}

		public int Y
		{
			get;
		}

		public WindowPoint(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public ImagePoint ToImage(double scale, int x, int y) => new ImagePoint((int)Math.Floor((this.X - x) / scale), (int)Math.Floor((this.Y - y) / scale));

		public ImagePoint ToImage(ZoomControl zoom) => this.ToImage(zoom.Scale, zoom.ViewportPositionX, zoom.ViewportPositionY);

		public static WindowPoint operator -(WindowPoint lhs, WindowPoint rhs) => new WindowPoint(lhs.X - rhs.X, lhs.Y - rhs.Y);

		public static WindowPoint operator +(WindowPoint lhs, WindowPoint rhs) => new WindowPoint(lhs.X + rhs.X, lhs.Y + rhs.Y);
	}
}