using System.ComponentModel.DataAnnotations;

namespace HarmonicArchiveBackend.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(256)]
        public string Password { get; set; } // Store hashed passwords, not plain text

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // "normal" or "admin"
       
        public ICollection<MusicSheet> MusicSheets { get; set; }
    }
}
