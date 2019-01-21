using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class Trap : AreaElement
	{
		public static Color DrawingColor => Color.FromArgb(255, 47, 158, 0);

		public Trap(int width, int height) : base(width, height, Trap.DrawingColor, IconType.Trap)
		{
		}
	}
}