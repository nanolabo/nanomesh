using System;

namespace Nanomesh
{
    public static class OrderStatistics
    {
        private static T FindMedian<T>(T[] arr, int i, int n)
        {
            if (i <= n)
                Array.Sort(arr, i, n); // Sort the array  
            else
                Array.Sort(arr, n, i);
            return arr[n / 2]; // Return middle element  
        }

        // Returns k'th smallest element  
        // in arr[l..r] in worst case  
        // linear time. ASSUMPTION: ALL  
        // ELEMENTS IN ARR[] ARE DISTINCT  
        public static T FindKthSmallest<T>(T[] arr, int l, int r, int k) where T : IComparable<T>
        {
            // If k is smaller than  
            // number of elements in array  
            if (k > 0 && k <= r - l + 1)
            {
                int n = r - l + 1; // Number of elements in arr[l..r]  

                // Divide arr[] in groups of size 5,  
                // calculate median of every group  
                // and store it in median[] array.  
                int i;

                // There will be floor((n+4)/5) groups;  
                T[] median = new T[(n + 4) / 5];
                for (i = 0; i < n / 5; i++)
                    median[i] = FindMedian(arr, l + i * 5, 5);

                // For last group with less than 5 elements  
                if (i * 5 < n)
                {
                    median[i] = FindMedian(arr, l + i * 5, n % 5);
                    i++;
                }

                // Find median of all medians using recursive call.  
                // If median[] has only one element, then no need  
                // of recursive call  
                T medOfMed = (i == 1) ? median[i - 1] : FindKthSmallest(median, 0, i - 1, i / 2);

                // Partition the array around a random element and  
                // get position of pivot element in sorted array  
                int pos = Partition(arr, l, r, medOfMed);

                // If position is same as k  
                if (pos - l == k - 1)
                    return arr[pos];
                if (pos - l > k - 1) // If position is more, recur for left  
                    return FindKthSmallest(arr, l, pos - 1, k);

                // Else recur for right subarray  
                return FindKthSmallest(arr, pos + 1, r, k - pos + l - 1);
            }

            // If k is more than number of elements in array  
            return default(T);
        }

        private static void Swap<T>(ref T[] arr, int i, int j)
        {
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        // It searches for x in arr[l..r], and  
        // partitions the array around x.  
        private static int Partition<T>(T[] arr, int l, int r, T x) where T : IComparable<T>
        {
            // Search for x in arr[l..r] and move it to end  
            int i;
            for (i = l; i < r; i++)
                if (arr[i].CompareTo(x) == 0)
                    break;
            Swap(ref arr, i, r);

            // Standard partition algorithm  
            i = l;
            for (int j = l; j <= r - 1; j++)
            {
                if (arr[j].CompareTo(x) <= 0)
                {
                    Swap(ref arr, i, j);
                    i++;
                }
            }
            Swap(ref arr, i, r);
            return i;
        }

    }
}
