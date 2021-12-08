using System.Collections.Generic;

namespace jIAnSoft.Nami.Core
{
    internal static class Lists
    {
        public static void Swap<T>(ref List<T> a, ref List<T> b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}