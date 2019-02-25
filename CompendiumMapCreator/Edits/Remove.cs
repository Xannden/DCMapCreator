using System.Collections.Generic;
using System.Linq;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public sealed class Remove : Edit
	{
		public List<(int index, Element element)> Removed { get; } = new List<(int, Element)>();

		public Remove(IList<Element> removed, IList<Element> list)
		{
			for (int i = 0; i < removed.Count; i++)
			{
				this.Removed.Add((list.IndexOf(removed[i]), removed[i]));

				if (removed[i] is NumberedElement n && !n.IsCopy)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j] is NumberedElement c && c.IsCopy && c.Number == n.Number && !this.Removed.Contains((j, list[j])))
						{
							this.Removed.Add((j, list[j]));
						}
					}
				}
			}

			this.Removed.Sort((lhs, rhs) => lhs.index.CompareTo(rhs.index));
		}

		public override void Apply(IList<Element> list)
		{
			for (int i = this.Removed.Count - 1; i >= 0; i--)
			{
				list.RemoveAt(this.Removed[i].index);
			}

			List<int> numbers = this.Removed.Where((i) => i.element is NumberedElement n && !n.IsCopy).Select((i) => ((NumberedElement)i.element).Number).ToList();

			numbers.Sort();

			numbers.Reverse();

			foreach (int number in numbers)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is NumberedElement n && n.Number > number)
					{
						n.Number--;
					}
				}
			}
		}

		public override void Undo(IList<Element> list)
		{
			List<int> numbers = this.Removed.Where((i) => i.element is NumberedElement n && !n.IsCopy).Select((i) => ((NumberedElement)i.element).Number).ToList();

			numbers.Sort();

			foreach (int number in numbers)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is NumberedElement n && n.Number >= number)
					{
						n.Number++;
					}
				}
			}

			for (int i = 0; i < this.Removed.Count; i++)
			{
				list.Insert(this.Removed[i].index, this.Removed[i].element);
			}
		}
	}
}