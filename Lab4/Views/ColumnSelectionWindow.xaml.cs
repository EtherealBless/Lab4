using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace Lab4.Views
{
    public class ColumnItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public partial class ColumnSelectionWindow : Window
    {
        public List<string> SelectedColumns { get; private set; }
        private List<ColumnItem> _columnItems;

        public ColumnSelectionWindow(IEnumerable<string> columns)
        {
            InitializeComponent();
            
            _columnItems = columns.Select(c => new ColumnItem { Name = c, IsSelected = false }).ToList();
            ColumnsListBox.ItemsSource = _columnItems;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColumns = _columnItems.Where(c => c.IsSelected).Select(c => c.Name).ToList();
            if (SelectedColumns.Count == 0)
            {
                MessageBox.Show("Please select at least one column.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
