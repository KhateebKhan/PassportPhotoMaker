using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace PhotoMaker.Helper
{
    public class ImageUtility
    {
        public static void ResizeImage(string inputPath, string outputPath, int width, int height)
        {
            using (Image src = Image.FromFile(inputPath))
            using (Bitmap bmp = new Bitmap(width, height))
            {
                bmp.SetResolution(300, 300); // PRINT QUALITY

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.DrawImage(src, 0, 0, width, height);
                }

                bmp.Save(outputPath, ImageFormat.Jpeg);
            }
        }
    }
}