using System;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace AniTrackerG
{
    class Database
    {
        static string Hash(string p)
        {
            using (var sha = SHA256.Create())
            {
                return string.Concat(
                    sha.ComputeHash(Encoding.UTF8.GetBytes(p))
                       .Select(b => b.ToString("x2"))
                );
            }
        }
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
    PasswordHash TEXT,
    Role TEXT DEFAULT 'Viewer'
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
                var pass = Hash("123123");

                var cmd = new SqliteCommand(@"
INSERT OR IGNORE INTO Users (Email, Username, PasswordHash, Role)
VALUES ('admin@admin.com', 'admin', @p, 'Admin');
", conn);

                cmd.Parameters.AddWithValue("@p", pass);

                cmd.ExecuteNonQuery();
            }
        }
    }
}