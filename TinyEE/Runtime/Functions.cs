using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyEE
{
    public static class Functions
    {
        //TODO: unit test these functions, too
        //NOTE: currently, all the functions available to the TinyEE runtime are hard-coded here
        //NOTE: all function names MUST be in uppercase (runtime binder ignore the case of function names found inside expressions)

        #region Conversion
        public static DateTime TIME(dynamic input)
        {
            return Convert.ToDateTime(input);
        }

        public static DateTime TIME(string input, string format)
        {
            return DateTime.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static TimeSpan DURATION(string input)
        {
            return TimeSpan.Parse(input);
        }

        public static long INT(dynamic input)
        {
            return Convert.ToInt64(input);
        }

        public static decimal DECIMAL(dynamic input)
        {
            return Convert.ToDecimal(input);
        }

        public static Uri URI(string input)
        {
            return new Uri(input);
        }

        public static string STR(dynamic input)
        {
            return input == null ? String.Empty : input.ToString();
        }

        public static byte[] BYTES(string input)
        {
            return Convert.FromBase64String(input);
        }

        public static string BASE64(byte[] input)
        {
            return Convert.ToBase64String(input);
        }
        #endregion
        
        #region Generator
        public static DateTime TIME()
        {
            return DateTime.Now;
        }

        public static DateTime TODAY()
        {
            return DateTime.Today;
        }

        public static int RND()
        {
            return new Random().Next();
        }

        public static Guid GUID()
        {
            return Guid.NewGuid();
        }

        public static Range<int> NUMBERS(int left, int right, int step = 1)
        {
            return new Range<int>(left, right, x => x + 1, x => x - 1, (x, y) => x > y ? 0 : y - x + 1);
        }

        public static Range<DateTime> DATES(string leftStr, string rightStr)
        {
            DateTime left  = DateTime.Parse(leftStr),
                     right = DateTime.Parse(rightStr);
            return DATES(left, right);
        }

        public static Range<DateTime> DATES(DateTime left, DateTime right)
        {
            return new Range<DateTime>(left, right, x => x.AddDays(1), x => x.AddDays(-1), (x, y) => x.Date > y.Date ? 0 : (y.Date - x.Date).Days + 1);
        }
        #endregion

        #region Collections
        public static IDictionary<string,object> DICTIONARY(KeyValuePair<string,object>[] kvps)
        {
            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static decimal SUMDEC(IEnumerable<dynamic> values)
        {
            return values.Aggregate(0m, (d, seed) => seed + Convert.ToDecimal(d));
        }

        public static long SUM(IEnumerable<dynamic> values)
        {
            return values.Aggregate(0L, (i,seed) => seed + Convert.ToInt64(i));
        }
        #endregion

        #region Flow
        public static dynamic IF(bool condition, dynamic then, dynamic @else)
        {
            return condition ? then : @else;
        }
        #endregion
    }
}