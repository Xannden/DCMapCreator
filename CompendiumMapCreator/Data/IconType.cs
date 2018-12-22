using System;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		Cursor,
		NormalChest,
		TrappedChest,
		LockedChest,
		LockedDoor,
		LeverValveRune,
		ControlBox,
		Collectible,
		Lore,
		Natural,
		Arcane,
		QuestItem,
		QuestNPC,
		SecretDoor,
		QuestExit,
		Portal,
		Label,
	}

	public static class IconTypeExtensions
	{
		public static IconType FromDescription(this string s)
		{
			switch (s)
			{
				case "Cursor":
					return IconType.Cursor;

				case "Normal Chest":
					return IconType.NormalChest;

				case "Trapped Chest":
					return IconType.TrappedChest;

				case "Locked Chest":
					return IconType.LockedChest;

				case "Locked Door":
					return IconType.LockedDoor;

				case "Lever/Valve/Rune":
					return IconType.LeverValveRune;

				case "Control Box":
					return IconType.ControlBox;

				case "All Collectibles":
					return IconType.Collectible;

				case "Lore Collectibles":
					return IconType.Lore;

				case "Natural Collectibles":
					return IconType.Natural;

				case "Arcane Collectibles":
					return IconType.Arcane;

				case "Quest Item":
					return IconType.QuestItem;

				case "Quest NPC":
					return IconType.QuestNPC;

				case "Secret Door":
					return IconType.SecretDoor;

				case "Quest Exit":
					return IconType.QuestExit;

				case "Waypoint/Portal":
					return IconType.Portal;

				case "Label":
					return IconType.Label;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static string GetDescription(this IconType type)
		{
			switch (type)
			{
				case IconType.Cursor:
					return "Cursor";

				case IconType.NormalChest:
					return "Normal Chest";

				case IconType.TrappedChest:
					return "Trapped Chest";

				case IconType.LockedChest:
					return "Locked Chest";

				case IconType.LockedDoor:
					return "Locked Door";

				case IconType.LeverValveRune:
					return "Lever/Valve/Rune";

				case IconType.ControlBox:
					return "Control Box";

				case IconType.Collectible:
					return "All Collectibles";

				case IconType.Lore:
					return "Lore Collectibles";

				case IconType.Natural:
					return "Natural Collectibles";

				case IconType.Arcane:
					return "Arcane Collectibles";

				case IconType.QuestItem:
					return "Quest Item";

				case IconType.QuestNPC:
					return "Quest NPC";

				case IconType.SecretDoor:
					return "Secret Door";

				case IconType.QuestExit:
					return "Quest Exit";

				case IconType.Portal:
					return "Waypoint/Portal";

				case IconType.Label:
					return "Label";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static string GetImageFile(this IconType type)
		{
			switch (type)
			{
				case IconType.Cursor:
					return "Icons/cursor.png";

				case IconType.NormalChest:
					return "Icons/normalChest.png";

				case IconType.TrappedChest:
					return "Icons/trappedChest.png";

				case IconType.LockedChest:
					return "Icons/lockedChest.png";

				case IconType.LockedDoor:
					return "Icons/lockedDoor.png";

				case IconType.LeverValveRune:
					return "Icons/leverValve.png";

				case IconType.ControlBox:
					return "Icons/controlBox.png";

				case IconType.Collectible:
					return "Icons/collectible.png";

				case IconType.Lore:
					return "Icons/book.png";

				case IconType.Natural:
					return "Icons/paw.png";

				case IconType.Arcane:
					return "Icons/rune.png";

				case IconType.QuestItem:
					return "Icons/questItem.png";

				case IconType.QuestNPC:
					return "Icons/questNPC.png";

				case IconType.SecretDoor:
					return "Icons/secretDoor.png";

				case IconType.QuestExit:
					return "Icons/questExit.png";

				case IconType.Portal:
					return "Icons/portal.png";

				case IconType.Label:
					return "Icons/label.png";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}