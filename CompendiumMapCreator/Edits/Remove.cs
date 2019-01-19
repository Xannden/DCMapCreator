using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public sealed class Remove : Edit
	{
		private readonly List<NumberedElement> numbered = new List<NumberedElement>();

		public List<Element> Removed { get; }

		public Remove(List<Element> removed)
		{
			this.Removed = removed;

			for (int i = 0; i < this.Removed.Count; i++)
			{
				if (this.Removed[i] is NumberedElement n && !n.IsCopy)
				{
					this.numbered.Add(n);
				}
			}

			this.numbered.Sort((lhs, rhs) => rhs.Number.CompareTo(lhs.Number));

			this.Removed.RemoveAll((e) => e is NumberedElement && !e.IsCopy);
		}

		public override void Apply(IList<Element> list)
		{
			foreach (Element element in this.Removed)
			{
				list.Remove(element);
			}

			foreach (NumberedElement element in this.numbered)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].GetType() != element.GetType())
					{
						continue;
					}

					if (list[i] is NumberedElement n && n.Number > element.Number && !list[i].IsCopy)
					{
						n.Number--;
					}
				}
				list.Remove(element);
			}
		}

		public override void Undo(IList<Element> list)
		{
			foreach (Element element in this.Removed)
			{
				list.Add(element);
			}

			for (int i = this.numbered.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].GetType() != this.numbered[i].GetType())
					{
						continue;
					}

					if (list[j] is NumberedElement n && n.Number >= this.numbered[i].Number && !list[j].IsCopy)
					{
						n.Number++;
					}
				}
				list.Add(this.numbered[i]);
			}
		}
	}
}