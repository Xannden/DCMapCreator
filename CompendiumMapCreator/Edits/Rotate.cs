using System.Collections.Generic;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public sealed class Rotate : Edit
	{
		private readonly ElementVM element;
		private readonly bool clockwise;

		public Rotate(ElementVM element, bool clockwise)
		{
			this.element = element;
			this.clockwise = clockwise;
		}

		public override void Apply(IList<ElementVM> list)
		{
			if (this.clockwise)
			{
				this.element.RotateCW();
			}
			else
			{
				this.element.RotateCC();
			}
		}

		public override void Undo(IList<ElementVM> list)
		{
			if (this.clockwise)
			{
				this.element.RotateCC();
			}
			else
			{
				this.element.RotateCW();
			}
		}
	}
}