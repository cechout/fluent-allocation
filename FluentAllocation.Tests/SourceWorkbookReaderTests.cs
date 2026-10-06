using System.Linq;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using Xunit;
using static FluentAllocation.Tests.Workbooks;

namespace FluentAllocation.Tests
{
    public class SourceWorkbookReaderTests
    {
        private static readonly object?[][] ThreeOptions =
        {
            Row(1, "Art", 2),
            Row(2, "Music", 2),
            Row(3, "Sport", 2)
        };

        private static AllocationInput Read(object?[][] participants, object?[][] options, string[]? attributes = null, AllocationRequest? request = null)
        {
            using var workbook = Source(participants, options, attributes);
            return SourceWorkbookReader.Read(workbook, request ?? Request(participants, options, attributes?.Length ?? 0));
        }

        private static string ReadError(object?[][] participants, object?[][] options, string[]? attributes = null, AllocationRequest? request = null)
        {
            return Assert.Throws<AllocationException>(() => Read(participants, options, attributes, request)).Message;
        }


        // === reading ===

        [Fact]
        public void ReadsParticipantsOptionsAndAttributesInSourceOrder()
        {
            AllocationInput input = Read(
                new[] { Row(7, "Ada", "10a", 2, 1), Row(3, "Ben", "10b", 3, 2) },
                ThreeOptions,
                new[] { "Class" });

            Assert.Equal(new[] { 7, 3 }, input.Participants.Select(participant => participant.Id));
            Assert.Equal("Ada", input.Participants[0].Name.Text);
            Assert.Equal("10b", input.Participants[1].Attributes["class"].Text);
            Assert.Equal(new[] { 2, 1 }, input.Participants[0].Priorities);
            Assert.Equal(new[] { "Art", "Music", "Sport" }, input.Options.Select(option => option.Name.Text));
            Assert.Equal(2, input.Options[2].Capacity);
            Assert.Equal(new[] { "Class" }, input.AttributeNames);
        }

        [Fact]
        public void TextThatReadsAsAWholeNumberIsAnId()
        {
            AllocationInput input = Read(new[] { Row("5", "Ada", " 2 ") }, ThreeOptions);

            Assert.Equal(5, input.Participants[0].Id);
            Assert.Equal(new[] { 2 }, input.Participants[0].Priorities);
        }

        [Fact]
        public void ACountBelowTheDataReadsOnlyThatManyRows()
        {
            object?[][] participants = { Row(1, "Ada", 1), Row(2, "Ben", 2), Row(3, "Cy", 3) };
            AllocationInput input = Read(participants, ThreeOptions, request: Request(participants, ThreeOptions) with { ParticipantCount = 2 });

            Assert.Equal(new[] { 1, 2 }, input.Participants.Select(participant => participant.Id));
        }


        // === priorities ===

        [Fact]
        public void AnEmptyPriorityIsSkippedAndTheLaterOnesMoveUp()
        {
            AllocationInput input = Read(new[] { Row(1, "Ada", null, 3, null, 1) }, ThreeOptions);

            Assert.Equal(new[] { 3, 1 }, input.Participants[0].Priorities);
        }

        [Fact]
        public void APriorityThatIsNoOptionIsSkipped()
        {
            AllocationInput input = Read(new[] { Row(1, "Ada", 9, 0, -1, 2) }, ThreeOptions);

            Assert.Equal(new[] { 2 }, input.Participants[0].Priorities);
        }

        [Fact]
        public void ARepeatedPriorityIsSkipped()
        {
            AllocationInput input = Read(new[] { Row(1, "Ada", 2, 2, 3, 2, 1) }, ThreeOptions);

            Assert.Equal(new[] { 2, 3, 1 }, input.Participants[0].Priorities);
        }

        // version 1 dropped every id above the number of options, although the ids may skip numbers
        [Fact]
        public void OptionIdsWithGapsAreValidPriorities()
        {
            object?[][] options = { Row(1, "Art", 1), Row(2, "Music", 1), Row(5, "Sport", 1) };
            AllocationInput input = Read(new[] { Row(1, "Ada", 5, 3, 1) }, options);

            Assert.Equal(new[] { 5, 1 }, input.Participants[0].Priorities);
        }

        [Fact]
        public void APriorityThatIsNotANumberIsAnError()
        {
            string message = ReadError(new[] { Row(1, "Ada", "Art") }, ThreeOptions);

            Assert.Equal("Sheet 1 \"People\", C2: \"Art\" is not an option Id.", message);
        }

        [Fact]
        public void APriorityWithAFractionIsAnError()
        {
            string message = ReadError(new[] { Row(1, "Ada", 1.5) }, ThreeOptions);

            Assert.Contains("C2", message);
        }


        // === ids and capacities ===

        [Fact]
        public void AnEmptyIdInsideTheCountIsAnError()
        {
            object?[][] participants = { Row(1, "Ada", 1) };
            string message = ReadError(participants, ThreeOptions, request: Request(participants, ThreeOptions) with { ParticipantCount = 2 });

            Assert.Equal("Sheet 1 \"People\", A3: the Id is empty, but Participants is set to 2. Lower it to the number of rows in the sheet.", message);
        }

        [Fact]
        public void AnIdThatIsNotAWholeNumberIsAnError()
        {
            string message = ReadError(new[] { Row("x1", "Ada", 1) }, ThreeOptions);

            Assert.Equal("Sheet 1 \"People\", A2: the Id \"x1\" is not a whole number.", message);
        }

        [Fact]
        public void ADuplicateParticipantIdIsAnError()
        {
            string message = ReadError(new[] { Row(1, "Ada", 1), Row(1, "Ben", 2) }, ThreeOptions);

            Assert.Equal("Sheet 1 \"People\", A3: the Id 1 appears twice.", message);
        }

        [Fact]
        public void ADuplicateOptionIdIsAnError()
        {
            string message = ReadError(new[] { Row(1, "Ada", 1) }, new[] { Row(1, "Art", 1), Row(1, "Music", 1) });

            Assert.Equal("Sheet 2 \"Things\", A3: the Id 1 appears twice.", message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData("many")]
        [InlineData(null)]
        public void ACapacityThatIsNoWholeNumberFromZeroIsAnError(object? capacity)
        {
            string message = ReadError(new[] { Row(1, "Ada", 1) }, new[] { Row(1, "Art", capacity) });

            Assert.Equal("Sheet 2 \"Things\", C2: the capacity has to be a whole number of 0 or more.", message);
        }


        // === workbook shape ===

        [Fact]
        public void AWorkbookWithOneSheetIsAnError()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            workbook.AddWorksheet("Only");

            var error = Assert.Throws<AllocationException>(() => SourceWorkbookReader.Read(workbook, Request(new object?[0][], new object?[0][])));
            Assert.Equal("The source workbook needs two sheets: the participants first, the options second.", error.Message);
        }

        [Fact]
        public void TwoAttributesWithTheSameNameAreAnError()
        {
            string message = ReadError(new[] { Row(1, "Ada", "10a", "10A", 1) }, ThreeOptions, new[] { "Class", "class" });

            Assert.Equal("Sheet 1 \"People\", D1: two attribute columns are both named \"class\".", message);
        }

        [Fact]
        public void AMissingFileIsAnError()
        {
            var request = Request(new object?[0][], new object?[0][]);

            var error = Assert.Throws<AllocationException>(() => SourceWorkbookReader.Read("does-not-exist.xlsx", request));
            Assert.Equal("The source workbook no longer exists. Choose it again.", error.Message);
        }
    }
}
