using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Helpers
{
    /// <summary>
    /// Helper class để xử lý DateTime với múi giờ Vietnam (UTC+7)
    /// </summary>
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Lấy thời gian hiện tại theo múi giờ Vietnam (UTC+7)
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

        /// <summary>
        /// Lấy thời gian hiện tại theo múi giờ Vietnam (UTC+7) - dạng DateTimeOffset
        /// </summary>
        public static DateTimeOffset NowOffset => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTimeZone);

        /// <summary>
        /// Chuyển đổi DateTime từ UTC sang Vietnam time
        /// </summary>
        public static DateTime ToVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Nếu không có timezone info, giả sử là UTC
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), VietnamTimeZone);
            }
            else if (utcDateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
            }
            else
            {
                // Nếu đã là local time, chuyển sang UTC rồi sang Vietnam time
                return TimeZoneInfo.ConvertTime(utcDateTime, VietnamTimeZone);
            }
        }

        /// <summary>
        /// Chuyển đổi DateTime từ Vietnam time sang UTC
        /// </summary>
        public static DateTime ToUtc(DateTime vietnamDateTime)
        {
            if (vietnamDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Giả sử là Vietnam time
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(vietnamDateTime, DateTimeKind.Unspecified), VietnamTimeZone);
            }
            else
            {
                return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
            }
        }
    }
}

