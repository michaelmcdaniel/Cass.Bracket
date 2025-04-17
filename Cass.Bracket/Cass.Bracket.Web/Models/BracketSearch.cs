namespace Cass.Bracket.Web.Models
{
    public class BracketSearch
    {
        public int? UserId { get; set; }
        public bool? Private { get; set; }

        public int? OwnerId { get; set; }

        public BracketStatus? Status { get; set; }

        public string? Name { get; set; }

		public int Page { get; set; }
        public int Size { get; set; }

        public mcZen.Data.OrderBy OrderBy { get; set; } = new mcZen.Data.OrderBy("Bracket.Created DESC");

        public static BracketSearch Parse(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new BracketSearch();
            string[] splits = input.Split(' ');
            BracketSearch retVal = new BracketSearch();
            foreach(var val in splits)
            {
                if (val.StartsWith("in:") && Enum.TryParse<BracketStatus>(val.Substring(3), out var status))
                {
                    retVal.Status = status;
                }
                else
                {
                    retVal.Name += val + " ";
                }
            }
            retVal.Name = retVal.Name?.Trim();
            return retVal;
		}
    }
}
