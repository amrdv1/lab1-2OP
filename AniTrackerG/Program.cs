using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace AniTrackerG
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLitePCL.Batteries.Init();
            Console.OutputEncoding = Encoding.UTF8;
            Database.Init();

            int? userId = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== AniTracker ===\n");

                if (userId == null)
                {
                    Console.WriteLine("1. Логін");
                    Console.WriteLine("2. Реєстрація");
                    Console.WriteLine("0. Вихід");

                    var c = Console.ReadLine();

                    if (c == "1") userId = Login();
                    else if (c == "2") Register();
                    else if (c == "0") break;
                }
                else
                {
                    Console.WriteLine("1. Мій список");
                    Console.WriteLine("2. Додати аніме");
                    Console.WriteLine("3. Оновити прогрес");
                    Console.WriteLine("4. Пошук");
                    Console.WriteLine("5. Вийти");

                    var c = Console.ReadLine();

                    if (c == "1") ShowList(userId.Value);
                    else if (c == "2") AddAnime(userId.Value);
                    else if (c == "3") UpdateProgress(userId.Value);
                    else if (c == "4") Search(userId.Value);
                    else if (c == "5") userId = null;
                }
            }
        }

        static int? Login()
        {
            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Пароль: ");
            string pass = Hash(Console.ReadLine());

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT Id FROM Users WHERE Email=@e AND PasswordHash=@p", conn);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", pass);

                var result = cmd.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);
            }

            Console.WriteLine("Помилка!");
            Console.ReadKey();
            return null;
        }

        static void Register()
        {
            Console.Write("Username: ");
            string name = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Пароль: ");
            string pass = Hash(Console.ReadLine());

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand("INSERT INTO Users(Username,Email,PasswordHash) VALUES(@u,@e,@p)", conn);
                cmd.Parameters.AddWithValue("@u", name);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", pass);

                try { cmd.ExecuteNonQuery(); }
                catch { Console.WriteLine("Email вже існує"); }
            }

            Console.ReadKey();
        }

        static void AddAnime(int userId)
        {
            Console.Write("Назва: ");
            string title = Console.ReadLine();

            Console.Write("Всього серій: ");
            int total = int.Parse(Console.ReadLine());

            Console.Write("Переглянуто: ");
            int watched = int.Parse(Console.ReadLine());

            Console.Write("Оцінка: ");
            int score = int.Parse(Console.ReadLine());

            Console.Write("Статус: ");
            string status = Console.ReadLine();

            Console.Write("Відгук: ");
            string review = Console.ReadLine();

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd1 = new SqliteCommand("INSERT INTO MediaItems(Title,TotalEpisodes) VALUES(@t,@te); SELECT last_insert_rowid();", conn);
                cmd1.Parameters.AddWithValue("@t", title);
                cmd1.Parameters.AddWithValue("@te", total);

                long mediaId = (long)cmd1.ExecuteScalar();

                var cmd2 = new SqliteCommand("INSERT INTO UserLibrary(UserId,MediaId,Status,Score,WatchedEpisodes) VALUES(@u,@m,@s,@sc,@w)", conn);
                cmd2.Parameters.AddWithValue("@u", userId);
                cmd2.Parameters.AddWithValue("@m", mediaId);
                cmd2.Parameters.AddWithValue("@s", status);
                cmd2.Parameters.AddWithValue("@sc", score);
                cmd2.Parameters.AddWithValue("@w", watched);
                cmd2.ExecuteNonQuery();

                if (!string.IsNullOrWhiteSpace(review))
                {
                    var cmd3 = new SqliteCommand("INSERT INTO Reviews(UserId,MediaId,Content,CreatedAt) VALUES(@u,@m,@c,@d)", conn);
                    cmd3.Parameters.AddWithValue("@u", userId);
                    cmd3.Parameters.AddWithValue("@m", mediaId);
                    cmd3.Parameters.AddWithValue("@c", review);
                    cmd3.Parameters.AddWithValue("@d", DateTime.Now.ToString());
                    cmd3.ExecuteNonQuery();
                }
            }
        }

        static void ShowList(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
                SELECT m.Title, m.TotalEpisodes, u.WatchedEpisodes, u.Score, u.Status, r.Content
                FROM UserLibrary u
                JOIN MediaItems m ON u.MediaId = m.Id
                LEFT JOIN Reviews r ON r.MediaId = m.Id AND r.UserId = u.UserId
                WHERE u.UserId=@id", conn);

                cmd.Parameters.AddWithValue("@id", userId);

                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    int total = Convert.ToInt32(r["TotalEpisodes"]);
                    int watched = Convert.ToInt32(r["WatchedEpisodes"]);

                    double percent = total > 0 ? (double)watched / total * 100 : 0;

                    Console.WriteLine("\n" + r["Title"] + " | " + watched + "/" + total + " (" + percent.ToString("F0") + "%)");
                    Console.WriteLine("Оцінка: " + r["Score"] + " | Статус: " + r["Status"]);
                    Console.WriteLine("Відгук: " + r["Content"]);
                    Console.WriteLine(new string('-', 30));
                }
            }

            Console.ReadKey();
        }

        static void UpdateProgress(int userId)
        {
            Console.Write("Назва: ");
            string title = Console.ReadLine();

            Console.Write("Новий прогрес: ");
            int w = int.Parse(Console.ReadLine());

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
                UPDATE UserLibrary 
                SET WatchedEpisodes=@w 
                WHERE MediaId=(SELECT Id FROM MediaItems WHERE Title=@t)
                AND UserId=@u", conn);

                cmd.Parameters.AddWithValue("@w", w);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@u", userId);

                cmd.ExecuteNonQuery();
            }

            Console.ReadKey();
        }

        static void Search(int userId)
        {
            Console.Write("Пошук: ");
            string q = Console.ReadLine();

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
                SELECT Title FROM MediaItems 
                WHERE Title LIKE @q", conn);

                cmd.Parameters.AddWithValue("@q", "%" + q + "%");

                var r = cmd.ExecuteReader();

                while (r.Read())
                    Console.WriteLine(r["Title"]);
            }

            Console.ReadKey();
        }

        static string Hash(string p)
        {
            using (var sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(p))
                    .Select(b => b.ToString("x2")));
            }
        }
    }
}