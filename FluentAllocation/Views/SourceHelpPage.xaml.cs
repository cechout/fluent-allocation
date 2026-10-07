using FluentAllocation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FluentAllocation.Views
{
    // help: how the source workbook is laid out, in five steps
    public sealed partial class SourceHelpPage : Page
    {
        public HelpViewModel ViewModel { get; } = new HelpViewModel(5);

        public SourceHelpPage()
        {
            this.InitializeComponent();
        }
    }
}
