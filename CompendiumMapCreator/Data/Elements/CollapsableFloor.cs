using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class CollapsibleFloor : AreaElement
	{
		public static Color DrawingColor = Color.FromArgb(127, 0, 0);

		public CollapsibleFloor(int width, int height) : base(width, height, DrawingColor, IconType.CollapsibleFloor)
		{
		}
	}
}