using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class MapRelocate : NumberedElement
	{
		public MapRelocate(int number)
			: base(number, Color.FromArgb(204, 38, 255), IconType.MapRelocate)
		{
		}
	}
}