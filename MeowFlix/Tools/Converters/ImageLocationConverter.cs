using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MeowFlix.Common;

public class ImageLocationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // 将字符串路径转换为 ImageSource
        if (value is string imagePath)
        {
            string cachedImagePath = GetCachedImagePath(imagePath);

            if (File.Exists(cachedImagePath))
            {
                // 如果缓存文件存在，直接返回缓存路径
                return new BitmapImage(new Uri(cachedImagePath, UriKind.RelativeOrAbsolute));
            }
            else
            {
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
        }

        // 如果无法转换，返回空
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private string GetCachedImagePath(string originalImagePath)
    {
        // 构造缓存路径
        string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        string videoImageDirectory = Path.Combine(dataDirectory, "VideoImage");

        if (!Directory.Exists(videoImageDirectory))
        {
            Directory.CreateDirectory(videoImageDirectory);
        }

        // 生成MD5作为文件名的唯一标识符
        string uniqueIdentifier = VideoFileHelper.GetMD5Hash(originalImagePath);
        string newFileName = $"{uniqueIdentifier}.jpg";

        return Path.Combine(videoImageDirectory, newFileName);
    }

}
