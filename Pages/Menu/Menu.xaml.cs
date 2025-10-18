using Kurszuteilung.Classes;
using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Menu
{
    public partial class Menu : Page
    {
        //Constructor
        public Menu()
        {
            InitializeComponent();
            SetButtons();
        }

        private void SetButtons()
        {
            //DeHighlight every button
            Button1_Standard.Visibility = Visibility.Collapsed;
            Button2_Standard.Visibility = Visibility.Visible;

            Button1_Setup.Visibility = Visibility.Collapsed;
            Button2_Setup.Visibility = Visibility.Visible;

            Button1_Input.Visibility = Visibility.Collapsed;
            Button2_Input.Visibility = Visibility.Visible;

            //Highlight the selected button
            if (Globals.CurrentPage == "/Pages/Evaluate/Evaluate.xaml")
            {
                //Standard
                Button1_Standard.Visibility = Visibility.Visible;
                Button2_Standard.Visibility = Visibility.Collapsed;
            }
            if (Globals.CurrentPage == "/Pages/Setup/Setup.xaml")
            {
                //Setup
                Button1_Setup.Visibility = Visibility.Visible;
                Button2_Setup.Visibility = Visibility.Collapsed;
            }
            if (Globals.CurrentPage == "/Pages/Input/Input.xaml")
            {
                //Input
                Button1_Input.Visibility = Visibility.Visible;
                Button2_Input.Visibility = Visibility.Collapsed;
            }
            if (Globals.CurrentPage == "/Pages/Imprint/Imprint.xaml")
            {
                //Imprint
                //Button1_Imprint.Visibility = Visibility.Visible;
                //Button2_Imprint.Visibility = Visibility.Collapsed;
            }
        }

        //Menu button
        private void Click_Menu(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri(Globals.CurrentPage, UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }

        //Navigate Buttons
        private void Click_Standard(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Evaluate/Evaluate.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }

        private void Click_Input(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Input/Input.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }

        private void Click_Setup(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Setup/Setup.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }

        private void Click_Imprint(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Imprint/Imprint.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }
    }
}
