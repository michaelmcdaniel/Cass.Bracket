using System.ComponentModel.DataAnnotations;

namespace Cass.Bracket.Web.Models.Views
{
    public class AuthenticationModel
    {
        [EmailAddress]
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
        public required string Name { get; set; }
        public bool RememberMe { get; set; }

    }
}
