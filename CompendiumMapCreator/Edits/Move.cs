using System.Collections.Generic;

namespace CompendiumMapCreator.Edits
{
	public sealed class Move : Edit
	{
		public int X { get; }

		public int Y { get; }

		public IList<Element> Elements { get; }

		public Move(int x, int y, IList<Element> elements)
		{
			this.X = x;
			this.Y = y;
			this.Elements = elements;
		}

		public override void Apply(IList<Element> list)
		{
			for (int i = 0; i < this.Elements.Count; i++)
			{
				if (list.Contains(this.Elements[i]))
				{
					this.Elements[i].X += this.X;
					this.Elements[i].Y += this.Y;
				}
			}
		}

		public override void Undo(IList<Element> list)
		{
			for (int i = 0; i < this.Elements.Count; i++)
			{
				if (list.Contains(this.Elements[i]))
				{
					this.Elements[i].X -= this.X;
					this.Elements[i].Y -= this.Y;
				}
			}
		}
	}
}