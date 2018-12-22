using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public class Copy : Edit
	{
		public Element Clone
		{
			get;
		}

		public Copy(Element source, ImagePoint? point)
		{
			switch (source)
			{
				case Label e:
					this.Clone = new Label("", e.Number);
					break;

				case Portal p:
					this.Clone = new Portal(p.Number);
					break;

				default:
					this.Clone = new Element(source.Type);
					break;
			}

			this.Clone.IsCopy = true;
			this.Clone.X = source.X;
			this.Clone.Y = source.Y;

			if (point != null)
			{
				this.Clone.X = point.Value.X;
				this.Clone.Y = point.Value.Y;
			}
		}

		public override void Apply(IList<Element> list) => list.Add(this.Clone);

		public override void Undo(IList<Element> list) => list.Remove(this.Clone);
	}
}