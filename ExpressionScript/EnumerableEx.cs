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
    }
}
