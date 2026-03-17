using System;

namespace AniTracker.Models
{
    public class ListEntry
    {
        private Guid _id;
        private MediaTitle _media;
        private string _status;
        private int _currentProgress;
        private DateTime _addedAt;

        public Guid Id { get => _id; set => _id = value; }
        public MediaTitle Media { get => _media; set => _media = value; }
        public string Status { get => _status; set => _status = value; }
        public int CurrentProgress { get => _currentProgress; set => _currentProgress = value; }
        public DateTime AddedAt { get => _addedAt; set => _addedAt = value; }

        public ListEntry() { }

        public ListEntry(MediaTitle media, string status)
        {
            Id = Guid.NewGuid();
            Media = media;
            Status = status;
            CurrentProgress = 0;
            AddedAt = DateTime.Now;
        }

        public void IncrementProgress()
        {
            if (CurrentProgress < Media.TotalEpisodes)
            {
                CurrentProgress++;
            }
        }

        public override string ToString() => $"Прогрес: {Media.TitleName} - {CurrentProgress}/{Media.TotalEpisodes} ({Status})";
    }
}