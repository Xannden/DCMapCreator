using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public class Copy : Edit
	{
		private List<Element> Clones { get; }

		public Copy(IList<Element> source, ImagePoint point)
		{
			this.Clones = new List<Element>();

			(int x_min, int y_min, int x_max, int y_max) = (int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

			for (int i = 0; i < source.Count; i++)
			{
				if (source[i].X < x_min)
				{
					x_min = source[i].X;
				}

				if (source[i].Y < y_min)
				{
					y_min = source[i].Y;
				}

				if (source[i].X + source[i].Width > x_max)
				{
					x_max = source[i].X + source[i].Width;
				}

				if (source[i].Y + source[i].Height > y_max)
				{
					y_max = source[i].Y + source[i].Height;
				}
			}

			int mid_x = x_min + ((x_max - x_min) / 2);
			int mid_y = y_min + ((y_max - y_min) / 2);

			for (int i = 0; i < source.Count; i++)
			{
				Element clone = null;

				switch (source[i])
				{
					case Label e:
						clone = new Label("", e.Number);
						break;

					case Portal p:
						clone = new Portal(p.Number);
						break;

					default:
						clone = new Element(source[i].Type);
						break;
				}

				clone.IsCopy = true;

				clone.X = source[i].X - mid_x + point.X;
				clone.Y = source[i].Y - mid_y + point.Y;

				this.Clones.Add(clone);
			}
		}

		public override void Apply(IList<Element> list)
		{
			for (int i = 0; i < this.Clones.Count; i++)
			{
				list.Add(this.Clones[i]);
			}
		}

		public override void Undo(IList<Element> list)
		{
			for (int i = 0; i < this.Clones.Count; i++)
			{
				list.Remove(this.Clones[i]);
			}
		}
	}
}