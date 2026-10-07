using FluentAllocation.ViewModels;
using Microsoft.UI.Xaml;
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

        // a scroll viewer measures its content without a height limit, so the star rows would collapse to
        // their content; a floor at the viewport height lets them share out whatever the window has
        private void FormScroller_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FormGrid.MinHeight = e.NewSize.Height;
        }
    }
}
