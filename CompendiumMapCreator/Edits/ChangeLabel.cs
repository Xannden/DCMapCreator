using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public sealed class ChangeLabel : Edit
	{
		public Label Element { get; }

		public string Old { get; }

		public string New { get; }

		public ChangeLabel(Label element, string newLabel)
		{
			this.Element = element;
			this.Old = element.Text;
			this.New = newLabel;
		}

		public override void Apply(IList<Element> list) => this.Element.Text = this.New;

		public override void Undo(IList<Element> list) => this.Element.Text = this.Old;
	}
}