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
        public static Bitmap BuildA4Sheet(Bitmap passportPhoto, SheetStyle style)
        {
            int a4W = 2480; // A4 @ 300 DPI
            int a4H = 3508;

            Bitmap sheet = new Bitmap(a4W, a4H);
            Graphics g = Graphics.FromImage(sheet);
            g.Clear(Color.White);

            int pw = passportPhoto.Width;  // 413 px
            int ph = passportPhoto.Height; // 531 px

            int margin = 80;
            int spacing = 80;

            int startX = margin;
            int startY = margin;

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    int x = startX + c * (pw + spacing);
                    int y = startY + r * (ph + spacing);

                    if (style == SheetStyle.StyledGrid)
                    {
                        g.FillRectangle(Brushes.LightGray, x - 20, y - 20, pw + 40, ph + 40);
                        g.FillRectangle(Brushes.White, x - 10, y - 10, pw + 20, ph + 20);
                        g.DrawRectangle(Pens.Gray, x - 1, y - 1, pw + 2, ph + 2);
                    }

                    if (style == SheetStyle.PassportWithBorders)
                    {
                        g.DrawRectangle(new Pen(Color.Black, 3), x - 5, y - 5, pw + 10, ph + 10);
                    }

                    g.DrawImage(passportPhoto, x, y, pw, ph);
                }
            }

            g.Dispose();
            return sheet;
        }
    }
}
