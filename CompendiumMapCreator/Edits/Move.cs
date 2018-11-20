using System.Collections.Generic;

namespace CompendiumMapCreator.Edits
{
	public sealed class Move : Edit
	{
		public int X { get; }

		public int Y { get; }

		public Element Element { get; }

		public Move(int x, int y, Element element)
		{
			this.X = x;
			this.Y = y;
			this.Element = element;
		}

		public override void Apply(IList<Element> list)
		{
			if (list.Contains(this.Element))
			{
				this.Element.X += this.X;
				this.Element.Y += this.Y;
			}
		}

		public override void Undo(IList<Element> list)
		{
			if (list.Contains(this.Element))
			{
				this.Element.X -= this.X;
				this.Element.Y -= this.Y;
			}
		}
	}
}