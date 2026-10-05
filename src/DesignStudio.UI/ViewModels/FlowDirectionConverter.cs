using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DesignStudio.Localization;

namespace DesignStudio.UI.ViewModels;

public sealed class LanguageToFlowDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Language.Arabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}


public sealed class WindowVisibilityConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && text is not null && (text.Contains("Window", StringComparison.OrdinalIgnoreCase) || text.Contains("نافذة", StringComparison.Ordinal))
            ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}


public sealed class BooleanVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool visible && visible ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
