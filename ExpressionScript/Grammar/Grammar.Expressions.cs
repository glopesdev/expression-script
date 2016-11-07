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
        static Func<TValue, TValue, TValue> Operator<TValue>(Func<TValue, TValue, TValue> func)
        {
            return func;
        }

        public static Parser<Expression> ExpressionTree()
        {
            return NonAssignmentExpression();
        }

        public static Parser<Expression> NonAssignmentExpression()
        {
            return ConditionalExpression();
        }

        public static Parser<Expression> ConditionalExpression()
        {
            return NullCoalescingExpression().SelectMany(x =>
                (from q in Char('?')
                 from e1 in ExpressionTree()
                 from c in Char(':')
                 from e2 in ExpressionTree()
                 select Expression.Condition(x, e1, e2))
                 .Or(Return(x)));
        }

        public static Parser<Expression> NullCoalescingExpression()
        {
            return ConditionalOrExpression().ChainLeft(
                String("??").Select(x => Operator<Expression>(Expression.Coalesce)));
        }

        public static Parser<Expression> ConditionalOrExpression()
        {
            return ConditionalAndExpression().ChainLeft(
                String("||").Select(x => Operator<Expression>(Expression.OrElse)));
        }

        public static Parser<Expression> ConditionalAndExpression()
        {
            return InclusiveOrExpression().ChainLeft(
                String("&&").Select(x => Operator<Expression>(Expression.AndAlso)));
        }

        public static Parser<Expression> InclusiveOrExpression()
        {
            return ExclusiveOrExpression().ChainLeft(
                Char('|').Select(x => Operator<Expression>(Expression.Or)));
        }

        public static Parser<Expression> ExclusiveOrExpression()
        {
            return AndExpression().ChainLeft(
                Char('^').Select(x => Operator<Expression>(Expression.ExclusiveOr)));
        }

        public static Parser<Expression> AndExpression()
        {
            return EqualityExpression().ChainLeft(
                Char('&').Select(x => Operator<Expression>(Expression.And)));
        }

        public static Parser<Expression> EqualityExpression()
        {
            return RelationalExpression().ChainLeft(Or(
                String("==").Select(x => Operator<Expression>(Expression.Equal)),
                String("!=").Select(x => Operator<Expression>(Expression.NotEqual))));
        }

        public static Parser<Expression> RelationalExpression()
        {
            return ShiftExpression().ChainLeft(Or(
                Char('<').Select(x => Operator<Expression>(Expression.LessThan)),
                Char('>').Select(x => Operator<Expression>(Expression.GreaterThan)),
                String("<=").Select(x => Operator<Expression>(Expression.LessThanOrEqual)),
                String(">=").Select(x => Operator<Expression>(Expression.GreaterThanOrEqual))));
        }

        public static Parser<Expression> ShiftExpression()
        {
            return AdditiveExpression().ChainLeft(Or(
                String("<<").Select(x => Operator<Expression>(Expression.LeftShift)),
                String(">>").Select(x => Operator<Expression>(Expression.RightShift))));
        }

        public static Parser<Expression> AdditiveExpression()
        {
            return MultiplicativeExpression().ChainLeft(Or(
                Char('+').Select(x => Operator<Expression>(Expression.Add)),
                Char('-').Select(x => Operator<Expression>(Expression.Subtract))));
        }

        public static Parser<Expression> MultiplicativeExpression()
        {
            return UnaryExpression().ChainLeft(Or(
                Char('*').Select(x => Operator<Expression>(Expression.Multiply)),
                Char('/').Select(x => Operator<Expression>(Expression.Divide)),
                Char('%').Select(x => Operator<Expression>(Expression.Modulo))));
        }

        public static Parser<Expression> UnaryExpression()
        {
            return PrimaryExpression();
        }

        public static Parser<Expression> PrimaryExpression()
        {
            return Or(
                PrimaryNoArrayCreationExpression(),
                ArrayCreationExpression());
        }

        public static Parser<Expression> PrimaryNoArrayCreationExpression()
        {
            return Or(
                Literal(),
                ParenthesizedExpression());
        }

        public static Parser<Expression> ArrayCreationExpression()
        {
            return Parser.Empty<Expression>();
        }

        public static Parser<Expression> ParenthesizedExpression()
        {
            return Defer(ExpressionTree).BracketedBy(
                Char('('),
                Char(')'));
        }
    }
}
