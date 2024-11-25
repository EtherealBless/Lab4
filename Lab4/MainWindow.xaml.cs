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
            InitializeComponent();
            _viewModel = new SortingViewModel();
            DataContext = _viewModel;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.UpdateVisualizationSize(e.NewSize.Width, e.NewSize.Height);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}