using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV3 : Project
	{
		public ProjectV3(Image image)
			: base(image)
		{
		}

		public ProjectV3(string file, string title)
			: base(file)
		{
			this.Title = title;
		}

		protected override Element ReadElement(BinaryReader reader)
		{
			IconType? type = this.ReadType(reader.ReadInt32());

			if (type == null)
			{
				return null;
			}

			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			bool isCopy = reader.ReadBoolean();

			int length = reader.ReadInt32();

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

						element = new Trap(width, height);
					}

					break;

				case IconType.CollapsibleFloor:
					{
						int width = reader.ReadInt32();
						int height = reader.ReadInt32();

						element = new CollapsibleFloor(width, height);
					}

					break;

				case IconType.Entrance:
					{
						Rotation rotation = (Rotation)reader.ReadInt32();

						element = new Entrance(rotation);
					}

					break;

				default:
					reader.BaseStream.Seek(length, SeekOrigin.Current);

					element = new Element(type.Value);
					break;
			}

			element.X = x;
			element.Y = y;
			element.IsCopy = isCopy;

			return element;
		}

		private IconType? ReadType(int value)
		{
			return value switch
			{
				0 => IconType.Cursor,
				10 => IconType.NormalChest,
				20 => IconType.TrappedChest,
				30 => IconType.LockedChest,
				35 => IconType.RareChest,
				40 => IconType.LockedDoor,
				41 => IconType.BlockedDoor,
				50 => IconType.Opener,
				60 => IconType.TrapBox,
				70 => IconType.AnyCollectible,
				80 => IconType.Lore,
				90 => IconType.Natural,
				100 => IconType.Arcane,
				110 => IconType.QuestItem,
				111 => IconType.QuestItemUse,
				120 => IconType.QuestNPC,
				130 => IconType.SecretDoor,
				140 => IconType.QuestExit,
				150 => IconType.Portal,
				160 => IconType.Label,
				170 => IconType.Trap,
				180 => IconType.CollapsibleFloor,
				181 => IconType.Drop,
				190 => IconType.Entrance,
				_ => null,
			};
		}
	}
}