using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanolabo
{
    public partial class DecimateModifier
    {
		public class PairCollapse : IComparable<PairCollapse>, IEquatable<PairCollapse>
		{
#if DEBUG
			public IEdgeType type;
#endif

			public int posA;
			public int posB;
			public Vector3 result;
			public double error;

			public PairCollapse(int pos1, int pos2)
			{
				this.posA = pos1;
				this.posB = pos2;
			}

			public override int GetHashCode()
			{
				unsafe
				{
					return posA + posB;
				}
			}

			public override bool Equals(object obj)
			{
				PairCollapse pc = (PairCollapse)obj;
				return Compare(this, pc) == 0;
			}

			public bool Equals(PairCollapse pc)
			{
				return Compare(this, pc) == 0;
			}

			public int CompareTo(PairCollapse other)
			{
				return Compare(this, other);
			}

			private static int Compare(PairCollapse x, PairCollapse y)
			{
				int lret = 0;
				if (Object.ReferenceEquals(x, y))
				{
					lret = 0;
				}
				else if (Object.ReferenceEquals(null, x))
				{
					lret = 1;
				}
				else if (Object.ReferenceEquals(null, y))
				{
					lret = -1;
				}
				else
				{
					lret = ((x.posA == y.posA && x.posB == y.posB) || (x.posA == y.posB && x.posB == y.posA)) ? 0 : x.error > y.error ? 1 : -1;
				}

				return lret;
			}

			public static bool operator ==(PairCollapse x, PairCollapse y)
			{
				return Compare(x, y) == 0;
			}

			public static bool operator !=(PairCollapse x, PairCollapse y)
			{
				return Compare(x, y) != 0; ;
			}

			public override string ToString()
			{
#if DEBUG
				return $"<A:{posA} B:{posB} error:{error} type:{type}>";
#else
				return $"<A:{posA} B:{posB} error:{error}>";
#endif
			}
		}
	}
}