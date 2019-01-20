using System.Drawing;

namespace CompendiumMapCreator.Data
{
	public class Label : NumberedElement
	{
		public Label(string text, int number) : base(number, Color.Yellow, IconType.Label)
		{
			this.Text = text;
		}

		public string Text { get; set; }
	}
}