using System;
using System.Globalization;
using System.Windows.Data;

namespace library.Converters
{
    public class BorrowingStatusConverter : IMultiValueConverter
    {
        public static readonly BorrowingStatusConverter Instance = new BorrowingStatusConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "Неизвестно";

            var returnDate = values[0] as DateTime?;
            var dueDate = values[1] as DateTime?;

            if (returnDate.HasValue)
                return "Возвращена";

            if (dueDate.HasValue && dueDate.Value < DateTime.Now.Date)
                return "Просрочена";

            return "Активна";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}