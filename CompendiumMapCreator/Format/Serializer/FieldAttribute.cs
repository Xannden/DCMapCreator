using System;
using System.Runtime.CompilerServices;

namespace CompendiumMapCreator.Format.Serializer
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class FieldAttribute : Attribute
	{
		public FieldAttribute([CallerLineNumber] int order = 0, [CallerMemberName] string name = "")
		{
			this.Order = order;
			this.Name = name;
		}

		public string Name { get; }

		public int Order { get; }
	}
}