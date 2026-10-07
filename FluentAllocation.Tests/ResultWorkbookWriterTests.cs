using System;
using System.Linq;
using ClosedXML.Excel;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using Xunit;
using static FluentAllocation.Tests.Workbooks;

namespace FluentAllocation.Tests
{
    public class ResultWorkbookWriterTests
    {
        // Ada and Cy get Art, Ben gets Music; Di wanted Art too and is left over
        private static readonly object?[][] Participants =
        {
            Row(1, "Ada", "10b", 16, 1, 2),
            Row(2, "Ben", "10a", 15, 2, 1),
            Row(3, "Cy", "10A", 16, 1, 2),
            Row(4, "Di", "10b", 17, 1)
        };

        private static readonly object?[][] Options =
        {
            Row(1, "Art", 2),
            Row(2, "Music", 1)
        };

        private static readonly string[] AttributeNames = { "Class", "Age" };

        private static XLWorkbook Build(string? groupAttribute = null)
        {
            using var source = Source(Participants, Options, AttributeNames);
            AllocationRequest request = Request(Participants, Options, AttributeNames.Length) with { GroupAttribute = groupAttribute };
            AllocationInput input = SourceWorkbookReader.Read(source, request);
            AllocationResult result = AllocationEngine.Allocate(input, request.PriorityCount, 1, new Random(1));

            return ResultWorkbookWriter.Build(input, result, request);
        }

        // the shown text of a row, empty cells as ""
        private static string[] RowText(IXLWorksheet sheet, int row, int columns)
        {
            return Enumerable.Range(1, columns).Select(column => sheet.Cell(row, column).GetFormattedString()).ToArray();
        }


        [Fact]
        public void TheSheetsComeInOrderAndOnlyWhenNeeded()
        {
            using XLWorkbook plain = Build();
            using XLWorkbook grouped = Build("class");

            Assert.Equal(new[] { "Participants", "Options", "Unallocated" }, plain.Worksheets.Select(sheet => sheet.Name));
            Assert.Equal(new[] { "Participants", "Options", "Groups", "Unallocated" }, grouped.Worksheets.Select(sheet => sheet.Name));
        }

        [Fact]
        public void NoUnallocatedSheetWhenEveryoneIsServed()
        {
            object?[][] participants = { Row(1, "Ada", 1) };
            using var source = Source(participants, Options);
            AllocationRequest request = Request(participants, Options);
            AllocationInput input = SourceWorkbookReader.Read(source, request);
            AllocationResult result = AllocationEngine.Allocate(input, 1, 1, new Random(1));

            using XLWorkbook workbook = ResultWorkbookWriter.Build(input, result, request);

            Assert.Equal(new[] { "Participants", "Options" }, workbook.Worksheets.Select(sheet => sheet.Name));
        }

        // the attributes alphabetically, the options in the order given
        [Fact]
        public void TheParticipantsSheetListsEveryoneInSourceOrder()
        {
            using XLWorkbook workbook = Build();
            IXLWorksheet sheet = workbook.Worksheet("Participants");

            Assert.Equal(new[] { "Id", "Name", "Age", "Class", "Option 1" }, RowText(sheet, 1, 5));
            Assert.Equal(new[] { "1", "Ada", "16", "10b", "Art" }, RowText(sheet, 2, 5));
            Assert.Equal(new[] { "2", "Ben", "15", "10a", "Music" }, RowText(sheet, 3, 5));
            Assert.Equal(new[] { "4", "Di", "17", "10b", "" }, RowText(sheet, 5, 5));
            Assert.True(sheet.Cell(6, 1).IsEmpty());
        }

        [Fact]
        public void TheOptionsSheetHasABlockPerOption()
        {
            using XLWorkbook workbook = Build();
            IXLWorksheet sheet = workbook.Worksheet("Options");

            Assert.Equal("Art", sheet.Cell(1, 1).GetFormattedString());
            Assert.Equal(new[] { "Id", "Name", "Age", "Class", "" }, RowText(sheet, 2, 5));
            Assert.Equal(new[] { "1", "Ada" }, RowText(sheet, 3, 2));
            Assert.Equal(new[] { "3", "Cy" }, RowText(sheet, 4, 2));
            Assert.True(sheet.Row(5).IsEmpty());
            Assert.Equal("Music", sheet.Cell(6, 1).GetFormattedString());
            Assert.Equal(new[] { "2", "Ben" }, RowText(sheet, 8, 2));
        }

        // case-insensitive like the grouping in version 1, each value spelled as it first appears
        [Fact]
        public void TheGroupsSheetHasABlockPerValueAlphabetically()
        {
            using XLWorkbook workbook = Build("class");
            IXLWorksheet sheet = workbook.Worksheet("Groups");

            Assert.Equal("class 10a", sheet.Cell(1, 1).GetFormattedString());
            Assert.Equal(new[] { "Id", "Name", "Age", "Class", "Option 1" }, RowText(sheet, 2, 5));
            Assert.Equal(new[] { "2", "Ben" }, RowText(sheet, 3, 2));
            Assert.Equal(new[] { "3", "Cy" }, RowText(sheet, 4, 2));
            Assert.True(sheet.Row(5).IsEmpty());
            Assert.Equal("class 10b", sheet.Cell(6, 1).GetFormattedString());
            Assert.Equal(new[] { "1", "Ada" }, RowText(sheet, 8, 2));
            Assert.Equal(new[] { "4", "Di" }, RowText(sheet, 9, 2));
        }

        [Fact]
        public void TheUnallocatedSheetListsWhoIsLeftOver()
        {
            using XLWorkbook workbook = Build();
            IXLWorksheet sheet = workbook.Worksheet("Unallocated");

            Assert.Equal(new[] { "Id", "Name", "Age", "Class", "Option 1" }, RowText(sheet, 1, 5));
            Assert.Equal(new[] { "4", "Di", "17", "10b", "" }, RowText(sheet, 2, 5));
            Assert.True(sheet.Cell(3, 1).IsEmpty());
        }

        [Fact]
        public void ValuesKeepTheirTypes()
        {
            object?[][] participants = { Row(1, "Ada", 16, new DateTime(2026, 7, 1), 1) };
            object?[][] options = { Row(1, new DateTime(2026, 8, 3), 1) };
            string[] attributes = { "Age", "Start" };

            using var source = Source(participants, options, attributes);
            AllocationRequest request = Request(participants, options, attributes.Length);
            AllocationInput input = SourceWorkbookReader.Read(source, request);
            AllocationResult result = AllocationEngine.Allocate(input, 1, 1, new Random(1));
            using XLWorkbook workbook = ResultWorkbookWriter.Build(input, result, request);
            IXLWorksheet sheet = workbook.Worksheet("Participants");

            Assert.True(sheet.Cell(2, 1).Value.IsNumber);
            Assert.True(sheet.Cell(2, 3).Value.IsNumber);
            Assert.True(sheet.Cell(2, 4).Value.IsDateTime);
            Assert.Contains("2026", sheet.Cell(2, 4).GetFormattedString());
            Assert.Contains("2026", sheet.Cell(2, 5).GetFormattedString());
            Assert.Equal(new DateTime(2026, 8, 3), sheet.Cell(2, 5).Value.GetDateTime());
        }

        [Fact]
        public void AnUnknownGroupAttributeNamesTheKnownOnes()
        {
            var error = Assert.Throws<AllocationException>(() => Build("Team"));

            Assert.Equal("There is no attribute named \"Team\". The attributes are: Class, Age.", error.Message);
        }
    }
}
