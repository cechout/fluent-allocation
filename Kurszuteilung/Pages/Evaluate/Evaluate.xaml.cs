using Kurszuteilung.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Kurszuteilung.Pages.Evaluate
{
    public partial class Evaluate : Page
    {
        public Excel? Excel1;

        //Constructor
        public Evaluate()
        {
            InitializeComponent();
            Globals.CurrentPage = "/Pages/Evaluate/Evaluate.xaml";

            //Initialize Excel Class on another Thread
            Task.Run(() => InitializeExcel());
        }

        //Kill all excel processes every time the page opens
        private void InitializeExcel()
        {
            Excel1 = new();
            Excel1.KillProcesses();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Excel1.Close();
        }

        private bool StartEvaluation()
        {
            //"try" in case of any error that could occur during the evaluation
            try
            {
                EvaluateC Evaluate1 = new();

                Evaluate1.ReadFromExcel();
                Evaluate1.SortUsers();
                Evaluate1.WriteToExcel();

                Button_Completed.Visibility = Visibility.Visible;
                return true;
            }
            catch (Exception e)
            {
                Button_Error.Visibility = Visibility.Visible;
                InitializeExcel();

                Globals.ErrorMessage = e.Message;

                return false;
            }
        }

        public static bool IsInteger(string input1)
        {
            //Here is no need for a variable
            return int.TryParse(input1, out _);
        }

        //Add/Subtract Value from NrOfEverything TextBox
        private static void CalculateValue(System.Windows.Controls.TextBox textBox1, int mode)
        {
            int textBox1Value = 0;

            //Check if current value in textbox is a number
            if ((!(textBox1.Text == "")) && IsInteger(textBox1.Text))
            {
                textBox1Value = int.Parse(textBox1.Text);
            }

            if (mode == 1)
            {
                //Add
                textBox1Value++;
            }
            else
            {
                //Subtract
                textBox1Value--;
                if (textBox1Value <= -1) textBox1Value = 0;
            }

            textBox1.Text = textBox1Value.ToString();
        }

        //Navigate
        private void Click_Menu(object sender, RoutedEventArgs e)
        {
            Uri pageFunctionUri = new Uri("/Pages/Menu/Menu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(pageFunctionUri);
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

                //File.Create(filePath);
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

        //NrOfEverything Buttons
        //NrOfUser
        private void Click_AddUser(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfUsers, 1);
        }
        private void Click_SubtractUser(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfUsers, 2);
        }
        //NrOfSubjects
        private void Click_AddSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfSubjects, 1);
        }
        private void Click_SubtractSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfSubjects, 2);
        }
        //NrOfAttributes
        private void Click_AddAttribute(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfAttributes, 1);
        }
        private void Click_SubtractAttribute(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfAttributes, 2);
        }
        //NrOfPriorities
        private void Click_AddPrio(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfPrios, 1);
        }
        private void Click_SubtractPrio(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfPrios, 2);
        }
        //NrOfRequiredSubjects
        private void Click_AddRequiredSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfRequiredSubjects, 1);
        }
        private void Click_SubtractRequiredSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfRequiredSubjects, 2);
        }

        //Grouping Buttons
        private void Click_EnableGrouping(object sender, RoutedEventArgs e)
        {
            Button_GroupingDisabled.Visibility = Visibility.Collapsed;
            Button_GroupingEnabled.Visibility = Visibility.Visible;

            Globals.GroupByUserAttribute = true;
        }

        private void Click_DisableGrouping(object sender, RoutedEventArgs e)
        {
            Button_GroupingDisabled.Visibility = Visibility.Visible;
            Button_GroupingEnabled.Visibility = Visibility.Collapsed;

            Globals.GroupByUserAttribute = false;
        }

        //Evaluate Button
        private void Click_Evaluate(object sender, RoutedEventArgs e)
        {
            Button_Evaluate.Visibility = Visibility.Collapsed;

            //Set setting Variables
            Globals.PathWithQuotes = true;
            Globals.GroupNextToEachOther = false;
            Globals.ShowNotSortedStudents = true;
            Globals.ResetAfterError = false;

            //Set Input Variables
            //if (Globals.PathWithQuotes)
            //{
            //    //Cut first and last char out if ist a "
            //    //TextBox1
            //    if (Textbox_Input1.Text.Length >= 2 && Textbox_Input1.Text[0] == '\"' && Textbox_Input1.Text[Textbox_Input1.Text.Length - 1] == '\"')
            //    {
            //        Globals.ExcelFilePath = Textbox_Input1.Text.Substring(1, Textbox_Input1.Text.Length - 2);
            //    }

            //    //TextBox2
            //    if (Textbox_Input2.Text.Length >= 2 && Textbox_Input2.Text[0] == '\"' && Textbox_Input2.Text[Textbox_Input2.Text.Length - 1] == '\"')
            //    {
            //        Globals.PathSavedFile = Textbox_Input2.Text.Substring(1, Textbox_Input2.Text.Length - 2);
            //    }
            //}
            //else
            //{
            //    Globals.ExcelFilePath = Textbox_Input1.Text;
            //    Globals.PathSavedFile = Textbox_Input2.Text;
            //}

            Globals.NrOfUsers = int.Parse(Textbox_NrOfUsers.Text);
            Globals.NrOfSubjects = int.Parse(Textbox_NrOfSubjects.Text);
            Globals.NrOfPrios = int.Parse(Textbox_NrOfPrios.Text);
            Globals.NrOfRequiredSubjects = int.Parse(Textbox_NrOfRequiredSubjects.Text);
            Globals.NrOfUserAttributes = int.Parse(Textbox_NrOfAttributes.Text);

            //Set SaveLocationMode and GroupByUserAttribute
            if (Button_CreateNewFileEnabled.Visibility == Visibility.Visible) Globals.SaveLocationMode = 1;
            else Globals.SaveLocationMode = 2;

            if (Button_GroupingEnabled.Visibility == Visibility.Visible)
            {
                Globals.GroupByUserAttribute = true;
                Globals.ProcessAttribute = Textbox_ProcessAttribute.Text;
            }
            else
            {
                Globals.GroupByUserAttribute = false;
                Globals.ProcessAttribute = "";
            }

            //This code updates the UI, so that the Button_Evaluate disappears when pressed
            Dispatcher.BeginInvoke(new Action(() =>
            {
                bool evaluationSuccessful = StartEvaluation();

                // When StartEvaluation() is finished, update the UI
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (evaluationSuccessful == true)
                    {
                        Button_Evaluate.Visibility = Visibility.Visible;
                    }
                }), DispatcherPriority.ContextIdle, null);
            }), DispatcherPriority.Background, null);
        }

        //Button Completed
        private void Click_Completed(object sender, RoutedEventArgs e)
        {
            Button_Completed.Visibility = Visibility.Collapsed;
            Button_Evaluate.Visibility = Visibility.Visible;
        }

        //Error Button
        private void Click_Error(object sender, RoutedEventArgs e)
        {
            Button_Error.Visibility = Visibility.Collapsed;
            Button_Evaluate.Visibility = Visibility.Visible;

            //if (Globals.ResetAfterError)
            //{
            //    Textbox_Input1.Text = "";
            //    Textbox_Input2.Text = "";

            //    Textbox_NrOfUsers.Text = "0";
            //    Textbox_NrOfSubjects.Text = "0";
            //    Textbox_NrOfAttributes.Text = "0";
            //    Textbox_NrOfPrios.Text = "0";
            //    Textbox_NrOfRequiredSubjects.Text = "0";

            //    Textbox_ProcessAttribute.Text = "";
            //}
        }
    }
}
