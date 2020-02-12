using System;
using System.Reflection;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public static class IconTypeExtensions
	{
		public static IconType? FromName(this string s)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo[] fields = typeInfo.GetFields();

			for (int i = 0; i < fields.Length; i++)
			{
				NameAttribute attribute = null;
				if ((fields[i]?.TryGetCustomAttribute(out attribute) ?? false) && string.Equals(attribute.Name, s, StringComparison.OrdinalIgnoreCase))
				{
					return (IconType)fields[i].GetValue(null);
				}
			}

			return null;
		}

		public static Image GetImage(this IconType item)
		{
			return Image.GetImageFromResources(item.GetImageFile());
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

		public static string GetName(this IconType item)
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

		public static string GetToolTip(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			ToolTipAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return attribute.ToolTip;
			}

			return null;
		}
	}
}