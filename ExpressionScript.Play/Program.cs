using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript.Play
{
    class MyExpression : Expression
    {
        public override bool CanReduce
        {
            get { return true; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { return typeof(int); }
        }

        public override Expression Reduce()
        {
            return base.Reduce();
        }
    }

    class Program
    {
        //static void MapTests()
        //{
        //    var map = Map<int, int>.Empty;
        //    for (int k = 0; k < 10; k++)
        //    {
        //        if (k == 3) continue;
        //        map = map.Add(k, k);
        //    }

        //    foreach (var pv in map.GreaterThanOrEqual(3))
        //    {
        //        Console.WriteLine(pv);
        //    }

        //    int v;
        //    map.TryGetValue(3, out v);
        //    Console.WriteLine(v);
        //}

        static void LambdaTests()
        {
            var i = Parser.Keyword();
            var r2 = i.Parse("int");
            Console.WriteLine(r2.Value);

            var p = Parser.ExpressionTree();
            var r = p.Parse("(int x, int y) => x + y");
            Console.WriteLine(r.Value);
        }

        static void BlockTests()
        {
            var p = Parser.WhitespaceOrLineTerminators().SelectMany(x => Parser.Statement());
            var r = p.Parse(@"
            {
              var x = default(int);
              string s = null;
              {
                System.Action<int> y = a => a;
              }
            }");
            Console.WriteLine(r.Value);
        }

        static void StaticTests()
        {
            var p = Parser.ExpressionTree();
            var r = p.Parse("System.Array.ConvertAll(new int[0], (int x) => x)");
            //var r = p.Parse("System.Array.ConvertAll(new int[0],null)");
            Console.WriteLine(r.Value);
        }

        static void TypeTests()
        {
            var p = Parser.Type();
            var r = p.Parse("System.Type");
            Console.WriteLine(r.Value);
        }

        static void QualifiedNameTests()
        {
            var p = Parser.StaticMemberAccess();
            var r = p.Parse("System.Collections.Generic.EqualityComparer<int>.Default");
            Console.WriteLine(r.Value);
        }

        static void Main(string[] args)
        {
            QualifiedNameTests();
            //TypeTests();
            //StaticTests();
            //LambdaTests();
            //MapTests();
            BlockTests();

            var x1 = Expression.Parameter(typeof(int), "x");
            var x2 = Expression.Parameter(typeof(int), "y");
            var q = Expression.Block(new[] { x1, x2 }, new[]
            {
                Expression.Assign(x1, Expression.Constant(2)),
                Expression.Assign(x2, Expression.Constant(2)),
                Expression.Add(x1, x2)
            });
            Console.WriteLine(q);

            var e = new MyExpression();
            var e2 = new MyExpression();
            var b = Expression.Add(e, e2);
            var c = b.ReduceExtensions();
            var bl = Expression.Lambda(c);

            Console.WriteLine(b);


            

            //var p2 = Parser.AdditiveExpression();
            //var r2 = p2.Parse(string.Join(string.Empty, Enumerable.Repeat("1+", 10000)).TrimEnd('+'));
            //Console.WriteLine(r2.Value);

            var p = Parser.Char().Many();
            var i = new string('c', 10000);
            var r = p.Parse(i);
            Console.WriteLine(r.Value.Length);

            r = p.Parse(i);
            Console.WriteLine(r.Value.Length);

            //var pp = Parser.TypeArgumentList();
            //var exx = pp.Parse("<int,int>");
            //Console.WriteLine(exx.Value);

            var ppp = -+-+1;
            //var comment = Parser.Comment();
            //var ex = comment.Parse("/* fjhfkjdhhkdhf jfdlfjd ***** dkfjdkj * dfklj d** //// d****d d ***/");
            //Console.WriteLine(ex.Value);

            var parser = Parser.ExpressionTree();
            var ex2 = parser.Parse("new int[]");
            //var ex2 = parser.Parse("typeof(int).Assembly.GetTypes()[0].AssemblyQualifiedName[0]");
            var l = Expression.Lambda(ex2.Value).Compile().DynamicInvoke();
            Console.WriteLine(l);

            var ex = parser.Parse("typeof(System.Collections.Generic.Dictionary<int,int>)");
            Console.WriteLine(ex.Value);

            var expression = parser.Parse("3 > 2 ?/*dlfkj*/ 1+(1+2)*2 : 5");
            var lambda = Expression.Lambda(expression.Value).Compile();
            var value = lambda.DynamicInvoke();
            Console.WriteLine(value);
        }
    }
}
