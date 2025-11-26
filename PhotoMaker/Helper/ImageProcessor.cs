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
                Color bg = GetColor(options.BackgroundColor);
                Bitmap replaced = ReplaceBackground(original, bg);

                // STEP 2: Resize to EXACT width/height selected by user
                Bitmap resized = ResizePassportPhoto(
                    replaced,
                    options.WidthPx,
                    options.HeightPx
                );

                // STEP 3: Generate sheet
                if (options.SheetCount > 1)
                    resized = GenerateSheet(resized, options.SheetCount);

                using (MemoryStream output = new MemoryStream())
                {
                    resized.Save(output, ImageFormat.Png);
                    return output.ToArray();
                }
            }
        }

        private Color GetColor(string bg)
        {
            if (bg == "white") return Color.White;
            if (bg == "gray") return Color.LightGray;
            if (bg == "transparent") return Color.Transparent;

            if (bg.StartsWith("#"))
                return ColorTranslator.FromHtml(bg);

            return Color.FromArgb(0, 148, 255); // default blue
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

        // ⭐ FINAL FIXED VERSION — uses WidthPx/HeightPx from user selection
        private Bitmap ResizePassportPhoto(Bitmap img, int targetWidth, int targetHeight)
        {
            float ratio = Math.Min(
                (float)targetWidth / img.Width,
                (float)targetHeight / img.Height
            );

            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);

            Bitmap canvas = new Bitmap(targetWidth, targetHeight);

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                int offsetX = (targetWidth - newWidth) / 2;
                int offsetY = (targetHeight - newHeight) / 2;

                g.DrawImage(img, offsetX, offsetY, newWidth, newHeight);
            }

            return canvas;
        }

        private Bitmap GenerateSheet(Bitmap img, int sheetCount)
        {
            int rows = 1, cols = 1;

            if (sheetCount == 2) { rows = 1; cols = 2; }
            if (sheetCount == 4) { rows = 2; cols = 2; }
            if (sheetCount == 6) { rows = 2; cols = 3; }
            if (sheetCount == 8) { rows = 2; cols = 4; }
            if (sheetCount == 12) { rows = 3; cols = 4; }

            int width = img.Width * cols;
            int height = img.Height * rows;

            Bitmap sheet = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(sheet))
            {
                g.Clear(Color.White);

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
