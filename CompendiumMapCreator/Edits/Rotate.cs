using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public sealed class Rotate : Edit
	{
		private readonly Entrance element;
		private readonly bool clockwise;

		public Rotate(Entrance element, bool clockwise)
		{
			this.element = element;
			this.clockwise = clockwise;
		}

		public override void Apply(IList<Element> list)
		{
			if (this.clockwise)
			{
				this.element.Rotate_Clockwise();
			}
			else
			{
				this.element.Rotate_CounterClockwise();
			}
		}

		public override void Undo(IList<Element> list)
		{
			if (this.clockwise)
			{
				this.element.Rotate_CounterClockwise();
			}
			else
			{
				this.element.Rotate_Clockwise();
			}
		}
	}
}