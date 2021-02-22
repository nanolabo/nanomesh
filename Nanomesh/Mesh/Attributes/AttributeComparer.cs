using System.Collections.Generic;

namespace Nanomesh
{
    public class AttributeComparer : IEqualityComparer<int>
    {
		private Attributes _attributes;

		public AttributeComparer(Attributes attributes)
        {
			_attributes = attributes;
		}

        public bool Equals(int x, int y)
        {
			foreach (var list in _attributes.Values)
			{
				if (!list[x].Equals(list[y]))
					return false;
			}
			return true;
		}

        public int GetHashCode(int index)
        {
            unchecked
            {
				int hash = 0;
				foreach (var list in _attributes.Values)
                {
					hash = hash * 31 + list[index].GetHashCode();
				}
				return hash;
            }
        }
    }
}