using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class Input<TState> : IEquatable<Input<TState>>, IEnumerable<char>
    {
        readonly string source;
        readonly TState state;
        readonly int offset;

        public Input(string source, TState state)
            : this(source, 0, state)
        {
        }

        public Input(string source, int offset, TState state)
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
            this.state = state;
        }

        public int Length
        {
            get { return source.Length - offset; }
        }

        public TState State
        {
            get { return state; }
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

        public Input<TState> MoveNext()
        {
            return offset < source.Length ? new Input<TState>(source, offset + 1, state) : this;
        }

        public bool Equals(Input<TState> other)
        {
            if (other == null) return false;
            return source == other.source && offset == other.offset &&
                   EqualityComparer<TState>.Default.Equals(state, other.state);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Input<TState>;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode() * 43 + 31 * offset.GetHashCode() +
                   EqualityComparer<TState>.Default.GetHashCode(state);
        }

        public static bool operator ==(Input<TState> left, Input<TState> right)
        {
            if (left == null) return right == null;
            return left.Equals(right);
        }

        public static bool operator !=(Input<TState> left, Input<TState> right)
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
