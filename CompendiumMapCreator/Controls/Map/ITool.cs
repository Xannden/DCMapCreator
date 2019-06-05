using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CompendiumMapCreator.Controls
{
	public interface ITool
	{
		ImageSource Icon { get; }

		bool IsSelectable { get; }

		string Name { get; }

		string ToolTip { get; }

		IconType Type { get; }
	}
}