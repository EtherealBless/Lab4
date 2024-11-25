using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4
{
    public class QuickSort<T> : ISortingAlgorithm<T>
    {
        private readonly IComparer<T> _comparer;
        private List<SortingStep<T>> _steps;
        private T[] _array;
        private CancellationToken _cancellationToken;

        public QuickSort(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public async Task<SortingResult<T>> SortAsync(T[] array, CancellationToken cancellationToken)
        {
            _steps = new List<SortingStep<T>>();
            _array = (T[])array.Clone();
            _cancellationToken = cancellationToken;

            await QuickSortAsync(0, array.Length - 1);

            if (cancellationToken.IsCancellationRequested)
                _steps.Add(StatusSortingStep<T>.Cancelled(_array));
            else
                _steps.Add(StatusSortingStep<T>.Completed(_array));

            return new SortingResult<T>(_steps);
        }

        private async Task QuickSortAsync(int low, int high)
        {
            if (low < high && !_cancellationToken.IsCancellationRequested)
            {
                _steps.Add(new PartitionStep<T>(_array, high, low, high));
                int pivotIndex = await PartitionAsync(low, high);
                
                await Task.Delay(1); // Yield to prevent blocking
                
                await QuickSortAsync(low, pivotIndex - 1);
                await QuickSortAsync(pivotIndex + 1, high);
            }
        }

        private async Task<int> PartitionAsync(int low, int high)
        {
            T pivot = _array[high];
            int i = low - 1;

            for (int j = low; j < high && !_cancellationToken.IsCancellationRequested; j++)
            {
                _steps.Add(new ComparisonSortingStep<T>(_array, j, high));

                if (_comparer.Compare(_array[j], pivot) < 0)
                {
                    i++;
                    // Swap elements
                    T temp = _array[i];
                    _array[i] = _array[j];
                    _array[j] = temp;
                    _steps.Add(new SwapSortingStep<T>(_array, i, j));
                }

                await Task.Delay(1); // Yield to prevent blocking
            }

            // Place pivot in correct position
            T temp2 = _array[i + 1];
            _array[i + 1] = _array[high];
            _array[high] = temp2;
            _steps.Add(new SwapSortingStep<T>(_array, i + 1, high));

            return i + 1;
        }
    }
}
