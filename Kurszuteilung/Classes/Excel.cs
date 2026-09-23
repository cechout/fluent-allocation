using Microsoft.Office.Interop.Excel;
using System.Diagnostics;

namespace Kurszuteilung.Classes
{
    public class Excel
    {
        readonly string path;

        readonly _Application excel1 = new Microsoft.Office.Interop.Excel.Application();
        Workbook WorkBook1;
        Worksheet WorkSheet1;

        //Constructor 1
        public Excel()
        {

        }

        //Constructor 2
        public Excel(string path)
        {
            try
            {
                this.path = path;
                WorkBook1 = excel1.Workbooks.Open(path);
            }
            catch (Exception) { }
        }

        public void ResetFile(int numberOfSheets)
        {
            foreach (Worksheet currentWorkSheet in WorkBook1.Worksheets)
            {
                currentWorkSheet.Cells.Clear();
            }

            while (WorkBook1.Worksheets.Count < numberOfSheets)
            {
                WorkSheet1 = WorkBook1.Worksheets.Add();
            }
        }

        public void CreateNewFile()
        {
            WorkBook1 = excel1.Workbooks.Add();
            WorkSheet1 = WorkBook1.Worksheets[1];
        }

        public void CreateNewSheet()
        {
            WorkSheet1 = WorkBook1.Worksheets.Add();
        }

        public string ReadCell(int row, int column, int sheet)
        {
            WorkSheet1 = WorkBook1.Worksheets[sheet];

            if (WorkSheet1.Cells[row, column].Value2 != null)
            {
                //"" to automatically convert it into a string
                return "" + WorkSheet1.Cells[row, column].Value2;
            }
            else return "";
        }

        public void Close()
        {
            try
            {
                excel1.Quit();

                // Call garbage collector
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //Delete unnecessary Sheets
                Microsoft.Office.Interop.Excel.Application excelApp = new();
                Workbook workbook = excelApp.Workbooks.Open(path);

                // Loop through all the worksheets and delete from the end until only the desired number of sheets are left
                while (workbook.Worksheets.Count > Globals.NumOfSheets)
                {
                    int lastSheetIndex = workbook.Worksheets.Count;
                    Worksheet lastSheet = (Worksheet)workbook.Worksheets[lastSheetIndex];
                    lastSheet.Delete();
                }

                // Save and close the workbook
                workbook.Save();
                workbook.Close();
                excelApp.Quit();

                KillProcesses();
            }
            catch (Exception)
            {
                KillProcesses();
            }
        }

        public void KillProcesses()
        {
            //Kill all excel processes
            var excelProcesses = Process.GetProcesses().Where(p => p.ProcessName.Equals("EXCEL"));

            foreach (var process in excelProcesses)
            {
                process.Kill();
            }
        }

        public void WriteCell(int row, int column, string value1, int sheet)
        {
            WorkSheet1 = WorkBook1.Worksheets[sheet];

            WorkSheet1.Cells[row, column].Value2 = value1;
        }

        public void Save(int numOfUserIdsNotSorted)
        {
            RenameSheets(numOfUserIdsNotSorted);
            WorkBook1.Save();
        }

        public void SaveAs(string path, int numOfUserIdsNotSorted)
        {
            RenameSheets(numOfUserIdsNotSorted);
            WorkBook1.SaveAs(path);
        }

        private void RenameSheets(int numOfUserIdsNotSorted)
        {
            //Rename every Sheet to a number so that there doesnt occur errors because of a double name
            int index1 = 1;
            foreach (Worksheet currentWorkSheet in WorkBook1.Sheets)
            {
                currentWorkSheet.Name = index1.ToString();
                index1++;
            }

            //First 2 Sheets "Schüler and "Fächer" are mandatory
            WorkBook1.Sheets[1].Name = "Schüler";
            WorkBook1.Sheets[2].Name = "Fächer";

            //Check if 3rd Sheet is for attributes or for the not sorted users
            if (Globals.NumOfSheets == 3 && numOfUserIdsNotSorted > 0)
            {
                WorkBook1.Sheets[3].Name = "FF-Schüler";
            }
            else if (Globals.NumOfSheets == 3 && numOfUserIdsNotSorted == 0)
            {
                WorkBook1.Sheets[3].Name = "Attribute";
            }

            if (Globals.NumOfSheets == 4)
            {
                WorkBook1.Sheets[3].Name = "Attribute";
                WorkBook1.Sheets[4].Name = "FF-Schüler";
            }
        }
    }
}
