using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4
{
    public interface ISortingAlgorithm<T>
    {
        Task<SortingResult<T>> SortAsync(T[] array, CancellationToken cancellationToken);
    }

    public class SelectionSort<T> : ISortingAlgorithm<T>
    {
        private readonly IComparer<T> _comparer;

        public SelectionSort(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public async Task<SortingResult<T>> SortAsync(T[] array, CancellationToken cancellationToken)
        {
            var steps = new List<SortingStep<T>>();
            var currentArray = (T[])array.Clone();

            for (int i = 0; i < array.Length - 1 && !cancellationToken.IsCancellationRequested; i++)
            {
                int minIndex = i;
                steps.Add(new ComparisonSortingStep<T>(currentArray, i, i));

                for (int j = i + 1; j < array.Length && !cancellationToken.IsCancellationRequested; j++)
                {
                    steps.Add(new ComparisonSortingStep<T>(currentArray, j, minIndex, minIndex));

                    if (_comparer.Compare(currentArray[j], currentArray[minIndex]) < 0)
                    {
                        minIndex = j;
                    }
                }

                if (minIndex != i)
                {
                    // Swap elements
                    T temp = currentArray[i];
                    currentArray[i] = currentArray[minIndex];
                    currentArray[minIndex] = temp;
                    steps.Add(new SwapSortingStep<T>(currentArray, i, minIndex));
                }
                else
                {
                    steps.Add(StatusSortingStep<T>.AlreadyInPosition(currentArray, i));
                }

                await Task.Delay(1); // Yield to prevent blocking
            }

            if (cancellationToken.IsCancellationRequested)
                steps.Add(StatusSortingStep<T>.Cancelled(currentArray));
            else
                steps.Add(StatusSortingStep<T>.Completed(currentArray));

            return new SortingResult<T>(steps);
        }
    }
}
