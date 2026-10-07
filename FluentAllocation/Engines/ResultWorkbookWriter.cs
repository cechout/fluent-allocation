using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAllocation.Models;

namespace FluentAllocation.Engines
{
    // writes the result workbook:
    // Participants, Options, Groups (only when grouping) and Unallocated (only when someone is), in that order;
    // the chosen file holds exactly these sheets afterwards, whether it was new or existing
    public static class ResultWorkbookWriter
    {
        // === fields ===

        // --- sheet names ---
        public const string ParticipantsSheet = "Participants";
        public const string OptionsSheet = "Options";
        public const string GroupsSheet = "Groups";
        public const string UnallocatedSheet = "Unallocated";

        // attribute names and values sort the same way everywhere, ignoring case as the grouping does
        private static readonly StringComparer SortOrder = StringComparer.InvariantCultureIgnoreCase;


        // === public api ===

        public static void Write(string path, AllocationInput input, AllocationResult result, AllocationRequest request)
        {
            using XLWorkbook workbook = Build(input, result, request);

            // saved to a stream of our own, so a file that is open elsewhere fails here with a message rather than
            // halfway through
            try
            {
                using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                workbook.SaveAs(stream);
            }
            catch (IOException exception)
            {
                throw new AllocationException("The result workbook is open in another program. Close it there and allocate again.", exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new AllocationException("The result workbook cannot be written here. Choose a folder you can save to.", exception);
            }
        }

        public static XLWorkbook Build(AllocationInput input, AllocationResult result, AllocationRequest request)
        {
            var workbook = new XLWorkbook();
            List<string> attributeNames = input.AttributeNames.OrderBy(name => name, SortOrder).ToList();
            int optionColumns = request.OptionsPerParticipant;

            // participants
            IXLWorksheet participants = workbook.AddWorksheet(ParticipantsSheet);
            int row = WriteHeader(participants, 1, attributeNames, optionColumns);
            WriteParticipants(participants, row, input.Participants, attributeNames, result, withOptions: true);

            // options, one block per option in source order
            IXLWorksheet options = workbook.AddWorksheet(OptionsSheet);
            row = 1;
            foreach (Option option in input.Options)
            {
                Set(options.Cell(row, 1), option.Name.Value);
                row = WriteHeader(options, row + 1, attributeNames, 0);

                IEnumerable<Participant> members = input.Participants.Where(participant => result.Assignments[participant].Contains(option));
                row = WriteParticipants(options, row, members, attributeNames, result, withOptions: false) + 1;
            }

            // groups, one block per value of the chosen attribute
            if (request.GroupAttribute != null)
            {
                IXLWorksheet groups = workbook.AddWorksheet(GroupsSheet);
                string key = ResolveAttribute(input, request.GroupAttribute);
                row = 1;

                foreach (string value in GroupValues(input, key))
                {
                    groups.Cell(row, 1).Value = $"{request.GroupAttribute} {value}";
                    row = WriteHeader(groups, row + 1, attributeNames, optionColumns);

                    IEnumerable<Participant> members = input.Participants.Where(participant => SortOrder.Equals(participant.Attributes[key].Text, value));
                    row = WriteParticipants(groups, row, members, attributeNames, result, withOptions: true) + 1;
                }
            }

            // unallocated
            if (result.Unallocated.Count > 0)
            {
                IXLWorksheet unallocated = workbook.AddWorksheet(UnallocatedSheet);
                row = WriteHeader(unallocated, 1, attributeNames, optionColumns);
                WriteParticipants(unallocated, row, result.Unallocated, attributeNames, result, withOptions: true);
            }

            return workbook;
        }

        // the attribute as it is spelled in the workbook, for a name typed in any case
        public static string ResolveAttribute(AllocationInput input, string typedName)
        {
            string? match = input.AttributeNames.FirstOrDefault(name => string.Equals(name, typedName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            string known = input.AttributeNames.Count == 0
                ? "The source workbook has no attributes."
                : $"The attributes are: {string.Join(", ", input.AttributeNames)}.";
            throw new AllocationException($"There is no attribute named \"{typedName.Trim()}\". {known}");
        }


        // === private helpers ===

        // every distinct value alphabetically, each spelled as it first appears
        private static IEnumerable<string> GroupValues(AllocationInput input, string key)
        {
            return input.Participants
                .Select(participant => participant.Attributes[key].Text)
                .Distinct(SortOrder)
                .OrderBy(value => value, SortOrder);
        }

        // Id, Name, the attributes, then Option 1 to Option n; returns the row below it
        private static int WriteHeader(IXLWorksheet sheet, int row, List<string> attributeNames, int optionColumns)
        {
            int column = 1;
            sheet.Cell(row, column++).Value = "Id";
            sheet.Cell(row, column++).Value = "Name";
            foreach (string name in attributeNames) sheet.Cell(row, column++).Value = name;
            for (int number = 1; number <= optionColumns; number++) sheet.Cell(row, column++).Value = $"Option {number}";

            return row + 1;
        }

        // one row per participant; returns the row below the last one
        private static int WriteParticipants(IXLWorksheet sheet, int row, IEnumerable<Participant> participants, List<string> attributeNames, AllocationResult result, bool withOptions)
        {
            foreach (Participant participant in participants)
            {
                int column = 1;
                sheet.Cell(row, column++).Value = participant.Id;
                Set(sheet.Cell(row, column++), participant.Name.Value);
                foreach (string name in attributeNames) Set(sheet.Cell(row, column++), participant.Attributes[name].Value);

                if (withOptions)
                {
                    foreach (Option option in result.Assignments[participant]) Set(sheet.Cell(row, column++), option.Name.Value);
                }

                row++;
            }

            return row;
        }

        // the value as it was read; an empty source cell stays empty
        private static void Set(IXLCell cell, XLCellValue value)
        {
            if (!value.IsBlank) cell.Value = value;
        }
    }
}
