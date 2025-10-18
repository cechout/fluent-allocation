using System.Data.SqlClient;

namespace Kurszuteilung.Classes
{
    class Functions1 : Functions
    {
        public Excel Excel1 = new(Globals.ExcelFilePath);

        public int CurrentColumn;

        public void ResetDatabase()
        {
            con1.Open();
            command1 = new SqlCommand("DELETE FROM Subjects", con1);
            command1.ExecuteNonQuery();

            command1 = new SqlCommand("DELETE FROM Users", con1);
            command1.ExecuteNonQuery();

            command1 = new SqlCommand("DELETE FROM Priorities", con1);
            command1.ExecuteNonQuery();

            command1 = new SqlCommand("DELETE FROM UserSubjectAssignments", con1);
            command1.ExecuteNonQuery();

            command1 = new SqlCommand("DELETE FROM UserAttributes", con1);
            command1.ExecuteNonQuery();
            con1.Close();
        }

        public void WriteSubjectsToDatabase(int excelSheet)
        {
            int currentRow = 2;
            int currentColumn = 1;

            con1.Open();

            while (currentRow <= Globals.NrOfSubjects + 1)
            {
                command1 = new SqlCommand("INSERT INTO Subjects (Id, Name, Capacity) VALUES (@Id, @Name, @Capacity)", con1);

                while (currentColumn <= 3)
                {
                    string value1 = Excel1.ReadCell(currentRow, currentColumn, excelSheet);

                    if (currentColumn == 1) command1.Parameters.AddWithValue("@Id", int.Parse(value1));
                    if (currentColumn == 2) command1.Parameters.AddWithValue("@Name", value1);
                    if (currentColumn == 3) command1.Parameters.AddWithValue("@Capacity", int.Parse(value1));

                    currentColumn++;
                }

                command1.ExecuteNonQuery();
                currentRow++;
                currentColumn = 1;
            }

            con1.Close();

        }

        public void InsertUsers(int currentRow, int currentColumn, int excelSheet)
        {
            con1.Open();
            command1 = new SqlCommand("INSERT INTO Users (Id, Name, Sorted) VALUES (@Id, @Name, @Sorted)", con1);

            while (currentColumn <= 2)
            {
                string value1 = Excel1.ReadCell(currentRow, currentColumn, excelSheet);

                if (currentColumn == 1) command1.Parameters.AddWithValue("@Id", int.Parse(value1));
                if (currentColumn == 2) command1.Parameters.AddWithValue("@Name", value1);

                currentColumn++;
            }

            CurrentColumn = currentColumn;

            command1.Parameters.AddWithValue("@Sorted", 1);
            command1.ExecuteNonQuery();
            con1.Close();
        }

        public void InsertUserAttributes(int currentRow, int currentColumn, int excelSheet)
        {
            con1.Open();
            command1 = new SqlCommand("INSERT INTO UserAttributes (UserId, Name, Value) VALUES (@UserId, @Name, @Value)", con1);

            //-2 to subtract the Id and the name which are both mandatory
            while (currentColumn <= Globals.NrOfUserAttributes + 2)
            {
                //add the presaved UserId
                string value1 = Excel1.ReadCell(currentRow, 1, excelSheet);
                command1.Parameters.AddWithValue("@UserId", int.Parse(value1));

                //Add the Column/Attribute Name
                value1 = Excel1.ReadCell(1, currentColumn, excelSheet);
                command1.Parameters.AddWithValue("@Name", value1);

                //Add the Attribute Value
                value1 = Excel1.ReadCell(currentRow, currentColumn, excelSheet);
                command1.Parameters.AddWithValue("@Value", value1);

                command1.ExecuteNonQuery();

                currentColumn++;
            }

            con1.Close();
        }

        public string GetUserNr(int currentRow, int currentColumn, int excelSheet)
        {
            return Excel1.ReadCell(currentRow, currentColumn, excelSheet);
        }

        public string GetValue(int currentRow, int currentColumn, int excelSheet)
        {
            return Excel1.ReadCell(currentRow, currentColumn, excelSheet);
        }

        public void InsertPriorities(int currentColumn, string currentUserId, string value1, int shiftPriorities)
        {
            con1.Open();
            command1 = new SqlCommand("INSERT INTO Priorities (UserId, SubjectId, Priority) VALUES (@UserId, @SubjectId, @Priority)", con1);
            command1.Parameters.AddWithValue("@UserId", int.Parse(currentUserId));
            command1.Parameters.AddWithValue("@SubjectId", int.Parse(value1));
            command1.Parameters.AddWithValue("@Priority", currentColumn - 2 - shiftPriorities - Globals.NrOfUserAttributes);
            command1.ExecuteNonQuery();
            con1.Close();
        }
    }
}
