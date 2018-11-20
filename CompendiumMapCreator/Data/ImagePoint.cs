namespace CompendiumMapCreator.Data
{
	public struct ImagePoint
	{
		public int X { get; }

		public int Y { get; }

		public ImagePoint(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public static ImagePoint operator -(ImagePoint lhs, ImagePoint rhs) => new ImagePoint(lhs.X - rhs.X, lhs.Y - rhs.Y);

		public static ImagePoint operator +(ImagePoint lhs, ImagePoint rhs) => new ImagePoint(lhs.X + rhs.X, lhs.Y + rhs.Y);

		public WindowPoint ToWindow(double scale, int x, int y) => new WindowPoint((int)((this.X * scale) + x), (int)((this.Y * scale) + y));
	}
}