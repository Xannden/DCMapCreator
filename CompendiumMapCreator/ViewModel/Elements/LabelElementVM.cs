using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.ViewModel
{
	public class LabelElementVM : NumberedElementVM
	{
		private string text;

		internal LabelElementVM(ElementId id, int number)
			: base(id, number)
		{
		}

		public string Text
		{
			get => this.text;

			set
			{
				this.text = value;
				this.SendPropertyChanged(this, nameof(this.ToolTip));
			}
		}

		public override string ToolTip => this.Text != string.Empty ? this.Text : null;

		public override ElementVM Clone()
		{
			return new LabelElementVM(this.Id, this.Number);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(this.Number);
			writer.Write(this.Text ?? string.Empty);
		}

		protected override void ReadData(BinaryReader reader, int dataLength)
		{
			this.Number = reader.ReadInt32();
			this.Text = reader.ReadString();
		}
	}
}