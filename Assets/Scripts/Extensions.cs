using System;

public static class Extensions
{
    /* l is for left index and r is right index of the 
                    sub-array of arr to be sorted */
    public static void MergeSort<T>(this T[] arr, int l, int r) where T : IComparable<T>
    {
        if (l < r)
        {
            // Same as (l+r)/2, but avoids overflow for 
            // large l and h 
            int m = l + (r - l) / 2;

            // Sort first and second halves 
            MergeSort(arr, l, m);
            MergeSort(arr, m + 1, r);

            Merge(arr, l, m, r);
        }
    }

    // Merges two subarrays of arr[]. 
    // First subarray is arr[l..m] 
    // Second subarray is arr[m+1..r] 
    static void Merge<T>(T[] arr, int l, int m, int r) where T : IComparable<T>
    {
        int i, j, k;
        int n1 = m - l + 1;
        int n2 = r - m;

        /* create temp arrays */
        T[] L = new T[n1];
        T[] R = new T[n2];

        /* Copy data to temp arrays L[] and R[] */
        for (i = 0; i < n1; i++)
            L[i] = arr[l + i];
        for (j = 0; j < n2; j++)
            R[j] = arr[m + 1 + j];

        /* Merge the temp arrays back into arr[l..r]*/
        i = 0; // Initial index of first subarray 
        j = 0; // Initial index of second subarray 
        k = l; // Initial index of merged subarray 
        while (i < n1 && j < n2)
        {
            if (L[i].CompareTo(R[j]) < 0)
            {
                arr[k] = L[i];
                i++;
            }
            else
            {
                arr[k] = R[j];
                j++;
            }
            k++;
        }

        /* Copy the remaining elements of L[], if there 
           are any */
        while (i < n1)
        {
            arr[k] = L[i];
            i++;
            k++;
        }

        /* Copy the remaining elements of R[], if there 
           are any */
        while (j < n2)
        {
            arr[k] = R[j];
            j++;
            k++;
        }
    }
}
