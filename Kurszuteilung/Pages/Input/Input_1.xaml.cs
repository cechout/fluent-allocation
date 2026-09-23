using Kurszuteilung.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Input
{
    public partial class Input_1 : Page
    {
        //Constructor
        public Input_1()
        {
            InitializeComponent();
        }

        //Select Source File
        private void Click_SourceFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            //Filter
            openFileDialog.Filter = "Excel Worksheets|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                TextBlock_SourceFile.Text = filePath;

                Globals.ExcelFilePath = TextBlock_SourceFile.Text;
            }
        }
    }
}
