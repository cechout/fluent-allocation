using System.Data.SqlClient;

namespace Kurszuteilung.Classes
{
    class Functions3 : Functions
    {
        public Excel Excel1 = new();

        public int CurrentRow;
        public int CurrentColumn;

        public void CreateNewFile(bool groupByUserAttribute, List<int> userIdsNotSorted)
        {
            Excel1 = new Excel();
            Excel1.CreateNewFile();
            Excel1.CreateNewSheet();
            Globals.NumOfSheets = 2;

            if (groupByUserAttribute == true || userIdsNotSorted.Count > 0)
            {
                Excel1.CreateNewSheet();
                Globals.NumOfSheets = 3;
            }

            if (groupByUserAttribute == true && userIdsNotSorted.Count > 0)
            {
                Excel1.CreateNewSheet();
                Globals.NumOfSheets = 4;
            }
        }

        public void WriteInExistingFile(bool groupByUserAttribute, List<int> userIdsNotSorted)
        {
            Excel1 = new Excel(Globals.PathSavedFile);

            if (groupByUserAttribute == false && userIdsNotSorted.Count == 0)
            {
                Excel1.ResetFile(2);
                Globals.NumOfSheets = 2;
            }

            else if (groupByUserAttribute == false || userIdsNotSorted.Count == 0)
            {
                Excel1.ResetFile(3);
                Globals.NumOfSheets = 3;
            }

            else if (groupByUserAttribute == true && userIdsNotSorted.Count > 0)
            {
                Excel1.ResetFile(4);
                Globals.NumOfSheets = 4;
            }
        }

        public void SaveNewFile(int numOfUserIdsNotSorted)
        {
            Excel1.SaveAs(Globals.PathSavedFile, numOfUserIdsNotSorted);
        }

        public void SaveExistingFile(int numOfUserIdsNotSorted)
        {
            Excel1.Save(numOfUserIdsNotSorted);
        }

        public void CloseExcel()
        {
            Excel1.Close();
        }

        public List<string> GetUserAttributes()
        {
            List<string> attributeNames = new();

            command1 = new SqlCommand("SELECT DISTINCT (Name) FROM UserAttributes", con1);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                attributeNames.Add(dataReader1.GetString(0));
            }

            dataReader1.Close();

            return attributeNames;
        }

        public List<int> GetSortUserIds()
        {
            List<int> sortUserIds = new();

            con1.Open();
            command1 = new SqlCommand("SELECT Id FROM Users", con1);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                sortUserIds.Add(dataReader1.GetInt32(0));
            }

            dataReader1.Close();
            con1.Close();

            return sortUserIds;
        }

        public List<int> GetGroupIds(int currentSubjectId)
        {
            List<int> groupIds = new();

            con1.Open();
            command1 = new SqlCommand("SELECT Id FROM Users INNER JOIN UserSubjectAssignments ON Users.Id = UserSubjectAssignments.UserId WHERE UserSubjectAssignments.SubjectId = @SubjectId", con1);
            command1.Parameters.AddWithValue("@SubjectId", currentSubjectId);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                groupIds.Add(dataReader1.GetInt32(0));
            }

            dataReader1.Close();
            con1.Close();

            return groupIds;
        }

        public List<string> GetSubjectsIds()
        {
            List<string> subjectIds = new();

            con1.Open();
            command1 = new SqlCommand("SELECT Id FROM Subjects", con1);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                subjectIds.Add(dataReader1.GetInt32(0).ToString());
            }

            dataReader1.Close();
            con1.Close();

            return subjectIds;
        }

        public void SetGeneralHeaders(int currentRow, int currentColumn, int currentSubjectId, int headerMode, bool withSubjects, string currentAttribute, int excelSheet)
        {
            int startColumn = currentColumn;

            con1.Open();

            //honestly, I dont really know why this list is at this point, but it works :)
            List<string> attributeNames;

            //Set name of specific subject as header
            if (headerMode == 1)
            {
                command1 = new SqlCommand("SELECT Name FROM Subjects WHERE Id = @Id", con1);
                command1.Parameters.AddWithValue("@Id", currentSubjectId);
                dataReader1 = command1.ExecuteReader();

                while (dataReader1.Read())
                {
                    Excel1.WriteCell(currentRow, 1, dataReader1.GetString(0), excelSheet);

                    //subheader is one row below main header
                    currentRow += 1;
                }

                dataReader1.Close();
            }

            //Set name of specific attribute as header
            if (headerMode == 2)
            {
                Excel1.WriteCell(currentRow, 1, Globals.ProcessAttribute + " " + currentAttribute, excelSheet);

                //subheader is one row below main header
                currentRow += 1;
            }

            //--subheading construction (general purpose subheader)--
            Excel1.WriteCell(currentRow, currentColumn - 2, "Id", excelSheet);
            Excel1.WriteCell(currentRow, currentColumn - 1, "Name", excelSheet);

            //Set Attribute Names
            attributeNames = GetUserAttributes();

            while (currentColumn <= Globals.NrOfUserAttributes + 2)
            {
                Excel1.WriteCell(currentRow, currentColumn, attributeNames[currentColumn - 3], excelSheet);

                currentColumn++;
            }

            //Set subjects (not required in sheet nr2 where you group by subjects)
            if (withSubjects)
            {
                int subjectCounter = 1;

                while (currentColumn - 2 - Globals.NrOfUserAttributes <= Globals.NrOfRequiredSubjects)
                {
                    Excel1.WriteCell(currentRow, currentColumn, "Subject" + subjectCounter, excelSheet);
                    subjectCounter++;
                    currentColumn++;
                }
            }

            CurrentRow = currentRow;

            con1.Close();
        }

        public void InsertUsersInExcel(int currentRow, int currentColumn, List<int> sortIds, bool withSubjects, int excelSheet)
        {
            int currentStartColumn = currentColumn;

            con1.Open();

            foreach (int currentUserId in sortIds)
            {
                //Set Id and name
                command1 = new SqlCommand("SELECT Name FROM Users WHERE Id = @Id", con1);
                command1.Parameters.AddWithValue("@Id", currentUserId);
                dataReader1 = command1.ExecuteReader();

                while (dataReader1.Read())
                {
                    Excel1.WriteCell(currentRow, 1, currentUserId.ToString(), excelSheet);
                    Excel1.WriteCell(currentRow, 2, dataReader1.GetString(0), excelSheet);
                }

                dataReader1.Close();

                //Set Attributes
                command1 = new SqlCommand("SELECT Value FROM UserAttributes WHERE UserId = @UserId", con1);
                command1.Parameters.AddWithValue("@UserId", currentUserId);
                dataReader1 = command1.ExecuteReader();

                while (dataReader1.Read())
                {
                    Excel1.WriteCell(currentRow, currentColumn, dataReader1.GetString(0), excelSheet);
                    currentColumn++;
                }

                dataReader1.Close();

                if (withSubjects)
                {
                    //Set subjects
                    command1 = new SqlCommand("SELECT Subjects.Name FROM Subjects INNER JOIN UserSubjectAssignments On Subjects.Id = UserSubjectAssignments.SubjectId WHERE UserSubjectAssignments.UserId = @UserId", con1);
                    command1.Parameters.AddWithValue("@UserId", currentUserId);
                    dataReader1 = command1.ExecuteReader();

                    while (dataReader1.Read())
                    {
                        Excel1.WriteCell(currentRow, currentColumn, dataReader1.GetString(0), excelSheet);
                        currentColumn++;
                    }

                    dataReader1.Close();
                }

                currentRow++;
                currentColumn = currentStartColumn;
            }

            CurrentRow = currentRow;
            CurrentColumn = currentColumn;

            dataReader1.Close();
            con1.Close();
        }

        public List<string> GetAttributeValues()
        {
            List<string> attributeValues = new();

            con1.Open();
            command1 = new SqlCommand("SELECT DISTINCT(Value) FROM UserAttributes WHERE Name = @AttributeName", con1);
            command1.Parameters.AddWithValue("@AttributeName", Globals.ProcessAttribute);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                attributeValues.Add(dataReader1.GetString(0));
            }

            dataReader1.Close();
            con1.Close();

            return attributeValues;
        }

        public List<int> GetGroupIdsWhereUserAttribute(string currentAttribute)
        {
            List<int> groupIdsWhereUserAttribute = new();

            con1.Open();
            command1 = new SqlCommand("SELECT Id FROM Users INNER JOIN UserAttributes ON Users.Id = UserAttributes.UserId WHERE UserAttributes.Name = @AttributeName AND UserAttributes.Value = @AttributeValue", con1);
            command1.Parameters.AddWithValue("AttributeName", Globals.ProcessAttribute);
            command1.Parameters.AddWithValue("AttributeValue", currentAttribute);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                groupIdsWhereUserAttribute.Add(dataReader1.GetInt32(0));
            }

            dataReader1.Close();
            con1.Close();

            return groupIdsWhereUserAttribute;
        }
    }
}
