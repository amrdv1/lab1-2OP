using System;
using System.Collections.Generic;

namespace AniTracker.Models
{
    public class User
    {
        private Guid _id;
        private string _username;
        private string _email;
        private string _passwordHash;
        private DateTime _registeredAt;
        private List<ListEntry> _personalList;

        // Для JSON властивості повинні мати public set
        public Guid Id { get => _id; set => _id = value; }
        public string Username { get => _username; set => _username = value; }
        public string Email { get => _email; set => _email = value; }
        public string PasswordHash { get => _passwordHash; set => _passwordHash = value; }
        public DateTime RegisteredAt { get => _registeredAt; set => _registeredAt = value; }
        public List<ListEntry> PersonalList { get => _personalList; set => _personalList = value; }

        
        public User() { }

        public User(string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RegisteredAt = DateTime.Now;
            PersonalList = new List<ListEntry>();
        }

        public void AddToList(ListEntry entry)
        {
            if (!PersonalList.Contains(entry))
            {
                PersonalList.Add(entry);
            }
        }

        public override string ToString() => $"[{RegisteredAt:dd.MM.yyyy}] Користувач: {Username}";
    }
}