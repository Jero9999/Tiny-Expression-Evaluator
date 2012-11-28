using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TinyEE
{
    /// <summary>
    /// A range is a data structure that define a continuous sequence of elements along a vector
    /// Pros:
    /// - It is memory efficient: the range of integers from 1 to 1048576 can fills a 1MB array. Most of the time, we need only 2 integers: [1..1048576] (2 bytes)
    /// - It is lazy (a good thing): by implementing IEnumerable of T, it can stream elements on-demand, without occupying precious memory upfront
    /// - It is immutable (another good thing)
    /// - Its behaviour is highly customizable: the caller can dictate how to fetch the next/previous element in the range by  supplying a delegate
    /// Cons:
    /// - As with anything implementing IEnumerable, it can be misused (repeated iterations)
    /// - Instantiation by constructor is a bit complicated, so use some of the static factory methods below
    /// </summary>
    public struct Range<T> : IEnumerable<T>,IComparable<Range<T>> where T : struct, IComparable<T>
    {
        #region Ctor and props
        private readonly T _lft, _rgt;
        private readonly Func<T, T> _fnFwd, _fnBwd;
        private readonly Func<T, T, int> _fnSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Range&lt;T&gt;"/> struct.
        /// </summary>
        /// <param name="left">The left boundary.</param>
        /// <param name="right">The right boundary.</param>
        /// <param name="fnStepForward">This function takes in an element and returns the one after it in the range</param>
        /// <param name="fnStepBackward">This function takes in an element and returns the one before it in the range.</param>
        /// <param name="fnGetSize">This function takes in the left and right boundaries and returns the range's calculated size.</param>
        public Range(T left, T right, Func<T, T> fnStepForward, Func<T, T> fnStepBackward, Func<T, T, int> fnGetSize)
        {
            if (fnStepForward == null)
            {
                throw new ArgumentNullException("fnStepForward");
            }
            if (fnStepBackward == null)
            {
                throw new ArgumentNullException("fnStepBackward");
            }
            if (fnGetSize == null)
            {
                throw new ArgumentNullException("fnGetSize");
            }
            _lft = left;
            _rgt = right;
            _fnFwd = fnStepForward;
            _fnBwd = fnStepBackward;
            _fnSize = fnGetSize;
        }

        /// <summary>
        /// Gets the size of the range (how many elements does it have).
        /// </summary>
        public int Size { get { return _fnSize(_lft, _rgt); } }

        /// <summary>
        /// Gets the left boundary.
        /// </summary>
        public T Left { get { return _lft; } }

        /// <summary>
        /// Gets the right boundary.
        /// </summary>
        public T Right { get { return _rgt; } } 
        #endregion

        #region Spatial relation to other ranges or points
        /// <summary>
        /// Is this point somewhere within the boundaries of this range?
        /// NOTE:this does not guarantee that the point will be iterated through
        /// </summary>
        public bool Contains(T point)
        {
            return Left.CompareTo(point) <= 0 &&
                    point.CompareTo(Right) <= 0;
        }

        /// <summary>
        /// Does this range contain the other one?
        /// </summary>
        public bool Contains(Range<T> other)
        {
            return Left.CompareTo(other.Left) <= 0 &&
                   other.Right.CompareTo(Right) <= 0;
        }

        /// <summary>
        /// Does this range intersects with the other one?
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Intersect(Range<T> other)
        {
            return Contains(other.Left) ||
                   Contains(other.Right) ||
                   Contains(other);
        }

        /// <summary>
        /// Is it true that these 2 ranges have nothing in common?
        /// </summary>
        public bool Exclude(Range<T> other)
        {
            return other.Left.CompareTo(Right) > 0 ||
                   other.Right.CompareTo(Left) < 0;
        } 
        #endregion

        #region Splitting bannana
        /// <summary>
        /// Divide the range at the provided middle point.
        /// Example: [0..9].Divide(5) = [[0..5][6..9]]
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public IEnumerable<Range<T>> Divide(T point)
        {
            if (Size == 0)
            {
                yield return this;
            }
            else
            {
                if (Contains(point))
                {
                    yield return Project(Left, point);
                    yield return Project(_fnFwd(point), Right);
                }
                else
                {
                    yield return this;
                }
            }
        }

        /// <summary>
        /// Divide the range into constituents containing parts or the whole of the other range and itself.
        /// Example: [0..9].Divide([4..6]) = [[0..4],[4..6],[7..9]]
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public IEnumerable<Range<T>> Divide(Range<T> other)
        {
            /*
            Cases for this / other:

             * other.Exclude(this) ==> this
            _________|___________________|_______ #
            ____|__|_____________________________ +
            _________|___________________|_______ = 1p
            
            _________|___________________|_______ #
            _________________________________|__| +
            _________|___________________|_______ = 1p
            
             * other.Contains(this) ==> this
            _________|___________________|_______ #
            ______|__________________________|___ +
            _________|___________________|_______ = 1p
            
            _________________|___________________ #
            _________________|___________________ + 
            _________________|___________________ = 1p
            
            _________|___________________|_______ #
            _________|___________________|_______ + 
            _________|___________________|_______ = 1p

             * other.Contains(this.Left) ==> [this.Left..other.Right][other.Right+1..this.Right]
            _________|___________________|_______ #
            ______|__|___________________________ + 
            _________||__________________|_______ = 2p

            _________|___________________|_______ #
            ______|_____|________________________ + 
            _________|__||_______________|_______ = 2p

            _________|___________________|_______ #
            _________|__|________________________ +
            _________|__||_______________|_______ = 2p

            _________|___________________|_______ #
            _________|___________________________ * 
            _________||__________________|_______ = 2p
    
             * other.Contains(this.Right) ==> [this.Left..other.Left-1][other.Left..this.Right]
            _________|___________________|_______ #
            __________________________|__|_______ + 
            _________|_______________||__|_______ = 2p

            _________|___________________|_______ #
            __________________________|_____|____ + 
            _________|_______________||__|_______ = 2p

            _________|___________________|_______ #
            _____________________________|__|____ +
            _________|__________________||_______ = 2p

            _________|___________________|_______ #
            _____________________________|_______ +
            _________|__________________||_______ = 2p
    
             * this.Contains(other) ==> [this.Left..other.Left-1][other.Left..other.Right][other.Right+1..this.Right]
            _________|___________________|_______ #
            _______________|__|__________________ + 
            _________|____||__||_________|_______ = 3p
 
            _________|___________________|_______ #
            __________________|__________________ + 
            _________|_______|||_________|_______ = 3p 

            */

            if (other.Size == 0 || Size == 0 || other.Contains(this) || other.Exclude(this))
            {
                yield return this;
            }
            else
            {
                if (other.Contains(Left))
                {
                    yield return Project(Left, other.Right);
                    yield return Project(_fnFwd(other.Right), Right);
                }
                else if (other.Contains(Right))
                {
                    yield return Project(Left, _fnBwd(other.Left));
                    yield return Project(other.Left, Right);
                }
                else if (Contains(other))
                {
                    yield return Project(Left, _fnBwd(other.Left));
                    yield return other;
                    yield return Project(_fnFwd(other.Right), Right);
                }
                else
                {
                    throw new InvalidOperationException("Should never reach this point");
                }
            }
        }

        /// <summary>
        /// Splits the range into many parts.
        /// Example: [0..9].Divide([1..1], [4..6]) = [[0..0],[1..1],[2..3],[4..6],[7..9]]
        /// NOTE: the ranges to divide by MUST be sorted in ascending order before division
        /// </summary>
        /// <param name="others">The others.</param>
        public IEnumerable<Range<T>> Divide(params Range<T>[] others)
        {
            return Divide(others.AsEnumerable());
        }

        /// <summary>
        /// Splits the range into many parts.
        /// Example: [0..9].Divide([1..1], [4..6]) = [[0..0],[1..1],[2..3],[4..6],[7..9]]
        /// NOTE: the ranges to divide by MUST be sorted in ascending order before division
        /// </summary>
        /// <param name="others">The others.</param>
        public IEnumerable<Range<T>> Divide(IEnumerable<Range<T>> others)
        {
            if (others == null)
            {
                yield return this;
            }
            else
            {
                others = others.OrderBy(x => x);
                var container = this;
                foreach (var range in others)
                {
                    var parts = container.Divide(range).ToList();
                    if (parts.Count <= 1)
                    {
                        continue;
                    }
                    for (var i = 0; i < parts.Count - 1; i++)
                    {
                        yield return parts[i];
                    }
                    container = parts[parts.Count - 1];
                }
                yield return container;
            }
        }
        #endregion

        #region Project (create another range on the same axis)
        /// <summary>
        /// Create another range based on this one (basically, project me to another part of the axis)
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public Range<T> Project(T left, T right)
        {
            return new Range<T>(left, right, _fnFwd, _fnBwd, _fnSize);
        } 
        #endregion

        #region Override IEnumerable
        /// <summary>
        /// Returns an enumerator that iterates through the sequence of elements in this range.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = Left; i.CompareTo(Right) <= 0; i = _fnFwd(i))
            {
                yield return i;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the sequence of elements in this range.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        } 
        #endregion

        #region Override object

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This object is less than the <paramref name="other"/> parameter.
        /// Zero
        /// This object is equal to <paramref name="other"/>.
        /// Greater than zero
        /// This object is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(Range<T> other)
        {
            int result;
            if (Intersect(other))
            {
                result = 0;
            }
            else if (Right.CompareTo(other.Left) < 0)
            {
                result = -1;
            }
            else if (Left.CompareTo(other.Right) > 0)
            {
                result = 1;
            }
            else
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this range.
        /// </summary>
        public override string ToString()
        {
            return "[" + Left + ".." + Right + "]";
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
            if (obj.GetType() != typeof(Range<T>)) return false;
            return Equals((Range<T>)obj);
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
            return other._lft.Equals(_lft) && other._rgt.Equals(_rgt);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _lft.GetHashCode();
                result = (result * 397) ^ _rgt.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Range<T> left, Range<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Range<T> left, Range<T> right)
        {
            return !left.Equals(right);
        } 
        #endregion

        #region Static Factories
        /// <summary>
        /// Dateses the specified from.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public static Range<DateTime> Dates(DateTime from, DateTime to)
        {
            return new Range<DateTime>( left:           from.Date,
                                        right:          to.Date,
                                        fnStepBackward: x => x.Date.AddDays(-1),
                                        fnStepForward:  x => x.Date.AddDays(1),
                                        fnGetSize:      (x, y) => x.Date > y.Date 
                                                                    ? 0 
                                                                    : (y.Date - x.Date).Days + 1);
        }

        /// <summary>
        /// Numerics the specified from.
        /// </summary>
        /// <param name="left">The left bound.</param>
        /// <param name="right">The right bound.</param>
        /// <returns></returns>
        public static Range<int> Numeric(int left, int right)
        {
            return new Range<int>(  left: left, 
                                    right: right,
                                    fnStepBackward  : x => x - 1, 
                                    fnStepForward   : x => x + 1, 
                                    fnGetSize       : (x, y) => x > y 
                                                                    ? 0 
                                                                    : y - x + 1);
        }
        #endregion
    }
}