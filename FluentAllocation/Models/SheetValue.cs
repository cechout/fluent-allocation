using ClosedXML.Excel;

namespace FluentAllocation.Models
{
    // a cell taken over from the source workbook:
    // Value is written back as it was read (a number stays a number, a date a date), Text is what the cell shows
    // and what grouping and headers go by
    public sealed record SheetValue(string Text, XLCellValue Value)
    {
        public static SheetValue Empty { get; } = new SheetValue(string.Empty, Blank.Value);
    }
}
