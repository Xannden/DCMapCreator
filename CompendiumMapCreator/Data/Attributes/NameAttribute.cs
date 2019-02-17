using System;

namespace CompendiumMapCreator.Data
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NameAttribute : Attribute
	{
		public string Name { get; }

		public NameAttribute(string name)
		{
			this.Name = name;
		}
	}
}