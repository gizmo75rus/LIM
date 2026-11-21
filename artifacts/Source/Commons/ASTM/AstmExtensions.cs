using System;
using System.Globalization;

namespace AstmDriver.Commons.ASTM
{
    internal static class AstmExtensions
    {
        public static string ToAstmDate(this DateTime dateTime, AstmDateTimeUsage astmDateUsage)
        {
            switch (astmDateUsage)
            {
                default:
                    {
                        if (astmDateUsage == AstmDateTimeUsage.Date)
                        {
                            goto case AstmDateTimeUsage.Date;
                        }
                        if (astmDateUsage == AstmDateTimeUsage.DateTime)
                        {
                            goto case AstmDateTimeUsage.DateTime;
                        }
                        string Result = default;
                        if (astmDateUsage != AstmDateTimeUsage.Time)
                        {
                            return Result;
                        }
                        goto case AstmDateTimeUsage.Time;
                    }
                case AstmDateTimeUsage.Date:
                    return dateTime.Year.ToString("D4") + dateTime.Month.ToString("D2") + dateTime.Day.ToString("D2");
                case AstmDateTimeUsage.DateTime:
                    return dateTime.Year.ToString("D4") + dateTime.Month.ToString("D2") + dateTime.Day.ToString("D2") + dateTime.Hour.ToString("D2") + dateTime.Minute.ToString("D2") + dateTime.Second.ToString("D2");
                case AstmDateTimeUsage.Time:
                    return dateTime.Hour.ToString("D2") + dateTime.Minute.ToString("D2") + dateTime.Second.ToString("D2");
            }
        }

        public static DateTime AstmStringToDateTime(this string input, AstmDateTimeUsage astmDateTimeUsage)
        {
            switch (astmDateTimeUsage)
            {
                default:
                    {
                        if (astmDateTimeUsage == AstmDateTimeUsage.Date)
                        {
                            goto case AstmDateTimeUsage.Date;
                        }
                        if (astmDateTimeUsage == AstmDateTimeUsage.DateTime)
                        {
                            goto case AstmDateTimeUsage.DateTime;
                        }
                        DateTime Result = default;
                        if (astmDateTimeUsage != AstmDateTimeUsage.Time)
                        {
                            return Result;
                        }
                        goto case AstmDateTimeUsage.Time;
                    }
                case AstmDateTimeUsage.Date:
                    return DateTime.ParseExact(input, "yyyyMMdd", CultureInfo.InvariantCulture);
                case AstmDateTimeUsage.DateTime:
                    return DateTime.ParseExact(input, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                case AstmDateTimeUsage.Time:
                    return DateTime.ParseExact(input, "HHmmss", CultureInfo.InvariantCulture);
            }
        }
    }
}
