using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManagerCLI.Extension
{
    public static class IEnumerable
    {
        public static void Foreach<T>(this IEnumerable<T> items, Action<T> action) => items.Select(w => { action(w); return true; }).ToArray();
    }
}