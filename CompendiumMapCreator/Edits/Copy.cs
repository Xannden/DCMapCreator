using System.Collections.Generic;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Edits
{
	public class Copy : Edit
	{
		public Element Clone { get; }

		public Copy(Element source, ImagePoint? point)
		{
			if (source is NumberedElement e)
			{
				this.Clone = new NumberedElementCopy(e);
			}
			else
			{
				this.Clone = new ElementCopy(source);
			}

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