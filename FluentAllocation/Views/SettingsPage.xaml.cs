using FluentAllocation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FluentAllocation.Views
{
    // the settings page; not cached, so it reads the settings fresh on every visit
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
