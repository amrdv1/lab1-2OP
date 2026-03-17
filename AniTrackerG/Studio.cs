using System;

namespace AniTracker.Models
{
    public class Studio
    {
        private Guid _id;
        private string _name;

        public Guid Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }

        public Studio() { }

        public Studio(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public override string ToString() => $"Студія: {Name}";
    }
}