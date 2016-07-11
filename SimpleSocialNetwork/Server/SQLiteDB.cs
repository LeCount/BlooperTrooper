using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SharedResources;

namespace ServerProgram
{
    class SQLiteDB
    {
        private SQLiteConnection DBconnection = null;
        private SQLiteCommand query = null;
        private string dB_file;

        internal SQLiteDB(string fiel)
        {
                dB_file = fiel;
                Connect();
        }

        internal void Connect()
        {
            String connection_string = String.Format("Data Source=" + dB_file.ToString() + ";Version=3;");

            try
            {
                DBconnection = new SQLiteConnection(connection_string);
                DBconnection.Open();
            }
            catch(Exception)
            {
                Console.WriteLine("Could not open database: " + dB_file.ToString());
                Console.ReadLine();
                System.Environment.Exit(0);
            }
        }

        public void Disconnect()
        {
            DBconnection.Close();
        }

        internal int GetUserId(String username)
        {
            int id = -1;

            if(EntryExistsInTable(username, "User", "username"))
            {
                query = new SQLiteCommand();
                query.Connection = DBconnection;

                query.CommandText = "SELECT user_id, * FROM User WHERE username = " + username;
                string text = query.CommandText;

                object obj = query.ExecuteNonQuery();
                id = Convert.ToInt32(obj);

                return id;
            }
            else
                return - 1;
        }

        internal void AddNewUser(string suggested_username, string suggested_password, string code)
        {
            query = new SQLiteCommand();
            query.Connection = DBconnection;

            SQLiteParameter param1 = new SQLiteParameter("@USER", DbType.String) { Value = suggested_username };
            SQLiteParameter param2 = new SQLiteParameter("@PASSWORD", DbType.String) { Value = suggested_password };
            SQLiteParameter param3 = new SQLiteParameter("@CODE", DbType.String) { Value = code };

            query.Parameters.Add(param1);
            query.Parameters.Add(param2);
            query.Parameters.Add(param3);

            query.CommandText = "INSERT INTO User(username, password, confirmation_code) VALUES(@USER, @PASSWORD, @CODE)";

            string text = query.CommandText;

            query.ExecuteNonQuery();
        }

        internal bool EntryExistsInTable(string entry, string table, string column)
        {

            query = new SQLiteCommand();
            query.Connection = DBconnection;

            SQLiteParameter param = new SQLiteParameter("@ENTRY", DbType.String) { Value = entry };

            query.Parameters.Add(param);

            query.CommandText = "SELECT COUNT (" + column.ToString() + ") FROM " + table.ToString() + " WHERE " + column.ToString() + " = @ENTRY";

            string text = query.CommandText;

            object obj = query.ExecuteScalar();
            int occurrences = Convert.ToInt32(obj);

            if (occurrences > 0)
                return false;
            else
                return true;
        }

        internal string GetMail(string username)
        {
            throw new NotImplementedException();
        }

        internal string GetName(string username)
        {
            throw new NotImplementedException();
        }

        internal string GetSurname(string username)
        {
            throw new NotImplementedException();
        }

        internal string GetAbout(string username)
        {
            throw new NotImplementedException();
        }

        internal string GetInterest(string username)
        {
            throw new NotImplementedException();
        }

        internal List<UserEvent> GetEvents(string username)
        {
            throw new NotImplementedException();
        }

        internal List<string> GetFriends(string username)
        {
            throw new NotImplementedException();
        }
    }
}
