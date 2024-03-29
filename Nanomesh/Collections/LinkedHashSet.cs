using System;
using System.Collections;
using System.Collections.Generic;

namespace Nanomesh
{
    public class LinkedHashSet<T> : IReadOnlyCollection<T> where T : IComparable<T>
    {
        private readonly Dictionary<T, LinkedHashNode<T>> elements;
        private LinkedHashNode<T> first, last;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashSet{T}"/> class.
        /// </summary>
        public LinkedHashSet()
        {
            elements = new Dictionary<T, LinkedHashNode<T>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashSet{T}"/> class.
        /// </summary>
        /// <param name="initialValues"></param>
        public LinkedHashSet(IEnumerable<T> initialValues) : this()
        {
            UnionWith(initialValues);
        }

        public LinkedHashNode<T> First => first;

        public LinkedHashNode<T> Last => last;

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count => elements.Count;

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            elements.Clear();
            first = null;
            last = null;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(T item)
        {
            return elements.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <typeparamref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int index = arrayIndex;

            foreach (T item in this)
            {
                array[index++] = item;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(T item)
        {
            if (elements.TryGetValue(item, out LinkedHashNode<T> node))
            {
                elements.Remove(item);
                Unlink(node);
                return true;
            }

            return false;
        }

        #endregion


        #region Implementation of ISet<T>

        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in either the current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            ISet<T> otherSet = AsSet(other);

            LinkedHashNode<T> current = first;
            while (current != null)
            {
                if (!otherSet.Contains(current.Value))
                {
                    elements.Remove(current.Value);
                    Unlink(current);
                }
                current = current.Next;
            }
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both. 
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                if (elements.TryGetValue(item, out LinkedHashNode<T> node))
                {
                    elements.Remove(item);
                    Unlink(node);
                }
                else
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <returns>
        /// true if the current set is a superset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            int numberOfOthers = CountOthers(other, out int numberOfOthersPresent);

            // All others must be present.
            return numberOfOthersPresent == numberOfOthers;
        }

        /// <summary>
        /// Determines whether the current set is a correct superset of a specified collection.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ISet`1"/> object is a correct superset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set. </param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int numberOfOthers = CountOthers(other, out int numberOfOthersPresent);

            // All others must be present, plus we need to have at least one additional item.
            return numberOfOthersPresent == numberOfOthers && numberOfOthers < Count;
        }

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <returns>
        /// true if the current set is equal to <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param><exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            int numberOfOthers = CountOthers(other, out int numberOfOthersPresent);

            return numberOfOthers == Count && numberOfOthersPresent == Count;
        }

        /// <summary>
        /// Adds an element to the current set and returns a value to indicate if the element was successfully added. 
        /// </summary>
        /// <returns>
        /// true if the element is added to the set; false if the element is already in the set.
        /// </returns>
        /// <param name="item">The element to add to the set.</param>
        public bool Add(T item)
        {
            if (elements.ContainsKey(item))
            {
                return false;
            }

            LinkedHashNode<T> node = new LinkedHashNode<T>(item) { Previous = last };

            if (first == null)
            {
                first = node;
            }

            if (last != null)
            {
                last.Next = node;
            }

            last = node;

            elements.Add(item, node);

            return true;
        }

        public bool AddAfter(T item, LinkedHashNode<T> itemInPlace)
        {
            if (elements.ContainsKey(item))
            {
                return false;
            }

            LinkedHashNode<T> node = new LinkedHashNode<T>(item) { Previous = itemInPlace };

            if (itemInPlace.Next != null)
            {
                node.Next = itemInPlace.Next;
                itemInPlace.Next.Previous = node;
            }
            else
            {
                last = node;
            }

            itemInPlace.Next = node;

            elements.Add(item, node);

            return true;
        }

        public bool PushAfter(T item, LinkedHashNode<T> itemInPlace)
        {
            if (elements.ContainsKey(item))
            {
                return false;
            }

            LinkedHashNode<T> node = Last;
            Unlink(node);
            elements.Remove(node.Value);
            node.Value = item;
            node.Next = null;
            node.Previous = itemInPlace;

            if (itemInPlace.Next != null)
            {
                node.Next = itemInPlace.Next;
                itemInPlace.Next.Previous = node;
            }
            else
            {
                last = node;
            }

            itemInPlace.Next = node;

            elements.Add(item, node);

            return true;
        }

        public bool AddBefore(T item, LinkedHashNode<T> itemInPlace)
        {
            if (elements.ContainsKey(item))
            {
                return false;
            }

            LinkedHashNode<T> node = new LinkedHashNode<T>(item) { Next = itemInPlace };

            if (itemInPlace.Previous != null)
            {
                node.Previous = itemInPlace.Previous;
                itemInPlace.Previous.Next = node;
            }
            else
            {
                first = node;
            }

            itemInPlace.Previous = node;

            elements.Add(item, node);

            return true;
        }

        public bool PushBefore(T item, LinkedHashNode<T> itemInPlace)
        {
            if (elements.ContainsKey(item))
            {
                return false;
            }

            LinkedHashNode<T> node = Last;
            Unlink(node);
            elements.Remove(node.Value);
            node.Value = item;
            node.Previous = null;
            node.Next = itemInPlace;

            if (itemInPlace.Previous != null)
            {
                node.Previous = itemInPlace.Previous;
                itemInPlace.Previous.Next = node;
            }
            else
            {
                first = node;
            }

            itemInPlace.Previous = node;

            elements.Add(item, node);

            return true;
        }

        #endregion

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/> struct that can be used to iterate through the collection.
        /// </returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }


        /// <summary>
        /// Count the elements in the given collection and determine both the total
        /// count and how many of the elements that are present in the current set.
        /// </summary>
        private int CountOthers(IEnumerable<T> items, out int numberOfOthersPresent)
        {
            numberOfOthersPresent = 0;
            int numberOfOthers = 0;

            foreach (T item in items)
            {
                numberOfOthers++;
                if (Contains(item))
                {
                    numberOfOthersPresent++;
                }
            }
            return numberOfOthers;
        }


        /// <summary>
        /// Cast the given collection to an ISet&lt;T&gt; if possible. If not,
        /// return a new set containing the items.
        /// </summary>
        private static ISet<T> AsSet(IEnumerable<T> items)
        {
            return items as ISet<T> ?? new HashSet<T>(items);
        }


        /// <summary>
        /// Unlink a node from the linked list by updating the node pointers in
        /// its preceeding and subsequent node. Also update the _first and _last
        /// pointers if necessary.
        /// </summary>
        private void Unlink(LinkedHashNode<T> node)
        {
            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
            }

            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }

            if (ReferenceEquals(node, first))
            {
                first = node.Next;
            }

            if (ReferenceEquals(node, last))
            {
                last = node.Previous;
            }
        }

        public class LinkedHashNode<TElement>
        {
            public TElement Value;
            public LinkedHashNode<TElement> Next;
            public LinkedHashNode<TElement> Previous;

            public LinkedHashNode(TElement value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private LinkedHashNode<T> _node;
            private T _current;

            internal Enumerator(LinkedHashSet<T> set)
            {
                _current = default(T);
                _node = set.first;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_node == null)
                {
                    return false;
                }

                _current = _node.Value;
                _node = _node.Next;
                return true;
            }

            /// <inheritdoc />
            public T Current => _current;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }

        public void AddMin(T item)
        {
            LinkedHashNode<T> current = Last;
            while (current != null && item.CompareTo(current.Value) < 0)
            {
                current = current.Previous;
            }

            if (current == Last)
            {
                return;
            }

            if (current == null)
            {
                AddBefore(item, First);
            }
            else
            {
                AddAfter(item, current);
            }
        }

        public void PushMin(T item)
        {
            LinkedHashNode<T> current = Last;
            while (current != null && item.CompareTo(current.Value) < 0)
            {
                current = current.Previous;
            }

            if (current == Last)
            {
                return;
            }

            if (current == null)
            {
                PushBefore(item, First);
            }
            else
            {
                PushAfter(item, current);
            }
        }
    }
}