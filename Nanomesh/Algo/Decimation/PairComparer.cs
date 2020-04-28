using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanolabo
{
    public partial class DecimateModifier
    {
		private class PairComparer : IComparer<PairCollapse>
		{
			public int Compare(PairCollapse x, PairCollapse y)
			{
				return x.CompareTo(y);
			}
		}
	}
}