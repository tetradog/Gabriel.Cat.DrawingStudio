﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.DrawingStudio;
namespace Gabriel.Cat.Extension
{
    public static class ExtensionBitmap
    {
        delegate byte[] MetodoColor(byte[] colorValue,byte[] colorKey);
        delegate void MetodoTrataMientoPixel(ref byte r, ref byte g, ref byte b);
        #region BitmapImportado
        /// <summary>
        /// Recorta una imagen en formato Bitmap
        /// </summary>
        /// <param name="localizacion">localizacion de la esquina izquierda de arriba</param>
        /// <param name="tamaño">tamaño del rectangulo</param>
        /// <param name="bitmapARecortar">bitmap para recortar</param>
        /// <returns>bitmap resultado del recorte</returns>
        public static Bitmap Recortar(this Bitmap bitmapARecortar, Point localizacion, Size tamaño)
        {

            Rectangle rect = new Rectangle(localizacion.X, localizacion.Y, tamaño.Width, tamaño.Height);
            Bitmap cropped = bitmapARecortar.Clone(rect, bitmapARecortar.PixelFormat);
            return cropped;

        }
        public static Bitmap Escala(this Bitmap imgAEscalar, decimal escala)
        {
            return Resize(imgAEscalar, new Size(Convert.ToInt32(imgAEscalar.Size.Width * escala), Convert.ToInt32(imgAEscalar.Size.Height * escala)));
        }
        public static Bitmap Resize(this Bitmap imgToResize, Size size)
        {
            Bitmap bmpResized;
            try
            {
                bmpResized = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((System.Drawing.Image)bmpResized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }

            }
            catch
            {
                bmpResized = imgToResize;
            }

            return bmpResized;
        }




        public static Color[,] GetColorMatriu(this Bitmap bmp)
        {
            Color[,] matriz = new Color[bmp.Width, bmp.Height];
            bmp.TrataBytes((arrayBytes) =>
            {
                ulong posicion = 0;
                for (int y = 0, yFinal = bmp.Width; y < yFinal; y++)
                    for (int x = 0, xFinal = bmp.Height; x < xFinal; x++, posicion += 4)
                        matriz[x, y] = Color.FromArgb(arrayBytes[posicion], arrayBytes[posicion + 1], arrayBytes[posicion + 2], arrayBytes[posicion + 3]);

            });
            return matriz;
        }
        public static Bitmap GetBitmap(this Color[,] array)
        {
            Bitmap bmp = new Bitmap(array.GetLength(DimensionMatriz.X), array.GetLength(DimensionMatriz.Y));
            bmp.TrataBytes((arrayBytes) =>
            {
                ulong posicion = 0;
                for (ulong y = 0, yFinal = (ulong)array.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                    for (ulong x = 0, xFinal = (ulong)array.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++, posicion += 4)
                    {
                        arrayBytes[posicion] = array[x, y].A;
                        arrayBytes[posicion + 1] = array[x, y].R;
                        arrayBytes[posicion + 2] = array[x, y].G;
                        arrayBytes[posicion + 3] = array[x, y].B;
                    }

            });
            return bmp;
        }
        public static byte[,] GetMatriuBytes(this Bitmap bmp)
        {
            byte[] bytesArray = bmp.GetBytes();
            return bytesArray.ToMatriu(bmp.Height, DimensionMatriz.Y);
        }
        public static void SetMatriuBytes(this Bitmap bmp, byte[,] matriuBytes)
        {
            if (bmp.Height * bmp.Width * 3 != matriuBytes.GetLength(DimensionMatriz.Y) * matriuBytes.GetLength(DimensionMatriz.X))
                throw new Exception("La matriz no tiene las medidas de la imagen");

            bmp.TrataBytes((arrayBytes) =>
            {
                ulong posicion = 0;
                for (ulong y = 0, yFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                    for (ulong x = 0, xFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++)
                    {
                        arrayBytes[posicion++] = matriuBytes[x, y];
                    }


            });

        }

        public static void TrataBytes(this Bitmap bmp, MetodoTratarByteArray metodo)
        {
            BitmapData bmpData = bmp.LockBits();
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            ptr.CopyTo(rgbValues);
            if (metodo != null)
            {
                metodo(rgbValues);//se modifican los bytes :D
                                  // Copy the RGB values back to the bitmap
                rgbValues.CopyTo(ptr);
            }
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

        }
        public static unsafe void TrataBytes(this Bitmap bmp, MetodoTratarBytePointer metodo)
        {

            BitmapData bmpData = bmp.LockBits();
            // Get the address of the first line.

            IntPtr ptr = bmpData.Scan0;
            if (metodo != null)
            {
                metodo((byte*)ptr.ToPointer());//se modifican los bytes :D
            }
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

        }
        public static int LengthBytes(this Bitmap bmp)
        {
            int multiplicadorPixel = bmp.IsArgb() ? 4 : 3;
            return bmp.Height * bmp.Width * multiplicadorPixel;
        }
        public static bool IsArgb(this Bitmap bmp)
        {
            bool isArgb = false;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    isArgb = true;
                    break;
            }
            return isArgb;
        }


        public static Bitmap ChangeColorCopy(this Bitmap bmp, PixelColors color)
        {
            Bitmap bmpClon = bmp.Clone() as Bitmap;
            ChangeColor(bmpClon, color);
            return bmpClon;
        }
        public static unsafe void ChangeColor(this Bitmap bmp, PixelColors color)
        {
            bmp.TrataBytes((rgbArray) => { ICambiaColor(rgbArray, bmp.IsArgb(), bmp.LengthBytes(), color); });
        }

        private static unsafe void ICambiaColor(byte* rgbImg, bool isArgb, int lenght, PixelColors color)
        {
            int r = 0, g = 1, b = 2;
            byte byteR, byteG, byteB;
            int incremento = 3;
            MetodoTrataMientoPixel metodoTratamiento=null;
            if (isArgb)
            {
                incremento++;
            }
            switch (color)
            {
                case PixelColors.Sepia:
                  metodoTratamiento=  Image.IToSepia;
                    break;
                case PixelColors.Inverted:
                    metodoTratamiento = Image.ToInvertit;
                    break;
                case PixelColors.GrayScale:
                    metodoTratamiento = Image.ToEscalaDeGrises;
                    break;
                case PixelColors.Blue:
                    metodoTratamiento = Image.ToAzul;
                    break;
                case PixelColors.Red:
                    metodoTratamiento = Image.ToRojo;
                    break;
                case PixelColors.Green:
                    metodoTratamiento = Image.ToVerde;
                    break;


            }
            //me salto el alfa
            for (int i = 0; i < lenght; i += incremento)
            {


                byteR = rgbImg[i + r];
                byteG = rgbImg[i + g];
                byteB = rgbImg[i + b];

                metodoTratamiento(ref byteR,ref byteG,ref byteB);
                rgbImg[i + r] = byteR;
                rgbImg[i + g] = byteG;
                rgbImg[i + b] = byteB;

            }

        }

        #endregion

        public static void CambiarPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.CambiarPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void CambiarPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colorValue,colorKey) =>
            {
                return colorValue;
            };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        public static void EfectoPixel(this Bitmap bmp, Color aMezclarConTodos,bool saltarsePixelsTransparentes=true)
        {
            int incremento = bmp.IsArgb() ? 4 : 3;
            int aux;
            bool mezclar = true;
            const byte TRANSPARENTE = 0x00;
            bmp.TrataBytes((byteArray) =>
            {
                for (int i = 0, iFinal = bmp.LengthBytes(); i < iFinal; i += incremento)
                {
                    if (incremento == 4)
                    {
                        if(saltarsePixelsTransparentes)
                           mezclar = byteArray[i + Pixel.A] != TRANSPARENTE;
                        if (mezclar)
                        {
                            //MEZCLO LA A
                            aux = byteArray[i + Pixel.A] + aMezclarConTodos.A;
                            if (aux > 255) aux = 255;
                            byteArray[i + Pixel.A] = (byte)aux;
                        }
                    }
                    if (mezclar)
                    {
                        //MEZCLO LA R
                        aux = byteArray[i + Pixel.R] + aMezclarConTodos.R;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.R] = (byte)aux;
                        //MEZCLO LA G
                        aux = byteArray[i + Pixel.G] + aMezclarConTodos.G;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.G] = (byte)aux;
                        //MEZCLO LA B
                        aux = byteArray[i + Pixel.B] + aMezclarConTodos.B;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.B] = (byte)aux;
                    }
                }
            });
        }
        public static void MezclaPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.MezclaPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void MezclaPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colorValue, arrayKey) =>
            {
                byte[] colorMezclado = null;
                int aux;
                if (colorValue != null && arrayKey != null)
                {
                    colorMezclado = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {

                        aux = colorValue[i] + arrayKey[i];
                        aux /= 2;
                        colorMezclado[i] =(byte) aux;
                        //if (aux[i] > 255) aux[i] = 255;

                    }
                    
                }
             /*   else if (colorValue != null)
                    colorMezclado = colorValue;
                else
                    colorMezclado =Color.FromArgb(Serializar.ToInt( arrayKey));*/
                return colorMezclado;
            };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        static void ICambiaPixel(Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue, MetodoColor metodo)
        {
            DiccionarioColor2 diccionario = new DiccionarioColor2(colorsKeyValue);
            byte[] colorLeido;
            byte[] colorObtenido;
            const byte AOPACA = 0xFF;
            int incremento = bmp.IsArgb() ? 4 : 3;
            bmp.TrataBytes((byteArray) =>
            {
                for (int i = 0, iFin = bmp.LengthBytes(); i < iFin; i += incremento)
                {
                    colorLeido = new byte[] { AOPACA, byteArray[i + Pixel.R], byteArray[i + Pixel.G], byteArray[i + Pixel.B] };
                    if (incremento == 4)
                    {
                        colorLeido[Pixel.A] = byteArray[i + Pixel.A];
                    }
                    colorObtenido = metodo(diccionario.ObtenerPrimero(colorLeido), colorLeido);
                    if (colorObtenido != null)
                    {

                        if (incremento == 4)
                        {
                            byteArray[i + Pixel.A] = colorObtenido[0];
                        }

                        byteArray[i + Pixel.R] = colorObtenido[1];
                        byteArray[i + Pixel.G] = colorObtenido[2];
                        byteArray[i + Pixel.B] = colorObtenido[3];
                    }

                }
            });
        }

    }
}
