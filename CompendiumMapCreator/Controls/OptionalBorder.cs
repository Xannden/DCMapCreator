using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompendiumMapCreator.Controls
{
	public sealed class OptionalBorder : Decorator
	{
		public static readonly DependencyProperty ShouldDisplayProperty = DependencyProperty.Register(nameof(ShouldDisplay), typeof(bool), typeof(OptionalBorder), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

		public bool ShouldDisplay
		{
			get => (bool)this.GetValue(ShouldDisplayProperty);
			set => this.SetValue(ShouldDisplayProperty, value);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (!this.ShouldDisplay)
			{
				return;
			}

			Rect area = new System.Windows.Rect(0.5, 0.5, this.DesiredSize.Width - 1, this.DesiredSize.Height - 1);
			Pen pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0xC0, 0xC0, 0xC0)), 1);

			// TopLeft
			drawingContext.DrawLine(pen, new Point(area.TopLeft.X - 0.5, area.TopLeft.Y), new Point(area.TopLeft.X + 3.5, area.TopLeft.Y));
			drawingContext.DrawLine(pen, area.TopLeft, new Point(area.TopLeft.X, area.TopLeft.Y + 3.5));

			// TopRight
			drawingContext.DrawLine(pen, new Point(area.TopRight.X + 0.5, area.TopRight.Y), new Point(area.TopRight.X - 3.5, area.TopRight.Y));
			drawingContext.DrawLine(pen, area.TopRight, new Point(area.TopRight.X, area.TopRight.Y + 3.5));

			// BottomLeft
			drawingContext.DrawLine(pen, new Point(area.BottomLeft.X - 0.5, area.BottomLeft.Y), new Point(area.BottomLeft.X + 3.5, area.BottomLeft.Y));
			drawingContext.DrawLine(pen, area.BottomLeft, new Point(area.BottomLeft.X, area.BottomLeft.Y - 3.5));

			// BottomRight
			drawingContext.DrawLine(pen, new Point(area.BottomRight.X + 0.5, area.BottomRight.Y), new Point(area.BottomRight.X - 3.5, area.BottomRight.Y));
			drawingContext.DrawLine(pen, area.BottomRight, new Point(area.BottomRight.X, area.BottomRight.Y - 3.5));
		}
	}
}