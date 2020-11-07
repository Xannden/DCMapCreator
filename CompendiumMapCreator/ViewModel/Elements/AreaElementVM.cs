using System.IO;
using System.Windows.Media;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.ViewModel.Elements;

namespace CompendiumMapCreator.ViewModel
{
	public class AreaElementVM : BackgroundElementVM
	{
		private int width;
		private int height;

		internal AreaElementVM(ElementId id, int width, int height)
			: base(id)
		{
			this.width = width;
			this.height = height;
		}

		public override Image Image => null;

		public override int Height => this.height;

		public override int Width => this.width;

		public SolidColorBrush ColorBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(this.Element.Color[0], this.Element.Color[1], this.Element.Color[2]));

		public System.Drawing.Color Color => System.Drawing.Color.FromArgb(this.Element.Color[0], this.Element.Color[1], this.Element.Color[2]);

		public override ElementVM Clone()
		{
			return new AreaElementVM(this.Id, this.Width, this.Height);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(this.width);
			writer.Write(this.height);
		}

		protected override void ReadData(BinaryReader reader, int dataLength)
		{
			this.width = reader.ReadInt32();
			this.height = reader.ReadInt32();
		}
	}
}