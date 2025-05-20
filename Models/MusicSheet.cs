using HarmonicArchiveBackend.Models;

public class MusicSheet
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Year { get; set; }

    // Relationships
    public int TitleId { get; set; }
    public Title Title { get; set; } = new Title();

    public int ComposerId { get; set; }
    public Composer Composer { get; set; } = new Composer();

    public ICollection<MusicSheetGenre> MusicSheetGenres { get; set; }
    public ICollection<MusicSheetInstrument> MusicSheetInstruments { get; set; }

    public string MusicFilePath { get; set; } = string.Empty; // Path to the PDF/image file

    // Add these for 1:n relationship
    public int UserId { get; set; }
    public User User { get; set; }
}