using System.Collections.ObjectModel;
using CompendiumMapCreator.ViewModel.Elements;

namespace CompendiumMapCreator.ViewModel
{
	public class ElementList
	{
		public ElementList()
		{
			this.BasicElements = new ObservableCollection<ElementVM>();
			this.LabelElements = new ObservableCollection<LabelElementVM>();
			this.BackgroundElements = new ObservableCollection<BackgroundElementVM>();
		}

		public ObservableCollection<BackgroundElementVM> BackgroundElements { get; protected set; }

		public ObservableCollection<LabelElementVM> LabelElements { get; protected set; }

		public ObservableCollection<ElementVM> BasicElements
		{
			get; protected set;
		}
	}
}