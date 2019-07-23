using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BitcoinTransactionTool.Converters {
    public class NullReplaceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(parameter) ? null : value;
        }
    }
}