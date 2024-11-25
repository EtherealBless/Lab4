using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lab4.Controls
{
    public partial class LegendItem : UserControl
    {
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(LegendItem));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(LegendItem));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public LegendItem()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
