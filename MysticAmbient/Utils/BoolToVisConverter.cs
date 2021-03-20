using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MysticAmbient.Utils
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisConverter : IValueConverter
    {
        enum Types
        {
            /// <summary>
            /// True to Visible, False to Collapsed
            /// </summary>
            t2v_f2c,
            /// <summary>
            /// True to Visible, False to Hidden
            /// </summary>
            t2v_f2h,
            /// <summary>
            /// True to Collapsed, False to Visible
            /// </summary>
            t2c_f2v,
            /// <summary>
            /// True to Hidden, False to Visible
            /// </summary>
            t2h_f2v,
        }
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            string p = (string)parameter;
            var type = (Types)Enum.Parse(typeof(Types), (string)parameter);
            switch (type)
            {
                case Types.t2v_f2c:
                    return b ? Visibility.Visible : Visibility.Collapsed;
                case Types.t2v_f2h:
                    return b ? Visibility.Visible : Visibility.Hidden;
                case Types.t2c_f2v:
                    return b ? Visibility.Collapsed : Visibility.Visible;
                case Types.t2h_f2v:
                    return b ? Visibility.Hidden : Visibility.Visible;
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var v = (Visibility)value;
            string p = (string)parameter;
            var type = (Types)Enum.Parse(typeof(Types), (string)parameter);
            switch (type)
            {
                case Types.t2v_f2c:
                    if (v == Visibility.Visible)
                        return true;
                    else if (v == Visibility.Collapsed)
                        return false;
                    break;
                case Types.t2v_f2h:
                    if (v == Visibility.Visible)
                        return true;
                    else if (v == Visibility.Hidden)
                        return false;
                    break;
                case Types.t2c_f2v:
                    if (v == Visibility.Visible)
                        return false;
                    else if (v == Visibility.Collapsed)
                        return true;
                    break;
                case Types.t2h_f2v:
                    if (v == Visibility.Visible)
                        return false;
                    else if (v == Visibility.Hidden)
                        return true;
                    break;
            }
            throw new InvalidOperationException();
        }
    }
}
