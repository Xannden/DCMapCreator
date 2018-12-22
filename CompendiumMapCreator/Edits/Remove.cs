using System.Collections.Generic;
using System.Diagnostics;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public sealed class Remove : Edit
	{
		public Element Removed
		{
			get;
		}

		public Remove(Element removed)
		{
			this.Removed = removed;
		}

		public override void Apply(IList<Element> list)
		{
			Debug.Assert(this.Removed != null);

			if (this.Removed is NumberedElement r && !this.Removed.IsCopy)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].GetType() != this.Removed.GetType())
					{
						continue;
					}

					if (list[i] is NumberedElement e && e.Number > r.Number)
					{
						e.Number--;
					}
				}
			}

			list.Remove(this.Removed);
		}

		public override void Undo(IList<Element> list)
		{
			Debug.Assert(this.Removed != null);

			if (this.Removed is NumberedElement r && !this.Removed.IsCopy)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].GetType() != this.Removed.GetType())
					{
						continue;
					}

					if (list[i] is NumberedElement e && e.Number >= r.Number)
					{
						e.Number++;
					}
				}
			}

			list.Add(this.Removed);
		}
	}
}