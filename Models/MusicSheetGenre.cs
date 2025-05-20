namespace HarmonicArchiveBackend.Models
{
    public class MusicSheetGenre
    {
        public int MusicSheetId { get; set; }
        public MusicSheet MusicSheet { get; set; } = new MusicSheet();

    public int GenreId { get; set; }
        public Genre Genre { get; set; } = new Genre();
    }
}
