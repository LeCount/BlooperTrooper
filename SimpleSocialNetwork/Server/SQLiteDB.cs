using System;
using System.Data.SQLite;

namespace ServerProgram
{
    class SQLiteDB
    {
        private SQLiteConnection DBconnection = null;
        private string dB_file;

        public SQLiteDB(string fiel)
        {
            try
            {
                dB_file = fiel;
                DBconnection = new SQLiteConnection("Data Source = serverDB.db;Version=3;");
                DBconnection.Open();
            }
            catch(Exception)
            {

            }
}
    }
}
