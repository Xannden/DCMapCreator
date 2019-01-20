using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class ProjectV2 : Project
	{
		public ProjectV2(Image image) : base(image)
		{
		}

		public ProjectV2(string file) : base(file)
		{
		}

		protected override Element ReadElement(BinaryReader reader, IconType type)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			bool isCopy = reader.ReadBoolean();

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

				case IconType.Trap:
					{
						int width = reader.ReadInt32();
						int height = reader.ReadInt32();

						element = new Trap(0, 0, width, height);
					}
					break;

				default:
					element = new Element(type);
					break;
			}

			element.X = x;
			element.Y = y;
			element.IsCopy = isCopy;

			return element;
		}
	}
}