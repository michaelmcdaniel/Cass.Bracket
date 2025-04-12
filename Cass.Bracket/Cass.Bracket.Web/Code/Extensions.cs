namespace Cass.Bracket.Web
{
    public static class Extensions
    {
        public static string ToJavascript(this string? s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\n", " ").Replace("\t", " ").Replace("'", "\\'");
        }

        public static T Shift<T>(this List<T> list)
        {
            T retVal = list[0];
            list.RemoveAt(0);
            return retVal;
        }
        public static T Pop<T>(this List<T> list)
        {
            T retVal = list[list.Count-1];
            list.RemoveAt(list.Count-1);
            return retVal;
        }
    }
}
