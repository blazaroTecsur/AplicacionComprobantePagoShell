using System.Globalization;

namespace Infor.Abstractions.Helpers
{
    public class InforHelper
    {
        public static decimal? ToDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result))
            {
                return result;
            }

            return null;
        }
        private const string formatDate = "yyyyMMdd HH:mm:ss.fff";
        public static DateTime? ToDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.ParseExact(
                value,
                formatDate,
                CultureInfo.InvariantCulture);
        }
    }
}