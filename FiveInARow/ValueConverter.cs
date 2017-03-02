using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FiveInARow
{
    class PlayerKindValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (PlayerKind)value;
            if (targetType==typeof(string))
            {
                switch (v)
                {
                    case PlayerKind.Human:
                        return "人类";
                    case PlayerKind.AI:
                        return "电脑";
                    default:
                        throw new NotSupportedException();
                }
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    class ChessPieceKindValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (ChessPieceKind)value;
            if (targetType == typeof(string))
            {
                switch (v)
                {
                    case ChessPieceKind.Empty:
                        return "未知";
                    case ChessPieceKind.WriteChessPiece:
                        return "白子";

                    case ChessPieceKind.BlackChessPiece:
                        return "黑子";

                    default:
                        throw new NotSupportedException();

                }
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
