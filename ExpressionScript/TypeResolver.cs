using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class TypeResolver : ITypeResolver
    {
        public static readonly TypeResolver Default = new TypeResolver();

        private TypeResolver()
        {
        }

        public Type GetType(string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}
