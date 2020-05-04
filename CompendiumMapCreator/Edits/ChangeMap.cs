using System.Collections.Generic;
using CompendiumMapCreator.Format;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Edits
{
	public class ChangeMap : Edit
	{
		private readonly Project project;
		private readonly Image oldImage;
		private readonly Image newImage;

		public ChangeMap(Project project, Image newImage)
		{
			this.project = project;
			this.oldImage = this.project.Image;
			this.newImage = newImage;
		}

		public override void Apply(IList<ElementVM> list)
		{
			this.project.Image = this.newImage;
		}

		public override void Undo(IList<ElementVM> list)
		{
			this.project.Image = this.oldImage;
		}
	}
}