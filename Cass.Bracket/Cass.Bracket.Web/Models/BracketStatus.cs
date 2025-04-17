namespace Cass.Bracket.Web.Models
{
    public enum BracketStatus : byte
	{
		/// <summary>
		/// The bracket is pending and has not yet started.
		/// </summary>
		Pending = 1,
		/// <summary>
		/// The bracket is open for registration.
		/// </summary>
		Open = 2,
		/// <summary>
		/// The bracket is active and in progress.
		/// </summary>
		Active = 4,
		/// <summary>
		/// The bracket is complete.
		/// </summary>
		Complete = 8
	}
    
}
