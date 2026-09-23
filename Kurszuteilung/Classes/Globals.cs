using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kurszuteilung.Classes
{
    class Globals
    {
        //Input Variables
        public static int NrOfUsers { get; set; }
        public static int NrOfSubjects { get; set; }
        public static int NrOfPrios { get; set; }
        public static int NrOfRequiredSubjects { get; set; }
        public static int NrOfUserAttributes { get; set; }

        public static bool GroupByUserAttribute { get; set; }
        public static string ProcessAttribute { get; set; }

        public static string ExcelFilePath { get; set; }
        public static int SaveLocationMode { get; set; }
        public static string PathSavedFile { get; set; }

        //Processing Variables
        public static int NumOfSheets { get; set; }

        //Settings Variables
        public static bool PathWithQuotes { get; set; }
        public static bool GroupNextToEachOther { get; set; }
        public static bool ShowNotSortedStudents { get; set; }
        public static bool ResetAfterError { get; set; }

        //Current Page
        public static string CurrentPage { get; set; }

        //Error Message
        public static string ErrorMessage { get; set; }
    }
}
