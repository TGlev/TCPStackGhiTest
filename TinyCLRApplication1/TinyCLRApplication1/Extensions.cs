using System.Collections;
using System.Text;
using System;

namespace TinyCLRApplication1
{
    public static class Extensions
    {
        public static bool StartsWith(this string s, string value)
        {
            return s.IndexOf(value) == 0;
        }

        public static bool Contains(this string s, string value)
        {
            return s.IndexOf(value) > 0;
        }

        /// <summary>
        /// Replace all occurances of the 'find' string with the 'replace' string.
        /// </summary>
        /// <param name="content">Original string to operate on</param>
        /// <param name="find">String to find within the original string</param>
        /// <param name="replace">String to be used in place of the find string</param>
        /// <returns>Final string after all instances have been replaced.</returns>
        /// Credit: https://github.com/Alex-developer/NETMF.CommonExtensions/blob/master/NetMf.CommonExtensions/StringExtensions.cs
        public static string Replace(this string content, string find, string replace)
        {
            int startFrom = 0;
            int findItemLength = find.Length;

            int firstFound = content.IndexOf(find, startFrom);
            StringBuilder returning = new StringBuilder();

            string workingString = content;

            while ((firstFound = workingString.IndexOf(find, startFrom)) >= 0)
            {
                returning.Append(workingString.Substring(0, firstFound));
                returning.Append(replace);

                // the remaining part of the string.
                workingString = workingString.Substring(firstFound + findItemLength,
                    workingString.Length - (firstFound + findItemLength));
            }

            returning.Append(workingString);

            return returning.ToString();

        }

        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }

        public static double ToDegrees(this double val)
        {
            return (180 / Math.PI) * val;
        }

        public static double Map(this double value, double fromSource, double toSource, double fromTarget,
            double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static double Map(this int value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return Map((double)value, fromSource, toSource, fromTarget, toTarget);
        }

        public static bool Between(this double value, double a, double b)
        {
            return value >= a && value <= b;
        }

        public static bool Between(this int value, double a, double b)
        {
            return value >= a && value <= b;
        }

        public static double Constrain(this double value, double min, double max)
        {
            if (value < min)
                value = min;
            if (value > max)
                value = max;
            return value;
        }

        public static int Constrain(this int value, int min, int max)
        {
            if (value < min)
                value = min;
            if (value > max)
                value = max;
            return value;
        }

        public static double Multiply(this double value, double multiplier)
        {
            return value * multiplier;
        }

        public static int AngleDifference(this int angle1, int angle2)
        {
            int diff = (angle2 - angle1 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }

        public static double AngleDifference(this double angle1, double angle2)
        {
            double diff = (angle2 - angle1 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }

        public static byte[] ToIpByteArray(this string stringIp)
        {
            var ip = new byte[4];
            var splitIp = stringIp.Split('.');

            for (var i = 0; i < 4; i++)
                ip[i] = byte.Parse(splitIp[i]);

            return ip;
        }

        public static int MillisecondsAgo(this DateTime time)
        {
            return (int) (DateTime.Now - time).TotalMilliseconds;
        }

        public static string ToNumericalValue(this bool value)
        {
            return value ? "1" : "0";
        }
        
        public static int RoundTo(this int value, int roundTo)
        {
            return (int)(Math.Round(value / roundTo) * roundTo);
        }

        public static double RoundTo(this double value, double roundTo)
        {
            return (int)(Math.Round(value / roundTo) * roundTo);
        }

        public static bool ContainsType(this ArrayList list, Type type)
        {
            foreach (var item in list)
            {
                if(item == null) continue;
                if (item.GetType() == type)
                    return true;

            }

            return false;
        }

    }
}