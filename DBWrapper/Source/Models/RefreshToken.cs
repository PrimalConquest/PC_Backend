namespace DBWrapper.Source.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        public string TokenHash { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
