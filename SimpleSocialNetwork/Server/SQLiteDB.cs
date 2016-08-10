using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SharedResources;

namespace Program
{
    class SQLiteDB
    {
        private SQLiteConnection DBconnection = null;
        private SQLiteCommand query = null;
        private string dB_file;

        internal SQLiteDB(string file)
        {
                dB_file = file;
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

            query = new SQLiteCommand();
            query.Connection = DBconnection;

            query.CommandText = "SELECT username FROM User";

            try
            {
                query.ExecuteScalar();
            }
            catch(Exception)
            {
                // Tables does not exist -> create the tables 
                query.CommandText = "CREATE TABLE User(" + 
                                    "id_user INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    "username TEXT NOT NULL UNIQUE," +
                                    "password TEXT NOT NULL," +
                                    "confirmation_code TEXT" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE Contact(" +
                                    "id_user INTEGER NOT NULL PRIMARY KEY UNIQUE," +
                                    "mail TEXT NOT NULL," +
                                    "name TEXT," +
                                    "surname TEXT," +
                                    "FOREIGN KEY(`id_user`) REFERENCES User(id_user)" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE Info(" +
                                    "id_user INTEGER NOT NULL PRIMARY KEY UNIQUE," +
                                    "about TEXT," +
                                    "interests TEXT," +
                                    "FOREIGN KEY(`id_user`) REFERENCES User(id_user)" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE Relation(" +
                                    "id_relationship INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    "id_user1 INTEGER NOT NULL," +
                                    "id_user2 INTEGER NOT NULL," +
                                    "FOREIGN KEY(`id_user1`) REFERENCES User(id_user)," +
                                    "FOREIGN KEY(`id_user2`) REFERENCES User(id_user)" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE Ip(" +
                                    "id_ip INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    "id_user INTEGER NOT NULL," +
                                    "ip_addr TEXT NOT NULL," +
                                    "name TEXT," +
                                    "FOREIGN KEY(`id_user`) REFERENCES User(id_user)" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE Event(" +
                                    "id_user INTEGER NOT NULL," +
                                    "text TEXT," +
                                    "date TEXT," +
                                    "FOREIGN KEY(`id_user`) REFERENCES User(id_user)" +
                                    ")";
                query.ExecuteNonQuery();
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

            try
            {
                query.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                Console.WriteLine("Could not add new user. The database has been configured incorrectly.");
            }
        }

        internal bool EntryExistsInTable(string entry, string table, string column)
        {

            query = new SQLiteCommand();
            query.Connection = DBconnection;

            SQLiteParameter param0 = new SQLiteParameter("@ENTRY", DbType.String) { Value = entry };

            query.Parameters.Add(param0);

            query.CommandText = "SELECT count(*) FROM " + table + " WHERE " + column + " = @ENTRY";

            object obj = query.ExecuteScalar();
            int occurrences = Convert.ToInt32(obj);

            if (occurrences > 0)
                return true;
            else
                return false;
        }

        internal List<String> GetAllUsernames()
        {
            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = "SELECT username FROM User";

            string text = query.CommandText;

            SQLiteDataReader reader = query.ExecuteReader();

            List<String> all_users = new List<string>();


            while(reader.Read())
            {
                all_users.Add(reader.GetString(0));
            }


            reader.Close();
            return all_users;
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

        public List<UserEvent> GetAllEventsFromUser( string username)
        {
            //temp code...
            List<UserEvent> test = new List<UserEvent>();

            for(int i=0; i<10; i++)
            {
                UserEvent nextEvent = new UserEvent();
                nextEvent.text = String.Format("{0} has done some amazing shit this day...", username, i);
                nextEvent.time = new DateTime(2016, 8, i);
                test.Add(nextEvent);
            }

            return test;
        }

        internal List<string> GetFriends(string username)
        {
            throw new NotImplementedException();
        }

        internal bool CheckFriendStatus(string username1, string username2)
        {
            return false;
        }

        internal void AddFriendRelation(string requester, string responder)
        {
            return;
        }
    }
}
