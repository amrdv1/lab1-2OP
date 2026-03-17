using System;

namespace AniTracker.Models
{
    public class Review
    {
        private Guid _id;
        private User _author;
        private MediaTitle _media;
        private string _content;
        private DateTime _createdAt;

        public Guid Id { get => _id; set => _id = value; }
        public User Author { get => _author; set => _author = value; }
        public MediaTitle Media { get => _media; set => _media = value; }
        public string Content { get => _content; set => _content = value; }
        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }

        public Review() { }

        public Review(User author, MediaTitle media, string content)
        {
            Id = Guid.NewGuid();
            Author = author;
            Media = media;
            Content = content;
            CreatedAt = DateTime.Now;
        }
    }
}