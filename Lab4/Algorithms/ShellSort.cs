using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4
{
    public class ShellSort<T> : ISortingAlgorithm<T>
    {
        private readonly IComparer<T> _comparer;

        public ShellSort(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public async Task<SortingResult<T>> SortAsync(T[] array, CancellationToken cancellationToken)
        {
            var steps = new List<SortingStep<T>>();
            var currentArray = (T[])array.Clone();
            int n = array.Length;

            // Start with a large gap, then reduce the gap
            for (int gap = n/2; gap > 0 && !cancellationToken.IsCancellationRequested; gap /= 2)
            {
                steps.Add(new ShellSortGapStep<T>(currentArray, gap));

                // Do a gapped insertion sort for this gap size.
                // The first gap elements a[0..gap-1] are already in gapped order
                // keep adding one more element until the entire array is gap sorted
                for (int i = gap; i < n && !cancellationToken.IsCancellationRequested; i++)
                {
                    // add a[i] to the elements that have been gap sorted
                    // save a[i] in temp and make a hole at position i
                    T temp = currentArray[i];
                    steps.Add(new InsertionStep<T>(currentArray, i, i, temp));

                    // shift earlier gap-sorted elements up until the correct location for a[i] is found
                    int j;
                    for (j = i; j >= gap && !cancellationToken.IsCancellationRequested; j -= gap)
                    {
                        steps.Add(new ComparisonSortingStep<T>(currentArray, j - gap, j));
                        if (_comparer.Compare(currentArray[j - gap], temp) > 0)
                        {
                            currentArray[j] = currentArray[j - gap];
                            steps.Add(new ShiftStep<T>(currentArray, j - gap, j));
                        }
                        else
                            break;
                    }

                    // put temp (the original a[i]) in its correct location
                    currentArray[j] = temp;
                    if (j != i)
                        steps.Add(new InsertionStep<T>(currentArray, i, j, temp));

                    await Task.Delay(1); // Yield to prevent blocking
                }
            }

            if (cancellationToken.IsCancellationRequested)
                steps.Add(StatusSortingStep<T>.Cancelled(currentArray));
            else
                steps.Add(StatusSortingStep<T>.Completed(currentArray));

            return new SortingResult<T>(steps);
        }
    }
}
