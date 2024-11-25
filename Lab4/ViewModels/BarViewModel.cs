using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;

namespace Lab4.ViewModels
{
    public class BarViewModel : INotifyPropertyChanged
    {
        private double _height;
        private double _width;
        private double _top;
        private double _left;
        private Brush _fill;
        private Dictionary<string, double> _columnValues = new Dictionary<string, double>();
        private string _tooltip;

        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                if (_top != value)
                {
                    _top = value;
                    OnPropertyChanged(nameof(Top));
                }
            }
        }

        public double Left
        {
            get => _left;
            set
            {
                if (_left != value)
                {
                    _left = value;
                    OnPropertyChanged(nameof(Left));
                }
            }
        }

        public Brush Fill
        {
            get => _fill;
            set
            {
                if (_fill != value)
                {
                    _fill = value;
                    OnPropertyChanged(nameof(Fill));
                }
            }
        }

        public string Tooltip
        {
            get => _tooltip;
            set
            {
                if (_tooltip != value)
                {
                    _tooltip = value;
                    OnPropertyChanged(nameof(Tooltip));
                }
            }
        }

        public Dictionary<string, double> ColumnValues
        {
            get => _columnValues;
            set
            {
                _columnValues = value;
                UpdateTooltip();
                OnPropertyChanged(nameof(ColumnValues));
            }
        }

        private void UpdateTooltip()
        {
            var tooltipBuilder = new System.Text.StringBuilder();
            foreach (var kvp in _columnValues)
            {
                tooltipBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            Tooltip = tooltipBuilder.ToString().TrimEnd();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
