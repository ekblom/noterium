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
	/// Interaction logic for TableEditor.xaml
	/// </summary>
	public partial class TableEditor
	{
	    private readonly string _tableString;
	    private readonly DocumentEntitiy _currentEntity;
	    private DataTable _currentTable;
	    private List<TextAlignment> _columnAlignments;

	    public delegate void TableSaveError(DocumentEntitiy entitiy, Exception e);
	    public delegate void TableSave(string newTable, DocumentEntitiy entitiy);

	    public event TableSave OnTableSave;
	    public event TableSaveError OnTableSaveError;

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

	    private void InitColAlignments()
	    {
	        for (int i = 0; i < GridTable.Columns.Count; i++)
	        {
	            var col = GridTable.Columns[i];
                //col.GetCellContent()

	        }

        }

	    private void GenerateTable(string rowsString, out DataTable table, out List<TextAlignment> colAlignments)
        {
            table = new DataTable();
            colAlignments = new List<TextAlignment>();

            List<string> rows = rowsString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!rows.Any())
                return;

            Regex reg = new Regex("([-:|].*)", RegexOptions.Compiled | RegexOptions.Singleline);


            string head = rows.First().Trim();
            string[] headColumns = head.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < headColumns.Length; i++)
            {
                DataColumn c = new DataColumn();
                table.Columns.Add(c);
            }

            foreach (string rowString in rows)
            {
                string s = rowString.Trim();
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                string[] rowColumns = s.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (reg.IsMatch(s))
                {
                    for (int i = 0; i < rowColumns.Length; i++)
                    {
                        string text = rowColumns[i].Trim();
                        if (text.StartsWith(":-") && text.EndsWith("-:"))
                        {
                            colAlignments.Add(TextAlignment.Center);
                        }
                        else if (text.StartsWith(":-"))
                        {
                            colAlignments.Add(TextAlignment.Left);
                        }
                        else if (text.EndsWith("-:"))
                        {
                            colAlignments.Add(TextAlignment.Right);
                        }
                        else
                            colAlignments.Add(TextAlignment.Left);

                    }

                    continue;
                }

                DataRow row = table.Rows.Add();
                for (int i = 0; i < rowColumns.Length; i++)
                {
                    string text = rowColumns[i].Trim();
                    row[i] = text;
                }
            }
        }

        private void AddRowBefore(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                DataRowView dr = firstCell.Item as DataRowView;
                if (dr == null)
                    return;

                int rowIndex = _currentTable.Rows.IndexOf(dr.Row);
                DataRow newRow = _currentTable.NewRow();
                _currentTable.Rows.InsertAt(newRow, rowIndex);
            }
        }

        private void RemoveRow(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                DataRowView dr = firstCell.Item as DataRowView;
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
                DataRowView dr = firstCell.Item as DataRowView;
                if (dr == null)
                    return;

                int rowIndex = _currentTable.Rows.IndexOf(dr.Row);
                if (_currentTable.Rows.Count == (rowIndex + 1))
                {
                    _currentTable.Rows.Add();
                }
                else
                {
                    DataRow newRow = _currentTable.NewRow();
                    _currentTable.Rows.InsertAt(newRow, rowIndex + 1);
                }
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }

        private void AddColumnBefore(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                int index = firstCell.Column.DisplayIndex;
                DataGridTextColumn textColumn = new DataGridTextColumn();
                GridTable.Columns.Insert(index, textColumn);

                FixColumnNames();
            }
        }

        private void RemoveColumn(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                int index = firstCell.Column.DisplayIndex;
                GridTable.Columns.RemoveAt(index);
                FixColumnNames();
            }
        }

        private void AddColumnAfter(object sender, RoutedEventArgs e)
        {
            var firstCell = GridTable.SelectedCells.FirstOrDefault();
            if (firstCell.IsValid)
            {
                int index = firstCell.Column.DisplayIndex;
                string columnname = "COLUMN" + GridTable.Columns.Count;
                var view = (DataView)GridTable.ItemsSource;
                var c = view.Table.Columns.Add(columnname);
                DataGridTextColumn textColumn = new DataGridTextColumn();
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
            for (int i = 0; i < GridTable.Columns.Count; i++)
            {
                DataGridColumn dataGridColumn = GridTable.Columns[i];
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
                StringBuilder builder = new StringBuilder();
                var table = ((DataView)GridTable.ItemsSource).Table;
                List<string[]> rows = new List<string[]>();
                foreach (DataRow row in table.Rows)
                {
                    var strings = row.ItemArray.ToList().ConvertAll(o => o.ToString()).ToArray();
                    rows.Add(strings);
                }

                Dictionary<int, int> longestStringLengths = new Dictionary<int, int>();
                foreach (string[] strings in rows)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        string s = strings[i].Trim();
                        if (!longestStringLengths.ContainsKey(i))
                            longestStringLengths.Add(i, 0);

                        if (s.Length > longestStringLengths[i])
                        {
                            if (!s.StartsWith(":-") && !s.EndsWith("-:"))
                                longestStringLengths[i] = s.Length;
                        }
                    }
                }

                foreach (string[] strings in rows)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        int longestStringLength = longestStringLengths[i];
                        string s = strings[i];
                        if (s.StartsWith(":-") && s.EndsWith("-:"))
                        {
                            strings[i] = ":" + "-".PadRight(longestStringLength, '-') + ":";
                        }
                        else if (s.StartsWith(":-"))
                        {
                            strings[i] = ":" + "-".PadRight(longestStringLength, '-') + " ";
                        }
                        else if (s.EndsWith("-:"))
                        {
                            strings[i] = " " + "-".PadRight(longestStringLength, '-') + ":";
                        }
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
