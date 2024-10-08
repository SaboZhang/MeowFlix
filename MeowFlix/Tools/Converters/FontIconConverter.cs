﻿namespace MeowFlix.Common;
public class FontIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string glyphName)
        {
            return new FontIcon { Glyph = glyphName };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
