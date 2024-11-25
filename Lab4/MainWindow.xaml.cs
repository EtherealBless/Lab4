using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Lab4.ViewModels;

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
                Console.ReadLine();
                InitializeComponent();
                Console.ReadLine();
                _viewModel = new SortingViewModel();
                Console.ReadLine();
                DataContext = _viewModel;
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
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