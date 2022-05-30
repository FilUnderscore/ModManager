using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public static class Utils
    {
        public static string StringFromList(this List<string> list)
        {
            return StringFromList(list, ", ");
        }

        public static string StringFromList(this List<string> list, string separator)
        {
            return StringFromList(list, str => str, separator);
        }

        public static string StringFromList<T>(this List<T> list, Func<T, string> getter, string separator)
        {
            if (list == null)
                return "null";

            StringBuilder builder = new StringBuilder();

            for(int index = 0; index < list.Count; index++)
            {
                builder.Append(getter(list[index]));

                if (index < list.Count - 1)
                    builder.Append(separator);
            }

            return builder.ToString();
        }

        public static string StringFromList<T>(this List<T> list, Func<T, string> getter)
        {
            return StringFromList<T>(list, getter, ", ");
        }
    }
}
