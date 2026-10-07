using FluentAllocation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FluentAllocation.Views
{
    // the allocation form; cached, so it keeps what was entered across a navigation
    public sealed partial class AllocationPage : Page
    {
        public AllocationViewModel ViewModel { get; } = new AllocationViewModel(App.FileDialogs);

        public AllocationPage()
        {
            this.InitializeComponent();
        }
    }
}
