using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using Xunit;
using static FluentAllocation.Tests.Workbooks;

namespace FluentAllocation.Tests
{
    // whole runs against files on disk, each test in a temp folder of its own
    public sealed class AllocationPipelineTests : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), "FluentAllocation.Tests", Guid.NewGuid().ToString("N"));

        private static readonly object?[][] Participants = { Row(1, "Ada", "10a", 1), Row(2, "Ben", "10b", 1) };
        private static readonly object?[][] Options = { Row(1, "Art", 1) };

        public AllocationPipelineTests()
        {
            Directory.CreateDirectory(_folder);
        }

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch { /* a temp folder left behind costs nothing */ }
        }

        private AllocationRequest WriteSource()
        {
            string sourcePath = Path.Combine(_folder, "source.xlsx");
            using (var workbook = Source(Participants, Options, new[] { "Class" })) workbook.SaveAs(sourcePath);

            return Request(Participants, Options, 1) with
            {
                SourcePath = sourcePath,
                ResultPath = Path.Combine(_folder, "result.xlsx")
            };
        }


        [Fact]
        public void ARunWritesTheResultFile()
        {
            AllocationRequest request = WriteSource();

            AllocationResult result = AllocationPipeline.Run(request, new Random(1));

            using var workbook = new XLWorkbook(request.ResultPath);
            Assert.Equal(new[] { "Participants", "Options", "Unallocated" }, workbook.Worksheets.Select(sheet => sheet.Name));
            Assert.Single(result.Unallocated);
        }

        // an existing file ends up holding the result sheets and nothing else
        [Fact]
        public void AnExistingResultFileIsReplaced()
        {
            AllocationRequest request = WriteSource() with { Target = ResultTarget.ExistingFile };
            using (var old = new XLWorkbook())
            {
                for (int index = 1; index <= 5; index++) old.AddWorksheet($"Old {index}").Cell(1, 1).Value = "old";
                old.SaveAs(request.ResultPath);
            }

            AllocationPipeline.Run(request, new Random(1));

            using var workbook = new XLWorkbook(request.ResultPath);
            Assert.Equal(new[] { "Participants", "Options", "Unallocated" }, workbook.Worksheets.Select(sheet => sheet.Name));
        }

        [Fact]
        public void TheSourceCannotBeTheResult()
        {
            AllocationRequest request = WriteSource();
            request = request with { ResultPath = request.SourcePath.ToUpperInvariant() };

            var error = Assert.Throws<AllocationException>(() => AllocationPipeline.Run(request, new Random(1)));
            Assert.Equal("The result workbook cannot be the source workbook, it would overwrite the source.", error.Message);
        }

        [Fact]
        public void GroupingNeedsAName()
        {
            AllocationRequest request = WriteSource() with { GroupAttribute = " " };

            var error = Assert.Throws<AllocationException>(() => AllocationPipeline.Run(request, new Random(1)));
            Assert.Equal("Enter the name of the attribute to group by.", error.Message);
        }

        // checked before the allocation, so the result file is never touched
        [Fact]
        public void AnUnknownGroupAttributeLeavesTheResultFileAlone()
        {
            AllocationRequest request = WriteSource() with { GroupAttribute = "Team" };

            Assert.Throws<AllocationException>(() => AllocationPipeline.Run(request, new Random(1)));
            Assert.False(File.Exists(request.ResultPath));
        }

        [Fact]
        public void AResultFileOpenElsewhereSaysSo()
        {
            AllocationRequest request = WriteSource();
            using var locked = new FileStream(request.ResultPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            var error = Assert.Throws<AllocationException>(() => AllocationPipeline.Run(request, new Random(1)));
            Assert.Equal("The result workbook is open in another program. Close it there and allocate again.", error.Message);
        }

        // Excel holds an open workbook with a write lock but lets others read it
        [Fact]
        public void ASourceOpenElsewhereCanStillBeRead()
        {
            AllocationRequest request = WriteSource();
            using var held = new FileStream(request.SourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            AllocationPipeline.Run(request, new Random(1));

            Assert.True(File.Exists(request.ResultPath));
        }
    }
}
