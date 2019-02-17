using System;
using System.Drawing;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using MColor = System.Windows.Media.Color;

namespace CompendiumMapCreator.ViewModel
{
	public class DragSelect : IDrag
	{
		private ImagePoint start;

		public Rectangle Selection { get; private set; }

		public MColor Color => MColor.FromRgb(0, 120, 215);

		public DragSelect(ImagePoint start)
		{
			this.start = start;
		}

		public void Update(int x, int y, Project project)
		{
			this.Selection = Rectangle.FromLTRB(Math.Min(this.start.X, x), Math.Min(this.start.Y, y), Math.Max(this.start.X, x) + 1, Math.Max(this.start.Y, y) + 1);

			for (int i = 0; i < project.Selected.Count; i++)
			{
				if (!this.Selection.IntersectsWith(new Rectangle(project.Selected[i].X, project.Selected[i].Y, project.Selected[i].Width, project.Selected[i].Height)))
				{
					project.Selected.Remove(project.Selected[i]);
					i--;
				}
			}

			for (int i = 0; i < project.Elements.Count; i++)
			{
				if (project.Selected.Contains(project.Elements[i]))
				{
					continue;
				}

				if (this.Selection.IntersectsWith(new Rectangle(project.Elements[i].X, project.Elements[i].Y, project.Elements[i].Width, project.Elements[i].Height)))
				{
					project.Selected.Add(project.Elements[i]);
				}
			}
		}

		public (bool apply, Edit) End() => (false, null);
	}
}