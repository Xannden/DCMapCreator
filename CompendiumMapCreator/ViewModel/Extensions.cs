namespace CompendiumMapCreator.ViewModel
{
	public static class Extensions
	{
		public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color c) => System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
	}
}