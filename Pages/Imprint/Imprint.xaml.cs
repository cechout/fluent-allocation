using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Imprint
{
    public partial class Imprint : Page
    {
        //Constructor
        public Imprint()
        {
            InitializeComponent();

            //Globals.CurrentPage = "/Pages/Imprint.xaml";
        }

        private void Click_Menu(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Menu/Menu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }
    }
}
