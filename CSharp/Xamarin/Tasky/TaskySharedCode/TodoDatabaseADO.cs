using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace Tasky.Shared
{
    /// <summary>
    ///     TaskDatabase uses ADO.NET to create the [Items] table and create,read,update,delete data
    /// </summary>
    public class TodoDatabase
    {
        private static readonly object locker = new object ();

        public SqliteConnection connection;

        public string path;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tasky.DL.TaskDatabase" /> TaskDatabase.
        ///     if the database doesn't exist, it will create the database and all the tables.
        /// </summary>
        public TodoDatabase (string dbPath) {
            var output = "";
            path = dbPath;
            // create the tables
            var exists = File.Exists (dbPath);

            if (!exists) {
                connection = new SqliteConnection ("Data Source=" + dbPath);

                connection.Open ();
                var commands = new[] {
                    "CREATE TABLE [Items] (_id INTEGER PRIMARY KEY ASC, Name NTEXT, Notes NTEXT, Done INTEGER);"
                };
                foreach (var command in commands) {
                    using (var c = connection.CreateCommand ()) {
                        c.CommandText = command;
                        var i = c.ExecuteNonQuery ();
                    }
                }
            }
            Console.WriteLine (output);
        }

        /// <summary>Convert from DataReader to Task object</summary>
        private TodoItem FromReader (SqliteDataReader r) {
            var t = new TodoItem {
                ID = Convert.ToInt32 (r["_id"]),
                Name = r["Name"].ToString (),
                Notes = r["Notes"].ToString (),
                Done = (Convert.ToInt32 (r["Done"]) == 1)
            };
            return t;
        }

        public IEnumerable<TodoItem> GetItems () {
            var tl = new List<TodoItem> ();

            lock (locker) {
                connection = new SqliteConnection ("Data Source=" + path);
                connection.Open ();
                using (var contents = connection.CreateCommand ()) {
                    contents.CommandText = "SELECT [_id], [Name], [Notes], [Done] from [Items]";
                    var r = contents.ExecuteReader ();
                    while (r.Read ()) {
                        tl.Add (FromReader (r));
                    }
                }
                connection.Close ();
            }
            return tl;
        }

        public TodoItem GetItem (int id) {
            var t = new TodoItem ();
            lock (locker) {
                connection = new SqliteConnection ("Data Source=" + path);
                connection.Open ();
                using (var command = connection.CreateCommand ()) {
                    command.CommandText = "SELECT [_id], [Name], [Notes], [Done] from [Items] WHERE [_id] = ?";
                    command.Parameters.Add (new SqliteParameter (DbType.Int32) {Value = id});
                    var r = command.ExecuteReader ();
                    while (r.Read ()) {
                        t = FromReader (r);
                        break;
                    }
                }
                connection.Close ();
            }
            return t;
        }

        public int SaveItem (TodoItem item) {
            lock (locker) {
                int r;
                if (item.ID != 0) {
                    connection = new SqliteConnection ("Data Source=" + path);
                    connection.Open ();
                    using (var command = connection.CreateCommand ()) {
                        command.CommandText = "UPDATE [Items] SET [Name] = ?, [Notes] = ?, [Done] = ? WHERE [_id] = ?;";
                        command.Parameters.Add (new SqliteParameter (DbType.String) {Value = item.Name});
                        command.Parameters.Add (new SqliteParameter (DbType.String) {Value = item.Notes});
                        command.Parameters.Add (new SqliteParameter (DbType.Int32) {Value = item.Done});
                        command.Parameters.Add (new SqliteParameter (DbType.Int32) {Value = item.ID});
                        r = command.ExecuteNonQuery ();
                    }
                    connection.Close ();
                    return r;
                }
                connection = new SqliteConnection ("Data Source=" + path);
                connection.Open ();
                using (var command = connection.CreateCommand ()) {
                    command.CommandText = "INSERT INTO [Items] ([Name], [Notes], [Done]) VALUES (? ,?, ?)";
                    command.Parameters.Add (new SqliteParameter (DbType.String) {Value = item.Name});
                    command.Parameters.Add (new SqliteParameter (DbType.String) {Value = item.Notes});
                    command.Parameters.Add (new SqliteParameter (DbType.Int32) {Value = item.Done});
                    r = command.ExecuteNonQuery ();
                }
                connection.Close ();
                return r;
            }
        }

        public int DeleteItem (int id) {
            lock (locker) {
                int r;
                connection = new SqliteConnection ("Data Source=" + path);
                connection.Open ();
                using (var command = connection.CreateCommand ()) {
                    command.CommandText = "DELETE FROM [Items] WHERE [_id] = ?;";
                    command.Parameters.Add (new SqliteParameter (DbType.Int32) {Value = id});
                    r = command.ExecuteNonQuery ();
                }
                connection.Close ();
                return r;
            }
        }
    }
}