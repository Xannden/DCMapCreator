using System;
using System.Reflection;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		[Name("Cursor")]
		[IconFile("cursor.png")]
		Cursor = 0,

		[Name("Normal Chest")]
		[IconFile("normalChest.png")]
		NormalChest = 10,

		[Name("Trapped Chest")]
		[IconFile("trappedChest.png")]
		TrappedChest = 20,

		[Name("Locked Chest")]
		[IconFile("lockedChest.png")]
		LockedChest = 30,

		[Name("Locked Door")]
		[IconFile("lockedDoor.png")]
		LockedDoor = 40,

		[Name("Blocked Door")]
		[IconFile("blockedDoor.png")]
		BlockedDoor = 41,

		[Name("Lever/Valve/Rune")]
		[IconFile("leverValve.png")]
		LeverValveRune = 50,

		[Name("Control Box")]
		[IconFile("controlBox.png")]
		ControlBox = 60,

		[Name("All Collectibles")]
		[IconFile("collectible.png")]
		Collectible = 70,

		[Name("Lore Collectibles")]
		[IconFile("book.png")]
		Lore = 80,

		[Name("Natural Collectibles")]
		[IconFile("paw.png")]
		Natural = 90,

		[Name("Arcane Collectibles")]
		[IconFile("rune.png")]
		Arcane = 100,

		[Name("Quest Item")]
		[IconFile("questItem.png")]
		QuestItem = 110,

		[Name("Quest Item Use")]
		[IconFile("questItemUse.png")]
		UseQuestItem = 111,

		[Name("Quest NPC")]
		[IconFile("questNPC.png")]
		QuestNPC = 120,

		[Name("Secret Door")]
		[IconFile("secretDoor.png")]
		SecretDoor = 130,

		[Name("Quest Exit")]
		[IconFile("questExit.png")]
		QuestExit = 140,

		[Name("Portal")]
		[IconFile("portal.png")]
		Portal = 150,

		[Name("Label")]
		[IconFile("label.png")]
		Label = 160,

		[Name("Trap")]
		[IconFile("trap.png")]
		Trap = 170,

		[Name("Collapsible Floor")]
		[IconFile("collapsibleFloor.png")]
		CollapsibleFloor = 180,

		[Name("Entrance")]
		[IconFile("entrance.png")]
		Entrance = 190,
	}

	public static class IconTypeExtensions
	{
		public static IconType FromDescription(this string s)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo[] fields = typeInfo.GetFields();

			for (int i = 0; i < fields.Length; i++)
			{
				NameAttribute attribute = null;
				if ((fields[i]?.TryGetCustomAttribute(out attribute) ?? false) && attribute.Name == s)
				{
					return (IconType)fields[i].GetValue(null);
				}
			}

			throw new ArgumentOutOfRangeException(nameof(s));
		}

		public static string GetDescription(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			NameAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return attribute.Name;
			}

			throw new ArgumentOutOfRangeException(nameof(item));
		}

		public static string GetImageFile(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			IconFileAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return "Icons/" + attribute.FileName;
			}

			throw new ArgumentOutOfRangeException(nameof(item));
		}
	}

	public static class AttributeExtensions
	{
		public static bool TryGetCustomAttribute<T>(this TypeInfo info, out T attribute)
			where T : Attribute
		{
			T[] attributes = (T[])info.GetCustomAttributes<T>();

			if (attributes.Length > 0)
			{
				attribute = attributes[0];
				return true;
			}

			attribute = default(T);
			return false;
		}

		public static bool TryGetCustomAttribute<T>(this MemberInfo info, out T attribute)
			where T : Attribute
		{
			T[] attributes = (T[])info?.GetCustomAttributes<T>();

			if (attributes?.Length > 0)
			{
				attribute = attributes[0];
				return true;
			}

			attribute = default(T);
			return false;
		}
	}
}