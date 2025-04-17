namespace Cass.Bracket.Web.Models
{
    public class User
    {
        public long Id { get; set; } = 0;
        public required string Email { get; set; }

        public required string Name { get; set; }
        public DateTimeOffset Created { get; set; }

        public required string Password { get; set; } 

        public bool ForcePasswordReset { get; set; } = false;

        public bool IsAdmin { get; set; } = false;
    }
}
