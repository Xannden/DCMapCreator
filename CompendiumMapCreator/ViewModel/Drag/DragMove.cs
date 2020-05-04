using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using MColor = System.Windows.Media.Color;

namespace CompendiumMapCreator.ViewModel
{
	public class DragMove : IDrag
	{
		private (int X, int Y) Change { get; set; }

		private IList<ElementVM> Elements { get; }

		private ImagePoint Start { get; }

		private ImagePoint[] Offsets { get; }

		public DragMove(IList<ElementVM> elements, ImagePoint start)
		{
			this.Elements = elements;
			this.Start = start;
			this.Offsets = this.Elements.Select(e => start - new ImagePoint(e.X, e.Y)).ToArray();

			for (int i = 0; i < this.Elements.Count; i++)
			{
				this.Elements[i].Opacity = 0.25;
			}
		}

		public void Update(int x, int y, Project project)
		{
			for (int i = 0; i < this.Elements.Count; i++)
			{
				this.Elements[i].X = x - this.Offsets[i].X;
				this.Elements[i].Y = y - this.Offsets[i].Y;
			}

			this.Change = (x - this.Start.X, y - this.Start.Y);
		}

		public (bool apply, Edit edit) End()
		{
			Edit result = null;

			(int xChanged, int yChanged) = this.Change;

			if (xChanged != 0 || yChanged != 0)
			{
				result = new Move(xChanged, yChanged, this.Elements);
			}

			for (int i = 0; i < this.Elements.Count; i++)
			{
				this.Elements[i].Opacity = 1;
			}

			return (false, result);
		}

		public MColor Color => MColor.FromRgb(0, 0, 0);

		public Rectangle Selection => Rectangle.Empty;
	}
}