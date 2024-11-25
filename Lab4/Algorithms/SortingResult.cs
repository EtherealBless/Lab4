using System.Collections.Generic;

namespace Lab4
{
    public class SortingResult<T>
    {
        public IReadOnlyList<SortingStep<T>> Steps { get; }

        public SortingResult(List<SortingStep<T>> steps)
        {
            Steps = steps;
        }
    }
}
