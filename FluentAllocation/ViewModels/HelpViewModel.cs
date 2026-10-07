using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FluentAllocation.ViewModels
{
    // one help section: which of its steps is shown, and the way back and forth through them
    // (the ends disable their button rather than doing nothing, as version 1 did)
    public partial class HelpViewModel : ObservableObject
    {
        public HelpViewModel(int stepCount)
        {
            StepCount = stepCount;
        }

        public int StepCount { get; }

        // 0 based; the pips pager writes it too
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BackCommand), nameof(NextCommand))]
        public partial int CurrentStep { get; set; }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void Back() => CurrentStep--;

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void Next() => CurrentStep++;

        private bool CanGoBack() => CurrentStep > 0;
        private bool CanGoNext() => CurrentStep < StepCount - 1;
    }
}
