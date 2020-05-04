using System.Collections.Generic;
using System.ComponentModel;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.ViewModel
{
	public class ToolVM : INotifyPropertyChanged
	{
		private ToolListItem? item;
		private Image icon;
		private bool isExpanded;
		private bool isSelected;

		public event PropertyChangedEventHandler PropertyChanged;

		public static ToolVM Cursor { get; } = new ToolVM("Cursor");

		public bool IsArea
		{
			get
			{
				if (this.ToolElement.HasValue)
				{
					return App.Config.GetElement(this.ToolElement.Value).Type == "area";
				}

				return false;
			}
		}

		public ElementId? ToolElement { get; }

		public string Name { get; }

		public Image Icon
		{
			get
			{
				if (this.icon == null && this.item.HasValue)
				{
					this.icon = Image.GetImageFromTool(this.item.Value);
				}
				else if (this.icon == null && this.ToolElement.HasValue)
				{
					this.icon = Image.GetImageFromElementId(this.ToolElement.Value);
				}

				return this.icon;
			}
		}

		public bool IsExpanded
		{
			get => this.isExpanded;

			set
			{
				this.isExpanded = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsExpanded)));
			}
		}

		public bool IsSelected
		{
			get => this.isSelected;

			set
			{
				this.isSelected = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}
		}

		public List<ToolVM> Elements { get; }

		public string ToolTip => App.Config.GetElement(this.ToolElement)?.ToolTip;

		internal ToolVM(ToolListItem item, List<ToolVM> elements)
		{
			this.item = item;
			this.ToolElement = item.DefaultElement;
			this.Name = item.Name;
			this.Elements = elements;
		}

		internal ToolVM(ElementId element)
		{
			this.item = null;
			this.ToolElement = element;

			this.Name = App.Config.GetElement(element).Name;
		}

		private ToolVM(string name)
		{
			this.Name = name;
		}

		public override string ToString() => this.Name;
	}
}