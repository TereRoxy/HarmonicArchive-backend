using System.ComponentModel.DataAnnotations;

namespace HarmonicArchiveBackend.Models
{
    public class Title
    {
        public int Id { get; set; }

        [MaxLength(255)] //Limit the length of the title to make it compatible with indexing
        public string Name { get; set; } = string.Empty;

        public ICollection<MusicSheet> MusicSheets { get; set; }
    }
}
