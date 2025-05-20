namespace HarmonicArchiveBackend.Data
{
    using Microsoft.AspNetCore.Http;

    public class MusicSheetUploadDto
    {
        public string Title { get; set; }
        public string Composer { get; set; }
        public int Year { get; set; }
        public string Key { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Instruments { get; set; }
        public IFormFile MusicFile { get; set; } // PDF/image file
        public int UserId { get; set; } // The ID of the user who uploaded the music sheet
    }
}
