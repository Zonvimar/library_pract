using System;
using System.Globalization;
using System.Windows.Data;

namespace library.Converters
{
    public class DaysRemainingConverter : IMultiValueConverter
    {
        public static readonly DaysRemainingConverter Instance = new DaysRemainingConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "Неизвестно";

            var dueDate = values[0] as DateTime?;
            var returnDate = values[1] as DateTime?;

            if (!dueDate.HasValue) return "Неизвестно";
            if (returnDate.HasValue) return "Возвращена";

            var today = DateTime.Now.Date;
            var daysRemaining = (dueDate.Value - today).Days;

            if (daysRemaining < 0)
                return $"Просрочена на {Math.Abs(daysRemaining)} дн.";
            else if (daysRemaining == 0)
                return "Последний день!";
            else if (daysRemaining <= 3)
                return $"Осталось {daysRemaining} дн. ⚠️";
            else
                return $"Осталось {daysRemaining} дн.";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}