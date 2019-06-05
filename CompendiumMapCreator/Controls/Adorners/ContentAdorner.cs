using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CompendiumMapCreator.Controls
{
	public class ContentAdorner : Adorner
	{
		private readonly ContentPresenter contentPresenter;
		private readonly VisualCollection visuals;
		private AdornerLayer layer;

		public ContentAdorner(UIElement element) : base(element)
		{
			this.visuals = new VisualCollection(this);
			this.contentPresenter = new ContentPresenter();
			this.visuals.Add(this.contentPresenter);
		}

		public ContentAdorner(UIElement element, Visual content) : this(element)
		{
			this.Content = content;
		}

		public object Content
		{
			get => this.contentPresenter.Content;
			set => this.contentPresenter.Content = value;
		}

		protected override int VisualChildrenCount => this.visuals.Count;

		public void Hide()
		{
			this.Visibility = Visibility.Hidden;
		}

		public void Show()
		{
			if (this.Content == null)
			{
				this.Content = this.MakeContent(this.AdornedElement);
			}

			if (this.layer == null)
			{
				this.layer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
				this.layer.Add(this);
			}

			this.Visibility = Visibility.Visible;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			this.contentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
			return this.contentPresenter.RenderSize;
		}

		protected override Visual GetVisualChild(int index) => this.visuals[index];

		protected virtual Visual MakeContent(UIElement adornedElement)
		{
			return null;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			this.contentPresenter.Measure(constraint);
			return this.contentPresenter.DesiredSize;
		}
	}
}