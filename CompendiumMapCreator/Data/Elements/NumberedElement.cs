using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CompendiumMapCreator.Data
{
	public class NumberedElement : Element
	{
		private static Dictionary<int, Image> digitIcons;
		private int number;

		public int Number
		{
			get => this.number;

			set
			{
				this.number = value;
				this.Image = CreateImage(this.number, this.Background);
			}
		}

		public Color Background { get; }

		public NumberedElement(int number, Color background, IconType type)
			: base(CreateImage(number, background), type)
		{
			this.number = number;
			this.Background = background;
		}

		private static Image CreateImage(int number, Color color)
		{
			if (digitIcons == null)
			{
				digitIcons = new Dictionary<int, Image>(10);

				for (int i = 0; i < 10; i++)
				{
					digitIcons.Add(i, Image.GetImageFromResources("Icons/" + i + ".png"));
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