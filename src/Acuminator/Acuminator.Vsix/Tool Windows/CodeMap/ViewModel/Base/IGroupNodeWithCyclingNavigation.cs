﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public interface IGroupNodeWithCyclingNavigation
	{
		bool AllowNavigation { get; }

		int CurrentNavigationIndex
		{
			get;
			set;
		}

		void NavigateToItem();

		bool CanNavigateToChild(TreeNodeViewModel child);
	}
}
