using System.Collections.Generic;
using System.Diagnostics;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public sealed class Add : Edit
	{
		public ElementVM Added { get; }

		public Add(ElementVM added)
		{
			this.Added = added;
		}

		public override void Apply(IList<ElementVM> list)
		{
			Debug.Assert(this.Added != null, "Added is null");

			if (this.Added.IsBackground)
			{
				int index = 0;

				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (list[i].IsBackground)
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

		public override void Undo(IList<ElementVM> list)
		{
			Debug.Assert(this.Added != null, "Added is null");

			list.Remove(this.Added);
		}
	}
}