using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompendiumMapCreator.Controls
{
	public class ZoomControl : ContentControl
	{
		static ZoomControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ZoomControl)));
		}

		public ZoomControl()
		{
			this.ClipToBounds = true;
		}

		private Point start;
		private Point origin;

		public double Scale
		{
			get => (double)this.GetValue(ScaleProperty);

			private set
			{
				if (this.ActualHeight * value < 10 || this.ActualWidth * value < 10)
				{
					return;
				}

				this.SetValue(ScaleProperty, value);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Scale)));
				this.ScaleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int ViewportPositionX
		{
			get => (int)this.GetValue(ViewportPositionXProperty);

			private set
			{
				this.SetValue(ViewportPositionXProperty, value);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ViewportPositionX)));
				this.ViewportChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int ViewportPositionY
		{
			get => (int)this.GetValue(ViewportPositionYProperty);

			private set
			{
				this.SetValue(ViewportPositionYProperty, value);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ViewportPositionY)));
				this.ViewportChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int ChildWidth
		{
			get => (int)this.GetValue(ChildWidthProperty);
			set => this.SetValue(ChildWidthProperty, value);
		}

		public int ChildHeight
		{
			get => (int)this.GetValue(ChildHeightProperty);
			set => this.SetValue(ChildHeightProperty, value);
		}

		public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(double), typeof(ZoomControl), new PropertyMetadata(1.0));
		public static readonly DependencyProperty ViewportPositionXProperty = DependencyProperty.Register(nameof(ViewportPositionX), typeof(int), typeof(ZoomControl));
		public static readonly DependencyProperty ViewportPositionYProperty = DependencyProperty.Register(nameof(ViewportPositionY), typeof(int), typeof(ZoomControl));
		public static readonly DependencyProperty ChildWidthProperty = DependencyProperty.Register(nameof(ChildWidth), typeof(int), typeof(ZoomControl));
		public static readonly DependencyProperty ChildHeightProperty = DependencyProperty.Register(nameof(ChildHeight), typeof(int), typeof(ZoomControl));

		public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

		public event EventHandler ScaleChanged;

		public event EventHandler ViewportChanged;

		public void Center()
		{
			this.ViewportPositionX = (int)((this.ActualWidth / 2) - (this.ChildWidth * this.Scale / 2));
			this.ViewportPositionY = (int)((this.ActualHeight / 2) - (this.ChildHeight * this.Scale / 2));
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				Point window = e.GetPosition(this);
				double imageX = (window.X - this.ViewportPositionX) / this.Scale;
				double imageY = (window.Y - this.ViewportPositionY) / this.Scale;

				this.Scale += (e.Delta * 0.001) + (e.Delta > 0 ? this.Scale * 0.05 : -(this.Scale * 0.05));

				this.ViewportPositionX = (int)(window.X - (imageX * this.Scale));
				this.ViewportPositionY = (int)(window.Y - (imageY * this.Scale));
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift))
			{
				this.ViewportPositionX += (int)(e.Delta * 0.5);
			}
			else
			{
				this.ViewportPositionY += (int)(e.Delta * 0.5);
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.Space))
			{
				this.start = e.GetPosition(this);
				this.origin = new Point(this.ViewportPositionX, this.ViewportPositionY);
				this.CaptureMouse();
			}
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) => this.ReleaseMouseCapture();

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.IsMouseCaptured)
			{
				Vector v = this.start - e.GetPosition(this);
				this.ViewportPositionX = (int)(this.origin.X - v.X);
				this.ViewportPositionY = (int)(this.origin.Y - v.Y);
			}
		}
	}
}