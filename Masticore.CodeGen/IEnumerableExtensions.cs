using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Masticore.CodeGen
{
    public static class IEnumerableExtensions
    {
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            enumerable?.ToList().ForEach(action);
        }

        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int index = 0;
            enumerable?.ToList().ForEach(i =>
            {
                action(i, index);
                index++;
            });
        }
    }
}