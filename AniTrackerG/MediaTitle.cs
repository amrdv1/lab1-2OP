using System;
using System.Collections.Generic;

namespace AniTracker.Models
{
    public class MediaTitle
    {
        private Guid _id;
        private string _titleName;
        private string _mediaType;
        private int _totalEpisodes;
        private Studio _productionStudio;
        private List<Genre> _genres;

        public Guid Id { get => _id; set => _id = value; }
        public string TitleName { get => _titleName; set => _titleName = value; }
        public string MediaType { get => _mediaType; set => _mediaType = value; }
        public int TotalEpisodes { get => _totalEpisodes; set => _totalEpisodes = value; }
        public Studio ProductionStudio { get => _productionStudio; set => _productionStudio = value; }
        public List<Genre> Genres { get => _genres; set => _genres = value; }

        public MediaTitle()
        {
            Genres = new List<Genre>();
        }

        public MediaTitle(string titleName, string mediaType, int totalEpisodes, Studio studio)
        {
            Id = Guid.NewGuid();
            TitleName = titleName;
            MediaType = mediaType;
            TotalEpisodes = totalEpisodes;
            ProductionStudio = studio;
            Genres = new List<Genre>();
        }

        public override string ToString() => $"[{MediaType}] {TitleName} (Епізодів: {TotalEpisodes})";
    }
}