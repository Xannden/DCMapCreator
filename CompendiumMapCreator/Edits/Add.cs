using System.Collections.Generic;
using System.Diagnostics;
using CompendiumMapCreator.Data;

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
			Debug.Assert(this.Added != null, "Added is null");

			if (this.Added is AreaElement)
			{
				int index = 0;

				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (list[i] is AreaElement)
					{
						index = i;
						break;
					}
				}

				index = index == 0 ? 0 : index + 1;

				list.Insert(index, this.Added);
			}
			else
			{
				list.Add(this.Added);
			}
		}

		public override void Undo(IList<Element> list)
		{
			Debug.Assert(this.Added != null, "Added is null");

			list.Remove(this.Added);
		}
	}
}