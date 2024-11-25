using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using Lab4.Commands;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Win32;
using Lab4.Models;
using Lab4.Views;

namespace Lab4.ViewModels
{
    public enum SortingAlgorithmType
    {
        Selection,
        Insertion,
        Shell,
        Quick
    }

    public class SortingViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BarViewModel> _bars;
        private string _statusText;
        private bool _canStart;
        private bool _canPause;
        private bool _canStep;
        private bool _canGoBack;
        private double _speed;
        private bool _canGenerate;
        private double _canvasWidth;
        private double _canvasHeight;
        private ObservableCollection<string> _eventLog;
        private ISortingAlgorithm<Dictionary<string, string>> _sortingAlgorithm;
        private CancellationTokenSource? _cancellationTokenSource;
        private SortingResult<Dictionary<string, string>> _sortingResult;
        private int _currentStepIndex;
        private bool _isPaused;
        private bool _isRunning;
        private Models.CsvData _csvData;
        private Dictionary<int, int> _elementIndentations = new Dictionary<int, int>();
        private SortingAlgorithmType _selectedAlgorithm;

        public ICommand OpenCsvCommand { get; }
        public ICommand GenerateArrayCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand GoBackCommand { get; }

        public SortingAlgorithmType SelectedAlgorithm
        {
            get => _selectedAlgorithm;
            set
            {
                _selectedAlgorithm = value;
                UpdateSortingAlgorithm();
                OnPropertyChanged(nameof(SelectedAlgorithm));
            }
        }

        public Array AvailableAlgorithms => Enum.GetValues(typeof(SortingAlgorithmType));

        public ObservableCollection<BarViewModel> Bars
        {
            get => _bars;
            set
            {
                _bars = value;
                OnPropertyChanged(nameof(Bars));
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public bool CanStart
        {
            get => _canStart;
            set
            {
                _canStart = value;
                OnPropertyChanged(nameof(CanStart));
            }
        }

        public bool CanPause
        {
            get => _canPause;
            set
            {
                _canPause = value;
                OnPropertyChanged(nameof(CanPause));
            }
        }

        public bool CanStep
        {
            get => _canStep;
            set
            {
                _canStep = value;
                OnPropertyChanged(nameof(CanStep));
            }
        }

        public bool CanGoBack
        {
            get => _canGoBack;
            set
            {
                _canGoBack = value;
                OnPropertyChanged(nameof(CanGoBack));
            }
        }

        public bool CanGenerate
        {
            get => _canGenerate;
            set
            {
                _canGenerate = value;
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        public double Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }

        public ObservableCollection<string> EventLog
        {
            get => _eventLog;
            set
            {
                _eventLog = value;
                OnPropertyChanged(nameof(EventLog));
            }
        }

        public SortingViewModel()
        {
            _bars = new ObservableCollection<BarViewModel>();
            _statusText = "Ready";
            _speed = 50;
            _canGenerate = true;
            _canStart = false;
            _canPause = false;
            _canStep = false;
            _canGoBack = false;
            _eventLog = new ObservableCollection<string>();
            _selectedAlgorithm = SortingAlgorithmType.Selection;
            UpdateSortingAlgorithm();
            _currentStepIndex = -1;
            _isRunning = false;

            OpenCsvCommand = new RelayCommand(ExecuteOpenCsv, () => CanGenerate);
            GenerateArrayCommand = new RelayCommand(ExecuteGenerateArray, () => CanGenerate);
            StartCommand = new RelayCommand(ExecuteStart, () => CanStart);
            PauseCommand = new RelayCommand(ExecutePause, () => CanPause);
            StepCommand = new RelayCommand(ExecuteStep, () => CanStep);
            GoBackCommand = new RelayCommand(ExecuteGoBack, () => CanGoBack);

            // Generate initial random array
            ExecuteGenerateArray();
        }

        private ISortingAlgorithm<Dictionary<string, string>> GetSortingAlgorithm()
        {
            var columns = _csvData?.SelectedColumns ?? new List<string>();
            var comparer = new CsvRowComparer(columns);
            
            return _selectedAlgorithm switch
            {
                SortingAlgorithmType.Selection => new SelectionSort<Dictionary<string, string>>(comparer),
                SortingAlgorithmType.Insertion => new InsertSort<Dictionary<string, string>>(comparer),
                SortingAlgorithmType.Shell => new ShellSort<Dictionary<string, string>>(comparer),
                SortingAlgorithmType.Quick => new QuickSort<Dictionary<string, string>>(comparer),
                _ => throw new ArgumentException("Invalid sorting algorithm type")
            };
        }

        private void UpdateSortingAlgorithm()
        {
            _sortingAlgorithm = GetSortingAlgorithm();
            LogEvent($"Selected {_selectedAlgorithm} sort algorithm");
        }

        private async void ExecuteOpenCsv()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _csvData = CsvData.LoadFromFile(dialog.FileName);
                    
                    // Filter out non-numeric columns
                    var numericColumns = _csvData.Headers.Where(header => 
                        _csvData.Rows.Any(row => 
                            double.TryParse(row[header], out _)
                        )
                    ).ToList();

                    if (numericColumns.Count == 0)
                    {
                        MessageBox.Show("No numeric columns found in the CSV file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var columnSelection = new ColumnSelectionWindow(numericColumns);
                    if (columnSelection.ShowDialog() == true)
                    {
                        _csvData.SelectedColumns = columnSelection.SelectedColumns;
                        await LoadCsvDataIntoArray();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadCsvDataIntoArray()
        {
            if (_csvData == null || _csvData.Rows.Count == 0)
            {
                return;
            }

            // Calculate ranges for normalization
            _csvData.CalculateColumnRanges();

            // Convert CSV data to array format
            var indices = Enumerable.Range(0, _csvData.Rows.Count).ToArray();

            // Create the sorting result with initial state
            var initialArray = indices.Select(i => new Dictionary<string, string> { 
                { "index", i.ToString() } 
            }).ToArray();
            var initialStep = StatusSortingStep<Dictionary<string, string>>.Completed(initialArray);
            _sortingResult = new SortingResult<Dictionary<string, string>>(new List<SortingStep<Dictionary<string, string>>> { initialStep });
            _currentStepIndex = 0;
            
            // Update visualization
            UpdateBars(_sortingResult.Steps[0]);
            StatusText = "CSV data loaded";
            LogEvent($"Loaded CSV file with {_csvData.Rows.Count} rows");

            // Update button states
            CanStart = true;
            CanPause = false;
            CanStep = false;
            CanGoBack = false;

            // Notify property changes
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStep));
            OnPropertyChanged(nameof(CanGoBack));
        }

        private void ExecuteGenerateArray()
        {
            if (_isRunning)
                return;

            GenerateRandomArray(20);
            _canStart = true;
            _canPause = false;
            _canStep = true;
            _canGoBack = false;
            UpdateStepButtons();
            LogEvent("Generated new random array");
        }

        private async void ExecuteStart()
        {
            if (_sortingResult == null || _isRunning) return;

            _isRunning = true;
            _isPaused = false;
            UpdateStepButtons();

            try
            {
                if (_currentStepIndex >= _sortingResult.Steps.Count)
                {
                    // If we're at the end, generate new sorting result
                    _cancellationTokenSource = new CancellationTokenSource();
                    var values = _csvData.Rows.Select(row => 
                        row.ToDictionary(column => column.Key, column => column.Value)
                    ).ToArray();
                    _sortingResult = await _sortingAlgorithm.SortAsync(values, _cancellationTokenSource.Token);
                    _currentStepIndex = 0;
                }

                await RunSortingAnimation();
            }
            finally
            {
                _isRunning = false;
                UpdateStepButtons();
            }
        }

        private void ExecutePause()
        {
            if (!_isRunning) return;
            _isPaused = true;
            UpdateStepButtons();
        }

        private void ExecuteStep()
        {
            if (_sortingResult == null || _currentStepIndex >= _sortingResult.Steps.Count || _isRunning) return;

            var step = _sortingResult.Steps[_currentStepIndex];
            UpdateBars(step);
            StatusText = step.Description;
            LogEvent($"Step {_currentStepIndex + 1}: {step.Description}");
            _currentStepIndex++;
            
            UpdateStepButtons();
        }

        private void ExecuteGoBack()
        {
            if (_sortingResult == null || _currentStepIndex <= 0 || _isRunning) return;

            _currentStepIndex--;
            var step = _sortingResult.Steps[_currentStepIndex];
            UpdateBars(step);
            StatusText = step.Description;
            LogEvent($"Went back to step {_currentStepIndex + 1}: {step.Description}");
            
            UpdateStepButtons();
        }

        private void GenerateRandomArray(int size)
        {
            var array = new int[size];
            var random = new Random();
            for (int i = 0; i < size; i++)
            {
                array[i] = random.Next(10, 100);
            }

            // Create initial step to show unsorted array
            var randomArray = array.Select((value, index) => new Dictionary<string, string> { 
                { "index", index.ToString() },
                { "value", value.ToString() }
            }).ToArray();
            var initialStep = StatusSortingStep<Dictionary<string, string>>.Completed(randomArray);
            _sortingResult = new SortingResult<Dictionary<string, string>>(new List<SortingStep<Dictionary<string, string>>> { initialStep });
            _currentStepIndex = 0;
            
            // Update visualization
            UpdateBars(_sortingResult.Steps[0]);
            StatusText = "Generated new array";
            LogEvent($"Generated array with {size} elements");
        }

        private void UpdateStepButtons()
        {
            CanStart = _sortingResult != null;
            CanPause = _isRunning;
            CanStep = _sortingResult != null && _currentStepIndex < _sortingResult.Steps.Count && !_isRunning;
            CanGoBack = _sortingResult != null && _currentStepIndex > 0 && !_isRunning;

            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStep));
            OnPropertyChanged(nameof(CanGoBack));
        }

        private async Task RunSortingAnimation()
        {
            while (_currentStepIndex < _sortingResult.Steps.Count && !_isPaused)
            {
                var step = _sortingResult.Steps[_currentStepIndex];
                UpdateBars(step);
                StatusText = step.Description;
                LogEvent($"Step {_currentStepIndex + 1}: {step.Description}");

                await Task.Delay((int)(1000 / Speed));
                _currentStepIndex++;
                UpdateStepButtons();
            }
        }

        public void UpdateVisualizationSize(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;

            // If we have an active step, update the visualization
            if (_sortingResult != null && _currentStepIndex >= 0 && _currentStepIndex < _sortingResult.Steps.Count)
            {
                UpdateBars(_sortingResult.Steps[_currentStepIndex]);
            }
        }

        private void UpdateBars(SortingStep<Dictionary<string, string>> step)
        {
            if (step?.ArrayState == null || step.ArrayState.Length == 0 || _csvData == null) return;

            const int padding = 10;
            const int indentSize = 20;
            double availableHeight = _canvasHeight - (2 * padding);
            double barHeight = Math.Max((availableHeight / step.ArrayState.Length) - 5, 5);
            double maxWidth = _canvasWidth - 20;

            // Calculate the maximum value for scaling
            double maxValue = 1.0; // Since we're using normalized values

            if (Bars.Count != step.ArrayState.Length)
            {
                Bars.Clear();
                for (int i = 0; i < step.ArrayState.Length; i++)
                {
                    Bars.Add(new BarViewModel());
                    _elementIndentations[i] = 0;
                }
            }

            // Handle QuickSort partition steps
            if (step is PartitionStep<Dictionary<string, string>> partition)
            {
                if (!partition.IsPartitionComplete)
                {
                    for (int i = partition.LeftPointer; i <= partition.RightPointer; i++)
                    {
                        _elementIndentations[i] = 1;
                    }
                }
                else
                {
                    for (int i = partition.LeftPointer; i <= partition.RightPointer; i++)
                    {
                        _elementIndentations[i] = 0;
                    }
                }
            }

            // Update all bars with their new values and positions
            for (int i = 0; i < step.ArrayState.Length; i++)
            {
                var bar = Bars[i];
                var rowDict = step.ArrayState[i];
                var dataIndex = int.Parse(rowDict.ContainsKey("index") ? rowDict["index"] : "0");
                var row = _csvData.Rows[dataIndex];

                // Calculate the maximum normalized value from selected columns for this row
                double maxNormalizedValue = 0;
                var columnValues = new Dictionary<string, double>();
                foreach (var column in _csvData.SelectedColumns)
                {
                    if (_csvData.TryGetNormalizedValue(row, column, out double normalizedValue))
                    {
                        columnValues[column] = normalizedValue;
                        maxNormalizedValue = Math.Max(maxNormalizedValue, normalizedValue);
                    }
                }

                double widthPercentage = maxNormalizedValue;
                bar.ColumnValues = columnValues;
                
                int currentIndent = _elementIndentations[i] * indentSize;
                
                bar.Width = widthPercentage * (maxWidth - currentIndent);
                bar.Height = barHeight;
                bar.Left = padding + currentIndent;
                bar.Top = padding + i * (barHeight + 5);

                // Get color based on current operation
                bar.Fill = GetBarColor(i, step);
            }

            if (step is StatusSortingStep<Dictionary<string, string>> status && status.Description.Contains("completed"))
            {
                for (int i = 0; i < step.ArrayState.Length; i++)
                {
                    _elementIndentations[i] = 0;
                }
            }
        }

        private static class Colors
        {
            public static readonly Brush Sorted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            public static readonly Brush Comparing = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
            public static readonly Brush Swapping = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722"));
            public static readonly Brush Shifting = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E91E63"));
            public static readonly Brush GapPivot = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9C27B0"));
            public static readonly Brush Current = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            public static readonly Brush Unsorted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607D8B"));
            public static readonly Brush MinimumElement = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF80AB"));
        }

        private Brush GetBarColor(int index, SortingStep<Dictionary<string, string>> step)
        {
            // Sorted elements
            var sortedIndices = GetSortedIndices(step);
            if (sortedIndices.Contains(index))
                return Colors.Sorted;

            // Current step coloring
            switch (step)
            {
                case ComparisonSortingStep<Dictionary<string, string>> compStep:
                    if (compStep.MinIndex.HasValue && index == compStep.MinIndex.Value)
                        return Colors.MinimumElement;
                    if (index == compStep.Index2)
                        return Colors.Comparing;
                    break;

                case SwapSortingStep<Dictionary<string, string>> swapStep:
                    if (index == swapStep.Index1 || index == swapStep.Index2)
                        return Colors.Swapping;
                    break;

                case ShiftStep<Dictionary<string, string>> shiftStep:
                    if (index == shiftStep.Index1 || index == shiftStep.Index2)
                        return Colors.Shifting;
                    break;

                case ShellSortGapStep<Dictionary<string, string>> shellGapStep:
                    var gap = shellGapStep.Gap;
                    if (index % gap == 0)
                        return Colors.GapPivot;
                    break;

                case InsertionStep<Dictionary<string, string>> insertion:
                    if (index == insertion.Index1)
                        return Colors.Current;
                    if (index == insertion.Index2)
                        return Colors.Comparing;
                    break;

                case PartitionStep<Dictionary<string, string>> partition:
                    if (index == partition.PivotIndex)
                        return Colors.GapPivot;
                    if (index == partition.LeftPointer || index == partition.RightPointer)
                        return Colors.Current;
                    break;
            }

            return Colors.Unsorted;
        }

        private HashSet<int> GetSortedIndices(SortingStep<Dictionary<string, string>> step)
        {
            var sorted = new HashSet<int>();

            // Handle final status step
            if (step is StatusSortingStep<Dictionary<string, string>> status && status.Description.Contains("Sorting completed"))
            {
                for (int i = 0; i < step.ArrayState.Length; i++)
                    sorted.Add(i);
                return sorted;
            }

            switch (_selectedAlgorithm)
            {
                case SortingAlgorithmType.Selection:
                    // In selection sort, elements from 0 to i-1 are sorted
                    if (step is StatusSortingStep<Dictionary<string, string>> statusStep)
                    {
                        if (statusStep.Description.StartsWith("Starting to find minimum from index"))
                        {
                            // Extract position from "Starting to find minimum from index X"
                            var position = int.Parse(statusStep.Description.Split(' ').Last());
                            for (int i = 0; i < position; i++)
                                sorted.Add(i);
                        }
                        else if (statusStep.Description.Contains("is already in its correct position"))
                        {
                            // Extract position from "Element at position X is already in its correct position"
                            var position = int.Parse(statusStep.Description.Split(' ')[3]);
                            for (int i = 0; i <= position; i++)
                                sorted.Add(i);
                        }
                        // Don't mark anything as sorted for MinFound status
                    }
                    else if (step is SwapSortingStep<Dictionary<string, string>> swapStep)
                    {
                        // After a swap, elements up to Index1 are sorted
                        for (int i = 0; i <= swapStep.Index1; i++)
                            sorted.Add(i);
                    }
                    else if (step is ComparisonSortingStep<Dictionary<string, string>> comparison)
                    {
                        // During comparison, only previously sorted elements remain sorted
                        var position = GetLastSortedPosition(step.ArrayState);
                        for (int i = 0; i < position; i++)
                            sorted.Add(i);
                    }
                    break;

                case SortingAlgorithmType.Insertion:
                    // In insertion sort, elements before CurrentIndex are sorted
                    if (step is InsertionStep<Dictionary<string, string>> insertStep)
                    {
                        for (int i = 0; i < insertStep.Index1; i++)
                            sorted.Add(i);
                    }
                    break;

                case SortingAlgorithmType.Shell:
                    // In shell sort, we don't mark elements as sorted until the final pass (gap = 1)
                    if (step is ShellSortGapStep<Dictionary<string, string>> shellGapStep && shellGapStep.Gap == 1)
                    {
                        for (int i = 0; i < step.Index1; i++)
                            sorted.Add(i);
                    }
                    break;

                case SortingAlgorithmType.Quick:
                    // In quicksort, elements are sorted when they're in their final position after partitioning
                    if (step is PartitionStep<Dictionary<string, string>> partitionStep && partitionStep.IsPartitionComplete)
                        sorted.Add(partitionStep.PivotIndex);
                    break;
            }

            return sorted;
        }

        // Helper method to determine the last sorted position in selection sort
        private int GetLastSortedPosition(Dictionary<string, string>[] array)
        {
            var comparer = new CsvRowComparer(_csvData?.SelectedColumns ?? new List<string>());
            for (int i = 1; i < array.Length; i++)
            {
                if (comparer.Compare(array[i], array[i - 1]) < 0)
                    return i - 1;
            }
            return array.Length - 1;
        }

        private void LogEvent(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EventLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
