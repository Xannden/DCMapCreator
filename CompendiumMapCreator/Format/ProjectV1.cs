using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class ProjectV1 : Project
	{
		public ProjectV1(Image image) : base(image)
		{
		}

		public ProjectV1(string file) : base(file)
		{
		}

		protected override Element ReadElement(BinaryReader reader, IconType type)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();

			Element element;

			switch (type)
			{
				case IconType.Label:
					{
						int number = reader.ReadInt32();
						string text = reader.ReadString();

						element = new Label(text, number);
					}
					break;

				case IconType.Portal:
					{
						int number = reader.ReadInt32();

						element = new Portal(number);
					}
					break;

				default:
					element = new Element(type);
					break;
			}

			element.X = x;
			element.Y = y;

			return element;
		}
	}
}