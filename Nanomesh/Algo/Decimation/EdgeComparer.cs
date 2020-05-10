using System.Collections.Generic;

namespace Nanolabo
{
    public partial class DecimateModifier
    {
		private class EdgeComparer : IComparer<EdgeCollapse>
		{
			public int Compare(EdgeCollapse x, EdgeCollapse y)
			{
				return x.CompareTo(y);
			}
		}
	}
}