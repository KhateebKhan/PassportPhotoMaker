using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoMaker.Helpers
{
    public enum SheetStyle
    {
        StyledGrid,
        CleanPassport,
        PassportWithBorders
    }

    public static class PassportSheetBuilder
    {
        public static Bitmap ResizeToPassport(Bitmap img)
        {
            // 35x45 mm at 300 DPI → 413x531 px
            int w = (int)((35f / 25.4f) * 300);
            int h = (int)((45f / 25.4f) * 300);

            Bitmap output = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, w, h);
            }

            return output;
        }
        public static Bitmap BuildCustomSheet(Bitmap passport, int count, int paperW, int paperH)
        {
            Bitmap canvas = new Bitmap(paperW, paperH);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);

                int padding = 20;
                int x = padding;
                int y = padding;

                for (int i = 0; i < count; i++)
                {
                    g.DrawImage(passport, x, y, passport.Width, passport.Height);

                    x += passport.Width + padding;

                    if (x + passport.Width > paperW)
                    {
                        x = padding;
                        y += passport.Height + padding;
                    }
                }
            }

            return canvas;
        }

        public static Bitmap BuildA4Sheet(Bitmap passportPhoto, int count)
        {
            int a4W = 2480;
            int a4H = 3508;

            Bitmap sheet = new Bitmap(a4W, a4H);
            Graphics g = Graphics.FromImage(sheet);
            g.Clear(Color.White);

            int pw = passportPhoto.Width;
            int ph = passportPhoto.Height;

            int margin = 80;
            int spacing = 60;

            // Determine grid automatically
            int cols = (count >= 4) ? 2 : 1;    // 1 or 2 columns
            int rows = (int)Math.Ceiling(count / (double)cols);

            int index = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (index >= count)
                        break;

                    int x = margin + c * (pw + spacing);
                    int y = margin + r * (ph + spacing);

                    g.DrawImage(passportPhoto, x, y, pw, ph);

                    index++;
                }
            }

            g.Dispose();
            return sheet;
        }

    }
}
