using System;
using FluentAllocation.Persistence.Models;

namespace FluentAllocation.Persistence.Services
{
    // the live settings:
    // every setter saves and raises its own event; MainWindow applies the theme
    public class SettingsService
    {
        // === singleton instance ===

        public static SettingsService Instance { get; } = new SettingsService();


        // === fields ===

        private AppTheme _appTheme = AppTheme.System;


        // === events ===

        public event Action<AppTheme>? ThemeChanged;


        // === settings ===

        public AppTheme AppTheme
        {
            get => _appTheme;
            set
            {
                if (_appTheme == value) return;

                _appTheme = value;
                Save();
                ThemeChanged?.Invoke(value);
            }
        }


        // === load and save ===

        // an unknown theme in the file falls back to the system one
        public void LoadFromData(AppSettingsData data)
        {
            _appTheme = Enum.IsDefined(data.AppTheme) ? data.AppTheme : AppTheme.System;
        }

        public AppSettingsData ToData() => new AppSettingsData { AppTheme = _appTheme };

        private void Save() => PersistenceService.Instance.SaveSettings(ToData());
    }
}
