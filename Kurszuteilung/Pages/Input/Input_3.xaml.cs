using System.Windows;
using System.Windows.Controls;

namespace Kurszuteilung.Pages.Input
{
    public partial class Input_3 : Page
    {
        //Constructor
        public Input_3()
        {
            InitializeComponent();
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

        //NrOfUser Buttons
        private void Click_AddUser(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfUsers, 1);
        }
        private void Click_SubtractUser(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfUsers, 2);
        }
        //NrOfSubjects Buttons
        private void Click_AddSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfSubjects, 1);
        }
        private void Click_SubtractSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfSubjects, 2);
        }
        //NrOfAttributes Buttons
        private void Click_AddAttribute(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfAttributes, 1);
        }
        private void Click_SubtractAttribute(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfAttributes, 2);
        }
        //NrOfPriorities Buttons
        private void Click_AddPrio(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfPrios, 1);
        }
        private void Click_SubtractPrio(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfPrios, 2);
        }
        //NrOfRequiredSubjects Buttons
        private void Click_AddRequiredSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfRequiredSubjects, 1);
        }
        private void Click_SubtractRequiredSubject(object sender, RoutedEventArgs e)
        {
            CalculateValue(Textbox_NrOfRequiredSubjects, 2);
        }
    }
}
