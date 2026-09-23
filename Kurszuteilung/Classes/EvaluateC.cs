namespace Kurszuteilung.Classes
{
    class EvaluateC
    {
        //call the constructor of the Functions class to set the database path
        public Functions _Functions = new();

        public Functions1 Functions1 = new();
        public Functions2 Functions2 = new();
        public Functions3 Functions3 = new();
        public MainWindow MainWindow1 = new();

        List<int> userIdsNotSorted = new();

        public void ReadFromExcel()
        {
            //Reset Database
            Functions1.ResetDatabase();

            //WriteSubjectsToDb
            Functions1.WriteSubjectsToDatabase(2);

            //write users to database
            int currentRow = 2;
            int currentColumn = 1;

            while (currentRow <= Globals.NrOfUsers + 1)
            {
                Functions1.InsertUsers(currentRow, currentColumn, 1);

                currentColumn = Functions1.CurrentColumn;
                Functions1.InsertUserAttributes(currentRow, currentColumn, 1);

                currentRow++;
                currentColumn = 1;
            }

            //write priorities to database
            currentRow = 2;
            currentColumn = 1;

            string currentUserId;
            bool value1IsValid = true;
            int shiftPriorities = 0;
            string value1;

            List<string> tempPriorities = new();

            while (currentRow <= Globals.NrOfUsers + 1)
            {
                currentUserId = Functions1.GetUserNr(currentRow, currentColumn, 1);

                //skip 2 columns to get from UserId to first UserAttribute
                currentColumn += 2 + Globals.NrOfUserAttributes;

                tempPriorities.Clear();

                while (currentColumn <= Globals.NrOfPrios + Globals.NrOfUserAttributes + 2)
                {
                    value1 = Functions1.GetValue(currentRow, currentColumn, 1);

                    //test for duplicate value within one user
                    foreach (string currentTempPrioritiesValue in tempPriorities)
                    {
                        if (int.Parse(currentTempPrioritiesValue) == int.Parse(value1))
                        {
                            value1IsValid = false;
                        }
                    }

                    tempPriorities.Add(value1);

                    //test for validity
                    if (int.Parse(value1) > Globals.NrOfSubjects || int.Parse(value1) < 1)
                    {
                        value1IsValid = false;
                    }

                    if (value1IsValid == true)
                    {
                        Functions1.InsertPriorities(currentColumn, currentUserId, value1, shiftPriorities);
                    }
                    else
                    {
                        //dont insert row, shift every other row -1
                        shiftPriorities++;
                    }

                    currentColumn++;
                    value1IsValid = true;
                }

                shiftPriorities = 0;
                currentRow++;
                currentColumn = 1;
            }
        }

        public void SortUsers()
        {
            int currentSubjectId = 1;
            bool sortUsers = true;

            List<int> userIdsForCurrentSubject;
            List<int> chosedUserIds = new();
            List<int> userIdsThatNeedSubject;
            List<string> subjectIds;

            int capacityOfCurrentSubject;
            int currentPriority = 1;

            Random random1 = new();
            int randomNumber1;

            //Get Subject Ids
            subjectIds = Functions3.GetSubjectsIds();

            userIdsThatNeedSubject = Functions2.GetUserIdsThatNeedSubject(Globals.NrOfRequiredSubjects);

            //loop through every priority
            while ((currentPriority <= Globals.NrOfPrios || userIdsThatNeedSubject.Count != 0) && sortUsers == true)
            {
                //loop through every subject (per priority)
                while (currentSubjectId <= int.Parse(subjectIds[subjectIds.Count - 1]) && userIdsThatNeedSubject.Count != 0)
                {
                    //Get Capacity of Subject
                    capacityOfCurrentSubject = Functions2.GetCapacityOfSubject(currentSubjectId);

                    //check if a User in userIdsThatNeedSubject already has this SubjectId
                    userIdsThatNeedSubject = Functions2.CheckForDublicateSubject(userIdsThatNeedSubject, currentSubjectId);

                    //Get which Users chosed Subject, get userIdsForCurrentSubject
                    userIdsForCurrentSubject = Functions2.GetWhichUsersChosedSubject(userIdsThatNeedSubject, currentSubjectId, currentPriority);

                    //switch the lists for random evaluation
                    if (userIdsForCurrentSubject.Count == 0 && currentPriority > Globals.NrOfPrios)
                    {
                        //userIdsForCurrentSubject = userIdsThatNeedSubject; //new code
                        sortUsers = false;
                        userIdsNotSorted = userIdsThatNeedSubject;
                    }

                    //if number of Users > capacity of the Subject, randomly select Users till subject if full
                    if (userIdsForCurrentSubject.Count >= capacityOfCurrentSubject)
                    {
                        while (chosedUserIds.Count < capacityOfCurrentSubject)
                        {
                            randomNumber1 = userIdsForCurrentSubject[random1.Next(userIdsForCurrentSubject.Count)];

                            if (!chosedUserIds.Contains(randomNumber1))
                            {
                                chosedUserIds.Add(randomNumber1);
                                userIdsForCurrentSubject.Remove(randomNumber1);
                            }
                        }

                        //Add chosedUserIds to UserSubjectAssignments table
                        Functions2.AddUsersToUserSubjectAssignments(chosedUserIds, currentSubjectId);
                    }

                    //if the number of Users < capacity of the Subject
                    else
                    {
                        //Add chosedUserIds to UserSubjectAssignments table
                        Functions2.AddUsersToUserSubjectAssignments(userIdsForCurrentSubject, currentSubjectId);
                    }

                    chosedUserIds.Clear();
                    userIdsForCurrentSubject.Clear();

                    currentSubjectId = Functions2.CheckIfCurrentSubjectIsFull(currentSubjectId, Globals.NrOfSubjects);

                    userIdsThatNeedSubject = Functions2.GetUserIdsThatNeedSubject(Globals.NrOfRequiredSubjects);
                }

                currentSubjectId = 0;
                currentPriority++;

                currentSubjectId = Functions2.CheckIfCurrentSubjectIsFull(currentSubjectId, Globals.NrOfSubjects);
            }
        }

        public void WriteToExcel()
        {
            //Startposition for the first subheader
            int startRow = 2;
            int startColumn = 3;

            List<int> sortUserIds;

            //Write in New File
            if (Globals.SaveLocationMode == 1)
            {
                Functions3.CreateNewFile(Globals.GroupByUserAttribute, userIdsNotSorted);
            }

            //Write in Existing File
            if (Globals.SaveLocationMode == 2)
            {
                Functions3.WriteInExistingFile(Globals.GroupByUserAttribute, userIdsNotSorted);
            }

            //Sort User Ids
            Functions3.SetGeneralHeaders(1, startColumn, 1, 0, true, "", 1);

            sortUserIds = Functions3.GetSortUserIds();
            Functions3.InsertUsersInExcel(startRow, startColumn, sortUserIds, true, 1);

            //Sort Subject Ids
            SortByGrouping(1, false, 2); //ExcelSheet originally: 2

            //Group By UserAttribute
            if (Globals.GroupByUserAttribute == true)
            {
                SortByGrouping(2, true, 3);
            }

            //List the not sorted Users
            if (userIdsNotSorted.Count > 0)
            {
                Functions3.SetGeneralHeaders(1, startColumn, 1, 0, true, "", Globals.NumOfSheets);
                Functions3.InsertUsersInExcel(startRow, startColumn, userIdsNotSorted, true, Globals.NumOfSheets);
            }

            //Save in New File
            if (Globals.SaveLocationMode == 1)
            {
                Functions3.SaveNewFile(userIdsNotSorted.Count);
            }

            //Save in Existing File
            if (Globals.SaveLocationMode == 2)
            {
                Functions3.SaveExistingFile(userIdsNotSorted.Count);
            }

            Functions3.CloseExcel();
        }

        private void SortByGrouping(int getAttributeValuesMode, bool withSubjects, int excelSheet)
        {
            List<string> attributeValues;
            List<int> groupIdsWhereAttributeValue;

            //Where to get the attributeValues from, because subjects are kinda attributes too
            if (getAttributeValuesMode == 1)
            {
                attributeValues = Functions3.GetSubjectsIds();
            }
            else
            {
                attributeValues = Functions3.GetAttributeValues();
            }

            //startColumn = 3 because the first 2 column are always "Id" and "Name"
            int currentRow = 1;
            int currentColumn = 3;

            foreach (string currentAttributeValue in attributeValues)
            {
                if (getAttributeValuesMode == 1)
                {
                    //Set header (subject) + subheader 
                    Functions3.SetGeneralHeaders(currentRow, currentColumn, int.Parse(currentAttributeValue), 1, false, "", excelSheet);

                    //get the UserIds that have the current attribute (subject)
                    groupIdsWhereAttributeValue = Functions3.GetGroupIds(int.Parse(currentAttributeValue));
                }
                else
                {
                    //Set header + subheader
                    Functions3.SetGeneralHeaders(currentRow, currentColumn, 1, 2, true, currentAttributeValue, excelSheet);

                    //get the UserIds that have the current attribute
                    groupIdsWhereAttributeValue = Functions3.GetGroupIdsWhereUserAttribute(currentAttributeValue);
                }

                //go 1 row below the subheader, reset column
                currentRow = Functions3.CurrentRow + 1;
                currentColumn = 3;

                Functions3.InsertUsersInExcel(currentRow, currentColumn, groupIdsWhereAttributeValue, withSubjects, excelSheet);
                groupIdsWhereAttributeValue.Clear();

                if (Globals.GroupNextToEachOther)
                {
                    //start at top again, 4 next to the other table so there is 1 space between the tables
                    currentRow = 1;
                    currentColumn = Functions3.CurrentColumn + 4;
                }
                else
                {
                    //go 1 row to leave space for the different tables grouped by attributes
                    currentRow = Functions3.CurrentRow + 1;
                    currentColumn = 3;
                }
            }
        }
    }
}
