using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Noterium.Code.Data;

namespace Noterium.Windows
{
    /// <summary>
    ///     Interaction logic for TableEditor.xaml
    /// </summary>
    public partial class TableEditor
    {
        public delegate void TableSave(string newTable, DocumentEntitiy entitiy);

        public delegate void TableSaveError(DocumentEntitiy entitiy, Exception e);

        private readonly DocumentEntitiy _currentEntity;
        private readonly string _tableString;
        private List<TextAlignment> _columnAlignments;
        private readonly DataTable _currentTable;

        public TableEditor(string tableString, DocumentEntitiy currentEntity)
        {
            _tableString = tableString;
            _currentEntity = currentEntity;
            InitializeComponent();

            GenerateTable(tableString, out _currentTable, out _columnAlignments);

            var view = _currentTable.AsDataView();
            view.AllowDelete = true;
            view.AllowEdit = true;
            view.AllowNew = true;

            GridTable.Columns.Clear();
            GridTable.ItemsSource = null;
            GridTable.ItemsSource = view;

            InitColAlignments();
        }

        public event TableSave OnTableSave;
        public event TableSaveError OnTableSaveError;

        private void InitColAlignments()
        {
            for (var i = 0; i < GridTable.Columns.Count; i++)
            {
                var col = GridTable.Columns[i];
                //col.GetCellContent()
            }
        }

        private void GenerateTable(string rowsString, out DataTable table, out List<TextAlignment> colAlignments)
        {
            table = new DataTable();
            colAlignments = new List<TextAlignment>();

            var rows = rowsString.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!rows.Any())
                return;

            var reg = new Regex("([-:|].*)", RegexOptions.Compiled | RegexOptions.Singleline);


            var head = rows.First().Trim();
            var headColumns = head.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < headColumns.Length; i++)
            {
                var c = new DataColumn();
                table.Columns.Add(c);
            }

            foreach (var rowString in rows)
            {
                var s = rowString.Trim();
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                var rowColumns = s.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

                if (reg.IsMatch(s))
                {
                    for (var i = 0; i < rowColumns.Length; i++)
                    {
                        var text = rowColumns[i].Trim();
                        if (text.StartsWith(":-") && text.EndsWith("-:"))
                            colAlignments.Add(TextAlignment.Center);
                        else if (text.StartsWith(":-"))
                            colAlignments.Add(TextAlignment.Left);
                        else if (text.EndsWith("-:"))
                            colAlignments.Add(TextAlignment.Right);
                        else
                            colAlignments.Add(TextAlignment.Left);
                    }

                    continue;
                }

                var row = table.Rows.Add();
                for (var i = 0; i < rowColumns.Length; i++)
                {
                    var text = rowColumns[i].Trim();
                    row[i] = text;
                }
            }
        }

        private void AddRowBefore(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var dr = firstCell.Item as DataRowView;
                if (dr == null)
                    return;

                var rowIndex = _currentTable.Rows.IndexOf(dr.Row);
                var newRow = _currentTable.NewRow();
                _currentTable.Rows.InsertAt(newRow, rowIndex);
            }
        }

        private void RemoveRow(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var dr = firstCell.Item as DataRowView;
                if (dr == null)
                    return;

                _currentTable.Rows.Remove(dr.Row);
            }
        }

        private void AddRowAfter(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var dr = firstCell.Item as DataRowView;
                if (dr == null)
                    return;

                var rowIndex = _currentTable.Rows.IndexOf(dr.Row);
                if (_currentTable.Rows.Count == rowIndex + 1)
                {
                    _currentTable.Rows.Add();
                }
                else
                {
                    var newRow = _currentTable.NewRow();
                    _currentTable.Rows.InsertAt(newRow, rowIndex + 1);
                }
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex().ToString();
        }

        private void AddColumnBefore(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var index = firstCell.Column.DisplayIndex;
                var textColumn = new DataGridTextColumn();
                GridTable.Columns.Insert(index, textColumn);

                FixColumnNames();
            }
        }

        private void RemoveColumn(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var index = firstCell.Column.DisplayIndex;
                GridTable.Columns.RemoveAt(index);
                FixColumnNames();
            }
        }

        private void AddColumnAfter(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                var index = firstCell.Column.DisplayIndex;
                var columnname = "COLUMN" + GridTable.Columns.Count;
                var view = (DataView) GridTable.ItemsSource;
                var c = view.Table.Columns.Add(columnname);
                var textColumn = new DataGridTextColumn();
                textColumn.Binding = new Binding(columnname);

                if (GridTable.Columns.Count == index + 1)
                {
                    GridTable.Columns.Add(textColumn);
                }
                else
                {
                    GridTable.Columns.Insert(index + 1, textColumn);
                    c.SetOrdinal(index + 1);
                }

                FixColumnNames();
            }
        }

        private void FixColumnNames()
        {
            for (var i = 0; i < GridTable.Columns.Count; i++)
            {
                var dataGridColumn = GridTable.Columns[i];
                dataGridColumn.Header = $"COLUMN{i + 1}";
            }
        }

        private void AlignColumnLeft(object sender, RoutedEventArgs e)
        {
        }

        private void AlignColumnCenter(object sender, RoutedEventArgs e)
        {
        }

        private void AlignColumnRight(object sender, RoutedEventArgs e)
        {
        }

        private void SaveTable(object sender, RoutedEventArgs e)
        {
            try
            {
                var builder = new StringBuilder();
                var table = ((DataView) GridTable.ItemsSource).Table;
                var rows = new List<string[]>();
                foreach (DataRow row in table.Rows)
                {
                    var strings = row.ItemArray.ToList().ConvertAll(o => o.ToString()).ToArray();
                    rows.Add(strings);
                }

                var longestStringLengths = new Dictionary<int, int>();
                foreach (var strings in rows)
                    for (var i = 0; i < strings.Length; i++)
                    {
                        var s = strings[i].Trim();
                        if (!longestStringLengths.ContainsKey(i))
                            longestStringLengths.Add(i, 0);

                        if (s.Length > longestStringLengths[i])
                            if (!s.StartsWith(":-") && !s.EndsWith("-:"))
                                longestStringLengths[i] = s.Length;
                    }

                foreach (var strings in rows)
                {
                    for (var i = 0; i < strings.Length; i++)
                    {
                        var longestStringLength = longestStringLengths[i];
                        var s = strings[i];
                        if (s.StartsWith(":-") && s.EndsWith("-:"))
                            strings[i] = ":" + "-".PadRight(longestStringLength, '-') + ":";
                        else if (s.StartsWith(":-"))
                            strings[i] = ":" + "-".PadRight(longestStringLength, '-') + " ";
                        else if (s.EndsWith("-:"))
                            strings[i] = " " + "-".PadRight(longestStringLength, '-') + ":";
                        else
                            strings[i] = " " + s.PadRight(longestStringLength) + " ";
                    }

                    builder.AppendLine($"|{string.Join("|", strings)}|");
                }

                OnTableSave?.Invoke(builder.ToString(), _currentEntity);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                OnTableSaveError?.Invoke(_currentEntity, exception);
            }

            Close();
        }

        private void CloseTableEditor(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}