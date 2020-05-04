using System;
using System.Drawing;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using MColor = System.Windows.Media.Color;

namespace CompendiumMapCreator.ViewModel
{
	public class DragAreaElement : IDrag
	{
		private readonly ElementId element;

		private readonly ImagePoint start;

		public Rectangle Selection { get; private set; }

		public MColor Color
		{
			get
			{
				byte[] color = App.Config.GetElement(this.element).Color;

				return MColor.FromRgb(color[0], color[1], color[2]);
			}
		}

		public DragAreaElement(ImagePoint start, ElementId element)
		{
			this.start = start;
			this.element = element;
		}

		public void Update(int x, int y, Project project) => this.Selection = Rectangle.FromLTRB(Math.Min(this.start.X, x), Math.Min(this.start.Y, y), Math.Max(this.start.X, x) + 1, Math.Max(this.start.Y, y) + 1);

		public (bool apply, Edit edit) End()
		{
			if (this.Selection.Height == 0 || this.Selection.Width == 0)
			{
				return (false, null);
			}

			ElementVM element = ElementVM.CreateAreaElement(this.element, this.Selection.Width, this.Selection.Height);

			element.X = this.Selection.Left;
			element.Y = this.Selection.Top;

			return (true, new Add(element));
		}
	}
}