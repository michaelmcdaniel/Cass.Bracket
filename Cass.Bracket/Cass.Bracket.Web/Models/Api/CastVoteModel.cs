namespace Cass.Bracket.Web.Models.Api
{
	public class CastVoteModel
	{
		public int Round { get; set; }
		public IDictionary<long, long>? Winners { get; set; }
	}
}
