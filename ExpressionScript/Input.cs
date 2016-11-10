using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class Input : IEquatable<Input>, IEnumerable<char>
    {
        readonly string source;
        readonly int offset;

        public Input(string source)
            : this(source, 0)
        {
        }

        public Input(string source, int offset)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (offset < 0 || offset > source.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            this.source = source;
            this.offset = offset;
        }

        public int Length
        {
            get { return source.Length - offset; }
        }

        public char Current
        {
            get
            {
                if (offset == source.Length)
                {
                    throw new InvalidOperationException("Input is at the end of source.");
                }

                return source[offset];
            }
        }

        public Input MoveNext()
        {
            return offset < source.Length ? new Input(source, offset + 1) : this;
        }

        public bool Equals(Input other)
        {
            if (other == null) return false;
            return source == other.source && offset == other.offset;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Input;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode() * 43 + offset.GetHashCode();
        }

        public static bool operator ==(Input left, Input right)
        {
            if (left == null) return right == null;
            return left.Equals(right);
        }

        public static bool operator !=(Input left, Input right)
        {
            if (left == null) return right != null;
            return !left.Equals(right);
        }

        public IEnumerator<char> GetEnumerator()
        {
            if (Length > 0)
            {
                yield return Current;
                foreach (var value in MoveNext())
                {
                    yield return value;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
