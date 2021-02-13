using System;

namespace Nanomesh
{
    public partial class DecimateModifier
    {
		public class EdgeCollapse : IComparable<EdgeCollapse>, IEquatable<EdgeCollapse>
		{
			public int posA;
			public int posB;
			public Vector3 result;
			public double error;

			private EdgeTopology _topology;

			public ref EdgeTopology Topology => ref _topology;

			public void SetTopology(EdgeTopology topology)
            {
				_topology = topology;
            }

			public EdgeCollapse(int posA, int posB)
			{
				this.posA = posA;
				this.posB = posB;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return posA + posB;
				}
			}

			public override bool Equals(object obj)
			{
				return Equals((EdgeCollapse)obj);
			}

			public bool Equals(EdgeCollapse pc)
			{
				if (ReferenceEquals(this, pc))
				{
					return true;
				}
				else
				{
					return (posA == pc.posA && posB == pc.posB) || (posA == pc.posB && posB == pc.posA);
				}
			}

			public int CompareTo(EdgeCollapse other)
			{
				return error > other.error ? 1 : error < other.error ? -1 : 0;
			}

			public static bool operator >(EdgeCollapse x, EdgeCollapse y)
			{
				return x.error > y.error;
			}

			public static bool operator >=(EdgeCollapse x, EdgeCollapse y)
			{
				return x.error >= y.error;
			}

			public static bool operator <(EdgeCollapse x, EdgeCollapse y)
			{
				return x.error < y.error;
			}

			public static bool operator <=(EdgeCollapse x, EdgeCollapse y)
			{
				return x.error <= y.error;
			}

			public static bool operator ==(EdgeCollapse x, EdgeCollapse y)
			{
				return x.Equals(y);
			}

			public static bool operator !=(EdgeCollapse x, EdgeCollapse y)
			{
				return !x.Equals(y);
			}

			public override string ToString()
			{
				return $"<A:{posA} B:{posB} error:{error} topology:{_topology}>";
			}
		}
	}
}