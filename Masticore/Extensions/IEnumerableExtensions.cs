using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Masticore
{
    /// <summary>
    /// Contains extension methods for IEnumerable types
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Allows running a ForEach directly from an IEnumerable without first casting to a list.
        /// Under the covers this still casts to a list so you still need to look out for the
        /// possiblity of enumerating an enumeration multiple times.
        /// </summary>
        /// <typeparam name="T">.NET type of the item in the enumeration.</typeparam>
        /// <param name="enumerable">Enumeration to foreach over</param>
        /// <param name="action">Action to run against each item in the enumeration.</param>
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            enumerable?.ToList().ForEach(action);
        }

        /// <summary>
        /// Allows running a ForEach directly from an IEnumerable without first casting to a 
        /// list and also providing the index of the item in the enumeration. Under the covers 
        /// this still casts to a list so you still need to look out for the possiblity of 
        /// enumerating an enumeration multiple times.
        /// </summary>
        /// <typeparam name="T">.NET type of the item in the enumeration.</typeparam>
        /// <param name="enumerable">Enumeration to foreach over</param>
        /// <param name="action">Action to run against each item in the enumeration.</param>
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