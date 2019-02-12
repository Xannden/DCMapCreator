namespace CompendiumMapCreator.Data
{
	public struct ImagePoint
	{
		public int X { get; set; }

		public int Y { get; set; }

		public ImagePoint(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public static ImagePoint operator -(ImagePoint lhs, ImagePoint rhs) => new ImagePoint(lhs.X - rhs.X, lhs.Y - rhs.Y);

		public static ImagePoint operator +(ImagePoint lhs, ImagePoint rhs) => new ImagePoint(lhs.X + rhs.X, lhs.Y + rhs.Y);

		public static ImagePoint operator /(ImagePoint lhs, int rhs) => new ImagePoint(lhs.X / rhs, lhs.Y / rhs);

		public WindowPoint ToWindow(ZoomControl zoom) => new WindowPoint((int)((this.X * zoom.Scale) + zoom.ViewportPositionX), (int)((this.Y * zoom.Scale) + zoom.ViewportPositionY));
	}
}