using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace Lab4.Models
{
    public class CsvData
    {
        public List<string> Headers { get; private set; }
        public List<Dictionary<string, string>> Rows { get; private set; }
        public List<string> SelectedColumns { get; set; }
        private Dictionary<string, (double Min, double Max)> _columnRanges;

        public CsvData()
        {
            Headers = new List<string>();
            Rows = new List<Dictionary<string, string>>();
            SelectedColumns = new List<string>();
            _columnRanges = new Dictionary<string, (double Min, double Max)>();
        }

        public static CsvData LoadFromFile(string filePath)
        {
            var csvData = new CsvData();
            const int maxRows = 40;
            int rowCount = 0;

            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Read headers
                if (!parser.EndOfData)
                {
                    csvData.Headers = parser.ReadFields().ToList();
                }

                // Read data rows
                while (!parser.EndOfData && rowCount < maxRows)
                {
                    var fields = parser.ReadFields();
                    var row = new Dictionary<string, string>();

                    for (int i = 0; i < csvData.Headers.Count && i < fields.Length; i++)
                    {
                        row[csvData.Headers[i]] = fields[i];
                    }

                    csvData.Rows.Add(row);
                    rowCount++;
                }
            }

            return csvData;
        }

        public void CalculateColumnRanges()
        {
            foreach (var column in Headers)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                bool hasNumericValues = false;

                foreach (var row in Rows)
                {
                    if (TryGetNumericValue(row, column, out double value))
                    {
                        min = Math.Min(min, value);
                        max = Math.Max(max, value);
                        hasNumericValues = true;
                    }
                }

                if (hasNumericValues)
                {
                    _columnRanges[column] = (min, max);
                }
            }
        }

        public bool TryGetNormalizedValue(Dictionary<string, string> row, string column, out double normalizedValue)
        {
            normalizedValue = 0;
            
            if (!TryGetNumericValue(row, column, out double value))
                return false;

            if (!_columnRanges.ContainsKey(column))
                return false;

            var (min, max) = _columnRanges[column];
            if (Math.Abs(max - min) < double.Epsilon)
                return false;

            normalizedValue = (value - min) / (max - min);
            return true;
        }

        public bool TryGetNumericValue(Dictionary<string, string> row, string column, out double value)
        {
            value = 0;
            if (!row.ContainsKey(column))
                return false;

            return double.TryParse(row[column], out value);
        }
    }
}
