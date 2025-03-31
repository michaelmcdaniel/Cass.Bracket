namespace Cass.Bracket.Web
{
    public static class Extensions
    {
        public static string ToJavascript(this string? s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\n", " ").Replace("\t", " ").Replace("'", "\\'");
        }
    }
}
