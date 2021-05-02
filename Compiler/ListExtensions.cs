using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    static class ListExtensions
    {
        public static void RemoveLast<T>(this List<T> list, int count)
        {
            list.RemoveRange(list.Count - count, count);
        }

        public static T Last<T>(this List<T> list, int posFromLast = 0)
        {
            return list[list.Count - 1 - posFromLast];
        }
    }
}
