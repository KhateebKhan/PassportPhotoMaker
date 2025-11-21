using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PhotoMaker.Helpers
{
    public static class ImageSheetBuilder
    {
        public static string BuildSheet(
            List<string> imagePaths,
            int imgW,
            int imgH,
            string outputFolder)
        {
            if (imagePaths == null || imagePaths.Count == 0)
                throw new Exception("No images passed to sheet builder.");

            int spacing = 20;       // CSS grid gap
            int padding = 10;       // CSS .photo-box padding
            int borderOuter = 2;    // CSS .photo-box border
            int borderInner = 1;    // CSS img border
            int maxCols = 4;

            int frameW = imgW + padding * 2 + borderInner * 2 + borderOuter * 2;
            int frameH = imgH + padding * 2 + borderInner * 2 + borderOuter * 2;

            int total = imagePaths.Count;
            int cols = Math.Min(maxCols, total);
            int rows = (int)Math.Ceiling(total / (double)cols);

            int sheetWidth =
                cols * frameW +
                (cols + 1) * spacing;

            int sheetHeight =
                rows * frameH +
                (rows + 1) * spacing;

            Bitmap sheet = new Bitmap(sheetWidth, sheetHeight);
            Graphics g = Graphics.FromImage(sheet);
            g.Clear(Color.White);

            int index = 0;

            for (int r = 0; r < rows; r++)
            {
                int remaining = total - (r * cols);
                int thisRow = Math.Min(cols, remaining);

                // Center last row
                int rowWidth = thisRow * frameW + (thisRow + 1) * spacing;
                int startX = (sheetWidth - rowWidth) / 2 + spacing;

                for (int c = 0; c < thisRow; c++)
                {
                    using (Image img = Image.FromFile(imagePaths[index]))
                    {
                        int x = startX + c * (frameW + spacing);
                        int y = spacing + r * (frameH + spacing);

                        // Outer border (2px)
                        using (SolidBrush brush = new SolidBrush(Color.LightGray))
                            g.FillRectangle(brush, x, y, frameW, frameH);

                        // White background like .photo-box
                        g.FillRectangle(Brushes.White,
                            x + borderOuter,
                            y + borderOuter,
                            frameW - borderOuter * 2,
                            frameH - borderOuter * 2);

                        // Inner image border (1px)
                        g.FillRectangle(Brushes.Gray,
                            x + borderOuter + padding - borderInner,
                            y + borderOuter + padding - borderInner,
                            imgW + borderInner * 2,
                            imgH + borderInner * 2);

                        // Actual image
                        g.DrawImage(img,
                            x + borderOuter + padding,
                            y + borderOuter + padding,
                            imgW,
                            imgH);
                    }

                    index++;
                }
            }

            string fileName = "sheet_" + Guid.NewGuid() + ".jpg";
            string savePath = Path.Combine(outputFolder, fileName);

            sheet.Save(savePath);
            g.Dispose();
            sheet.Dispose();

            return fileName;
        }
    }
}
  