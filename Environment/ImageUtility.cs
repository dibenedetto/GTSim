using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GTSim
{
	public class ImageUtility
	{
		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}

		public static string ExportBase64(Bitmap image, int width, int height, ImageFormat format, long quality)
		{
			if ((image.Width != width) || (image.Height != height))
			{
				Bitmap resized = new Bitmap(width, height);
				using (Graphics g = Graphics.FromImage(resized))
				{
					g.DrawImage(image, 0, 0, width, height);
				}
				image = resized;
			}

			ImageCodecInfo                 formatEncoder     = GetEncoder(format);
			System.Drawing.Imaging.Encoder encoder           = System.Drawing.Imaging.Encoder.Quality;
 			EncoderParameters              encoderParameters = new EncoderParameters(1);
 
			EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
			encoderParameters.Param[0] = encoderParameter;

			MemoryStream memStr = new MemoryStream();
			image.Save(memStr, formatEncoder, encoderParameters);

			return Convert.ToBase64String(memStr.ToArray());
 		}
	}
}
