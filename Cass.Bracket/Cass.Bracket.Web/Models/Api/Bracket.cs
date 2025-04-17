using System.ComponentModel.DataAnnotations;

namespace Cass.Bracket.Web.Models.Api
{
    public class Bracket
    {
        public long Id { get; set; } = 0;

        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }

        public bool Private { get; set; } = false;

        public int MinUsers { get; set; } = 2;
        public int MaxUsers { get; set; } = 0;

		public List<string> Opponents { get; set; } = new List<string>();

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Cutoff { get; set; }

        public bool Publish { get; set; }
    }
}
