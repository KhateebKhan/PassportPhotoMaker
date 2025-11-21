using PhotoMaker.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace PassportPhotoAI.Helpers
{
    public class ImageProcessor
    {
        public byte[] GeneratePassportPhoto(byte[] cutoutImage, PassportOptions options)
        {
            using (var ms = new MemoryStream(cutoutImage))
            using (var original = new Bitmap(ms))
            {
                // STEP 1: Replace background
                Color bg = options.BackgroundColor == "blue"
                    ? Color.FromArgb(0, 148, 255)
                    : Color.White;

                Bitmap replaced = ReplaceBackground(original, bg);

                // STEP 2: Resize based on passport type
                Bitmap resized = ResizePassportPhoto(replaced, options.SizeType);

                // STEP 3: Generate Sheet (if > 1)
                if (options.SheetCount > 1)
                    resized = GenerateSheet(resized, options.SheetCount);

                // Return final bytes
                using (MemoryStream output = new MemoryStream())
                {
                    resized.Save(output, ImageFormat.Png);
                    return output.ToArray();
                }
            }
        }

        private Bitmap ReplaceBackground(Bitmap original, Color bgColor)
        {
            Bitmap bmp = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(bgColor);
                g.DrawImage(original, 0, 0);
            }
            return bmp;
        }

        private Bitmap ResizePassportPhoto(Bitmap img, string type)
        {
            int width, height;

            if (type == "EU")
            {
                width = 413;
                height = 531;
            }
            else // US
            {
                width = 600;
                height = 600;
            }

            Bitmap resized = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(img, 0, 0, width, height);
            }

            return resized;
        }

        private Bitmap GenerateSheet(Bitmap img, int sheetCount)
        {
            int rows = 1, cols = 1;

            if (sheetCount == 4) { rows = 2; cols = 2; }
            else if (sheetCount == 6) { rows = 2; cols = 3; }
            else if (sheetCount == 12) { rows = 3; cols = 4; }

            int width = img.Width * cols;
            int height = img.Height * rows;

            Bitmap sheet = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(sheet))
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        g.DrawImage(img, c * img.Width, r * img.Height);
                    }
                }
            }

            return sheet;
        }
    }
}
