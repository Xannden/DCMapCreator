using System.Collections.Generic;

namespace CompendiumMapCreator.ViewModel
{
	internal class LabelWindowVM
	{
		public LabelWindowVM()
		{
			this.Lables = new List<LabelElementVM>
			{
				new LabelElementVM(new Data.ElementId("label"), 1)
				{
					Text = "One",
				},
				new LabelElementVM(new Data.ElementId("label"), 2)
				{
					Text = "Two",
				},
				new LabelElementVM(new Data.ElementId("label"), 3)
				{
					Text = "Three",
				},
			};
		}

		public List<LabelElementVM> Lables { get; set; }
	}
}