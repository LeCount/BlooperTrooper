using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SharedResources;

namespace Program
{
    class SQLiteDB
    {
        private const string COL_USER_ID_FK = "id_user";

        private const string TBL_USER = "User";
        private const string COL_USERNAME = "username";
        private const string COL_PASSWORD = "password";
        private const string COL_VERIFICATION = "confirmation_code";

        private const string TBL_CONTACT = "Contact";
        private const string COL_NAME = "name";
        private const string COL_SURENAME = "surname";
        private const string COL_MAIL = "confirmation_code";

        private const string TBL_INFO = "Info";
        private const string COL_ABOUT = "about";
        private const string COL_INTERESTS = "interests";

        private const string TBL_RELATION = "Relation";
        private const string COL_RELATION_ID = "id_relation";
        private const string COL_USER1 = "id_user1";
        private const string COL_USER2 = "id_user2";

        private const string TBL_IP = "Ip";
        private const string COL_IP_ID = "id_ip";
        private const string COL_IP_ADDR = "id_addr";

        private const string TBL_EVENT = "Event";
        private const string COL_POSTER = "id_poster";
        private const string COL_WALL_OWNER = "id_wall_owner";
        private const string COL_TEXT = "text";
        private const string COL_DATE = "date";

        private SQLiteConnection DBconnection = null;
        private SQLiteCommand query = null;
        private string dB_file;

        public SQLiteDB(string file)
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
                Environment.Exit(0);
            }

            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = "SELECT " + COL_USERNAME + " FROM " + TBL_USER;

            try
            {
                query.ExecuteScalar();
            }
            catch(Exception)
            {
                // Tables does not exist -> create the tables 
                query.CommandText = "CREATE TABLE " + TBL_USER + "(" + 
                                    COL_USER_ID_FK + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    COL_USERNAME + " TEXT NOT NULL UNIQUE," +
                                    COL_PASSWORD + " TEXT NOT NULL," +
                                    COL_VERIFICATION + " TEXT" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE " + TBL_CONTACT + "(" +
                                    COL_USER_ID_FK + " INTEGER NOT NULL PRIMARY KEY UNIQUE," +
                                    COL_MAIL + " TEXT NOT NULL," +
                                    COL_NAME + " TEXT," +
                                    COL_SURENAME + " TEXT," +
                                    "FOREIGN KEY(`"+ COL_USER_ID_FK + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE " + TBL_INFO + "(" +
                                    COL_USER_ID_FK + " INTEGER NOT NULL PRIMARY KEY UNIQUE," +
                                    COL_ABOUT + " TEXT," +
                                    COL_INTERESTS + " TEXT," +
                                    "FOREIGN KEY(`" + COL_USER_ID_FK + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE " + TBL_RELATION + "(" +
                                    COL_RELATION_ID + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    COL_USER1 + " INTEGER NOT NULL," +
                                    COL_USER2 + " INTEGER NOT NULL," +
                                    "FOREIGN KEY(`" + COL_USER1 + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")," +
                                    "FOREIGN KEY(`" + COL_USER2 + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE " + TBL_IP + "(" +
                                    COL_IP_ID + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                                    COL_USER_ID_FK + " INTEGER NOT NULL," +
                                    COL_IP_ADDR + " TEXT NOT NULL," +
                                    "FOREIGN KEY(`" + COL_USER_ID_FK + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    ")";
                query.ExecuteNonQuery();

                query.CommandText = "CREATE TABLE " + TBL_EVENT + "(" +
                                    COL_POSTER + " INTEGER NOT NULL," +
                                    COL_WALL_OWNER + " INTEGER NOT NULL," +
                                    COL_TEXT + " TEXT," +
                                    COL_DATE + " TEXT," +
                                    "FOREIGN KEY(`" + COL_POSTER + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    "FOREIGN KEY(`" + COL_WALL_OWNER + "`) REFERENCES " + TBL_USER + "(" + COL_USER_ID_FK + ")" +
                                    ")";
                query.ExecuteNonQuery();
            }
        }

        internal void Disconnect()
        {
            try { DBconnection.Close(); }
            catch (Exception) { }
        }

        internal bool EntryExistsInTable(string entry, string table, string column)
        {
            query = new SQLiteCommand();
            query.Connection = DBconnection;

            SQLiteParameter param0 = new SQLiteParameter("@ENTRY", DbType.String) { Value = entry };
            query.Parameters.Add(param0);
            query.CommandType = CommandType.Text;

            query.CommandText = "SELECT count(*) FROM " + table + " WHERE " + column + " = @ENTRY";

            object obj = query.ExecuteScalar();
            int occurrences = Convert.ToInt32(obj);

            if (occurrences > 0)
                return true;
            else
                return false;
        }

        internal string GetUsername(int id)
        {
            string username = null;

            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = string.Format("SELECT {0} FROM {1} WHERE {2} = {3}",
                COL_USERNAME,
                TBL_USER,
                COL_USER_ID_FK,
                id);

            object obj = query.ExecuteScalar();
            username = Convert.ToString(obj);
            return username;
        }

        internal int GetUserId(String username)
        {
            int user_id;

            if (EntryExistsInTable(username, TBL_USER, COL_USERNAME))
            {
                query = new SQLiteCommand();
                query.Connection = DBconnection;
                query.CommandType = CommandType.Text;

                query.CommandText = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'",
                    COL_USER_ID_FK,
                    TBL_USER,
                    COL_USERNAME,
                    username
                    );

                object obj = query.ExecuteScalar();
                user_id = Convert.ToInt32(obj);
                return user_id;
            }
            else
                return -1;
        }

        internal void AddNewUser(string username, string password, string verification_code)
        {
            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            SQLiteParameter param1 = new SQLiteParameter("@USER", DbType.String) { Value = username };
            SQLiteParameter param2 = new SQLiteParameter("@PASSWORD", DbType.String) { Value = password };
            SQLiteParameter param3 = new SQLiteParameter("@CODE", DbType.String) { Value = verification_code };

            query.Parameters.Add(param1);
            query.Parameters.Add(param2);
            query.Parameters.Add(param3);

            query.CommandText = string.Format("INSERT INTO {0}({1}, {2}, {3}) VALUES(@USER, @PASSWORD, @CODE)",
                TBL_USER,
                COL_USERNAME,
                COL_PASSWORD,
                COL_VERIFICATION
                );

            try{query.ExecuteNonQuery();}
            catch (SQLiteException){Console.WriteLine("[Error] DB could not add new user. The database has been configured incorrectly.");}
        }

        internal List<String> GetAllUsernames()
        {
            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = 
                string.Format("SELECT {0} FROM {1}",
                COL_USERNAME,
                TBL_USER
                );

            SQLiteDataReader reader = query.ExecuteReader();

            List<String> all_users = new List<string>();

            while(reader.Read())
            {
                all_users.Add(reader.GetString(0));
            }


            reader.Close();
            return all_users;
        }

        internal void AddWallPost(string poster, string wall_owner, string text)
        {
            int poster_id = GetUserId(poster);
            int wall_owner_id = GetUserId(wall_owner);

            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText =
                string.Format("INSERT INTO " + TBL_EVENT + "({0}, {1}, {2}, {3}) VALUES('{4}', '{5}', '{6}', '{7}')",
                COL_POSTER,
                COL_WALL_OWNER,
                COL_TEXT, COL_DATE,
                poster_id,
                wall_owner_id,
                text,
                DateTime.Now.ToString()
                );

            query.ExecuteNonQuery();
        }

        internal List<WallPost> GetAllEventsFromUser( string username)
        {
            int id_owner_of_wall = GetUserId(username);

            List<WallPost> all_events = new List<WallPost>();

            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = 
                string.Format("SELECT * FROM {0} WHERE {1} = {2}", 
                TBL_EVENT,
                COL_WALL_OWNER,
                id_owner_of_wall
                );

            SQLiteDataReader reader = query.ExecuteReader();
            List<String> all_event_texts = new List<string>();

            while (reader.Read())
            {
                WallPost wp = new WallPost();

                wp.writer = GetUsername(Convert.ToInt32(reader[COL_POSTER])); 
                wp.owner = GetUsername(Convert.ToInt32(reader[COL_WALL_OWNER])); 
                wp.text = Convert.ToString(reader[COL_TEXT]);  
                wp.time = Convert.ToDateTime(reader[COL_DATE]); 

                all_events.Add(wp);
            }

            reader.Close();
            return all_events;
        }

        internal void AddFriendRelation(string requester, string responder)
        {
            if (!FriendRelationExists(GetUserId(requester), GetUserId(responder)))
            {
                int id_requester = GetUserId(requester);
                int id_responder = GetUserId(responder);

                query = new SQLiteCommand();
                query.Connection = DBconnection;
                query.CommandType = CommandType.Text;

                query.CommandText =
                    string.Format("INSERT INTO {0}({1}, {2}) VALUES({3}, {4})",
                    TBL_RELATION,
                    COL_USER1,
                    COL_USER2,
                    id_requester,
                    id_responder
                    );

                query.ExecuteNonQuery();
            }
        }

        internal bool FriendRelationExists(int id_1st_user, int id_2nd_user)
        {
            if (id_1st_user == id_2nd_user)
                return false;

            query = new SQLiteCommand();
            query.Connection = DBconnection;
            query.CommandType = CommandType.Text;

            query.CommandText = 
                string.Format("SELECT Count(*) FROM {0} WHERE {1} = {2} AND {3} = {4} OR {5} = {6} AND {7} = {8}", 
                TBL_RELATION,
                COL_USER1,
                id_1st_user,
                COL_USER2, 
                id_2nd_user,
                COL_USER1,
                id_2nd_user,
                COL_USER2,
                id_1st_user
                );

            object obj = query.ExecuteScalar();
            int occurrences = Convert.ToInt32(obj);

            if (occurrences > 0)
                return true;
            else
                return false;
        }
    }
}
