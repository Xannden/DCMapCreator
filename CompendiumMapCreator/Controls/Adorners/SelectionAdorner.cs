using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CompendiumMapCreator.Controls
{
	internal class SelectionAdorner : Adorner
	{
		private readonly SelectionAdornerCrome crome;
		private readonly VisualCollection visuals;
		private AdornerLayer layer;

		public SelectionAdorner(FrameworkElement element) : base(element)
		{
			this.crome = new SelectionAdornerCrome
			{
				DataContext = element
			};
			this.visuals = new VisualCollection(this)
			{
				this.crome,
			};
		}

		private new MapItem AdornedElement => base.AdornedElement as MapItem;

		protected override int VisualChildrenCount => this.visuals.Count;

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			for (int i = 0; i < this.VisualChildrenCount; i++)
			{
				UIElement visual = this.GetVisualChild(i) as UIElement;

				visual.RenderTransform = new ScaleTransform(1 / this.AdornedElement.Scale, 1 / this.AdornedElement.Scale);
			}

			return base.GetDesiredTransform(transform);
		}

		public void Hide()
		{
			this.Visibility = Visibility.Hidden;
		}

		public void Show()
		{
			if (this.layer == null)
			{
				this.layer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
				this.layer.Add(this);
			}

			this.Visibility = Visibility.Visible;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Size size = this.AdornedElement.DesiredSize;
			Size scaledSize = new Size(size.Width * this.AdornedElement.Scale, size.Height * this.AdornedElement.Scale);

			this.crome.Arrange(new Rect((scaledSize.Width - finalSize.Width) / 2, (scaledSize.Height - finalSize.Height) / 2, finalSize.Width, finalSize.Height));
			return finalSize;
		}

		protected override Visual GetVisualChild(int index) => this.visuals[index];

		protected override Size MeasureOverride(Size constraint)
		{
			this.crome.Measure(constraint);

			return this.crome.DesiredSize;

			//return new Size(this.AdornedElement.RenderSize.Width * this.AdornedElement.Scale, this.AdornedElement.RenderSize.Height * this.AdornedElement.Scale);
		}
	}
}