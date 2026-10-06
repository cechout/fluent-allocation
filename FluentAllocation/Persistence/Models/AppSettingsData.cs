namespace FluentAllocation.Persistence.Models
{
    public enum AppTheme
    {
        System,
        Light,
        Dark
    }

    // the shape of settings.json; the initial values are the defaults
    public class AppSettingsData
    {
        public AppTheme AppTheme { get; set; } = AppTheme.System;
    }
}
