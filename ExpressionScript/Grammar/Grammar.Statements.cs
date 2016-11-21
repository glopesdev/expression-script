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
                DeclarationStatement(),
                EmptyStatement());
        }

        public static Parser<Expression> Block()
        {
            return from o in Token(Char('{'))
                   from statements in Statement().Many()
                   from c in Token(Char('}'))
                   from state in State()
                   select Expression.Block(state.GetScopeVariables(), statements);
        }

        public static Parser<Expression> EmptyStatement()
        {
            return Char(';').SelectMany(x => Empty<Expression>());
        }

        public static Parser<Expression> DeclarationStatement()
        {
            return from declaration in LocalVariableDeclaration()
                   from semicolon in Token(Char(';'))
                   select declaration;
        }

        public static Parser<Expression> LocalVariableDeclaration()
        {
            return from type in Token(Type())
                   from variable in LocalVariableDeclarator(type)
                   select variable;
        }

        public static Parser<Expression> LocalVariableDeclarator(Type type)
        {
            return (from identifier in Token(Identifier())
                    select Expression.Variable(type, identifier))
                    .SelectState((variable, state) => state.AddVariable(variable))
                    .SelectMany(variable =>
                        Optional<Expression>(from e in Token(Char('='))
                                             from initializer in LocalVariableInitializer()
                                             select Expression.Assign(variable, initializer),
                                             variable));
        }

        public static Parser<Expression> LocalVariableInitializer()
        {
            return ExpressionTree();
        }
    }
}
