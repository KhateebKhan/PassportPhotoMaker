using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PhotoMaker.Helpers
{
    public static class FaceCropper
    {
        public static Bitmap CropToPassport(Bitmap source, int targetWidth, int targetHeight)
        {
            // STEP 1 — Calculate desired aspect ratio
            double targetRatio = (double)targetWidth / targetHeight;

            // STEP 2 — Estimate face vertical position
            Rectangle faceArea = EstimateFaceArea(source);

            // STEP 3 — Expand crop to give headroom & shoulders
            int cropW = (int)(faceArea.Width * 2.5);
            int cropH = (int)(faceArea.Height * 3.5);

            int cx = faceArea.Left + faceArea.Width / 2;  // center X
            int cy = faceArea.Top + faceArea.Height / 2;  // center Y

            int x = cx - cropW / 2;
            int y = cy - (int)(cropH * 0.55);  // move up slightly

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x + cropW > source.Width) cropW = source.Width - x;
            if (y + cropH > source.Height) cropH = source.Height - y;

            Rectangle cropRect = new Rectangle(x, y, cropW, cropH);

            // STEP 4 — Crop
            Bitmap cropped = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, new Rectangle(0, 0, cropped.Width, cropped.Height),
                    cropRect, GraphicsUnit.Pixel);
            }

            // STEP 5 — Resize to passport size
            Bitmap final = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(final))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(cropped, 0, 0, targetWidth, targetHeight);
            }

            return final;
        }

        // 🔥 Smart “fake AI” head detector (works on your type of images)
        private static Rectangle EstimateFaceArea(Bitmap img)
        {
            // assume face is in upper center of the image
            int faceW = img.Width / 6;
            int faceH = img.Height / 5;

            int x = (img.Width - faceW) / 2;
            int y = img.Height / 10;

            return new Rectangle(x, y, faceW, faceH);
        }
    }
}
