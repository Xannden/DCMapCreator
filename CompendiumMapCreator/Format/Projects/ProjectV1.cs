using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV1 : Project
	{
		public ProjectV1(Image image) : base(image)
		{
		}

		public ProjectV1(string file) : base(file)
		{
		}

		protected override Element ReadElement(BinaryReader reader)
		{
			IconType? type = this.ReadType(reader.ReadInt32());

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
					element = new Element(type.Value);
					break;
			}

			element.X = x;
			element.Y = y;

			return element;
		}

		private IconType? ReadType(int value)
		{
			switch (value)
			{
				case 0:
					return IconType.Cursor;

				case 1:
					return IconType.NormalChest;

				case 2:
					return IconType.TrappedChest;

				case 3:
					return IconType.LockedChest;

				case 4:
					return IconType.LockedDoor;

				case 5:
					return IconType.Opener;

				case 6:
					return IconType.TrapBox;

				case 7:
					return IconType.AnyCollectible;

				case 8:
					return IconType.Lore;

				case 9:
					return IconType.Natural;

				case 10:
					return IconType.Arcane;

				case 11:
					return IconType.QuestItem;

				case 12:
					return IconType.QuestNPC;

				case 13:
					return IconType.SecretDoor;

				case 14:
					return IconType.QuestExit;

				case 15:
					return IconType.Portal;

				case 16:
					return IconType.Label;

				case 17:
					return IconType.Trap;

				case 18:
					return IconType.CollapsibleFloor;

				case 19:
					return IconType.Entrance;

				default:
					throw new InvalidDataException();
			}
		}
	}
}