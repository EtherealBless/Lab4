using System;

namespace Lab4
{
    public abstract class SortingStep<T>
    {
        public T[] ArrayState { get; }
        public int Index1 { get; }
        public int Index2 { get; }
        public abstract string Description { get; }

        protected SortingStep(T[] arrayState, int index1, int index2)
        {
            ArrayState = (T[])arrayState.Clone();
            Index1 = index1;
            Index2 = index2;
        }
    }

    public class ComparisonSortingStep<T> : SortingStep<T>
    {
        public int? MinIndex { get; }
        public override string Description => MinIndex.HasValue ? 
            $"Comparing current minimum ({ArrayState[MinIndex.Value]}) at position {MinIndex.Value} with element ({ArrayState[Index2]}) at position {Index2}" :
            $"Comparing elements at positions {Index1} ({ArrayState[Index1]}) and {Index2} ({ArrayState[Index2]})";

        public ComparisonSortingStep(T[] arrayState, int index1, int index2, int? minIndex = null) 
            : base(arrayState, index1, index2)
        {
            MinIndex = minIndex;
        }
    }

    public class SwapSortingStep<T> : SortingStep<T>
    {
        public override string Description => 
            $"Swapped elements: position {Index1} ({ArrayState[Index1]}) with position {Index2} ({ArrayState[Index2]})";

        public SwapSortingStep(T[] arrayState, int index1, int index2) 
            : base(arrayState, index1, index2)
        {
        }
    }

    public class ShiftStep<T> : SortingStep<T>
    {
        public override string Description => 
            $"Shifted element from position {Index1} ({ArrayState[Index1]}) to position {Index2}";

        public ShiftStep(T[] arrayState, int index1, int index2)
            : base(arrayState, index1, index2)
        {
        }
    }

    public class ShellSortGapStep<T> : SortingStep<T>
    {
        public int Gap { get; }
        public override string Description => 
            $"Using gap size: {Gap}";

        public ShellSortGapStep(T[] arrayState, int gap)
            : base(arrayState, -1, -1)
        {
            Gap = gap;
        }
    }

    public class PartitionStep<T> : SortingStep<T>
    {
        public int PivotIndex { get; }
        public int LeftPointer { get; }
        public int RightPointer { get; }
        public bool IsPartitionComplete { get; }

        public override string Description
        {
            get
            {
                if (IsPartitionComplete)
                    return $"Partition complete. Pivot ({ArrayState[PivotIndex]}) is now in its final position.";
                
                if (Index1 == -1 && Index2 == -1)
                    return $"Starting partition with pivot {ArrayState[PivotIndex]} at position {PivotIndex}";
                
                return $"Comparing elements: left pointer at position {LeftPointer} ({ArrayState[LeftPointer]}), " +
                       $"right pointer at position {RightPointer} ({ArrayState[RightPointer]})";
            }
        }

        public PartitionStep(T[] arrayState, int pivotIndex, int leftPointer, int rightPointer, bool isPartitionComplete = false)
            : base(arrayState, -1, -1)
        {
            PivotIndex = pivotIndex;
            LeftPointer = leftPointer;
            RightPointer = rightPointer;
            IsPartitionComplete = isPartitionComplete;
        }
    }

    public class InsertionStep<T> : SortingStep<T>
    {
        public T CurrentElement { get; }
        public override string Description => 
            $"Inserting element {CurrentElement} at position {Index2}";

        public InsertionStep(T[] arrayState, int index1, int index2, T currentElement)
            : base(arrayState, index1, index2)
        {
            CurrentElement = currentElement;
        }
    }

    public class StatusSortingStep<T> : SortingStep<T>
    {
        private readonly string _description;
        public override string Description => _description;

        private StatusSortingStep(T[] arrayState, string description)
            : base(arrayState, -1, -1)
        {
            _description = description;
        }

        public static StatusSortingStep<T> Completed(T[] arrayState) =>
            new StatusSortingStep<T>(arrayState, "Sorting completed!");

        public static StatusSortingStep<T> Cancelled(T[] arrayState) =>
            new StatusSortingStep<T>(arrayState, "Sorting cancelled.");

        public static StatusSortingStep<T> MinFound(T[] arrayState, int index) =>
            new StatusSortingStep<T>(arrayState, $"Found minimum element ({arrayState[index]}) at position {index}");

        public static StatusSortingStep<T> AlreadyInPosition(T[] arrayState, int index) =>
            new StatusSortingStep<T>(arrayState, $"Element ({arrayState[index]}) is already in correct position {index}");
    }
}
