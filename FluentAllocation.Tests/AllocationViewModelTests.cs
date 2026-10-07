using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAllocation.Models;
using FluentAllocation.Services;
using FluentAllocation.ViewModels;
using Xunit;
using static FluentAllocation.Tests.Workbooks;

namespace FluentAllocation.Tests
{
    // the allocation form without a window: the pickers are a queue of answers
    public sealed class AllocationViewModelTests : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), "FluentAllocation.Tests", Guid.NewGuid().ToString("N"));
        private readonly FakeDialogs _dialogs = new FakeDialogs();
        private readonly AllocationViewModel _viewModel;

        public AllocationViewModelTests()
        {
            Directory.CreateDirectory(_folder);
            _viewModel = new AllocationViewModel(_dialogs);
        }

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch { /* a temp folder left behind costs nothing */ }
        }

        private sealed class FakeDialogs : IFileDialogService
        {
            public Queue<string?> Answers { get; } = new Queue<string?>();
            public List<string> Calls { get; } = new List<string>();

            public Task<string?> PickWorkbookToOpenAsync()
            {
                Calls.Add("open");
                return Task.FromResult(Answers.Dequeue());
            }

            public Task<string?> PickWorkbookToSaveAsync()
            {
                Calls.Add("save");
                return Task.FromResult(Answers.Dequeue());
            }
        }

        private async Task PickSource(string? path)
        {
            _dialogs.Answers.Enqueue(path);
            await _viewModel.PickSourceCommand.ExecuteAsync(null);
        }

        private async Task PickResult(string? path)
        {
            _dialogs.Answers.Enqueue(path);
            await _viewModel.PickResultCommand.ExecuteAsync(null);
        }


        // === files ===

        [Fact]
        public async Task AllocateNeedsBothFiles()
        {
            Assert.False(_viewModel.AllocateCommand.CanExecute(null));

            await PickSource(@"C:\data\source.xlsx");
            Assert.False(_viewModel.AllocateCommand.CanExecute(null));

            await PickResult(@"C:\data\result.xlsx");
            Assert.True(_viewModel.AllocateCommand.CanExecute(null));
        }

        [Fact]
        public async Task TheButtonsShowTheFileNameOrThePrompt()
        {
            Assert.Equal("Click to choose", _viewModel.SourceLabel);

            await PickSource(@"C:\data\source.xlsx");

            Assert.Equal("source.xlsx", _viewModel.SourceLabel);
        }

        [Fact]
        public async Task ACancelledDialogKeepsTheFileBefore()
        {
            await PickSource(@"C:\data\source.xlsx");
            await PickSource(null);

            Assert.Equal(@"C:\data\source.xlsx", _viewModel.SourcePath);
        }

        // version 1 shared one path between the two modes, so a switch wrote to the file of the other mode
        [Fact]
        public async Task EachResultModeKeepsItsOwnFile()
        {
            await PickResult(@"C:\data\new.xlsx");
            _viewModel.ResultTargetIndex = 1;

            Assert.Null(_viewModel.ResultPath);
            Assert.Equal("Click to choose", _viewModel.ResultLabel);

            await PickResult(@"C:\data\existing.xlsx");
            _viewModel.ResultTargetIndex = 0;

            Assert.Equal(@"C:\data\new.xlsx", _viewModel.ResultPath);
            Assert.Equal(new[] { "save", "open" }, _dialogs.Calls);
        }


        // === counts ===

        [Theory]
        [InlineData(3.0, 3.0)]
        [InlineData(2.6, 3.0)]
        [InlineData(-4.0, 0.0)]
        [InlineData(double.NaN, 0.0)]
        public void ACountIsAlwaysAWholeNumberFromZero(double typed, double kept)
        {
            _viewModel.ParticipantCount = typed;

            Assert.Equal(kept, _viewModel.ParticipantCount);
        }


        // === runs ===

        private async Task PrepareRun(object?[][] participants, object?[][] options)
        {
            string sourcePath = Path.Combine(_folder, "source.xlsx");
            using (var workbook = Source(participants, options)) workbook.SaveAs(sourcePath);

            await PickSource(sourcePath);
            await PickResult(Path.Combine(_folder, "result.xlsx"));

            AllocationRequest counts = Request(participants, options);
            _viewModel.ParticipantCount = counts.ParticipantCount;
            _viewModel.OptionCount = counts.OptionCount;
            _viewModel.PriorityCount = counts.PriorityCount;
            _viewModel.OptionsPerParticipant = 1;
        }

        [Fact]
        public async Task ASuccessfulRunEndsSucceededAndADismissGoesBack()
        {
            await PrepareRun(new[] { Row(1, "Ada", 1) }, new[] { Row(1, "Art", 1) });

            await _viewModel.AllocateCommand.ExecuteAsync(null);

            Assert.True(_viewModel.IsSucceeded);
            Assert.True(File.Exists(_viewModel.ResultPath));
            Assert.False(_viewModel.AllocateCommand.CanExecute(null));

            _viewModel.DismissResultCommand.Execute(null);

            Assert.True(_viewModel.IsIdle);
            Assert.True(_viewModel.AllocateCommand.CanExecute(null));
        }

        [Fact]
        public async Task AFailedRunShowsTheMessage()
        {
            await PrepareRun(new[] { Row(1, "Ada", 1) }, new[] { Row(1, "Art", 1) });
            _viewModel.IsGrouping = true;
            _viewModel.GroupAttribute = "Team";

            await _viewModel.AllocateCommand.ExecuteAsync(null);

            Assert.True(_viewModel.IsFailed);
            Assert.Equal("There is no attribute named \"Team\". The source workbook has no attributes.", _viewModel.ErrorMessage);

            _viewModel.DismissResultCommand.Execute(null);

            Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        }

        [Fact]
        public void TheAttributeFieldFollowsTheCheckbox()
        {
            Assert.False(_viewModel.IsGroupAttributeEnabled);

            _viewModel.IsGrouping = true;

            Assert.True(_viewModel.IsGroupAttributeEnabled);
        }
    }
}
