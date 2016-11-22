using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public struct GenericName : IEquatable<GenericName>
    {
        public string Name;
        public Type[] TypeArguments;

        public GenericName(string name, Type[] typeArguments)
        {
            Name = name;
            TypeArguments = typeArguments;
        }

        public bool Equals(GenericName other)
        {
            return EqualityComparer<string>.Default.Equals(Name, other.Name) &&
                   EqualityComparer<Type[]>.Default.Equals(TypeArguments, other.TypeArguments);
        }

        public override bool Equals(object obj)
        {
            return obj is GenericName && Equals((GenericName)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<string>.Default.GetHashCode(Name) * 43 +
                   EqualityComparer<Type[]>.Default.GetHashCode(TypeArguments);
        }

        public override string ToString()
        {
            var typeArguments = TypeArguments;
            if (typeArguments != Type.EmptyTypes)
            {
                return Name + "<" + string.Join(",", (object[])typeArguments) + ">";
            }

            return Name;
        }
    }
}
