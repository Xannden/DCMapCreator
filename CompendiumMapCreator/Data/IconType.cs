using System;
using System.Reflection;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		[Name("Cursor")]
		[IconFile("cursor.png")]
		Cursor,

		[Name("Normal Chest")]
		[IconFile("normalChest.png")]
		NormalChest,

		[Name("Trapped Chest")]
		[IconFile("trappedChest.png")]
		TrappedChest,

		[Name("Locked Chest")]
		[IconFile("lockedChest.png")]
		LockedChest,

		[Name("Locked Door")]
		[IconFile("lockedDoor.png")]
		LockedDoor,

		[Name("Lever/Valve/Rune")]
		[IconFile("leverValve.png")]
		LeverValveRune,

		[Name("Control Box")]
		[IconFile("controlBox.png")]
		ControlBox,

		[Name("All Collectibles")]
		[IconFile("collectible.png")]
		Collectible,

		[Name("Lore Collectibles")]
		[IconFile("book.png")]
		Lore,

		[Name("Natural Collectibles")]
		[IconFile("paw.png")]
		Natural,

		[Name("Arcane Collectibles")]
		[IconFile("rune.png")]
		Arcane,

		[Name("Quest Item")]
		[IconFile("questItem.png")]
		QuestItem,

		[Name("Quest NPC")]
		[IconFile("questNPC.png")]
		QuestNPC,

		[Name("Secret Door")]
		[IconFile("secretDoor.png")]
		SecretDoor,

		[Name("Quest Exit")]
		[IconFile("questExit.png")]
		QuestExit,

		[Name("Portal")]
		[IconFile("portal.png")]
		Portal,

		[Name("Label")]
		[IconFile("label.png")]
		Label,

		[Name("Trap")]
		[IconFile("trap.png")]
		Trap,

		[Name("Collapsible Floor")]
		[IconFile("collapsibleFloor.png")]
		CollapsibleFloor,

		[Name("Entrance")]
		[IconFile("entrance.png")]
		Entrance,
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