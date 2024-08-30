using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;

namespace Exame2
{
    public class Canvas
    {
        public Bitmap bitmap;
        public float Width, Height;
        private byte[] bits;
        private int pixelFormatSize, stride;
        public float scale = 0.2f;
        public int power = -25;
        public Color color = Color.White;
        public List<int> movements = new List<int>{0, 0};
        private static byte r = 2;
        private static byte g = 1;
        private static byte b = 0;

   

   
        public Canvas(Size size)
        {
            Init(size.Width, size.Height);
        }

        private void Init(int width, int height)
        {
            PixelFormat format = PixelFormat.Format32bppArgb;
            bitmap = new Bitmap(width, height, format);
            Width = width;
            Height = height;
            pixelFormatSize = Image.GetPixelFormatSize(format) / 8;
            stride = width * pixelFormatSize;
            bits = new byte[stride * height];
            var handle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            IntPtr bitPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bits, 0);
            bitmap = new Bitmap(width, height, stride, format, bitPtr);
            
        }

        public void FastClear()
        {
            unsafe
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        currentLine[x] = 0; // Blue
                        currentLine[x + 1] = 0; // Green
                        currentLine[x + 2] = 0; // Red
                        currentLine[x + 3] = 0; // Alpha
                    }
                });

                bitmap.UnlockBits(bitmapData);
            }
        }
        static int[,] conv = {
            { -1, 0, 1 },
            { -2, 0 ,2 },
            { -1, 0, 1 }
            };
        static int factor = 1;
        static int offset = 0;

        public static Bitmap ConvolucionOptimized(Bitmap original, int width, int height)
        {
            Bitmap bitmapNuevo = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData dataSrc = original.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataDest = bitmapNuevo.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            unsafe
            {
                byte* ptrSrc = (byte*)dataSrc.Scan0;
                byte* ptrDest = (byte*)dataDest.Scan0;
                int stride = dataSrc.Stride;

                Parallel.For(1, height - 1, y =>
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int sumaR = 0, sumaG = 0, sumaB = 0;

                        for (int a = -1; a <= 1; a++)
                        {
                            for (int b = -1; b <= 1; b++)
                            {
                                int idx = ((y + b) * stride) + ((x + a) * bytesPerPixel);
                                sumaR += ptrSrc[idx + 2] * conv[a + 1, b + 1];
                                sumaG += ptrSrc[idx + 1] * conv[a + 1, b + 1];
                                sumaB += ptrSrc[idx] * conv[a + 1, b + 1];
                            }
                        }

                        sumaR = Clamp((sumaR / factor) + offset);
                        sumaG = Clamp((sumaG / factor) + offset);
                        sumaB = Clamp((sumaB / factor) + offset);

                        int idxDest = (y * stride) + (x * bytesPerPixel);
                        ptrDest[idxDest + 2] = (byte)sumaR;
                        ptrDest[idxDest + 1] = (byte)sumaG;
                        ptrDest[idxDest] = (byte)sumaB;
                    }
                });
            }

            original.UnlockBits(dataSrc);
            bitmapNuevo.UnlockBits(dataDest);

            return bitmapNuevo;
        }


        public void Render(Scene scene, List<Matrix4x4> rotationMatrix)
        {
            FastClear();
            scene.Render(this, rotationMatrix, power, color, movements); 
        }

        public void DrawShadedTriangle(Point p1, Point p2, Point p3, Color color, float h1, float h2, float h3)
        {
            // Ordenar los puntos por y
            if (p1.Y > p2.Y) { Swap(ref p1, ref p2); Swap(ref h1, ref h2); }
            if (p1.Y > p3.Y) { Swap(ref p1, ref p3); Swap(ref h1, ref h3); }
            if (p2.Y > p3.Y) { Swap(ref p2, ref p3); Swap(ref h2, ref h3); }

            // Dibujar el triángulo relleno con sombreado
            int totalHeight = p3.Y - p1.Y;
            for (int y = 0; y <= totalHeight; y++)
            {
                bool secondHalf = y > p2.Y - p1.Y || p2.Y == p1.Y;
                int segmentHeight = secondHalf ? p3.Y - p2.Y : p2.Y - p1.Y;
                float alpha = (float)y / totalHeight;
                float beta = (float)(y - (secondHalf ? p2.Y - p1.Y : 0)) / segmentHeight; // Evitar la división por cero

                Point A = Interpolate(p1, p3, alpha);
                Point B = secondHalf ? Interpolate(p2, p3, beta) : Interpolate(p1, p2, beta);
                float hA = Interpolate(h1, h3, alpha);
                float hB = secondHalf ? Interpolate(h2, h3, beta) : Interpolate(h1, h2, beta);

                if (A.X > B.X)
                {
                    Swap(ref A, ref B);
                    Swap(ref hA, ref hB);
                }

                for (int x = A.X; x <= B.X; x++)
                {
                    float phi = B.X == A.X ? 1.0f : (float)(x - A.X) / (B.X - A.X);
                    float h = Interpolate(hA, hB, phi);

                    Color shadedColor = Shade(color, h);
                    SetPixel(x, p1.Y + y, shadedColor);
                }
            }
        }

        private Point Interpolate(Point p1, Point p2, float gradient)
        {
            int x = (int)(p1.X + (p2.X - p1.X) * gradient);
            int y = (int)(p1.Y + (p2.Y - p1.Y) * gradient);
            return new Point(x, y);
        }

        private float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * gradient;
        }

        private void Swap(ref Point p1, ref Point p2)
        {
            Point temp = p1;
            p1 = p2;
            p2 = temp;
        }

        private void Swap(ref float h1, ref float h2)
        {
            float temp = h1;
            h1 = h2;
            h2 = temp;
        }

         private Color Shade(Color color, float intensity)
        {
            intensity = Clamp(intensity, 0f, 1f);
            int red = (int)(color.R * intensity);
            int green = (int)(color.G * intensity);
            int blue = (int)(color.B * intensity);
            return Color.FromArgb(Clamp(red, 0, 255), Clamp(green, 0, 255), Clamp(blue, 0, 255));
        }

        private void SetPixel(int x, int y, Color color)
        {
            // Primero, valida que x e y estén dentro de los límites del canvas.
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            // Calcula el índice dentro del arreglo de bytes.
            int index = (y * stride) + (x * pixelFormatSize);

            // Verifica que el índice calculado más el tamaño de un píxel no exceda los límites del arreglo.
            if (index < 0 || (index + 3) >= bits.Length) return;

            // Ahora es seguro establecer los valores de color en el arreglo de bytes.
            bits[index + 0] = color.B;
            bits[index + 1] = color.G;
            bits[index + 2] = color.R;
            bits[index + 3] = color.A;
        }


        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        private static int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

    }
}
