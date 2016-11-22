using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class NameTable
    {
        const string PrefixSeparator = ".";
        public static readonly NameTable Empty = new NameTable(0, Map<string, Type>.Empty, Map<string, ParameterExpression>.Empty);

        readonly int depth;
        readonly Map<string, Type> types;
        readonly Map<string, ParameterExpression> variables;

        private NameTable(int depth, Map<string, Type> types, Map<string, ParameterExpression> variables)
        {
            this.depth = depth;
            this.types = types;
            this.variables = variables;
        }

        string GetNameKey(string name)
        {
            return depth + PrefixSeparator + name;
        }

        public NameTable CreateScope()
        {
            return new NameTable(depth + 1, types, variables);
        }

        public NameTable AddType(string name, Type type)
        {
            return new NameTable(depth, types.Add(GetNameKey(name), type), variables);
        }

        public NameTable AddVariable(ParameterExpression variable)
        {
            return new NameTable(depth, types, variables.Add(GetNameKey(variable.Name), variable));
        }

        public NameTable AddVariable(IEnumerable<ParameterExpression> variables)
        {
            var map = this.variables;
            foreach (var variable in variables)
            {
                map = map.Add(GetNameKey(variable.Name), variable);
            }

            return new NameTable(depth, types, map);
        }

        public Type GetType(string name)
        {
            return types[GetNameKey(name)];
        }

        public ParameterExpression GetVariable(string name)
        {
            return variables[GetNameKey(name)];
        }

        public IEnumerable<KeyValuePair<string, Type>> GetScopeTypes()
        {
            var prefix = GetNameKey(string.Empty);
            return types.GreaterThanOrEqual(prefix).TakeWhile(x => x.Key.StartsWith(prefix));
        }

        public IEnumerable<ParameterExpression> GetScopeVariables()
        {
            var prefix = GetNameKey(string.Empty);
            return variables.GreaterThanOrEqual(prefix)
                            .TakeWhile(x => x.Key.StartsWith(prefix))
                            .Select(x => x.Value);
        }
    }
}
