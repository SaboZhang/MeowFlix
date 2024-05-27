﻿using MeowFlix.Database.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Common;

public class BaseMediaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value as BaseMediaTable;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
