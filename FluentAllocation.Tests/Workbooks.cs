using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using FluentAllocation.Models;

namespace FluentAllocation.Tests
{
    // builds source workbooks in memory, the way a user would lay them out:
    // a header row, then one row per participant or option, each cell written as given (a string stays text,
    // a number a number, null an empty cell)
    public static class Workbooks
    {
        public static XLWorkbook Source(object?[][] participants, object?[][] options, string[]? attributeNames = null)
        {
            var workbook = new XLWorkbook();

            IXLWorksheet participantSheet = workbook.AddWorksheet("People");
            var header = new List<object?> { "Id", "Name" };
            header.AddRange(attributeNames ?? new string[0]);
            int priorityColumns = participants.Length == 0 ? 0 : participants.Max(row => row.Length) - header.Count;
            for (int number = 1; number <= priorityColumns; number++) header.Add($"Priority {number}");
            WriteRow(participantSheet, 1, header.ToArray());
            for (int index = 0; index < participants.Length; index++) WriteRow(participantSheet, index + 2, participants[index]);

            IXLWorksheet optionSheet = workbook.AddWorksheet("Things");
            WriteRow(optionSheet, 1, new object?[] { "Id", "Name", "Capacity" });
            for (int index = 0; index < options.Length; index++) WriteRow(optionSheet, index + 2, options[index]);

            return workbook;
        }

        // the counts a form would send for exactly this data
        public static AllocationRequest Request(object?[][] participants, object?[][] options, int attributeCount = 0, int optionsPerParticipant = 1)
        {
            int priorityCount = participants.Length == 0 ? 0 : participants.Max(row => row.Length) - 2 - attributeCount;

            return new AllocationRequest
            {
                SourcePath = "source.xlsx",
                ResultPath = "result.xlsx",
                ParticipantCount = participants.Length,
                OptionCount = options.Length,
                AttributeCount = attributeCount,
                PriorityCount = priorityCount,
                OptionsPerParticipant = optionsPerParticipant
            };
        }

        public static object?[] Row(params object?[] cells) => cells;

        private static void WriteRow(IXLWorksheet sheet, int row, object?[] cells)
        {
            for (int column = 0; column < cells.Length; column++)
            {
                IXLCell cell = sheet.Cell(row, column + 1);
                switch (cells[column])
                {
                    case null: break;
                    case string text: cell.Value = text; break;
                    case int number: cell.Value = number; break;
                    case double number: cell.Value = number; break;
                    case System.DateTime date: cell.Value = date; break;
                }
            }
        }
    }
}
