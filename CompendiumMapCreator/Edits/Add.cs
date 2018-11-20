using System.Collections.Generic;
using System.Diagnostics;

namespace CompendiumMapCreator.Edits
{
	public sealed class Add : Edit
	{
		public Element Added { get; }

		public Add(Element added)
		{
			this.Added = added;
		}

		public override void Apply(IList<Element> list)
		{
			Debug.Assert(this.Added != null);

			list.Add(this.Added);
		}

		public override void Undo(IList<Element> list)
		{
			Debug.Assert(this.Added != null);

			list.Remove(this.Added);
		}
	}
}