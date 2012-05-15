using System;
using System.Collections;
using System.Collections.Generic;

namespace TinyEE
{
    public struct Range<T> : IEnumerable<T>, IEquatable<Range<T>> where T : struct,IComparable<T>, IEquatable<T>
    {
        private readonly T _lft;
        private readonly T _rgt;
        private readonly Func<T, T> _fnStep;
        private readonly int _size;

        public Range(T left, T right, int size, Func<T, T> fnStep)
        {
            if (fnStep == null)
            {
                throw new ArgumentNullException("fnStep");
            }
            _lft = left;
            _rgt = right;
            _size = size;
            _fnStep = fnStep;
        }

        public T Left { get { return _lft; } }

        public T Right { get { return _rgt; } }

        public int Size { get { return _size; } }

        public bool Contains(T point)
        {
            return _lft.CompareTo(point) <= 0 && _rgt.CompareTo(Right) <= 0;
        }

        public bool Contains(Range<T> other)
        {
            return _lft.CompareTo(other.Left) <= 0 && _rgt.CompareTo(other.Right) <= 0;
        }

        public bool Intersect(Range<T> other)
        {
            return (other.Left.CompareTo(Left) >= 0 && other.Right.CompareTo(Right) <= 0) ||
                   (other.Left.CompareTo(Left) <= 0 && other.Right.CompareTo(Left) >= 0) ||
                   (other.Left.CompareTo(Right) <= 0 && other.Right.CompareTo(Right) >= 0);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = Left; i.CompareTo(Right) <= 0; i = _fnStep(i))
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return "[" + Left + ".." + Right + "]";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _lft.GetHashCode();
                result = (result*397) ^ _rgt.GetHashCode();
                result = (result*397) ^ _size;
                return result;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Range<T> other)
        {
            return other._lft.Equals(_lft) && other._rgt.Equals(_rgt) && other._size == _size;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Range<T>)) return false;
            return Equals((Range<T>) obj);
        }

        public static bool operator ==(Range<T> left, Range<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right)
        {
            return !left.Equals(right);
        }
    }
}