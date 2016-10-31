using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<ConstantExpression> Boolean()
        {
            return String("true").Select(x => Expression.Constant(true)).Or(
                   String("false").Select(x => Expression.Constant(false)));
        }
    }
}
