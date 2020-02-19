using System.Drawing;

namespace Lib
{
    public static class Draw
    {
        public static Bitmap FontChar(byte pixelS, Resource.FontChar Character, Color[] colors)
        {
            byte maxWidth = (byte)(Character.BytesToRead * 8);
            byte maxHeight = Character.Height;

            Bitmap image = new Bitmap(maxWidth * pixelS, maxHeight * pixelS);

            Graphics g = Graphics.FromImage(image);

            SolidBrush colorActive = new SolidBrush(colors[0]);
            SolidBrush colorPasive = new SolidBrush(colors[1]);
            SolidBrush colorHidden = new SolidBrush(colors[2]);
            SolidBrush colorDrawing = new SolidBrush(Color.AliceBlue);

            Rectangle pixel = new Rectangle(0, 0, 0, 0);

            byte value;
            int x, y, pixelW, pixelH;

            using (g)
            {
                for (int i = 0; i < Character.Planes.Count; i++)
                {
                    y = 0;

                    y = y + (i * pixelS);

                    for (int j = 0; j < Character.Planes[i].Length; j++)
                    {
                        x = 0;

                        value = Character.Planes[i][j];

                        if (value == 0x00) { colorDrawing = colorPasive; }
                        if (value == 0x01) { colorDrawing = colorActive; }
                        if (j >= Character.Width) { colorDrawing = colorHidden; }

                        x = x + (j * pixelS);
                        pixelW = pixelS;
                        pixelH = pixelS;

                        pixel.X = x;
                        pixel.Y = y;
                        pixel.Width = pixelW;
                        pixel.Height = pixelH;

                        g.FillRectangle(colorDrawing, pixel);
                    }
                }
            }
            return image;
        }
    }
}
