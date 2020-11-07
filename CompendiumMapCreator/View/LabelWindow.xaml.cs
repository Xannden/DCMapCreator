using System.Collections.Generic;
using System.Windows;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.View
{
	/// <summary>
	/// Interaction logic for LabelWIndow.xaml
	/// </summary>
	public partial class LabelWindow : Window
	{
		public LabelWindow()
		{
			this.Lables = new List<LabelElementVM>
			{
				new LabelElementVM(new Data.ElementId("label"), 1),
				new LabelElementVM(new Data.ElementId("label"), 2),
				new LabelElementVM(new Data.ElementId("label"), 3),
			};

			this.DataContext = this;

			this.InitializeComponent();
		}

		public List<LabelElementVM> Lables { get; set; }
	}
}