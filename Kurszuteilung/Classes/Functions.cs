using System.Data.SqlClient;
using System.IO;

namespace Kurszuteilung.Classes
{
    class Functions
    {
        public Functions()
        {
            //main_database properties
            //--> build-action: content
            //--> copy to output directory: copy always/copy if never

            //AppDomain.CurrentDomain.BaseDirectory returns the base directory of the project
            //which is "C:\...\Evaluate Subjects 2\Evaluate Subjects 2\bin\Debug\net8.0-windows"
            string path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Database\main_database.mdf");
            string connectionString1 = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={path1};Integrated Security=True";

            con1 = new(connectionString1);
        }

        public SqlConnection con1;

        public SqlCommand command1;
        public SqlDataReader dataReader1;
        public SqlCommand command2;
        public SqlDataReader dataReader2;
    }
}
