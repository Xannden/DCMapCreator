using System.Drawing;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using MColor = System.Windows.Media.Color;

namespace CompendiumMapCreator.ViewModel
{
	public interface IDrag
	{
		void Update(int x, int y, Project project);

		(bool apply, Edit edit) End();

		MColor Color { get; }

		Rectangle Selection { get; }
	}
}