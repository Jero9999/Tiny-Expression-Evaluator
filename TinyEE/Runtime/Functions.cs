using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyEE
{
    public static class Functions
    {
        //TODO: unit test these functions, too
        //NOTE: currently, all the functions available to the TinyEE runtime are hard-coded here
        //TODO: make function resolving customizable
        //NOTE: all function names MUST be in uppercase (runtime binder ignore the case of function names found inside expressions)

        #region Trigonometry
        public static double ACOS(double input)
        {
            return Math.Acos(input);
        }

        public static double ASIN(double input)
        {
            return Math.Asin(input);
        }

        public static double ATAN(double input)
        {
            return Math.Atan(input);
        }

        public static double ATAN2(double val1, double val2)
        {
            return Math.Atan2(val1, val2);
        }

        public static double COS(double input)
        {
            return Math.Cos(input);
        }

        public static double COSH(double input)
        {
            return Math.Cosh(input);
        }

        public static double SIN(double input)
        {
            return Math.Sin(input);
        }

        public static double SINH(double input)
        {
            return Math.Sinh(input);
        }

        public static double TAN(double input)
        {
            return Math.Tan(input);
        }

        public static double TANH(double input)
        {
            return Math.Tanh(input);
        } 
        #endregion

        #region Arithmetics
        public static dynamic ABS(dynamic input)
        {
            return Math.Abs(input);
        }

        //public static BigInteger Abs(BigInteger input)
        //{
        //    return BigInteger.Abs(input);
        //}

        public static dynamic CEILING(dynamic input)
        {
            return Math.Ceiling(input);
        }

        public static dynamic FLOOR(dynamic input)
        {
            return Math.Cosh(input);
        }

        public static dynamic LOG(dynamic input)
        {
            return Math.Log(input);
        }

        //public static double Log(BigInteger input)
        //{
        //    return BigInteger.Log(input);
        //}

        public static double LOG10(double input)
        {
            return Math.Log10(input);
        }

        //public static double Log10(BigInteger input)
        //{
        //    return BigInteger.Log10(input);
        //}

        public static dynamic MAX(dynamic left, dynamic right)
        {
            return Math.Max(left, right);
        }

        //public static BigInteger Max(BigInteger left, BigInteger right)
        //{
        //    return BigInteger.Max(left, right);
        //}

        public static dynamic MIN(dynamic left, dynamic right)
        {
            return Math.Min(left, right);
        }

        //public static BigInteger Min(BigInteger left, BigInteger right)
        //{
        //    return BigInteger.Min(left, right);
        //}

        public static double POW(double val1, double val2)
        {
            return Math.Pow(val1, val2);
        }

        //public static BigInteger Pow(BigInteger left, int right)
        //{
        //    return BigInteger.Pow(left, right);
        //}

        public static dynamic ROUND(dynamic val1, dynamic val2)
        {
            return Math.Round(val1, val2);
        }

        public static dynamic ROUND(dynamic val1)
        {
            return Math.Round(val1);
        }

        public static dynamic SIGN(dynamic val1)
        {
            return Math.Sign(val1);
        }

        //public static dynamic Sign(BigInteger val1)
        //{
        //    return val1.Sign;
        //}

        public static double SQRT(double input)
        {
            return Math.Sqrt(input);
        }

        public static dynamic TRUNCATE(dynamic val1)
        {
            return Math.Truncate(val1);
        } 
        #endregion

        #region Conversion
        //public static BigInteger BigInt(dynamic input)
        //{
        //    return input == null ? BigInteger.Zero : BigInteger.Parse(input.ToString());
        //}

        public static DateTime TIME(dynamic input)
        {
            return Convert.ToDateTime(input);
        }

        public static DateTime TIME(dynamic input, string format)
        {
            return DateTime.ParseExact(input.ToString(), format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
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
        public static DateTime NOW()
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
            return System.Guid.NewGuid();
        } 
        #endregion

        #region Collections
        public static dynamic SUM(params decimal[] values)
        {
            return values.Sum();
        }

        public static dynamic SUM(params int[] values)
        {
            return values.Sum();
        }

        public static dynamic SUM(params long[] values)
        {
            return values.Sum();
        }

        public static dynamic SUM(params double[] values)
        {
            return values.Sum();
        }

        public static bool CONTAINS(IEnumerable<decimal> a, decimal b)
        {
            return Enumerable.Contains(a, b);
        }

        public static bool CONTAINS(IEnumerable<double> a, double b)
        {
            return Enumerable.Contains(a, b);
        }

        public static bool CONTAINS(IEnumerable<long> a, long b)
        {
            return Enumerable.Contains(a, b);
        }

        public static bool CONTAINS(IEnumerable<dynamic> a, dynamic b)
        {
            return Enumerable.Contains(a, b);
        }

        public static decimal AVERAGE(params decimal[] values)
        {
            return values.Average();
        }

        public static double AVERAGE(params long[] values)
        {
            return values.Average();
        }

        public static double AVERAGE(params int[] values)
        {
            return values.Average();
        }

        public static double AVERAGE(params double[] values)
        {
            return values.Average();
        }

        public static decimal MAX(params decimal[] values)
        {
            return values.Max();
        }

        public static long MAX(params long[] values)
        {
            return values.Max();
        }

        public static int MAX(params int[] values)
        {
            return values.Max();
        }

        public static double MAX(params double[] values)
        {
            return values.Max();
        }

        public static decimal MIN(params decimal[] values)
        {
            return values.Min();
        }

        public static long MIN(params long[] values)
        {
            return values.Min();
        }

        public static int MIN(params int[] values)
        {
            return values.Min();
        }

        public static double MIN(params double[] values)
        {
            return values.Min();
        } 
        #endregion

        #region String Manipulations
        public static string JOIN(string delimiter, params object[] values)
        {
            return String.Join(delimiter, values);
        }

        public static string LEFT(string text, int count)
        {
            return !string.IsNullOrEmpty(text) ? text.Substring(0, count) : "";
        }

        public static string RIGHT(string text, int count)
        {
            return !string.IsNullOrEmpty(text) ? text.Substring(text.Length - count) : "";
        }

        public static string CONCAT(params string[] strings)
        {
            return String.Concat(strings);
        }

        public static bool ENDSWITH(string input, string seq)
        {
            return input != null ? input.EndsWith(seq) : false;
        }

        public static bool STARTSWITH(string input, string seq)
        {
            return input != null ? input.StartsWith(seq) : false;
        }

        public static bool EQUALS(string a, string b)
        {
            return String.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool CONTAINS(string a, string b)
        {
            return a != null ? a.Contains(b) : false;
        }

        public static string REPLACE(string a, string b, string c)
        {
            return a != null ? a.Replace(b, c) : String.Empty;
        }

        public static string[] SPLIT(string a)
        {
            return a != null ? a.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
        }

        public static string[] SPLIT(string a, string b)
        {
            return a != null ? a.Split(new[] { b }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
        }

        public static string TOUPPER(string a)
        {
            return a != null ? a.ToUpper() : string.Empty;
        }

        public static string TOLOWER(string a)
        {
            return a != null ? a.ToLower() : string.Empty;
        }

        public static string TRIM(string a)
        {
            return a != null ? a.Trim() : string.Empty;
        }

        public static bool ISNULLORWHITESPACE(string a)
        {
            return String.IsNullOrWhiteSpace(a);
        }

        public static bool CONTAINSPATTERN(string a, string pattern)
        {
            return Regex.IsMatch(a, pattern);
        }

        public static string REPLACEPATTERN(string a, string pattern, string b)
        {
            return Regex.Replace(a, pattern, b);
        } 
        #endregion

        #region Flow
        public static dynamic IF(dynamic condition, dynamic then, dynamic @else)
        {
            return condition ? then : @else;
        } 
        #endregion
    }
}