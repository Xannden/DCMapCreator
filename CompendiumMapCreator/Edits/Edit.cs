using System.Collections.Generic;

namespace CompendiumMapCreator.Edits
{
	public abstract class Edit
	{
		public abstract void Apply(IList<Element> list);

		public abstract void Undo(IList<Element> list);
	}
}