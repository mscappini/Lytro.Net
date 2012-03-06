using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Lytro
{
    public static class ImageExtensions
    {
        public static void SaveJpeg(this Image img, string filePath, long quality)
        {
            Encoder qualityEncoder = Encoder.Quality;
            EncoderParameters codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(qualityEncoder, quality); // Ratio
            ImageCodecInfo jpegCodecInfo = GetEncoder(ImageFormat.Jpeg);
            img.Save(filePath, jpegCodecInfo, codecParams);
        }

        private static ImageCodecInfo GetEncoder(this ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }
    }
}