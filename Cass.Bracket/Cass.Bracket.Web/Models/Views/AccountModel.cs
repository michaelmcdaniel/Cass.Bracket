using System.ComponentModel.DataAnnotations;

namespace Cass.Bracket.Web.Models.Views
{
    public class AccountModel
    {
        [Required]
        public required string Name { get; set; }
        public string? Password { get; set; }
    }
}
