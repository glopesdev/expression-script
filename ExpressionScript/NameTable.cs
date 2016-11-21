using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class NameTable
    {
        public static readonly NameTable Empty = new NameTable(0, Map<string, Type>.Empty, Map<string, Expression>.Empty);

        readonly int depth;
        readonly Map<string, Type> types;
        readonly Map<string, Expression> variables;

        private NameTable(int depth, Map<string, Type> types, Map<string, Expression> variables)
        {
            this.depth = depth;
            this.types = types;
            this.variables = variables;
        }

        public NameTable CreateScope()
        {
            return new NameTable(depth + 1, types, variables);
        }

        public NameTable AddType(string name, Type type)
        {
            return new NameTable(depth, types.Add(name + depth, type), variables);
        }

        public NameTable AddVariable(string name, Expression variable)
        {
            return new NameTable(depth, types, variables.Add(name + depth, variable));
        }

        public Type GetType(string name)
        {
            return types[name + depth];
        }

        public Expression GetVariable(string name)
        {
            return variables[name + depth];
        }
    }
}
