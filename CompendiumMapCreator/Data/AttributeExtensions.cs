using System;
using System.Reflection;

namespace CompendiumMapCreator
{
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

			attribute = default;
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

			attribute = default;
			return false;
		}
	}
}