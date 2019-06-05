using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompendiumMapCreator.Controls
{
	public class DragHelper
	{
		private readonly Control control;
		private bool dragging;
		private Point lastPoint;
		private Point origin;
		private Point originScreen;

		public DragHelper(Control control)
		{
			this.control = control;
			this.control.MouseLeftButtonDown += this.MouseLeftDown;
			this.control.MouseLeftButtonUp += this.MouseLeftUp;
			this.control.MouseMove += this.MouseMove;
			this.control.LostMouseCapture += this.LostMouseCapture;
		}

		public event EventHandler<DragCompletedEventArgs> DragCompleted;

		public event EventHandler<DragDeltaEventArgs> DragDelta;

		public event EventHandler<DragStartedEventArgs> DragStarted;

		private void EndDrag(bool sendComplete)
		{
			if (this.dragging)
			{
				if (this.control.IsMouseCaptured)
				{
					this.control.ReleaseMouseCapture();
				}

				this.dragging = false;

				if (sendComplete)
				{
					this.DragCompleted?.Invoke(this, new DragCompletedEventArgs(this.origin, this.lastPoint, this.lastPoint.X - this.originScreen.X, this.lastPoint.Y - this.originScreen.Y));
				}

				this.origin = new Point();
				this.lastPoint = new Point();
				this.originScreen = new Point();
			}
		}

		private void LostMouseCapture(object sender, MouseEventArgs e)
		{
			if (!this.control.IsMouseCaptured)
			{
				this.EndDrag(true);
			}
		}

		private void MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			if (!this.dragging)
			{
				e.Handled = true;
				this.control.Focus();
				this.control.CaptureMouse();
				this.dragging = true;
				this.origin = e.GetPosition(this.control);
				this.lastPoint = this.originScreen = this.control.PointToScreen(this.origin);
				this.DragStarted?.Invoke(this, new DragStartedEventArgs(this.origin.X, this.origin.Y));
			}
		}

		private void MouseLeftUp(object sender, MouseButtonEventArgs e)
		{
			if (this.control.IsMouseCaptured && this.dragging)
			{
				e.Handled = true;
				this.dragging = false;
				this.control.ReleaseMouseCapture();

				Point p = e.GetPosition(this.control);
				Point screen = this.control.PointToScreen(p);

				this.DragCompleted?.Invoke(this, new DragCompletedEventArgs(this.origin, p, screen.X - this.originScreen.X, screen.Y - this.originScreen.Y));

				this.EndDrag(false);
			}
		}

		private void MouseMove(object sender, MouseEventArgs e)
		{
			if (this.dragging)
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					Point p = e.GetPosition(this.control);
					Point screen = this.control.PointToScreen(p);
					if (screen != this.lastPoint)
					{
						e.Handled = true;

						this.DragDelta?.Invoke(this, new DragDeltaEventArgs(screen.X - this.lastPoint.X, screen.Y - this.lastPoint.Y, p));
						this.lastPoint = screen;
					}
				}
				else
				{
					this.EndDrag(true);
				}
			}
		}
	}

	#region EventArgs

	public class DragCompletedEventArgs : EventArgs
	{
		public DragCompletedEventArgs(Point origin, Point position, double xChangeTotal, double yChangeTotal)
		{
			this.Origin = origin;
			this.Position = position;
			this.XChangeTotal = xChangeTotal;
			this.YChangeTotal = yChangeTotal;
		}

		public Point Origin { get; }

		public Point Position { get; }

		public double XChangeTotal { get; }

		public double YChangeTotal { get; }
	}

	public class DragDeltaEventArgs : EventArgs
	{
		public DragDeltaEventArgs(double xChange, double yChange, Point position)
		{
			this.XChange = xChange;
			this.YChange = yChange;
			this.Position = position;
		}

		public Point Position { get; }

		public double XChange { get; }

		public double YChange { get; }
	}

	public class DragStartedEventArgs : EventArgs
	{
		public DragStartedEventArgs(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		public double X { get; }

		public double Y { get; }
	}

	#endregion EventArgs
}