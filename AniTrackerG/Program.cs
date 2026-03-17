using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using AniTracker.Models;
using System.Linq;

namespace AniTrackerG
{
    class Program
    {
        private const string UsersDbPath = "users.json";
        private const string ReviewsDbPath = "reviews.json";

        private static List<User> allUsers = new List<User>();
        private static List<Review> allReviews = new List<Review>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            LoadData();

            // Создаем каталог (в старом стиле C# 7.3)
            List<MediaTitle> catalog = new List<MediaTitle>();
            catalog.Add(new MediaTitle("Mushoku Tensei", "Аніме", 24, new Studio("Studio Bind")));
            catalog.Add(new MediaTitle("Jujutsu Kaisen", "Аніме", 47, new Studio("MAPPA")));
            catalog.Add(new MediaTitle("Bleach", "Аніме", 366, new Studio("Pierrot")));

            User currentUser = null;

            while (true)
            {
                Console.WriteLine("\n==================================");
                if (currentUser == null)
                {
                    Console.WriteLine("1. Увійти (Логін)");
                    Console.WriteLine("2. Зареєструватися");
                }
                else
                {
                    Console.WriteLine("[Профіль: " + currentUser.Username + "]");
                    Console.WriteLine("1. Каталог аніме");
                    Console.WriteLine("2. Мій список");
                    Console.WriteLine("3. Додати аніме до себе");
                    Console.WriteLine("4. Відмітити прогрес (+1)");
                    Console.WriteLine("5. Написати відгук");
                    Console.WriteLine("6. Переглянути всі відгуки");
                    Console.WriteLine("7. Вийти з акаунту");
                }
                
                Console.WriteLine("0. Вихід з програми");
                Console.Write("\nОберіть дію: ");
                string choice = Console.ReadLine();

                if (choice == "0") { SaveData(); break; }
if (currentUser == null)
                {
                    if (choice == "1") Login(ref currentUser);
                    else if (choice == "2") Register();
                }
                else
                {
                    if (choice == "1")
                    {
                        for (int i = 0; i < catalog.Count; i++) 
                            Console.WriteLine((i + 1) + ". " + catalog[i].ToString());
                    }
                    else if (choice == "2")
                    {
                        if (currentUser.PersonalList.Count == 0) Console.WriteLine("Список порожній.");
                        foreach (var entry in currentUser.PersonalList) Console.WriteLine(entry.ToString());
                    }
                    else if (choice == "3") AddAnimeToUser(currentUser, catalog);
                    else if (choice == "4") UpdateProgress(currentUser);
                    else if (choice == "5") WriteReview(currentUser, catalog);
                    else if (choice == "6") ShowReviews();
                    else if (choice == "7") { currentUser = null; Console.WriteLine("Ви вийшли."); }
                }
            }
        }

        static void WriteReview(User user, List<MediaTitle> catalog)
        {
            Console.WriteLine("\nЯке аніме хочете прокоментувати?");
            for (int i = 0; i < catalog.Count; i++) Console.WriteLine((i + 1) + ". " + catalog[i].TitleName);
            
            int index;
            if (int.TryParse(Console.ReadLine(), out index) && index > 0 && index <= catalog.Count)
            {
                Console.WriteLine("Введіть текст вашого відгуку:");
                string content = Console.ReadLine();

                Review newReview = new Review(user, catalog[index - 1], content);
                allReviews.Add(newReview);
                SaveData();
                Console.WriteLine("=> Відгук успішно опубліковано!");
            }
            else Console.WriteLine("=> Помилка вибору.");
        }

        static void ShowReviews()
        {
            Console.WriteLine("\n=== ВСІ ВІДГУКИ КОРИСТУВАЧІВ ===");
            if (allReviews.Count == 0) Console.WriteLine("Відгуків ще немає.");
            foreach (Review rev in allReviews)
            {
                Console.WriteLine("[" + rev.CreatedAt.ToString("dd.MM") + "] " + rev.Author.Username + " про '" + rev.Media.TitleName + "':");
                Console.WriteLine("> " + rev.Content + "\n");
            }
        }

        static void Register()
        {
            Console.Write("Нікнейм: "); string name = Console.ReadLine();
            Console.Write("Email: "); string email = Console.ReadLine();
            Console.Write("Пароль: "); string pass = Console.ReadLine();

            allUsers.Add(new User(name, email, HashPassword(pass)));
            SaveData();
            Console.WriteLine("=> Готово! Тепер увійдіть.");
        }

        static void Login(ref User currentUser)
        {
            Console.Write("Email: "); string email = Console.ReadLine();
            Console.Write("Пароль: "); string pass = Console.ReadLine();
            string hash = HashPassword(pass);

            foreach (User user in allUsers)
            {
                if (user.Email == email && user.PasswordHash == hash)
                {
                    currentUser = user;
                    Console.WriteLine("=> Привіт, " + currentUser.Username + "!");
                    return;
                }
            }
            Console.WriteLine("=> Помилка входу.");
        }

        static void AddAnimeToUser(User user, List<MediaTitle> catalog)
        {
            Console.Write("Номер аніме з каталогу: ");
            int idx;
            if (int.TryParse(Console.ReadLine(), out idx) && idx > 0 && idx <= catalog.Count)
            {
                user.AddToList(new ListEntry(catalog[idx - 1], "Дивлюсь"));
                SaveData();
                Console.WriteLine("=> Додано!");
            }
        }
static void UpdateProgress(User user)
        {
            for (int i = 0; i < user.PersonalList.Count; i++) 
                Console.WriteLine((i + 1) + ". " + user.PersonalList[i].ToString());
            
            Console.Write("Оберіть номер для +1 серії: ");
            int idx;
            if (int.TryParse(Console.ReadLine(), out idx) && idx > 0 && idx <= user.PersonalList.Count)
            {
                user.PersonalList[idx - 1].IncrementProgress();
                SaveData();
                Console.WriteLine("=> Оновлено!");
            }
        }

        static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        static void SaveData()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            File.WriteAllText(UsersDbPath, JsonSerializer.Serialize(allUsers, options));
            File.WriteAllText(ReviewsDbPath, JsonSerializer.Serialize(allReviews, options));
        }

        static void LoadData()
        {
            if (File.Exists(UsersDbPath)) 
            {
                string json = File.ReadAllText(UsersDbPath);
                allUsers = JsonSerializer.Deserialize<List<User>>(json);
                if (allUsers == null) allUsers = new List<User>();
            }
            if (File.Exists(ReviewsDbPath)) 
            {
                string json = File.ReadAllText(ReviewsDbPath);
                allReviews = JsonSerializer.Deserialize<List<Review>>(json);
                if (allReviews == null) allReviews = new List<Review>();
            }
        }
    }
}