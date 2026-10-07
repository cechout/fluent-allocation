using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentAllocation.Controls
{
    public enum SheetCellKind
    {
        Frame,
        Required,
        Optional,
        Text,
        Number
    }

    public sealed record SheetCell(string Text, SheetCellKind Kind);

    // a few rows of a worksheet as Excel draws them, for the help pages:
    // column letters, row numbers, a header row and some values
    //
    // Headers is a comma separated list, a header ending in ? is optional; Rows separates rows with ; and
    // cells with |; one empty row always follows, the way a sheet runs on below its data
    public sealed partial class SheetPreview : UserControl
    {
        // === dependency properties ===

        public static readonly DependencyProperty HeadersProperty = DependencyProperty.Register(
            nameof(Headers), typeof(string), typeof(SheetPreview), new PropertyMetadata(string.Empty, OnContentChanged));

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            nameof(Rows), typeof(string), typeof(SheetPreview), new PropertyMetadata(string.Empty, OnContentChanged));

        public string Headers
        {
            get => (string)GetValue(HeadersProperty);
            set => SetValue(HeadersProperty, value);
        }

        public string Rows
        {
            get => (string)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }


        // === constructor ===

        public SheetPreview()
        {
            this.InitializeComponent();
        }


        // === building ===

        private static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SheetPreview)sender).Rebuild();
        }

        private void Rebuild()
        {
            CellGrid.Children.Clear();
            CellGrid.RowDefinitions.Clear();
            CellGrid.ColumnDefinitions.Clear();

            string[] headers = Headers.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string[] rows = Rows.Split(';', StringSplitOptions.RemoveEmptyEntries);

            for (int column = 0; column <= headers.Length; column++) CellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (int row = 0; row < rows.Length + 3; row++) CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // the frame: the corner, a letter per column, a number per row
            Place(0, 0, string.Empty, SheetCellKind.Frame);
            for (int column = 1; column <= headers.Length; column++) Place(0, column, ((char)('A' + column - 1)).ToString(), SheetCellKind.Frame);
            for (int row = 1; row < rows.Length + 3; row++) Place(row, 0, row.ToString(), SheetCellKind.Frame);

            // row 1, the header
            for (int column = 1; column <= headers.Length; column++)
            {
                string header = headers[column - 1];
                bool optional = header.EndsWith('?');
                Place(1, column, optional ? header.TrimEnd('?') : header, optional ? SheetCellKind.Optional : SheetCellKind.Required);
            }

            // the values, and an empty row below them
            for (int row = 0; row <= rows.Length; row++)
            {
                string[] cells = row < rows.Length ? rows[row].Split('|') : Array.Empty<string>();
                for (int column = 1; column <= headers.Length; column++)
                {
                    string text = column - 1 < cells.Length ? cells[column - 1].Trim() : string.Empty;
                    Place(row + 2, column, text, int.TryParse(text, out _) ? SheetCellKind.Number : SheetCellKind.Text);
                }
            }
        }

        private void Place(int row, int column, string text, SheetCellKind kind)
        {
            var cell = new ContentPresenter
            {
                Content = new SheetCell(text, kind),
                ContentTemplate = (DataTemplate)Resources[kind + "Cell"],
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, column);
            CellGrid.Children.Add(cell);
        }
    }
}
