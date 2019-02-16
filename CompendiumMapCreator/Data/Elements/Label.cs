using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class Label : NumberedElement
	{
		private string text;

		public Label(string text, int number) : base(number, Color.Yellow, IconType.Label)
		{
			this.Text = text;
		}

		public string Text
		{
			get => this.text;
			set
			{
				this.text = value;
				this.OnPropertyChanged(nameof(this.ToolTip));
			}
		}

		public override string ToolTip => this.Text;
	}
}