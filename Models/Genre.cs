﻿namespace HarmonicArchiveBackend.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<MusicSheetGenre> MusicSheetGenres { get; set; }
    }
}
