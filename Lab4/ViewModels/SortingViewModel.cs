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

namespace Lab4.ViewModels
{
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
        private readonly ISortingAlgorithm _sortingAlgorithm;
        private CancellationTokenSource? _cancellationTokenSource;
        private SortingResult _sortingResult;
        private int _currentStepIndex;
        private bool _isPaused;
        private int[] _array;

        public ICommand GenerateArrayCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand GoBackCommand { get; }

        public SortingViewModel()
        {
            _bars = new ObservableCollection<BarViewModel>();
            _statusText = "Ready";
            _speed = 50;
            _canGenerate = true;
            _eventLog = new ObservableCollection<string>();
            _sortingAlgorithm = new SelectionSort();
            _currentStepIndex = -1;

            GenerateArrayCommand = new RelayCommand(ExecuteGenerateArray, () => CanGenerate);
            StartCommand = new RelayCommand(ExecuteStart, () => CanStart);
            PauseCommand = new RelayCommand(ExecutePause, () => CanPause);
            StepCommand = new RelayCommand(ExecuteStep, () => CanStep);
            GoBackCommand = new RelayCommand(ExecuteGoBack, () => CanGoBack);
        }

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

        private void ExecuteGenerateArray()
        {
            GenerateRandomArray();
            UpdateButtonStates();
        }

        private async void ExecuteStart()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                CanStart = false;
                CanPause = true;
                CanStep = false;
                CanGoBack = false;
                CanGenerate = false;
                _isPaused = false;

                LogEvent("Starting sorting algorithm");
                _sortingResult = await _sortingAlgorithm.SortAsync((int[])_array.Clone(), _cancellationTokenSource.Token);
                _currentStepIndex = 0;

                await PlaySortingStepsAsync();

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    StatusText = "Sorting completed";
                    LogEvent("Sorting completed");
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error during sorting: {ex.Message}");
                MessageBox.Show($"Error during sorting: {ex.Message}");
            }
            finally
            {
                ResetUIState();
            }
        }

        private async Task PlaySortingStepsAsync()
        {
            while (_currentStepIndex < _sortingResult.Steps.Count && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await HandlePause();
                if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                var step = _sortingResult.Steps[_currentStepIndex];
                UpdateBars(step.ArrayState, step.Index1, step.Index2);
                StatusText = step.Description;
                LogEvent($"Step {_currentStepIndex + 1}: {step.Description}");

                await Task.Delay((int)(1000 / Speed));
                _currentStepIndex++;
                UpdateStepButtons();
            }
        }

        private async Task HandlePause()
        {
            while (_isPaused && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        private void ExecutePause()
        {
            _isPaused = !_isPaused;
            CanStep = _isPaused;
            CanGoBack = _isPaused;
            StatusText = _isPaused ? "Paused" : "Running";
            LogEvent(_isPaused ? "Sorting paused" : "Sorting resumed");
            UpdateButtonStates();
        }

        private void ExecuteStep()
        {
            if (_currentStepIndex < _sortingResult.Steps.Count)
            {
                var step = _sortingResult.Steps[_currentStepIndex];
                UpdateBars(step.ArrayState, step.Index1, step.Index2);
                StatusText = step.Description;
                LogEvent($"Step {_currentStepIndex + 1}: {step.Description}");
                _currentStepIndex++;
                UpdateStepButtons();
            }
        }

        private void ExecuteGoBack()
        {
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                var step = _sortingResult.Steps[_currentStepIndex];
                UpdateBars(step.ArrayState, step.Index1, step.Index2);
                StatusText = step.Description;
                LogEvent($"Went back to step {_currentStepIndex + 1}: {step.Description}");
                UpdateStepButtons();
            }
        }

        private void ResetUIState()
        {
            CanStart = true;
            CanPause = false;
            CanStep = false;
            CanGoBack = false;
            CanGenerate = true;
            _isPaused = false;
            UpdateButtonStates();
        }

        private void UpdateStepButtons()
        {
            CanGoBack = _currentStepIndex > 0;
            CanStep = _currentStepIndex < (_sortingResult?.Steps.Count ?? 0);
        }

        private void GenerateRandomArray()
        {
            var random = new Random();
            _array = new int[20];
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = random.Next(10, 100);
            }
            UpdateBars(_array);
            StatusText = "New array generated";
            CanStart = true;
            CanPause = false;
            CanStep = false;
            CanGoBack = false;
            LogEvent("Generated new random array");
        }

        public void UpdateVisualizationSize(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;

            if (Bars.Count > 0)
            {
                const int padding = 10;
                double availableHeight = _canvasHeight - (2 * padding);
                double barHeight = Math.Max((availableHeight / Bars.Count) - 5, 5);
                double maxWidth = _canvasWidth - 20;
                double maxBarValue = Bars.Max(b => b.Value);

                for (int i = 0; i < Bars.Count; i++)
                {
                    var bar = Bars[i];
                    double widthPercentage = (double)bar.Value / maxBarValue;
                    bar.Width = widthPercentage * maxWidth;
                    bar.Height = barHeight;
                    bar.Top = padding + i * (barHeight + 5);
                }
            }
        }

        public void UpdateBars(int[] array, int highlightIndex1 = -1, int highlightIndex2 = -1)
        {
            const int padding = 10;
            double availableHeight = _canvasHeight - (2 * padding);
            double barHeight = Math.Max((availableHeight / array.Length) - 5, 5);
            double maxWidth = _canvasWidth - 20;
            double maxBarValue = array.Max();

            if (Bars.Count != array.Length)
            {
                Bars.Clear();
                for (int i = 0; i < array.Length; i++)
                {
                    Bars.Add(new BarViewModel());
                }
            }

            for (int i = 0; i < array.Length; i++)
            {
                var bar = Bars[i];
                double widthPercentage = (double)array[i] / maxBarValue;
                bar.Value = array[i];
                bar.Width = widthPercentage * maxWidth;
                bar.Height = barHeight;
                bar.Top = padding + i * (barHeight + 5);
                bar.Fill = (i == highlightIndex1 || i == highlightIndex2) ?
                          Brushes.Red : Brushes.Blue;
            }
        }

        private void LogEvent(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EventLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }

        private void UpdateButtonStates()
        {
            LogEvent($"Updated button states - Start: {CanStart}, Pause: {CanPause}, Step: {CanStep}, Back: {CanGoBack}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
