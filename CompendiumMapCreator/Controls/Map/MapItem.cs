using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CompendiumMapCreator.Controls
{
	public class MapItem : ContentControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(MapItem), new FrameworkPropertyMetadata(IsSelectedChanged));
		public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(int), typeof(MapItem), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(int), typeof(MapItem), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		private readonly SelectionAdorner adorner;
		private readonly DragHelper drag;
		private (int X, int Y) point;

		static MapItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
		}

		public MapItem(MapControl control)
		{
			this.drag = new DragHelper(this);
			this.drag.DragStarted += this.Drag_DragStarted;
			this.drag.DragDelta += this.Drag_DragDelta;
			this.drag.DragCompleted += this.Drag_DragCompleted;

			this.adorner = new SelectionAdorner(this);
			this.IsTabStop = false;

			control.ScaleChanged += (s, e) =>
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Scale)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ScaledWidth)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ScaledHeight)));
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsSelected
		{
			get => (bool)this.GetValue(IsSelectedProperty);
			set => this.SetValue(IsSelectedProperty, value);
		}

		public new MapControl Parent => ItemsControl.ItemsControlFromItemContainer(this) as MapControl;

		public double Scale => this.Parent?.Scale ?? 1;

		public double ScaledHeight => this.Scale * this.ActualHeight;

		public double ScaledWidth => this.Scale * this.ActualWidth;

		public int X
		{
			get => (int)this.GetValue(XProperty);
			set => this.SetValue(XProperty, value);
		}

		public int Y
		{
			get => (int)this.GetValue(YProperty);
			set => this.SetValue(YProperty, value);
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			this.Parent.Select(this);
			base.OnPreviewMouseLeftButtonDown(e);
		}

		private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MapItem item)
			{
				if ((bool)e.NewValue)
				{
					item.adorner.Show();
				}
				else
				{
					item.adorner.Hide();
				}
			}
		}

		private void Drag_DragCompleted(object sender, DragCompletedEventArgs e)
		{
		}

		private void Drag_DragDelta(object sender, DragDeltaEventArgs e)
		{
			foreach (MapItem item in this.Parent.SelectedItems)
			{
				item.X += (int)Math.Floor(e.Position.X - this.point.X);
				item.Y += (int)Math.Floor(e.Position.Y - this.point.Y);
			}
		}

		private void Drag_DragStarted(object sender, DragStartedEventArgs e)
		{
			this.point = ((int)e.X, (int)e.Y);
		}
	}
}