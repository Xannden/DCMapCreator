using System;

namespace CompendiumMapCreator.Data
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ToolTipAttribute : Attribute
	{
		public string ToolTip { get; }

		public ToolTipAttribute(string tooltip)
		{
			this.ToolTip = tooltip;
		}
	}
}