using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace CompendiumMapCreator.Data
{
	public class Tool : INotifyPropertyChanged
	{
		private bool isExpanded;

		public Tool(IconType type)
		{
			this.Type = type;
			this.Description = this.Type.GetName();
		}

		public Tool(IconType type, string name)
		{
			this.Type = type;
			this.Description = name;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Description { get; }

		public bool IsExpanded
		{
			get => this.isExpanded;

			set
			{
				this.isExpanded = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsExpanded)));
			}
		}

		public ImageSource Image => this.Type.GetImage().BitmapImage;

		public bool IsSelectable { get; set; } = true;

		public List<Tool> Tools { get; set; }

		public string ToolTip => this.Type.GetToolTip();

		public IconType Type { get; set; }

		public Tool Next(Tool curr)
		{
			Tool result = this;
			if (curr == null || this.Tools == null || this.Tools.Count == 0)
			{
				return result;
			}

			if (this.Description == curr.Description)
			{
				return this.Tools[0];
			}

			for (int i = 0; i < this.Tools.Count; i++)
			{
				if (this.Tools[i]?.Description == curr.Description)
				{
					return i == (this.Tools.Count - 1) ? result : this.Tools[i + 1];
				}
			}

			return result;
		}
	}
}