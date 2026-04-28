using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace AniTrackerG
{
    class Database
    {
        private static string dbFile = "anime.db";
        private static string connectionString = "Data Source=" + dbFile;

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }

        public static void Init()
        {

            using (var conn = GetConnection())
            {
                conn.Open();

                new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT,
                    Email TEXT UNIQUE,
                    PasswordHash TEXT
                );", conn).ExecuteNonQuery();

                new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS MediaItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT,
                    TotalEpisodes INTEGER
                );", conn).ExecuteNonQuery();

                new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS UserLibrary (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    MediaId INTEGER,
                    Status TEXT,
                    Score INTEGER,
                    WatchedEpisodes INTEGER
                );", conn).ExecuteNonQuery();

                new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS Reviews (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    MediaId INTEGER,
                    Content TEXT,
                    CreatedAt TEXT
                );", conn).ExecuteNonQuery();
            }
        }
    }
}