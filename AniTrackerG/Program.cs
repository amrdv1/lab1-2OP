using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                    string role = GetUserRole(userId.Value);

                    Console.WriteLine($"Роль: {role}");
                    Console.WriteLine("1. Мій список");
                    Console.WriteLine("2. Додати аніме");
                    Console.WriteLine("3. Оновити прогрес");
                    Console.WriteLine("4. Пошук");

                    if (role == "Critic" || role == "Admin")
                        Console.WriteLine("5. Написати відгук");

                    if (role == "Admin")
                        Console.WriteLine("6. Зробити Admin");

                    Console.WriteLine("0. Вийти");

                    var c = Console.ReadLine();

                    if (c == "1") ShowList(userId.Value);
                    else if (c == "2") AddAnime(userId.Value);
                    else if (c == "3") UpdateProgress(userId.Value);
                    else if (c == "4") Search(userId.Value);
                    else if (c == "5") AddReview(userId.Value, role);
                    else if (c == "6") MakeAdmin(userId.Value, role);
                    else if (c == "0") userId = null;
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

            Console.WriteLine("Помилка входу");
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

                var cmd = new SqliteCommand(
                    "INSERT INTO Users(Username,Email,PasswordHash,Role) VALUES(@u,@e,@p,'Viewer')", conn);

                cmd.Parameters.AddWithValue("@u", name);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@p", pass);

                try { cmd.ExecuteNonQuery(); }
                catch { Console.WriteLine("Email вже існує"); }
            }

            Console.ReadKey();
        }

        static string GetUserRole(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT Role FROM Users WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                return cmd.ExecuteScalar()?.ToString() ?? "Viewer";
            }
        }

        static void CheckCriticUpgrade(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT COUNT(*) FROM UserLibrary WHERE UserId=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count >= 5)
                {
                    var update = new SqliteCommand(
                        "UPDATE Users SET Role='Critic' WHERE Id=@id AND Role='Viewer'", conn);
                    update.Parameters.AddWithValue("@id", userId);
                    update.ExecuteNonQuery();
                }
            }
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

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd1 = new SqliteCommand(
                    "INSERT INTO MediaItems(Title,TotalEpisodes) VALUES(@t,@te); SELECT last_insert_rowid();", conn);
                cmd1.Parameters.AddWithValue("@t", title);
                cmd1.Parameters.AddWithValue("@te", total);

                long mediaId = (long)cmd1.ExecuteScalar();

                var cmd2 = new SqliteCommand(
                    "INSERT INTO UserLibrary(UserId,MediaId,Status,Score,WatchedEpisodes) VALUES(@u,@m,@s,@sc,@w)", conn);

                cmd2.Parameters.AddWithValue("@u", userId);
                cmd2.Parameters.AddWithValue("@m", mediaId);
                cmd2.Parameters.AddWithValue("@s", status);
                cmd2.Parameters.AddWithValue("@sc", score);
                cmd2.Parameters.AddWithValue("@w", watched);

                cmd2.ExecuteNonQuery();
            }

            CheckCriticUpgrade(userId);
        }

        static void AddReview(int userId, string role)
        {
            if (role == "Viewer")
            {
                Console.WriteLine("❌ Потрібно 5 аніме");
                Console.ReadKey();
                return;
            }

            Console.Write("ID аніме: ");
            int mediaId = int.Parse(Console.ReadLine());

            Console.Write("Текст: ");
            string text = Console.ReadLine();

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(
                    "INSERT INTO Reviews(UserId,MediaId,Content,CreatedAt) VALUES(@u,@m,@c,@d)", conn);

                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@m", mediaId);
                cmd.Parameters.AddWithValue("@c", text);
                cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString());

                cmd.ExecuteNonQuery();
            }
        }

        static void MakeAdmin(int userId, string role)
        {
            if (role != "Admin")
            {
                Console.WriteLine("❌ Нема доступу");
                Console.ReadKey();
                return;
            }

            Console.Write("ID користувача: ");
            int id = int.Parse(Console.ReadLine());

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand("UPDATE Users SET Role='Admin' WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        static void ShowList(int userId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
SELECT m.Title, u.WatchedEpisodes, m.TotalEpisodes
FROM UserLibrary u
JOIN MediaItems m ON u.MediaId = m.Id
WHERE u.UserId=@id", conn);

                cmd.Parameters.AddWithValue("@id", userId);

                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Console.WriteLine($"{r["Title"]}: {r["WatchedEpisodes"]}/{r["TotalEpisodes"]}");
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
        }

        static void Search(int userId)
        {
            Console.Write("Пошук: ");
            string q = Console.ReadLine();

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqliteCommand(
                    "SELECT Title FROM MediaItems WHERE Title LIKE @q", conn);

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