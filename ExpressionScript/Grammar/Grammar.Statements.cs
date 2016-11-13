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
        public static Parser<Expression> Statement()
        {
            return Or(
                Block(),
                EmptyStatement());
        }

        public static Parser<Expression> Block()
        {
            return from o in Token(Char('{'))
                   from statements in Statement().Many()
                   from c in Token(Char('}'))
                   select Expression.Block(statements);
        }

        public static Parser<Expression> EmptyStatement()
        {
            return Char(';').SelectMany(x => Empty<Expression>());
        }
    }
}
