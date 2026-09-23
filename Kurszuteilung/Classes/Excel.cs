using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kurszuteilung.Classes
{
    public class Excel
    {
        readonly string path;

        readonly _Application excel1 = StartExcel();
        Workbook WorkBook1;
        Worksheet WorkSheet1;

        // every excel process this app started, so KillProcesses can clean up after the app without touching an
        // excel window the user has open next to it
        private static readonly List<int> StartedProcessIds = new();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        // starts a hidden excel instance and remembers its process
        //
        // DisplayAlerts is off because the instance is never shown: a prompt such as the one SaveAs raises for an
        // existing file would wait for a click nobody can give, and the evaluation would hang
        private static _Application StartExcel()
        {
            _Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.DisplayAlerts = false;

            GetWindowThreadProcessId(new IntPtr(excel.Hwnd), out int processId);

            lock (StartedProcessIds)
            {
                StartedProcessIds.Add(processId);
            }

            return excel;
        }

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
                _Application excelApp = StartExcel();
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

        // kills the excel processes this app started and only those, so a workbook the user has open in an excel
        // window of their own survives the evaluation
        public void KillProcesses()
        {
            lock (StartedProcessIds)
            {
                foreach (int processId in StartedProcessIds)
                {
                    try
                    {
                        Process process = Process.GetProcessById(processId);
                        if (process.ProcessName.Equals("EXCEL")) process.Kill();
                    }
                    catch { /* already gone, it either quit on its own or an earlier call killed it */ }
                }

                StartedProcessIds.Clear();
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
