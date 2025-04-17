using System.Security.Claims;

namespace Cass.Bracket.Web
{
    public static class Extensions
    {
        public static int Id(this ClaimsPrincipal user)
        {
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier)??"0", out var id);
            return id;
        }

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

        public static IEnumerable<T> MaskToList<T,E>(this E mask) where E : Enum
        {
            //if (typeof(T).IsSubclassOf(typeof(E)) == false) throw new ArgumentException();
            return Enum.GetValues(typeof(E)).Cast<E>().Where(m=>mask.HasFlag(m)).Cast<T>();
        }
    }
}
