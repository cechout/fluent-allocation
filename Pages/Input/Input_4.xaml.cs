using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Input
{
    public partial class Input_4 : Page
    {
        //Constructor
        public Input_4()
        {
            InitializeComponent();
        }

        //Grouping Buttons
        private void Click_EnableGrouping(object sender, RoutedEventArgs e)
        {
            Button_GroupingDisabled.Visibility = Visibility.Collapsed;
            Button_GroupingEnabled.Visibility = Visibility.Visible;
        }

        private void Click_DisableGrouping(object sender, RoutedEventArgs e)
        {
            Button_GroupingDisabled.Visibility = Visibility.Visible;
            Button_GroupingEnabled.Visibility = Visibility.Collapsed;
        }
    }
}
