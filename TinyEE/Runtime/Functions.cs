using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TinyEE
{
    /// <summary>
    /// A container for built in functions, some of which are a part of the runtime
    /// </summary>
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

        public static bool BOOLEAN(dynamic input)
        {
            return Convert.ToBoolean(input);
        }

        public static int INT(dynamic input)
        {
            return Convert.ToInt32(input);
        }

        public static int LONG(dynamic input)
        {
            return Convert.ToInt64(input);
        }

        public static int BIGINT(dynamic input)
        {
            return input is string ? BigInteger.Parse(input) : (BigInteger)input;
        }

        public static double DOUBLE(dynamic input)
        {
            return Convert.ToDouble(input);
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

        public static Range<DateTime> DATES(string leftStr, string rightStr)
        {
            DateTime left  = DateTime.Parse(leftStr),
                     right = DateTime.Parse(rightStr);
            return DATES(left, right);
        }

        public static Range<DateTime> DATES(DateTime left, DateTime right)
        {
            return Range<DateTime>.Dates(left, right);
        }
        #endregion

        #region Collections
        public static IDictionary<string,object> DICTIONARY(KeyValuePair<string,object>[] kvps)
        {
            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static dynamic SUM(IEnumerable<dynamic> values)
        {
            return SUM(values.ToArray());
        }

        public static dynamic SUM(params dynamic[] values)
        {
            return values.Aggregate(0, (i, seed) => seed + i);
        }
        #endregion
    }
}