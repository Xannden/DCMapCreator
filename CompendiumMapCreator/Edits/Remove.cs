using System.Collections.Generic;
using System.Linq;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public sealed class Remove : Edit
	{
		public List<(int index, ElementVM element)> Removed { get; } = new List<(int, ElementVM)>();

		public Remove(IList<ElementVM> removed, IList<ElementVM> list)
		{
			for (int i = 0; i < removed.Count; i++)
			{
				this.Removed.Add((list.IndexOf(removed[i]), removed[i]));

				if (removed[i] is NumberedElementVM n && !n.IsCopy)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j] is NumberedElementVM c && c.Id == n.Id && c.IsCopy && c.Number == n.Number && !this.Removed.Contains((j, list[j])))
						{
							this.Removed.Add((j, list[j]));
						}
					}
				}
			}
		}

		public override void Apply(IList<ElementVM> list)
		{
			for (int i = this.Removed.Count - 1; i >= 0; i--)
			{
				this.Removed[i] = (list.IndexOf(this.Removed[i].element), this.Removed[i].element);
				list.RemoveAt(this.Removed[i].index);
			}

			List<NumberedElementVM> elements = this.Removed.Where((i) => i.element is NumberedElementVM n && !n.IsCopy).Select((i) => i.element as NumberedElementVM).ToList();

			elements.Sort((lhs, rhs) => lhs.Number.CompareTo(rhs.Number));

			elements.Reverse();

			foreach (NumberedElementVM element in elements)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is NumberedElementVM n && n.Id == element.Id && n.Number > element.Number)
					{
						n.Number--;
					}
				}
			}
		}

		public override void Undo(IList<ElementVM> list)
		{
			List<NumberedElementVM> elements = this.Removed.Where((i) => i.element is NumberedElementVM n && !n.IsCopy).Select((i) => i.element as NumberedElementVM).ToList();

			elements.Sort();

			foreach (NumberedElementVM element in elements)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is NumberedElementVM n && n.Id == element.Id && n.Number >= element.Number)
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