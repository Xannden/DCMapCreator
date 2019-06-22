using System;

namespace CompendiumMapCreator.Format.Serializer
{
	[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
	public sealed class SerializeAttribute : Attribute
	{
		public SerializeAttribute(int magic, string name, int version)
		{
			this.Magic = magic;
			this.Name = name.ToCharArray();
			this.Version = version;
		}

		public int Magic { get; }

		public char[] Name { get; }

		public int Version { get; }
	}
}