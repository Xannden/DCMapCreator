using System;
using System.ComponentModel;
using System.Windows;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.ViewModel
{
	public class Editing : INotifyPropertyChanged
	{
		private Label label;
		private bool started;

		public Visibility Visibility { get; private set; } = Visibility.Collapsed;

		public int X { get; private set; }

		public int Y { get; private set; }

		public string Text { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<string, Label> Closing;

		public void Start(WindowPoint p, Label label)
		{
			if (this.started)
			{
				throw new InvalidOperationException();
			}

			this.Visibility = Visibility.Visible;
			this.label = label;
			this.Text = label.Text;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Visibility)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Text)));

			this.SetPosition(p.X, p.Y);
			this.started = true;
		}

		private void SetPosition(int x, int y)
		{
			this.X = x;
			this.Y = y;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.X)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Y)));
		}

		public void End()
		{
			if (!this.started)
			{
				return;
			}

			this.Visibility = Visibility.Collapsed;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Visibility)));

			if (!string.Equals(this.Text, this.label.Text, StringComparison.Ordinal))
			{
				this.Closing?.Invoke(this.Text, this.label);
			}

			this.started = false;
		}
	}
}