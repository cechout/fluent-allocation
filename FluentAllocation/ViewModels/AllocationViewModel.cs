using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using FluentAllocation.Services;

namespace FluentAllocation.ViewModels
{
    public enum RunState
    {
        Idle,
        Running,
        Succeeded,
        Failed
    }

    // the allocation form:
    // the two workbooks, the five counts and the grouping, and one run at a time through AllocationPipeline
    //
    // the run goes to the thread pool, so the window keeps responding while a large workbook is read and
    // written; the form is disabled until it is back
    public partial class AllocationViewModel : ObservableObject
    {
        // === fields ===

        private const string ChoosePrompt = "Click to choose";

        private readonly IFileDialogService _fileDialogs;


        // === constructor ===

        public AllocationViewModel(IFileDialogService fileDialogs)
        {
            _fileDialogs = fileDialogs;
        }


        // === workbooks ===

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SourceLabel))]
        [NotifyCanExecuteChangedFor(nameof(AllocateCommand))]
        public partial string? SourcePath { get; set; }

        // each mode keeps its own file, so switching back and forth never writes to the file of the other one
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ResultPath), nameof(ResultLabel))]
        [NotifyCanExecuteChangedFor(nameof(AllocateCommand))]
        public partial string? NewResultPath { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ResultPath), nameof(ResultLabel))]
        [NotifyCanExecuteChangedFor(nameof(AllocateCommand))]
        public partial string? ExistingResultPath { get; set; }

        // the index of the segment, in the order of ResultTarget
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ResultPath), nameof(ResultLabel))]
        [NotifyCanExecuteChangedFor(nameof(AllocateCommand))]
        public partial int ResultTargetIndex { get; set; }

        public ResultTarget ResultTarget => ResultTargetIndex == 1 ? ResultTarget.ExistingFile : ResultTarget.NewFile;

        public string? ResultPath => ResultTarget == ResultTarget.NewFile ? NewResultPath : ExistingResultPath;

        // the file name on the button, the full path in its tooltip
        public string SourceLabel => Label(SourcePath);
        public string ResultLabel => Label(ResultPath);


        // === counts ===
        // doubles, since that is what a NumberBox binds; every change is put back on a whole number from 0

        [ObservableProperty]
        public partial double ParticipantCount { get; set; }

        [ObservableProperty]
        public partial double OptionCount { get; set; }

        [ObservableProperty]
        public partial double AttributeCount { get; set; }

        [ObservableProperty]
        public partial double PriorityCount { get; set; }

        [ObservableProperty]
        public partial double OptionsPerParticipant { get; set; }

        partial void OnParticipantCountChanged(double value) => ParticipantCount = Whole(value);
        partial void OnOptionCountChanged(double value) => OptionCount = Whole(value);
        partial void OnAttributeCountChanged(double value) => AttributeCount = Whole(value);
        partial void OnPriorityCountChanged(double value) => PriorityCount = Whole(value);
        partial void OnOptionsPerParticipantChanged(double value) => OptionsPerParticipant = Whole(value);


        // === grouping ===

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupAttributeEnabled))]
        public partial bool IsGrouping { get; set; }

        [ObservableProperty]
        public partial string GroupAttribute { get; set; } = string.Empty;


        // === run state ===

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIdle), nameof(IsRunning), nameof(IsSucceeded), nameof(IsFailed), nameof(IsFormEnabled), nameof(IsGroupAttributeEnabled))]
        [NotifyCanExecuteChangedFor(nameof(AllocateCommand), nameof(PickSourceCommand), nameof(PickResultCommand))]
        public partial RunState State { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        public bool IsIdle => State == RunState.Idle;
        public bool IsRunning => State == RunState.Running;
        public bool IsSucceeded => State == RunState.Succeeded;
        public bool IsFailed => State == RunState.Failed;
        public bool IsFormEnabled => State != RunState.Running;
        public bool IsGroupAttributeEnabled => IsGrouping && IsFormEnabled;


        // === commands ===

        [RelayCommand(CanExecute = nameof(IsFormEnabled))]
        private async Task PickSourceAsync()
        {
            string? path = await _fileDialogs.PickWorkbookToOpenAsync();
            if (path != null) SourcePath = path;
        }

        // a new file goes through the save dialog, an existing one through the open dialog
        [RelayCommand(CanExecute = nameof(IsFormEnabled))]
        private async Task PickResultAsync()
        {
            if (ResultTarget == ResultTarget.NewFile)
            {
                string? path = await _fileDialogs.PickWorkbookToSaveAsync();
                if (path != null) NewResultPath = path;
            }
            else
            {
                string? path = await _fileDialogs.PickWorkbookToOpenAsync();
                if (path != null) ExistingResultPath = path;
            }
        }

        [RelayCommand(CanExecute = nameof(CanAllocate))]
        private async Task AllocateAsync()
        {
            AllocationRequest request = BuildRequest();
            State = RunState.Running;

            try
            {
                await Task.Run(() => AllocationPipeline.Run(request, new Random()));
                State = RunState.Succeeded;
            }
            catch (AllocationException exception)
            {
                ErrorMessage = exception.Message;
                State = RunState.Failed;
            }
            catch (Exception exception)
            {
                // nothing the pipeline knows to explain; the raw message is still more than nothing
                ErrorMessage = $"Something unexpected went wrong: {exception.Message}";
                State = RunState.Failed;
            }
        }

        // a click on the success or the error button brings the allocate button back
        [RelayCommand]
        private void DismissResult()
        {
            State = RunState.Idle;
            ErrorMessage = string.Empty;
        }

        private bool CanAllocate() => IsIdle && SourcePath != null && ResultPath != null;


        // === private helpers ===

        private AllocationRequest BuildRequest()
        {
            return new AllocationRequest
            {
                SourcePath = SourcePath!,
                ResultPath = ResultPath!,
                Target = ResultTarget,
                ParticipantCount = (int)ParticipantCount,
                OptionCount = (int)OptionCount,
                AttributeCount = (int)AttributeCount,
                PriorityCount = (int)PriorityCount,
                OptionsPerParticipant = (int)OptionsPerParticipant,
                GroupAttribute = IsGrouping ? GroupAttribute : null
            };
        }

        // an emptied box gives NaN, which counts as 0; the setter stops here once the value is whole
        private static double Whole(double value)
        {
            return double.IsNaN(value) || value < 0 ? 0 : Math.Round(value);
        }

        private static string Label(string? path)
        {
            return path == null ? ChoosePrompt : Path.GetFileName(path);
        }
    }
}
