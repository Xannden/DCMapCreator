using System.Collections.Generic;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public abstract class Edit
	{
		public abstract void Apply(IList<ElementVM> list);

		public abstract void Undo(IList<ElementVM> list);
	}
}