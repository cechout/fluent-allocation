using FluentAllocation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FluentAllocation.Views
{
    // help: what each field of the allocation form means, in four steps
    public sealed partial class InputHelpPage : Page
    {
        public HelpViewModel ViewModel { get; } = new HelpViewModel(4);

        public InputHelpPage()
        {
            this.InitializeComponent();
        }
    }
}
