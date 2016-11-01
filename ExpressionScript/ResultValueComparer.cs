using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    class ResultValueComparer<TValue> : IEqualityComparer<IResult<TValue>>
    {
        readonly IEqualityComparer<TValue> valueComparer;
        public static readonly ResultValueComparer<TValue> Default = new ResultValueComparer<TValue>();

        public ResultValueComparer()
            : this(EqualityComparer<TValue>.Default)
        {
        }

        public ResultValueComparer(IEqualityComparer<TValue> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            valueComparer = comparer;
        }

        public bool Equals(IResult<TValue> x, IResult<TValue> y)
        {
            if (x == null) return y == null;
            else if (y == null) return false;
            return valueComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(IResult<TValue> obj)
        {
            if (obj == null) return valueComparer.GetHashCode(default(TValue));
            return valueComparer.GetHashCode(obj.Value);
        }
    }
}
