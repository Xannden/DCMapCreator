using System.Collections.Generic;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public sealed class ChangeLabel : Edit
	{
		public LabelElementVM Element { get; }

		public string Old { get; }

		public string New { get; }

		public ChangeLabel(LabelElementVM element, string newLabel)
		{
			this.Element = element;
			this.Old = element.Text;
			this.New = newLabel;
		}

		public override void Apply(IList<ElementVM> list)
		{
			this.Element.Text = this.New;
		}

		public override void Undo(IList<ElementVM> list)
		{
			this.Element.Text = this.Old;
		}
	}
}