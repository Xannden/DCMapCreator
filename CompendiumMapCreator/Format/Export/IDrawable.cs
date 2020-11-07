using System.Drawing;

namespace CompendiumMapCreator.Format.Export
{
	public interface IDrawable
	{
		Size Layout(int width, int height);

		void Draw(Graphics g, Point p);
	}
}