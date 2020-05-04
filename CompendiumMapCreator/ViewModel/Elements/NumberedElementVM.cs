using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.ViewModel
{
	public class NumberedElementVM : ElementVM
	{
		private static Dictionary<int, Image> digitIcons;
		private readonly Color color;
		private int number;
		private Image icon;

		internal NumberedElementVM(ElementId id, int number)
			: base(id)
		{
			byte[] color = App.Config.GetElement(id).Color;

			this.color = Color.FromArgb(color[0], color[1], color[2]);

			this.Number = number;
		}

		public int Number
		{
			get => this.number;

			set
			{
				this.number = value;

				this.icon = CreateImage(this.number, this.color);

				this.SendPropertyChanged(this, nameof(this.Image));
			}
		}

		public override Image Image => this.icon;

		public override ElementVM Clone()
		{
			return new NumberedElementVM(this.Id, this.Number);
		}

		protected override void WriteData(BinaryWriter writer)
		{
			writer.Write(this.Number);
		}

		protected override void ReadData(BinaryReader reader, int dataLength)
		{
			this.Number = reader.ReadInt32();
		}

		private static Image CreateImage(int number, Color color)
		{
			if (digitIcons == null)
			{
				digitIcons = new Dictionary<int, Image>(10);

				for (int i = 0; i < 10; i++)
				{
					digitIcons.Add(i, new Image(Image.GetImageUri(i + ".png")));
				}
			}

			int[] digits = GetDigits(number);

			MemoryStream stream = new MemoryStream();

			using (Bitmap temp = new Bitmap(4 + (digits.Length * 6), 10))
			{
				using (Graphics g = Graphics.FromImage(temp))
				{
					g.DrawRectangle(new Pen(Color.Black), 0, 0, 3 + (digits.Length * 6), 9);
					g.FillRectangle(new SolidBrush(color), 1, 1, 2 + (digits.Length * 6), 8);

					for (int i = 0; i < digits.Length; i++)
					{
						g.DrawImage(digitIcons[digits[i]].DrawingImage, new Point(2 + (i * 6), 2));
					}
				}

				temp.Save(stream, ImageFormat.Png);
			}

			return new Image(stream);
		}

		private static int[] GetDigits(int number)
		{
			List<int> digits = new List<int>();

			do
			{
				digits.Add(number % 10);
				number /= 10;
			}
			while (number > 0);

			digits.Reverse();

			return digits.ToArray();
		}
	}
}