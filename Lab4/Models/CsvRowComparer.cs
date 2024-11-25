using System;
using System.Collections.Generic;

namespace Lab4.Models
{
    public class CsvRowComparer : IComparer<Dictionary<string, string>>
    {
        private readonly List<string> _columns;
        private const string DefaultValueColumn = "value";

        public CsvRowComparer(List<string> columns)
        {
            _columns = columns ?? new List<string> { DefaultValueColumn };
        }

        public int Compare(Dictionary<string, string> x, Dictionary<string, string> y)
        {
            if (_columns.Count == 0)
                _columns.Add(DefaultValueColumn);

            foreach (var column in _columns)
            {
                // For random arrays using the default value column
                if (column == DefaultValueColumn && x.ContainsKey(DefaultValueColumn) && y.ContainsKey(DefaultValueColumn))
                {
                    if (int.TryParse(x[DefaultValueColumn], out int xVal) && int.TryParse(y[DefaultValueColumn], out int yVal))
                    {
                        int comparison = xVal.CompareTo(yVal);
                        if (comparison != 0)
                            return comparison;
                        continue;
                    }
                }

                // For CSV data comparison
                if (!x.ContainsKey(column) || !y.ContainsKey(column))
                    continue;

                var xStr = x[column];
                var yStr = y[column];

                // Try parsing as numbers first
                if (double.TryParse(xStr, out double xNum) && double.TryParse(yStr, out double yNum))
                {
                    int comparison = xNum.CompareTo(yNum);
                    if (comparison != 0)
                        return comparison;
                }
                else
                {
                    // Fall back to string comparison
                    int comparison = string.Compare(xStr, yStr, StringComparison.Ordinal);
                    if (comparison != 0)
                        return comparison;
                }
            }

            return 0;
        }
    }
}
