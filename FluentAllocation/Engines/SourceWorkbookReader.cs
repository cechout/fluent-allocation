using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using FluentAllocation.Models;

namespace FluentAllocation.Engines
{
    // reads the source workbook:
    // sheet 1 holds the participants, sheet 2 the options; names and header labels do not matter, only the order
    // of the sheets and of the columns, and the counts from the form decide how far it reads
    //
    // every problem a user can fix ends in an AllocationException that names the sheet and the cell
    public static class SourceWorkbookReader
    {
        // === public api ===

        // shared for reading, so a workbook that is open in Excel can still be read
        public static AllocationInput Read(string path, AllocationRequest request)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var workbook = new XLWorkbook(stream);
                return Read(workbook, request);
            }
            catch (AllocationException)
            {
                throw;
            }
            catch (FileNotFoundException exception)
            {
                throw new AllocationException("The source workbook no longer exists. Choose it again.", exception);
            }
            catch (IOException exception)
            {
                throw new AllocationException($"The source workbook could not be opened: {exception.Message}", exception);
            }
            catch (Exception exception)
            {
                throw new AllocationException("The source workbook could not be read. It has to be an Excel workbook (.xlsx).", exception);
            }
        }

        public static AllocationInput Read(XLWorkbook workbook, AllocationRequest request)
        {
            if (workbook.Worksheets.Count < 2)
            {
                throw new AllocationException("The source workbook needs two sheets: the participants first, the options second.");
            }

            List<Option> options = ReadOptions(workbook.Worksheet(2), request.OptionCount);
            var optionIds = new HashSet<int>();
            foreach (Option option in options) optionIds.Add(option.Id);

            IXLWorksheet participantSheet = workbook.Worksheet(1);
            List<string> attributeNames = ReadAttributeNames(participantSheet, request.AttributeCount);
            List<Participant> participants = ReadParticipants(participantSheet, request, attributeNames, optionIds);

            return new AllocationInput(participants, options, attributeNames);
        }


        // === options ===

        // A id, B name, C capacity, from row 2
        private static List<Option> ReadOptions(IXLWorksheet sheet, int count)
        {
            var options = new List<Option>();
            var seen = new HashSet<int>();

            for (int row = 2; row <= count + 1; row++)
            {
                int id = ReadId(sheet, row, "Options", count, seen);

                IXLCell capacityCell = sheet.Cell(row, 3);
                if (!TryWholeNumber(capacityCell.Value, out int capacity) || capacity < 0)
                {
                    throw new AllocationException($"{Where(sheet, capacityCell)}: the capacity has to be a whole number of 0 or more.");
                }

                options.Add(new Option(id, ToSheetValue(sheet.Cell(row, 2)), capacity));
            }

            return options;
        }


        // === participants ===

        // the row 1 labels of the attribute columns, C onwards
        private static List<string> ReadAttributeNames(IXLWorksheet sheet, int count)
        {
            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int column = 3; column < 3 + count; column++)
            {
                IXLCell headerCell = sheet.Cell(1, column);
                string name = headerCell.GetFormattedString().Trim();

                // grouping and the result columns go by name, so two equal names could not be told apart
                if (!seen.Add(name))
                {
                    throw new AllocationException($"{Where(sheet, headerCell)}: two attribute columns are both named \"{name}\".");
                }

                names.Add(name);
            }

            return names;
        }

        // A id, B name, then the attribute columns, then the priority columns, from row 2
        private static List<Participant> ReadParticipants(IXLWorksheet sheet, AllocationRequest request, List<string> attributeNames, HashSet<int> optionIds)
        {
            var participants = new List<Participant>();
            var seen = new HashSet<int>();
            int firstPriorityColumn = 3 + attributeNames.Count;

            for (int row = 2; row <= request.ParticipantCount + 1; row++)
            {
                int id = ReadId(sheet, row, "Participants", request.ParticipantCount, seen);

                var attributes = new Dictionary<string, SheetValue>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < attributeNames.Count; index++)
                {
                    attributes[attributeNames[index]] = ToSheetValue(sheet.Cell(row, 3 + index));
                }

                List<int> priorities = ReadPriorities(sheet, row, firstPriorityColumn, request.PriorityCount, optionIds);

                participants.Add(new Participant(id, ToSheetValue(sheet.Cell(row, 2)), attributes, priorities));
            }

            return participants;
        }

        // walks the priority columns left to right; an empty cell, an id that is no option and an id this
        // participant already chose are skipped, so the later priorities move up a rank
        private static List<int> ReadPriorities(IXLWorksheet sheet, int row, int firstColumn, int count, HashSet<int> optionIds)
        {
            var priorities = new List<int>();

            for (int column = firstColumn; column < firstColumn + count; column++)
            {
                IXLCell cell = sheet.Cell(row, column);
                if (IsEmpty(cell.Value)) continue;

                if (!TryWholeNumber(cell.Value, out int optionId))
                {
                    throw new AllocationException($"{Where(sheet, cell)}: \"{cell.GetFormattedString()}\" is not an option Id.");
                }

                if (optionIds.Contains(optionId) && !priorities.Contains(optionId)) priorities.Add(optionId);
            }

            return priorities;
        }


        // === cells ===

        // column A of a counted row; empty means the count is larger than the data
        private static int ReadId(IXLWorksheet sheet, int row, string countName, int count, HashSet<int> seen)
        {
            IXLCell cell = sheet.Cell(row, 1);

            if (IsEmpty(cell.Value))
            {
                throw new AllocationException($"{Where(sheet, cell)}: the Id is empty, but {countName} is set to {count}. Lower it to the number of rows in the sheet.");
            }

            if (!TryWholeNumber(cell.Value, out int id))
            {
                throw new AllocationException($"{Where(sheet, cell)}: the Id \"{cell.GetFormattedString()}\" is not a whole number.");
            }

            if (!seen.Add(id))
            {
                throw new AllocationException($"{Where(sheet, cell)}: the Id {id} appears twice.");
            }

            return id;
        }

        private static SheetValue ToSheetValue(IXLCell cell)
        {
            return IsEmpty(cell.Value) ? SheetValue.Empty : new SheetValue(cell.GetFormattedString(), cell.Value);
        }

        private static bool IsEmpty(XLCellValue value)
        {
            return value.IsBlank || (value.IsText && string.IsNullOrWhiteSpace(value.GetText()));
        }

        // a number without a fraction, or text that reads as one
        private static bool TryWholeNumber(XLCellValue value, out int number)
        {
            number = 0;

            if (value.IsNumber)
            {
                double raw = value.GetNumber();
                if (raw != Math.Floor(raw) || raw < int.MinValue || raw > int.MaxValue) return false;

                number = (int)raw;
                return true;
            }

            return value.IsText && int.TryParse(value.GetText().Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
        }

        // sheet 1 "Students", C4
        private static string Where(IXLWorksheet sheet, IXLCell cell)
        {
            return $"Sheet {sheet.Position} \"{sheet.Name}\", {cell.Address.ToStringRelative()}";
        }
    }
}
