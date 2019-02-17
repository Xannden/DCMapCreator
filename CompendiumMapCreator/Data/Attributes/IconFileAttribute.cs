using System;

namespace CompendiumMapCreator.Data
{
	[AttributeUsage(AttributeTargets.Field)]
	public class IconFileAttribute : Attribute
	{
		public string FileName { get; }

		public IconFileAttribute(string fileName)
		{
			this.FileName = fileName;
		}
	}
}