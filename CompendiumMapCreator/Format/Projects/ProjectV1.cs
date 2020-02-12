using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV1 : Project
	{
		public ProjectV1(Image image)
			: base(image)
		{
		}

		public ProjectV1(string file)
			: base(file)
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
			return value switch
			{
				0 => IconType.Cursor,
				1 => IconType.NormalChest,
				2 => IconType.TrappedChest,
				3 => IconType.LockedChest,
				4 => IconType.LockedDoor,
				5 => IconType.Opener,
				6 => IconType.TrapBox,
				7 => IconType.AnyCollectible,
				8 => IconType.Lore,
				9 => IconType.Natural,
				10 => IconType.Arcane,
				11 => IconType.QuestItem,
				12 => IconType.QuestNPC,
				13 => IconType.SecretDoor,
				14 => IconType.QuestExit,
				15 => IconType.Portal,
				16 => IconType.Label,
				17 => IconType.Trap,
				18 => IconType.CollapsibleFloor,
				19 => IconType.Entrance,
				_ => throw new InvalidDataException(),
			};
		}
	}
}