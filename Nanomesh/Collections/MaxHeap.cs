using System;

namespace Nanomesh
{
    public static class MaxHeap
    {
        public static T FindKthLargest<T>(T[] nums, int k) where T : IComparable<T>
        {
            Heap<T> heap = new Heap<T>();
            heap.Heapify(nums, nums.Length);
            T data = default(T);
            for (int i = 0; i < k; i++)
            {
                data = heap.RemoveMax();
            }
            return data;
        }
    }

    public class Heap<T> where T : IComparable<T>
    {
        T[] arr;
        int count;
        int size;

        public int GetLeftChild(int pos)
        {
            int l = 2 * pos + 1;
            return l >= count ? -1 : l;
        }

        public int GetRightChild(int pos)
        {
            int r = 2 * pos + 2;
            return r >= count ? -1 : r;
        }

        public void Heapify(T[] num, int n)
        {
            arr = new T[n];
            size = n;
            for (int i = 0; i < n; i++)
                arr[i] = num[i];
            count = n;

            for (int i = (count - 1) / 2; i >= 0; i--)
            {
                PercolateDown(i);
            }
        }
        public void PercolateDown(int pos)
        {
            int l = GetLeftChild(pos);
            int r = GetRightChild(pos);
            int max = pos;
            if (l != -1 && arr[max].CompareTo(arr[l]) < 0)
                max = l;
            if (r != -1 && arr[max].CompareTo(arr[r]) < 0)
                max = r;
            if (max != pos)
            {
                T temp = arr[pos];
                arr[pos] = arr[max];
                arr[max] = temp;
                PercolateDown(max);
            }
        }
        public T RemoveMax()
        {
            T data = arr[0];
            arr[0] = arr[count - 1];
            count--;
            PercolateDown(0);
            return data;
        }
    }
}
