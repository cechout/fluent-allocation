using System.Data.SqlClient;
using System.IO;

namespace Kurszuteilung.Classes
{
    class Functions
    {
        public Functions()
        {
            string path1 = PrepareWorkingDatabase();
            string connectionString1 = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={path1};Integrated Security=True";

            con1 = new(connectionString1);
        }

        // the database the app works in is a copy per windows user under %LocalAppData%, taken from the empty
        // template next to the executable the first time it is needed
        //
        // the template itself is never attached: next to an installed app it sits in Program Files, where a normal
        // user cannot write, and LocalDB writes to every file it attaches, an older file is even upgraded to its own
        // version on the first attach
        private static string PrepareWorkingDatabase()
        {
            //main_database properties
            //--> build-action: content
            //--> copy to output directory: copy always/copy if never

            //AppDomain.CurrentDomain.BaseDirectory returns the base directory of the project
            //which is "C:\...\Evaluate Subjects 2\Evaluate Subjects 2\bin\Debug\net8.0-windows"
            string templateFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            string workingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kurszuteilung", "Database");
            string workingDatabase = Path.Combine(workingFolder, "main_database.mdf");

            // copied only while there is no working copy yet, and then as a pair; once LocalDB has attached a file
            // under its path, replacing that file makes the next attach fail, so an existing copy is never touched
            if (!File.Exists(workingDatabase))
            {
                Directory.CreateDirectory(workingFolder);
                File.Copy(Path.Combine(templateFolder, "main_database.mdf"), workingDatabase);
                File.Copy(Path.Combine(templateFolder, "main_database_log.ldf"), Path.Combine(workingFolder, "main_database_log.ldf"), true);
            }

            return workingDatabase;
        }

        public SqlConnection con1;

        public SqlCommand command1;
        public SqlDataReader dataReader1;
        public SqlCommand command2;
        public SqlDataReader dataReader2;
    }
}
