using Kurszuteilung.Classes;
using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Setup
{
    public partial class Setup : Page
    {
        private int CurrentPageIndex;

        public Setup()
        {
            InitializeComponent();

            Globals.CurrentPage = "/Pages/Setup/Setup.xaml";
            CurrentPageIndex = 1;
        }

        //Menu Button
        private void Click_Menu(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Menu/Menu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
        }

        //Navigate Buttons
        //Left
        private void Click_Left(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex -= 1;

            if (CurrentPageIndex == 0)
            {
                CurrentPageIndex = 1;
            }

            UpdateFrame();
        }

        //Right
        private void Click_Right(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex += 1;

            if (CurrentPageIndex == 6)
            {
                CurrentPageIndex = 5;
            }

            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (CurrentPageIndex == 1) Frame_Help.Source = new Uri("/Pages/Setup/Setup_1.xaml", UriKind.Relative);
            if (CurrentPageIndex == 2) Frame_Help.Source = new Uri("/Pages/Setup/Setup_2.xaml", UriKind.Relative);
            if (CurrentPageIndex == 3) Frame_Help.Source = new Uri("/Pages/Setup/Setup_3.xaml", UriKind.Relative);
            if (CurrentPageIndex == 4) Frame_Help.Source = new Uri("/Pages/Setup/Setup_4.xaml", UriKind.Relative);
            if (CurrentPageIndex == 5) Frame_Help.Source = new Uri("/Pages/Setup/Setup_5.xaml", UriKind.Relative);
        }
    }
}
