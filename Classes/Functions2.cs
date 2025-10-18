using System.Data.SqlClient;

namespace Kurszuteilung.Classes
{
    class Functions2 : Functions
    {
        public List<int> GetUserIdsThatNeedSubject(int NrOfRequiredSubjects)
        {
            con1.Open();

            List<int> userIdsThatNeedSubject = new();

            command2 = new SqlCommand("SELECT UserId FROM (SELECT Users.Id as UserId, ISNULL(count(UserSubjectAssignments.UserId), 0) as assignedSubjects FROM Users LEFT JOIN UserSubjectAssignments ON Users.Id = UserSubjectAssignments.UserId GROUP BY Users.Id) AS SubjectAssignments WHERE assignedSubjects < @NrOfRequiredSubjects", con1);
            command2.Parameters.AddWithValue("@NrOfRequiredSubjects", NrOfRequiredSubjects);
            dataReader2 = command2.ExecuteReader();

            while (dataReader2.Read())
            {
                userIdsThatNeedSubject.Add(dataReader2.GetInt32(0));
            }

            dataReader2.Close();
            con1.Close();

            return userIdsThatNeedSubject;
        }

        public int GetCapacityOfSubject(int currentSubjectId)
        {
            con1.Open();

            int capacityOfCurrentSubject = 0;

            command1 = new SqlCommand("SELECT Capacity FROM Subjects WHERE Id = @Id", con1);
            command1.Parameters.AddWithValue("@Id", currentSubjectId);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                capacityOfCurrentSubject = dataReader1.GetInt32(0);
            }

            dataReader1.Close();
            con1.Close();

            return capacityOfCurrentSubject;
        }

        public List<int> CheckForDublicateSubject(List<int> userIdsThatNeedSubject, int currentSubjectId)
        {
            con1.Open();

            List<int> toBeRemovedUserIds = new();

            foreach (int currentUserId in userIdsThatNeedSubject)
            {
                command1 = new SqlCommand("SELECT SubjectId FROM UserSubjectAssignments WHERE UserId = @UserId", con1);
                command1.Parameters.AddWithValue("@UserId", currentUserId);
                dataReader1 = command1.ExecuteReader();

                while (dataReader1.Read())
                {
                    if (dataReader1.GetInt32(0) == currentSubjectId)
                    {
                        toBeRemovedUserIds.Add(currentUserId);
                    }
                }

                dataReader1.Close();
            }

            foreach (int currentUserId in toBeRemovedUserIds)
            {
                userIdsThatNeedSubject.Remove(currentUserId);
            }

            toBeRemovedUserIds.Clear();

            con1.Close();

            return userIdsThatNeedSubject;
        }

        public List<int> GetWhichUsersChosedSubject(List<int> userIdsThatNeedSubject, int currentSubjectId, int currentPriority)
        {
            con1.Open();
            List<int> userIdsForCurrentSubject = new();

            command1 = new SqlCommand("SELECT UserId FROM Priorities WHERE SubjectId = @SubjectId AND Priority = @Priority", con1);
            command1.Parameters.AddWithValue("@SubjectId", currentSubjectId);
            command1.Parameters.AddWithValue("@Priority", currentPriority);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                //Check which Users actually need an additional Subject
                foreach (int currentUserId in userIdsThatNeedSubject)
                {
                    if (currentUserId == dataReader1.GetInt32(0))
                    {
                        userIdsForCurrentSubject.Add(dataReader1.GetInt32(0));
                    }
                }
            }

            dataReader1.Close();
            con1.Close();

            return userIdsForCurrentSubject;
        }

        public void AddUsersToUserSubjectAssignments(List<int> list1, int currentSubjectId)
        {
            con1.Open();

            foreach (int currentUserId in list1)
            {
                command1 = new SqlCommand("INSERT INTO UserSubjectAssignments (UserId, SubjectId) VALUES (@UserId, @SubjectId)", con1);
                command1.Parameters.AddWithValue("@UserId", currentUserId);
                command1.Parameters.AddWithValue("@SubjectId", currentSubjectId);
                command1.ExecuteNonQuery();

                command1 = new SqlCommand("UPDATE Subjects SET Capacity = Capacity - 1 WHERE Id = @Id", con1);
                command1.Parameters.AddWithValue("@Id", currentSubjectId);
                command1.ExecuteNonQuery();
            }

            con1.Close();
        }

        public int CheckIfCurrentSubjectIsFull(int currentSubjectId, int NrOfSubjects)
        {
            con1.Open();

            int capacityOfCurrentSubject = 0;
            bool currentSubjectIsFull;
            bool currentSubjectIdIsValid = false;

            List<int> subjectIds = new();

            //Get Subject Ids
            command1 = new SqlCommand("SELECT Id FROM Subjects", con1);
            dataReader1 = command1.ExecuteReader();

            while (dataReader1.Read())
            {
                subjectIds.Add(dataReader1.GetInt32(0));
            }

            dataReader1.Close();

            do
            {
                while (currentSubjectIdIsValid == false && currentSubjectId <= subjectIds[subjectIds.Count - 1])
                {
                    currentSubjectId++;

                    foreach (int currentSubjectIdInSubjectIds in subjectIds)
                    {
                        if (currentSubjectId == currentSubjectIdInSubjectIds)
                        {
                            currentSubjectIdIsValid = true;
                        }
                    }
                }

                currentSubjectIdIsValid = false;

                command1 = new SqlCommand("SELECT Capacity FROM Subjects WHERE Id = @Id", con1);
                command1.Parameters.AddWithValue("@Id", currentSubjectId);
                dataReader1 = command1.ExecuteReader();

                while (dataReader1.Read())
                {
                    capacityOfCurrentSubject = dataReader1.GetInt32(0);
                }

                dataReader1.Close();

                if (capacityOfCurrentSubject == 0)
                {
                    currentSubjectIsFull = true;
                }
                else
                {
                    currentSubjectIsFull = false;
                }
            }
            while (currentSubjectIsFull && currentSubjectId < NrOfSubjects);

            con1.Close();

            return currentSubjectId;
        }
    }
}
