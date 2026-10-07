using FluentAllocation.Persistence.Services;
using FluentAllocation.Services;
using Microsoft.UI.Xaml;

namespace FluentAllocation
{
    // the entry point; one window, and no activation beyond a plain launch
    public partial class App : Application
    {
        private Window? _window;

        // set by MainWindow, which the pickers belong to
        public static IFileDialogService FileDialogs { get; set; } = null!;

        public App()
        {
            InitializeComponent();

            PersistenceService.Initialize(AppDataFolder.Resolve());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // the settings are loaded before the window reads its theme
            SettingsService.Instance.LoadFromData(PersistenceService.Instance.LoadSettings());

            _window = new MainWindow();
            _window.Activate();
        }
    }
}
