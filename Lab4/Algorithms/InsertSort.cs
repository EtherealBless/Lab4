using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4
{
    public class InsertSort<T> : ISortingAlgorithm<T>
    {
        private readonly IComparer<T> _comparer;

        public InsertSort(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public async Task<SortingResult<T>> SortAsync(T[] array, CancellationToken cancellationToken)
        {
            var steps = new List<SortingStep<T>>();
            var currentArray = (T[])array.Clone();

            for (int i = 1; i < array.Length && !cancellationToken.IsCancellationRequested; i++)
            {
                T key = currentArray[i];
                int j = i - 1;

                steps.Add(new InsertionStep<T>(currentArray, i, i, key));

                while (j >= 0 && !cancellationToken.IsCancellationRequested)
                {
                    steps.Add(new ComparisonSortingStep<T>(currentArray, j, j + 1));
                    if (_comparer.Compare(currentArray[j], key) > 0)
                    {
                        currentArray[j + 1] = currentArray[j];
                        steps.Add(new ShiftStep<T>(currentArray, j, j + 1));
                        j--;
                    }
                    else
                        break;
                }

                currentArray[j + 1] = key;
                if (j + 1 != i)
                    steps.Add(new InsertionStep<T>(currentArray, i, j + 1, key));

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
