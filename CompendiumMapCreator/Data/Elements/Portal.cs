using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class Portal : NumberedElement
	{
		public Portal(int number)
			: base(number, Color.FromArgb(0, 255, 255), IconType.Portal)
		{
		}
	}
}