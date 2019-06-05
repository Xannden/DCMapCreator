using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CompendiumMapCreator.Controls
{
	public class MapControl : MultiSelector
	{
		#region Dependency Properties

		public static readonly DependencyProperty MapProperty = DependencyProperty.Register(nameof(Map), typeof(Image), typeof(MapControl), new PropertyMetadata(Map_Changed));
		public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(double), typeof(MapControl), new PropertyMetadata(1.0, Scale_Changed));
		public static readonly DependencyProperty ToolProperty = DependencyProperty.Register(nameof(Tool), typeof(ITool), typeof(MapControl));
		public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(MapControl));
		public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(MapControl));

		#endregion Dependency Properties

		private readonly DragHelper drag;

		static MapControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MapControl), new FrameworkPropertyMetadata(typeof(MapControl)));
		}

		public MapControl()
		{
			this.CanSelectMultipleItems = true;

			this.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas)));
			this.drag = new DragHelper(this);
			this.drag.DragStarted += this.Drag_DragStarted;
			this.drag.DragDelta += this.Drag_DragDelta;
			this.drag.DragCompleted += this.Drag_DragCompleted;
			this.IsTabStop = false;
			this.Loaded += this.MapControl_Loaded;
		}

		public event EventHandler ScaleChanged;

		public Image Map
		{
			get => (Image)this.GetValue(MapProperty);
			set => this.SetValue(MapProperty, value);
		}

		public double Scale
		{
			get => (double)this.GetValue(ScaleProperty);

			set
			{
				if (this.Map?.Height * value < 1 || this.Map?.Width * value < 1)
				{
					return;
				}

				this.SetValue(ScaleProperty, value);
			}
		}

		public ITool Tool
		{
			get => (ITool)this.GetValue(ToolProperty);
			set => this.SetValue(YProperty, value);
		}

		public double X
		{
			get => (double)this.GetValue(XProperty);
			set => this.SetValue(XProperty, value);
		}

		public double Y
		{
			get => (double)this.GetValue(YProperty);
			set => this.SetValue(YProperty, value);
		}

		public void Center()
		{
			if (this.Map == null)
			{
				return;
			}

			this.X = (this.ActualWidth - (this.Map.Width * this.Scale)) / 2;
			this.Y = (this.ActualHeight - (this.Map.Height * this.Scale)) / 2;
		}

		public void Select(object obj)
		{
			if (!this.SelectedItems.Contains(obj))
			{
				this.BeginUpdateSelectedItems();

				if (!Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					this.SelectedItems.Clear();
				}

				this.SelectedItems.Add(obj);

				this.EndUpdateSelectedItems();
			}
		}

		protected override DependencyObject GetContainerForItemOverride() => new MapItem(this);

		protected override bool IsItemItsOwnContainerOverride(object item) => item is MapItem;

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				return;
			}

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				Point window = e.GetPosition(this);

				double imageX = (window.X - this.X) / this.Scale;
				double imageY = (window.Y - this.Y) / this.Scale;

				this.Scale += (e.Delta * 0.001) + (e.Delta > 0 ? this.Scale * 0.05 : -(this.Scale * 0.05));

				this.X = (int)(window.X - (imageX * this.Scale));
				this.Y = (int)(window.Y - (imageY * this.Scale));
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift))
			{
				this.X += e.Delta * 0.5 / this.Scale;
			}
			else
			{
				this.Y += e.Delta * 0.5 / this.Scale;
			}
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			MapItem mapItem = element as MapItem;

			mapItem.SetBinding(Canvas.LeftProperty, "X");
			mapItem.SetBinding(Canvas.TopProperty, "Y");

			base.PrepareContainerForItemOverride(element, item);
		}

		private static void Map_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as MapControl)?.Center();
		}

		private static void Scale_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as MapControl)?.ScaleChanged?.Invoke(d, EventArgs.Empty);
		}

		private void Drag_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (e.XChangeTotal == 0 && e.YChangeTotal == 0 && !Keyboard.IsKeyDown(Key.Space))
			{
				this.UnselectAll();
			}
		}

		private void Drag_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.Space))
			{
				this.X += e.XChange;
				this.Y += e.YChange;
			}
		}

		private void Drag_DragStarted(object sender, DragStartedEventArgs e)
		{
		}

		private void MapControl_Loaded(object sender, RoutedEventArgs e) => this.Center();
	}
}