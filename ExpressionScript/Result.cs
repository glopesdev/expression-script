using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TInput = ExpressionScript.Input<ExpressionScript.ParserContext>;

namespace ExpressionScript
{
    public interface IResult<out TValue>
    {
        TValue Value { get; }

        TInput Tail { get; }
    }

    public class Result<TValue> : IResult<TValue>
    {
        public Result(TValue value, TInput tail)
        {
            Value = value;
            Tail = tail;
        }

        public TValue Value { get; private set; }

        public TInput Tail { get; private set; }
    }
}
