using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    static class EnumerableEx
    {
        internal static IEnumerable<TValue> Return<TValue>(TValue value)
        {
            yield return value;
        }

        internal static IEnumerable<TValue> Defer<TValue>(Func<IEnumerable<TValue>> enumerableFactory)
        {
            foreach (var value in enumerableFactory())
            {
                yield return value;
            }
        }

        internal static IEnumerable<TValue> Do<TValue>(this IEnumerable<TValue> source, Action<TValue> onNext)
        {
            foreach (var value in source)
            {
                onNext(value);
                yield return value;
            }
        }

        internal static IEnumerable<TValue> Concat<TValue>(TValue head, IEnumerable<TValue> tail)
        {
            yield return head;
            foreach (var value in tail)
            {
                yield return value;
            }
        }
    }
}
