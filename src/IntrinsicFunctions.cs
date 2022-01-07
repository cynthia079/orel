using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orel.Types;
using Newtonsoft.Json.Linq;

namespace Orel
{
    /// <summary>
    /// 标准内置方法
    /// 所有的数据类型，需要提供字符串和Object两个版本
    /// </summary>
    public static class IntrinsicFunctions
    {
        private static readonly ConcurrentDictionary<decimal, TimeZoneInfo> TimeZones = new ConcurrentDictionary<decimal, TimeZoneInfo>();
        private static TimeZoneInfo GetTimeZoneInfo(decimal? timezone)
        {
            decimal tz = timezone ?? 8;
            if (!TimeZones.TryGetValue(tz, out TimeZoneInfo timeZoneInfo))
            {
                string timezoneName = $"ORELTZ-{timezone}";
                timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(timezoneName, TimeSpan.FromHours((double)tz), timezoneName, timezoneName);
                TimeZones.TryAdd(tz, timeZoneInfo);
            }
            return timeZoneInfo;
        }

        #region DateTime        
        [MethodName("now")]
        public static DateTimeOffset? Now()
        {
            return Now(8);
        }

        [MethodName("now")]
        public static DateTimeOffset? Now(decimal? timezone)
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, GetTimeZoneInfo(timezone));
        }

        [MethodName("date", true)]
        public static DateTimeOffset? ToDate([Fallback] object value)
        {
            if (value == null)
                return null;
            if (value?.GetType() == typeof(DateTimeOffset))
            {
                return (DateTimeOffset)value;
            }
            if (value is JValue jValue && jValue.Type == JTokenType.Date)
            {
                return jValue.ToObject<DateTimeOffset>();
            }
            return ToDate(value?.ToString(), 8);
        }

        [MethodName("date")]
        public static DateTimeOffset? ToDate(string value)
        {
            return ToDate(value, 8);
        }

        [MethodName("date", true)]
        public static DateTimeOffset? ToDate([Fallback] object value, decimal? timezone)
        {
            return ToDate(value?.ToString(), timezone);
        }

        static string[] more_formats = new string[] {
            "ddd MMM dd HH:mm:ss zz00 yyyy",
            "ddd MMM dd HH:mm:ss zzz yyyy",
        };

        [MethodName("date")]
        public static DateTimeOffset? ToDate(string value, decimal? timezone)
        {
            DateTimeOffset result;
            if ((value.Contains("+") || value.Contains("Z", StringComparison.InvariantCultureIgnoreCase))
                && DateTimeOffset.TryParse(value, out result)) //如果含有时区信息，直接按时区转换返回，如果指定时区，则变换为新的时区
            {
                if (timezone.HasValue && result.Offset.Hours != timezone)
                {
                    return TimeZoneInfo.ConvertTime(result, GetTimeZoneInfo(timezone));
                }
                return result;
                //if (DateTimeOffset.TryParse(value, out DateTimeOffset result))
                //{
                //    return result;
                //}                
            }
            else if (DateTime.TryParse(value, out DateTime datetime))
            {
                return new DateTimeOffset(datetime, TimeSpan.FromHours((double)timezone));
            }
            else
            {
                var provider = CultureInfo.InvariantCulture.DateTimeFormat;
                if (DateTimeOffset.TryParseExact(value, more_formats, provider, DateTimeStyles.AllowWhiteSpaces, out result))
                {
                    if (timezone.HasValue && result.Offset.Hours != timezone)
                    {
                        return TimeZoneInfo.ConvertTime(result, GetTimeZoneInfo(timezone));
                    }
                    return result;
                }
            }
            return null;
        }

        [MethodName("date")]
        public static DateTimeOffset? ToDateFromUnixTimestamp(decimal? unixTimestamp)
        {
            return ToDateFromUnixTimestamp(unixTimestamp, 8);
        }

        [MethodName("date")]
        public static DateTimeOffset? ToDateFromUnixTimestamp(decimal? unixTimestamp, decimal? timezone)
        {
            if (unixTimestamp == null)
                return null;
            var baseTime = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.FromHours(0));
            DateTimeOffset dateTime;
            if (unixTimestamp >= 1000000000000)
            {
                //13位Timestamp识别为毫秒
                dateTime = baseTime.AddMilliseconds((double)unixTimestamp.Value);
            }
            else
            {
                dateTime = baseTime.AddSeconds((double)unixTimestamp.Value);
            }
            return TimeZoneInfo.ConvertTime(dateTime, GetTimeZoneInfo(timezone));
        }

        /// <summary>
        /// 返回10位ts
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [MethodName("ts")]
        public static decimal? ToUnixTimestamp(DateTimeOffset? dateTime)
        {
            if (dateTime == null)
                return null;
            var baseTime = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.FromHours(0));
            return (long)(dateTime.Value - baseTime).TotalSeconds; //TotalSeconds带小数，所以需要取整
        }

        /// <summary>
        /// 返回13位带毫秒数的ts
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [MethodName("ts_ms")]
        public static decimal? ToUnixTimestampWithMillisecond(DateTimeOffset? dateTime)
        {
            if (dateTime == null)
                return null;
            var baseTime = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.FromHours(0));
            return (long)(dateTime.Value - baseTime).TotalMilliseconds; //TotalMilliseconds带小数，所以需要取整
        }

        [MethodName("today")]
        public static DateTimeOffset? Today()
        {
            return Today(8);
        }

        [MethodName("today")]
        public static DateTimeOffset? Today(decimal? timezone)
        {
            DateTimeOffset date = Now(timezone).Value;
            return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset);
        }

        [MethodName("date_fmt")]
        public static string DateFormat(DateTimeOffset? dateTimeOffset, string format)
        {
            if (dateTimeOffset == null)
            {
                return null;
            }

            return dateTimeOffset.Value.ToString(format);
        }

        [MethodName("date_part")]
        public static decimal? DatePart(DateTimeOffset? dateTimeOffset, string part)
        {
            if (dateTimeOffset == null)
            {
                return 0;
            }

            DateTimeOffset dt = dateTimeOffset.Value;
            switch (part)
            {
                case "y":
                case "Y":
                    return dt.Year;
                case "M":
                    return dt.Month;
                case "D":
                case "d":
                    return dt.Day;
                case "H":
                case "h":
                    return dt.Hour;
                case "m":
                    return dt.Minute;
                case "s":
                case "S":
                    return dt.Second;
                default:
                    return null;
            }
        }

        [MethodName("day_span")]
        public static List<DateTimeRange> DaySpans(DateTimeOffset? dateTimeStart, DateTimeOffset? dateTimeEnd)
        {
            return GetTimeSpans(dateTimeStart, dateTimeEnd, current => current.AddDays(1).Date);
        }

        [MethodName("hour_span")]
        public static List<DateTimeRange> HourSpans(DateTimeOffset? dateTimeStart, DateTimeOffset? dateTimeEnd)
        {
            return GetTimeSpans(dateTimeStart, dateTimeEnd, current => current.AddHours(1));
        }

        /// <summary>
        /// 获取2个日期之间按指定单位产生的时间间隔
        /// </summary>
        /// <param name="dateTimeStart"></param>
        /// <param name="dateTimeEnd"></param>
        /// <returns></returns>
        private static List<DateTimeRange> GetTimeSpans(DateTimeOffset? dateTimeStart, DateTimeOffset? dateTimeEnd, Func<DateTimeOffset, DateTimeOffset> splitFunc)
        {
            var list = new List<DateTimeRange>();
            if (dateTimeStart == null || dateTimeEnd == null)
            {
                return list;
            }
            var current = dateTimeStart.Value;
            while (true)
            {
                var next = splitFunc(current);
                if (next >= dateTimeEnd)
                {
                    list.Add(new DateTimeRange() { Start = current, End = dateTimeEnd });
                    return list;
                }
                list.Add(new DateTimeRange() { Start = current, End = next });
                current = next;
            }
        }

        [MethodName("date2", true)]
        public static DateTimeOffset? ToDateExt(object value)
        {
            return ToDateExt(value, 8);
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt(string value)
        {
            return ToDateExt(value, 8);
        }

        [MethodName("date2", true)]
        public static DateTimeOffset? ToDateExt(object value, decimal? timezone)
        {
            return ToDateExt(value?.ToString(), timezone);
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt(string value, decimal? timezone)
        {
            return ToDateExt2(value, Now(timezone).Value, timezone);
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt2(string value, string refDateTime)
        {
            return ToDateExt2(value, ToDate(refDateTime));
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt2(string value, string refDateTime, decimal? timezone)
        {
            return ToDateExt2(value, ToDate(refDateTime, timezone));
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt2(string value, DateTimeOffset? refDateTime)
        {
            return ToDateExt2(value, refDateTime, refDateTime?.Offset.Hours ?? 8);
        }

        [MethodName("date2")]
        public static DateTimeOffset? ToDateExt2(string value, DateTimeOffset? refDateTime, decimal? timezone)
        {
            if (value == null)
            {
                return null;
            }
            //尝试处理标准格式日期
            DateTimeOffset? dto = ToDate(value, timezone);
            if (dto != null)
            {
                return dto;
            }
            //非标准格式处理            
            //N小时前等类型的数据
            var refTime = refDateTime ?? Now(timezone).Value;
            Match match = Regex.Match(value, @"(\d+)\s*([天月周时分秒年]|小时|分钟|秒钟|星期)前");
            if (match.Success)
            {
                int offset = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;
                switch (unit)
                {
                    case "时":
                    case "小时":
                        return refTime.AddHours(-offset);
                    case "分":
                    case "分钟":
                        return refTime.AddMinutes(-offset);
                    case "秒钟":
                    case "秒":
                        return refTime.AddSeconds(-offset);
                    case "天":
                        return refTime.AddDays(-offset);
                    case "月":
                        return refTime.AddMonths(-offset);
                    case "周":
                    case "星期":
                        return refTime.AddDays(-offset * 7);
                    case "年":
                        return refTime.AddYears(-offset);
                }
            }
            //描述型日期
            match = Regex.Match(value, @"(昨[天日]|前[天日]|今[天日])\s*([\d时点分秒：:]*)");
            if (match.Success)
            {
                string dayText = match.Groups[1].Value.Replace("：", ":");
                int dayOffset = dayText.StartsWith("昨") ? -1 : (dayText.StartsWith("前") ? -2 : 0);
                DateTimeOffset day = refTime.AddDays(dayOffset);
                int hour = 0, minute = 0, second = 0;
                if (match.Groups.Count > 2)
                {
                    if (DateTime.TryParse(match.Groups[2].Value.Replace("点", "时"), out DateTime time))
                    {
                        hour = time.Hour;
                        minute = time.Minute;
                        second = time.Second;
                    }
                    return new DateTimeOffset(day.Year, day.Month, day.Day, hour, minute, second, TimeSpan.FromHours((double)timezone));
                }
            }
            //特殊文字
            if (value.Contains("刚刚") || value.Contains("刚才"))
            {
                return refTime;
            }
            //处理含有非标准字符的日期文本，剔除掉前后无关字符后，先尝试转化
            string dateVal = GetDateValueBody(value);
            //整理不规范字符            
            dateVal = Regex.Replace(dateVal, "[年月日：]", m =>
            {
                switch (m.Value)
                {
                    case "年":
                    case "月":
                        return "-";
                    case "：":
                        return ":";
                    default:
                        return string.Empty;
                }
            });
            if (!string.IsNullOrEmpty(dateVal))
            {
                dto = ToDate(dateVal, timezone);
                if (dto != null)
                {
                    return dto;
                }
            }
            //缺少年份信息的数据，以当前年份补充
            match = Regex.Match(dateVal, @"\d{1,2}[-\/]\d{1,2}(\s+\d{1,2}:\d{1,2}(:\d{1,2})*)?");
            if (match.Success)
            {
                dateVal = $"{refTime.Year}-{match.Value}";
                return ToDate(dateVal, timezone);
            }
            return null;
        }

        private static char[] _dateUnit = new[] { '日', '分', '秒' };

        /// <summary>
        /// 获取日期元素的Body
        /// </summary>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        private static string GetDateValueBody(string dateValue)
        {
            var span = dateValue.AsSpan();
            int i = 0;
            int j = span.Length - 1;
            while (i < span.Length && (span[i] < '0' || span[i] > '9')) i++;
            while (j >= 0 && (span[j] < '0' || span[j] > '9') && !_dateUnit.Contains(span[j])) j--;
            if (i > j)
            {
                return string.Empty;
            }
            return span.Slice(i, j - i + 1).ToString();
        }

        [MethodName("date_add")]
        public static DateTimeOffset? DateAdd(DateTimeOffset? dateTimeOffset, string offset, bool IsMinus)
        {
            if (dateTimeOffset == null)
            {
                return null;
            }
            DateTimeOffset dt = dateTimeOffset.Value;
            var dateAdd = GetDateTimeOffsetCaculator(offset, IsMinus);
            dt = dateAdd(dt);
            return dt;
        }

        private static Func<DateTimeOffset, DateTimeOffset> GetDateTimeOffsetCaculator(string offset, bool isMinus)
        {
            MatchCollection matches = Regex.Matches(offset, @"(\d+)([YyMDdHhmSs])");
            if (matches.Count == 0)
            {
                throw new ArgumentException($"date offset:{offset} is an invalid argument", offset);
            }
            List<Func<DateTimeOffset, DateTimeOffset>> steps = new List<Func<DateTimeOffset, DateTimeOffset>>();
            foreach (IGrouping<string, (int num, string unit)> group in matches.Select(m => (num: int.Parse(m.Groups[1].Value), unit: m.Groups[2].Value)).GroupBy(m => m.unit))
            {
                if (group.Count() > 1)
                {
                    throw new ArgumentException($"date offset:{offset} is an invalid argument", offset);
                }
                (int num, string unit) item = group.First();
                int num = isMinus ? -item.num : item.num;
                switch (item.unit)
                {
                    case "Y":
                    case "y":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddYears(num)));
                        break;
                    case "M":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddMonths(num)));
                        break;
                    case "D":
                    case "d":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddDays(num)));
                        break;
                    case "H":
                    case "h":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddHours(num)));
                        break;
                    case "m":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddMinutes(num)));
                        break;
                    case "S":
                    case "s":
                        steps.Add(new Func<DateTimeOffset, DateTimeOffset>(dt => dt.AddSeconds(num)));
                        break;
                    default:
                        throw new ArgumentException($"date offset:{offset} is an invalid argument", offset);
                }
            }
            if (steps.Count == 1)
                return steps[0];
            return new Func<DateTimeOffset, DateTimeOffset>(dt =>
            {
                foreach (var fun in steps)
                {
                    dt = fun(dt);
                }
                return dt;
            });
        }

        #endregion

        #region Between
        [MethodName("between", true)]
        public static bool Between(DateTimeOffset? value, DateTimeOffset? lowBoundValue, DateTimeOffset? highBoundValue, bool includeLowBound, bool includeHighBound)
        {
            if (value == null || lowBoundValue == null || highBoundValue == null)
            {
                return false;
            }

            return BetweenImpl(value.Value, lowBoundValue.Value, highBoundValue.Value, includeLowBound, includeHighBound);
        }


        [MethodName("between", true)]
        public static bool Between(decimal? value, decimal? lowBoundValue, decimal? highBoundValue, bool includeLowBound, bool includeHighBound)
        {
            if (value == null || lowBoundValue == null || highBoundValue == null)
            {
                return false;
            }

            return BetweenImpl(value.Value, lowBoundValue.Value, highBoundValue.Value, includeLowBound, includeHighBound);
        }

        private static bool Between(string value, string lowBoundValue, string highBoundValue, bool includeLowBound, bool includeHighBound)
        {
            if (value == null || lowBoundValue == null || highBoundValue == null)
            {
                return false;
            }

            return BetweenImpl(value, lowBoundValue, highBoundValue, includeLowBound, includeHighBound);
        }

        public static bool BetweenImpl(IComparable value, IComparable lowBoundValue, IComparable highBoundValue, bool includeLowBound, bool includeHighBound)
        {
            if (value == null)
            {
                return false;
            }

            int ret1 = value.CompareTo(lowBoundValue);
            bool valid = includeLowBound ? ret1 >= 0 : ret1 > 0;
            if (valid)
            {
                int ret2 = value.CompareTo(highBoundValue);
                valid = includeHighBound ? ret2 <= 0 : ret2 < 0;
            }
            return valid;
        }
        #endregion

        #region NumericType
        [MethodName("num", true)]
        public static decimal? ToNum([FallbackAttribute] object value)
        {
            return ToNum(value?.ToString());
        }

        [MethodName("num")]
        public static decimal? ToNum(string numberStr)
        {
            if (decimal.TryParse(numberStr, out decimal result))
            {
                return result;
            }
            return null;
        }

        [MethodName("num2", true)]
        public static decimal? ToNumExt([FallbackAttribute] object value)
        {
            return ToNumExt(value?.ToString());
        }

        [MethodName("num2")]
        public static decimal? ToNumExt(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
            {
                return null;
            }

            if (decimal.TryParse(numberStr, NumberStyles.Any, CultureInfo.CurrentCulture.NumberFormat, out decimal val))
            {
                return val;
            }
            MatchCollection matches = Regex.Matches(numberStr, @"([\d,\.]+)([万千亿kK])");
            if (matches.Count == 0)
            {
                return null;
            }

            for (int i = matches.Count - 1; i >= 0; i--)
            {
                Match match = matches[i];
                if (!decimal.TryParse(match.Groups[1].Value, out decimal v))
                {
                    return null;
                }

                val = val + v * GetScale(match.Groups[2].Value);
            }
            return val;
        }

        private static int GetScale(string scaleType)
        {
            switch (scaleType)
            {
                case "亿":
                    return 100000000;
                case "万":
                    return 10000;
                case "千":
                case "k":
                case "K":
                    return 1000;
                default:
                    return 1;
            }
        }
        #endregion

        #region Text
        [MethodName("len")]
        public static decimal? TextLength(string value)
        {
            return value?.Length;
        }

        [MethodName("len", true)]
        public static decimal? TextLength([Fallback] object value)
        {
            return TextLength(value?.ToString());
        }

        [MethodName("trim")]
        public static string Trim(string value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Trim();
        }

        [MethodName("trim", true)]
        public static string Trim([Fallback] object value)
        {
            if (value == null)
            {
                return null;
            }

            return Trim(value?.ToString());
        }

        [MethodName("like", true)]
        public static bool Like([Fallback] object value, string matchValue)
        {
            return Like(value?.ToString(), matchValue);
        }

        [MethodName("like", true)]
        public static bool Like(string value, string matchValue)
        {
            if (matchValue == null || value == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(matchValue))
            {
                return string.IsNullOrEmpty(value);
            }

            bool leftMatch = matchValue[0] == '%';
            bool rightMatch = matchValue[matchValue.Length - 1] == '%';

            string target = matchValue.Trim('%');
            if (leftMatch && rightMatch)
            {
                return value.Contains(target, StringComparison.OrdinalIgnoreCase);
            }
            else if (leftMatch)
            {
                return value.EndsWith(target, StringComparison.OrdinalIgnoreCase);
            }
            else if (rightMatch)
            {
                return value.StartsWith(target, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return value.Equals(matchValue, StringComparison.OrdinalIgnoreCase);
            }
        }

        [MethodName("match")]
        public static bool Match(string value, string pattern)
        {
            if (value == null || pattern == null)
                return false;

            return Regex.Match(value, pattern).Success;
        }

        [MethodName("match", true)]
        public static bool Match([Fallback] object value, string pattern)
        {
            return Match(value?.ToString(), pattern);
        }

        [MethodName("replace")]
        public static string Replace(string value, string pattern, string replacement)
        {
            if (value == null || pattern == null || replacement == null)
                return null;

            return Regex.Replace(value, pattern, replacement);
        }

        [MethodName("replace", true)]

        public static string Replace([Fallback] object value, string pattern, string replacement)
        {
            return Replace(value?.ToString(), pattern, replacement);
        }

        [MethodName("extr")]
        //[MethodDescriptor("正则匹配文本，指定位置的匹配文本段", "匹配到的文本段", "目标文本", "正则", "获取位置")]
        public static string Extract(string value, string pattern, decimal? index)
        {
            if (value == null || pattern == null)
                return null;

            var match = Regex.Match(value, pattern);
            if (match.Success)
            {
                return match.Groups[(int)index].Value;
            }
            return null;
        }

        [MethodName("extr")]
        //[MethodDescriptor("获取所有正则比配的文本", "匹配到的文本段集合", "目标文本", "正则")]
        public static IList ExtractAll(string value, string pattern)
        {
            if (value == null || pattern == null)
                return null;

            var match = Regex.Match(value, pattern);
            if (match.Success)
            {
                return match.Groups.Values.Select(g => g.Value).ToArray();
            }
            return null;
        }
        #endregion

        #region List
        [MethodName("split")]
        public static IList Split(string value, string seperator)
        {
            return value?.Split(seperator);
        }

        [MethodName("range")]
        public static IList<DateTimeOffset> Range(DateTimeOffset? start, DateTimeOffset? stop, string step)
        {
            if (start == null || stop == null || string.IsNullOrEmpty(step))
                return Enumerable.Empty<DateTimeOffset>().ToList();
            var list = new List<DateTimeOffset>();
            var isMinus = step.StartsWith('-');
            var addFunc = GetDateTimeOffsetCaculator(step, isMinus);
            for (DateTimeOffset i = start.Value; i < stop.Value; i = addFunc(i))
                list.Add(i);

            return list;
        }

        [MethodName("range")]
        public static IList<decimal> Range(decimal? start, decimal? stop, decimal? step)
        {
            if (start == null || stop == null || step == null)
                return Enumerable.Empty<decimal>().ToList();
            var list = new List<decimal>();
            for (decimal i = start.Value; i < stop.Value; i += step.Value)
                list.Add(i);
            return list;
        }

        [MethodName("range")]
        public static IList<decimal> Range(decimal? start, decimal? stop)
        {
            return Range(start, stop, 1);
        }

        /// <summary>
        /// 将所有list中的元素意义合并到同一个列表中的一个元素
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        [MethodName("zip")]
        public static IList Zip(params IList[] lists)
        {
            var maxLength = lists.Max(s => s.Count);
            var newList = new List<List<object>>();
            for (int i = 0; i < maxLength; i++)
            {
                var tuple = new List<object>();
                foreach (var list in lists)
                {
                    tuple.Add(i >= list.Count ? null : list[i]);
                }
                newList.Add(tuple);
            }
            return newList;
        }

        /// <summary>
        /// 获取2个集合元素的直积（笛卡尔积）
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        [MethodName("product")]
        public static IList GetCartesianProduct(IList list1, IList list2)
        {
            var list = new List<IList>();
            foreach (var item1 in list1)
                foreach (var item2 in list2)
                {
                    list.Add(new List<object>() { item1, item2 });
                }
            return list;
        }
        #endregion

        #region General

        [MethodName("len")]
        public static decimal? ListLength(IList list)
        {
            if (list != null)
            {
                return list.Count;
            }
            return null;
        }

        [MethodName("text", true)]
        public static string ToText([Fallback] object value)
        {
            return value?.ToString();
        }

        [MethodName("text")]
        public static string ToText(decimal? value)
        {
            return value?.ToString();
        }

        [MethodName("text", true)]
        public static string ToText(int value)
        {
            return value.ToString();
        }

        [MethodName("text")]
        public static string ToText(DateTimeOffset? value)
        {
            return value?.ToString();
        }

        public static IList ToList(object value)
        {
            return value as IList;
        }

        /// <summary>
        /// 按Index访问列表，Index的基数为1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object ListIndex(IList list, decimal? index)
        {
            if (list == null)
            {
                return null;
            }
            if (index == null)
            {
                return null;
            }
            var len = list.Count;
            int i = index < 0 ? len + (int)index : (int)index - 1;//负数从底部查找，正数1表示第0个元素            
            if (i < 0 || i >= list.Count)
            {
                //throw new ArgumentOutOfRangeException("index");
                return null; //如果index越界，返回空
            }
            return list[i];
        }

        /// <summary>
        /// 指定范围获得子数组
        /// </summary>
        /// <param name="list"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IList ListRange(IList list, decimal? from, decimal? to)
        {
            if (from == null || from > list.Count)
            {
                return Enumerable.Empty<object>().ToList();
            }
            int start = from <= 0 ? list.Count + (int)from : (int)from - 1;
            int end = to == null ? list.Count : (list.Count < to ? list.Count : to < 0 ? list.Count + (int)to + 1 : (int)to);
            List<object> result = new List<object>();
            if (start < 0 || end < 0) return result; //允许反向查找，所以要检查最小边界
            if (start > end) return result;  //不允许边界倒置的情形，返回空
            for (int i = start; i < end; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }

        /// <summary>
        /// 实现IfElse选择，暂且使用方法实现
        /// TODO：使用表达式树构建
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="valueIfTrue"></param>
        /// <param name="valueIfFalse"></param>
        /// <returns></returns>
        [MethodName("if")]
        [Obsolete("已使用表达式树代替")]
        public static object IfElse(bool expression, [Fallback] object valueIfTrue, [Fallback] object valueIfFalse)
        {
            return expression ? valueIfTrue : valueIfFalse;
        }

        public static bool? ToBool(object obj)
        {
            if (obj is JToken jtoken)
            {
                if (jtoken.Type == JTokenType.Boolean)
                    return jtoken.Value<bool>();
                return null;
            }
            return obj as bool?;
        }

        /// <summary>
        /// 判断某值是否为空；
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodName("isnull")]
        public static bool IsNull([Fallback] object obj)
        {
            if (obj == null)
                return true;
            if (obj is JToken token)
            {
                return token.Type == JTokenType.Null;
            }
            return false;
        }

        public static T? ConvertValueType<T>(object obj) where T : struct
        {
            if (obj == null)
                return null;
            if (obj is JValue jv)
            {
                return jv.ToObject<T?>();
            }
            return (T?)obj;
        }

        public static T ConvertClassType<T>(object obj)
        {
            if (obj == null)
                return default;

            else if (typeof(T) == typeof(object))
                return (T)obj;

            else if (obj.GetType() == typeof(T))
                return (T)obj;

            else if (obj is JValue jv)
            {
                if ((Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) == typeof(DateTimeOffset))
                {
                    return (T)(object)ToDate(jv);
                }
                else
                {
                    return jv.ToObject<T>();
                }
            }
            else if (obj is IConvertible)
            {
                var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                if (type == typeof(DateTimeOffset))
                {
                    return (T)Convert.ChangeType(ToDate(obj), type);
                }
                else if (type == typeof(decimal))
                {
                    return (T)Convert.ChangeType(ToNum(obj), type);
                }
                return (T)Convert.ChangeType(obj, type);
            }
            return (T)obj;
        }

        public static List<T> ConvertListType<T>(IList list)
        {
            if (list == null)
                return default;
            var result = new List<T>();
            foreach (var item in list)
            {
                result.Add(ConvertClassType<T>(item));
            }
            return result;
        }

        /// <summary>
        /// 判断某值是否为空；如果是容器，判断是否包含元素
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodName("NOE")]
        public static bool IsNullOrEmpty([Fallback] object obj)
        {
            if (obj == null)
                return true;
            if (obj is JToken token)
            {
                return (token.Type == JTokenType.Array && !token.HasValues) ||
                       (token.Type == JTokenType.Object && !token.HasValues) ||
                       (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                       (token.Type == JTokenType.Null);
            }
            if (obj is string text)
            {
                return String.Empty == text;
            }
            if (obj is IList list)
            {
                return list.Count == 0;
            }
            return false;
        }

        [MethodName("join")]
        public static string Join([Fallback] object obj)
        {
            return Join(obj, ",");
        }

        [MethodName("join")]
        public static string Join([Fallback] object obj, string separator)
        {
            if (obj == null)
                return String.Empty;
            if (obj is JToken token)
            {
                if (token.Type == JTokenType.Null)
                    return String.Empty;
                if (token.Type == JTokenType.Array)
                    return Join(obj as IList, separator);
                return obj.ToString();
            }
            if (obj is IList)
            {
                return Join(obj, separator);
            }
            return obj.ToString();
        }

        [MethodName("join")]
        public static string Join(IList list)
        {
            return Join(list, ",");
        }

        [MethodName("join")]
        public static string Join(IList list, string separator)
        {
            if (list == null || list.Count == 0)
            {
                return String.Empty;
            }
            if (list.Count == 1)
            {
                return list[0].ToString();
            }
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count - 1; i++)
            {
                sb.Append(list[i]?.ToString());
                sb.Append(separator);
            }
            sb.Append(list[list.Count - 1].ToString());
            return sb.ToString();
        }

        [MethodName("guid")]
        [MethodName("uuid")]
        public static string GetUuid()
        {
            return Guid.NewGuid().ToString();
        }

        [MethodName("guid")]
        [MethodName("uuid")]
        public static string GetUuid(string format)
        {
            return Guid.NewGuid().ToString(format);
        }

        #endregion

        #region Json
        [MethodName("json2array")]
        [MethodName("j2a")]
        public static JArray JsonToArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            try
            {
                return JArray.Parse(value);
            }
            catch
            {
                return null;
            }
        }

        [MethodName("json2obj")]
        [MethodName("j2o")]
        public static JObject JsonToObject(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            try
            {
                return JObject.Parse(value);
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MethodNameAttribute : Attribute
    {
        /// <summary>
        /// 方法的对外名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 该方法是否属于内部调用（用户不可见）
        /// </summary>
        public bool IsInternal { get; private set; }
        public MethodNameAttribute(string name, bool isInternal = false)
        {
            Name = name;
            IsInternal = isInternal;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class FallbackAttribute : Attribute
    {
        public FallbackAttribute()
        {
        }
    }
}
