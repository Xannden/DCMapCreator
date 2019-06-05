using System.Windows;
using System.Windows.Controls;

namespace CompendiumMapCreator.Controls
{
	public class SelectionAdornerCrome : Control
	{
		static SelectionAdornerCrome()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionAdornerCrome), new FrameworkPropertyMetadata(typeof(SelectionAdornerCrome)));
		}
	}
}