using Kurszuteilung.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Kurszuteilung.Pages.Input
{
    public partial class Input_2 : Page
    {
        //Constructor
        public Input_2()
        {
            InitializeComponent();

            //TextBlock_Error.Text = Globals.ErrorMessage;
        }

        //Input Path Button Events
        private void Click_CreateNewFileDisabled(object sender, RoutedEventArgs e)
        {
            Button_UseExistingFileEnabled.Visibility = Visibility.Collapsed;
            Button_UseExistingFileDisabled.Visibility = Visibility.Visible;

            Button_CreateNewFileEnabled.Visibility = Visibility.Visible;
            Button_CreateNewFileDisabled.Visibility = Visibility.Collapsed;

            Button_OutputFileNew.Visibility = Visibility.Visible;
            Button_OutputFileExisting.Visibility = Visibility.Collapsed;

            Globals.SaveLocationMode = 1;
        }

        private void Click_UseExistingFileDisabled(object sender, RoutedEventArgs e)
        {
            Button_UseExistingFileEnabled.Visibility = Visibility.Visible;
            Button_UseExistingFileDisabled.Visibility = Visibility.Collapsed;

            Button_CreateNewFileEnabled.Visibility = Visibility.Collapsed;
            Button_CreateNewFileDisabled.Visibility = Visibility.Visible;

            Button_OutputFileNew.Visibility = Visibility.Collapsed;
            Button_OutputFileExisting.Visibility = Visibility.Visible;

            Globals.SaveLocationMode = 2;
        }

        //Select Output File
        //New
        private void Click_OutputFileNew(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Excel Worksheets|*.xlsx";

            //Filter
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.InitialDirectory = folderPath;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                Console.WriteLine($"Selected file path: {filePath}");

                File.Create(filePath);
                Console.WriteLine($"File '{Path.GetFileName(filePath)}' has been created in '{Path.GetDirectoryName(filePath)}'.");
                TextBlock_NewFile.Text = Path.GetDirectoryName(filePath) + @"\" + Path.GetFileName(filePath);

                Globals.PathSavedFile = TextBlock_NewFile.Text;
            }
        }

        //Existing
        private void Click_OutputFileExisting(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            //Filter
            openFileDialog.Filter = "Excel Worksheets|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                TextBlock_ExistingFile.Text = filePath;

                Globals.PathSavedFile = TextBlock_ExistingFile.Text;
            }
        }
    }
}
