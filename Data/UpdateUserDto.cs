namespace HarmonicArchiveBackend.Data
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? OldPassword { get; set; } // Nullable for cases where the password is not being updated
        public string? Password { get; set; } // Nullable for cases where the password is not being updated
    }
}
