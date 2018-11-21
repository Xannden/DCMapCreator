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

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}