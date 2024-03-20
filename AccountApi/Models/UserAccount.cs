namespace AccountApi.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }
        public string? AccountType { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
