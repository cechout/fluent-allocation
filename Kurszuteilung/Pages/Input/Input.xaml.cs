using Kurszuteilung.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Input
{
    public partial class Input : Page
    {
        private int CurrentPageIndex;

        //Constructor
        public Input()
        {
            InitializeComponent();

            Globals.CurrentPage = "/Pages/Input/Input.xaml";
            CurrentPageIndex = 1;
        }

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

            if (CurrentPageIndex == 5)
            {
                CurrentPageIndex = 4;
            }

            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (CurrentPageIndex == 1) Frame_Help.Source = new Uri("/Pages/Input/Input_1.xaml", UriKind.Relative);
            if (CurrentPageIndex == 2) Frame_Help.Source = new Uri("/Pages/Input/Input_2.xaml", UriKind.Relative);
            if (CurrentPageIndex == 3) Frame_Help.Source = new Uri("/Pages/Input/Input_3.xaml", UriKind.Relative);
            if (CurrentPageIndex == 4) Frame_Help.Source = new Uri("/Pages/Input/Input_4.xaml", UriKind.Relative);
        }
    }
}
