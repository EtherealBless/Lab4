using Lab4.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Lab4
{
    public partial class MainWindow : Window
    {
        private readonly SortingViewModel _viewModel;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow()
        {
            try
            {
                AllocConsole();
                InitializeComponent();
                _viewModel = new SortingViewModel();
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualizationSize();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualizationSize();
        }

        private void UpdateVisualizationSize()
        {
            if (_viewModel != null)
            {
                _viewModel.UpdateVisualizationSize(
                    visualizationCanvas.ActualWidth,
                    visualizationCanvas.ActualHeight);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
