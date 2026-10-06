using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAllocation.Persistence.Models;
using FluentAllocation.Persistence.Services;

namespace FluentAllocation.ViewModels
{
    // the settings page:
    // the theme, read from and written straight to SettingsService, and the version for the about section
    public partial class SettingsViewModel : ObservableObject
    {
        // the theme as the index of its entry in the combo box, which lists AppTheme in order
        [ObservableProperty]
        public partial int ThemeIndex { get; set; } = (int)SettingsService.Instance.AppTheme;

        // vx.y.z, read off the assembly, which the SDK stamps from Version in the csproj
        public string VersionLabel { get; } = FormatVersion();

        partial void OnThemeIndexChanged(int value)
        {
            // -1 while the combo box is being built
            if (value >= 0) SettingsService.Instance.AppTheme = (AppTheme)value;
        }

        private static string FormatVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? string.Empty : $"v{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
