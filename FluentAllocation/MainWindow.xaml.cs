using System;
using System.Linq;
using FluentAllocation.Persistence.Models;
using FluentAllocation.Persistence.Services;
using FluentAllocation.Services;
using FluentAllocation.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinUIEx;

namespace FluentAllocation
{
    // the app shell:
    // the navigation pane and the Frame every page is shown in; it also owns the theme, since that reaches the
    // caption buttons beyond the page
    public sealed partial class MainWindow : Window
    {
        // === fields ===

        // --- window size ---
        // in DIP; (the start size is the one of version 1)
        private const double StartWidth = 340;
        private const double StartHeight = 620;
        private const double MinWidth = 340;
        private const double MinHeight = 620;

        // --- title bar ---
        private const double TitleBarDeactivatedOpacity = 0.5; // TitleBarDeactivatedOpacity of the WinUI TitleBar

        private readonly WindowManager _windowManager;


        // === constructor ===

        public MainWindow()
        {
            this.InitializeComponent();
            this.AppWindow.SetIcon("Assets\\Icon\\Icon.ico");

            // the pickers need the window, and the first page asks for them while it is built
            App.FileDialogs = new FileDialogService(AppWindow.Id);

            ShowPage(typeof(AllocationPage));
            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First(item => (string)item.Tag == "Allocation");

            // our own title bar in the client area; transparent caption buttons, so the Mica shows through
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            this.SetTitleBar(AppTitleBar);
            this.Activated += MainWindow_Activated;

            ApplyTheme(SettingsService.Instance.AppTheme);
            SettingsService.Instance.ThemeChanged += ApplyTheme;

            _windowManager = WindowManager.Get(this);
            _windowManager.MinWidth = MinWidth;
            _windowManager.MinHeight = MinHeight;
            this.SetWindowSize(StartWidth, StartHeight);
        }


        // === navigation ===

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type? page = PageForTag(args.InvokedItemContainer.Tag?.ToString());

            // a second click on the item already shown would navigate the page onto itself
            if (page == null || MainFrame.CurrentSourcePageType == page) return;

            ShowPage(page);
        }

        private static Type? PageForTag(string? itemTag)
        {
            return itemTag switch
            {
                "Allocation" => typeof(AllocationPage),
                "SourceHelp" => typeof(SourceHelpPage),
                "InputHelp" => typeof(InputHelpPage),
                "Settings" => typeof(SettingsPage),
                _ => null
            };
        }

        private void ShowPage(Type page)
        {
            MainFrame.Navigate(page, null, new SuppressNavigationTransitionInfo());
        }


        // === title bar ===

        // the bar dims while another window is active, as the WinUI TitleBar does
        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            double opacity = args.WindowActivationState == WindowActivationState.Deactivated ? TitleBarDeactivatedOpacity : 1;

            AppTitleText.Opacity = opacity;
            AppTitleIcon.Opacity = opacity;
        }


        // === theming ===

        // applies a theme to the content and the caption buttons; (those only follow PreferredTheme)
        private void ApplyTheme(AppTheme theme)
        {
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme switch
                {
                    AppTheme.Light => ElementTheme.Light,
                    AppTheme.Dark => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }

            AppWindow.TitleBar.PreferredTheme = theme switch
            {
                AppTheme.Light => TitleBarTheme.Light,
                AppTheme.Dark => TitleBarTheme.Dark,
                _ => TitleBarTheme.UseDefaultAppMode
            };
        }
    }
}
