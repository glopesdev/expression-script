using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TInput = ExpressionScript.Input<ExpressionScript.NameTable>;

namespace ExpressionScript
{
    public delegate IEnumerable<IResult<TValue>> Parser<out TValue>(TInput input);
}
