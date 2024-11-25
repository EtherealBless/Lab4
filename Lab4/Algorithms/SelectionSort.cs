using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lab4
{
    public class SortingStep
    {
        public int[] ArrayState { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public string Description { get; set; }

        public SortingStep(int[] arrayState, int index1, int index2, string description)
        {
            ArrayState = (int[])arrayState.Clone();
            Index1 = index1;
            Index2 = index2;
            Description = description;
        }
    }

    public class SortingResult
    {
        public List<SortingStep> Steps { get; set; }
        public int[] SortedArray { get; set; }

        public SortingResult(List<SortingStep> steps, int[] sortedArray)
        {
            Steps = steps;
            SortedArray = sortedArray;
        }
    }

    public interface ISortingAlgorithm
    {
        Task<SortingResult> SortAsync(int[] array, CancellationToken cancellationToken);
    }

    public class SelectionSort : ISortingAlgorithm
    {
        public async Task<SortingResult> SortAsync(int[] array, CancellationToken cancellationToken)
        {
            var steps = new List<SortingStep>();
            int[] currentArray = (int[])array.Clone();
            int n = array.Length;

            for (int i = 0; i < n - 1 && !cancellationToken.IsCancellationRequested; i++)
            {
                int minIndex = i;
                steps.Add(new SortingStep(currentArray, i, -1, $"Starting to find minimum from index {i}"));

                for (int j = i + 1; j < n && !cancellationToken.IsCancellationRequested; j++)
                {
                    steps.Add(new SortingStep(currentArray, minIndex, j, $"Comparing elements at positions {minIndex} and {j}"));

                    if (currentArray[j] < currentArray[minIndex])
                    {
                        minIndex = j;
                        steps.Add(new SortingStep(currentArray, i, minIndex, $"Found new minimum at position {minIndex}"));
                    }
                }

                if (minIndex != i)
                {
                    int temp = currentArray[i];
                    currentArray[i] = currentArray[minIndex];
                    currentArray[minIndex] = temp;

                    steps.Add(new SortingStep(currentArray, i, minIndex, 
                        $"Swapped elements: position {i} ({currentArray[i]}) with position {minIndex} ({currentArray[minIndex]})"));
                }
                else
                {
                    steps.Add(new SortingStep(currentArray, i, i, $"Element at position {i} is already in its correct position"));
                }

                await Task.Delay(1); // Yield to prevent blocking
            }

            if (cancellationToken.IsCancellationRequested)
            {
                steps.Add(new SortingStep(currentArray, -1, -1, "Sorting cancelled"));
            }
            else
            {
                steps.Add(new SortingStep(currentArray, -1, -1, "Sorting completed"));
            }

            return new SortingResult(steps, currentArray);
        }
    }
}
