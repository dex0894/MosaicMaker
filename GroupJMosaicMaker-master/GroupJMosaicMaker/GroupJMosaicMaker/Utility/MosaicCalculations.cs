using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupJMosaicMaker.Utility
{
    /// <summary>
    ///     The mosaic calculation class
    /// </summary>
    public static class MosaicCalculations
    {
        #region Methods

        /// <summary>
        ///     rs the g bto int.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        /// <returns></returns>
        public static int RgbToInt(int red, int green, int blue)
        {
            return (red << 0) | (green << 8) | (blue << 16);
        }

        /// <summary>
        ///     Gets the pixel Bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        /// <summary>
        ///     Sets the pixel bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public static void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        /// <summary>
        ///     Finds the difference RGB.
        /// </summary>
        /// <param name="color1">The color1.</param>
        /// <param name="color2">The color2.</param>
        /// <returns></returns>
        public static int FindDifferenceRgb(Color color1, Color color2)
        {
            return Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B);
        }

        /// <summary>
        ///     Finds the color of the index of closest.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static int FindIndexOfClosestColor(List<Color> colors, Color target)
        {
            var difference = colors.Select(current => FindDifferenceRgb(current, target))
                                   .Min(current => current);
            return colors.FindIndex(current => FindDifferenceRgb(current, target) == difference);
        }

        /// <summary>
        ///     Calculates the average color of palette.
        /// </summary>
        public static List<Color> CalculateAverageColorOfPalette(List<byte[]> mosaicPalette)
        {
            var red = 0;
            var green = 0;
            var blue = 0;
            var total = 0;
            var averageColors = new List<Color>();
            foreach (var currentBytes in mosaicPalette)
            {
                for (var i = 0; i < 50; i++)
                {
                    for (var j = 0; j < 50; j++)
                    {
                        total++;
                        var pixelColor = GetPixelBgra8(currentBytes, i, j, 50, 50);

                        red += pixelColor.R;
                        green += pixelColor.G;
                        blue += pixelColor.B;
                    }
                }

                red /= total;
                green /= total;
                blue /= total;
                total = 0;
                var rgbValue = RgbToInt(red, green, blue);

                var averageColor = BitConverter.GetBytes(rgbValue);
                var color = Color.FromArgb(0, averageColor[0], averageColor[1], averageColor[2]);
                averageColors.Add(color);
            }

            return averageColors;
        }

        /// <summary>
        ///     Finds the average color of the grid.
        /// </summary>
        /// <param name="imagePixels">The image pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        /// <param name="total">The total.</param>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <returns></returns>
        public static Color FindAverageGridColor(byte[] imagePixels, uint imageWidth, uint imageHeight, int grid,
            ref int red, ref int green,
            ref int blue, ref int total, int i, int j)
        {
            for (var hPixel = i; hPixel < i + grid && hPixel < imageHeight; hPixel++)
            {
                for (var wPixel = j; wPixel < j + grid && wPixel < imageWidth; wPixel++)
                {
                    total++;
                    var pixelColor = GetPixelBgra8(imagePixels, i, j, imageWidth, imageHeight);

                    red += pixelColor.R;
                    green += pixelColor.G;
                    blue += pixelColor.B;
                }
            }

            red /= total;
            green /= total;
            blue /= total;
            var rgbValue = RgbToInt(red, green, blue);
            total = 0;
            var averageColor = BitConverter.GetBytes(rgbValue);
            var color = Color.FromArgb(0, averageColor[0], averageColor[1], averageColor[2]);
            return color;
        }

        /// <summary>
        ///     Converts the image to bw.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="pixels">The pixels.</param>
        /// <returns></returns>
        public static WriteableBitmap ConvertImageToBw(uint imageWidth, uint imageHeight, byte[] pixels)
        {
            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var color = getPixelBgra8(pixels, i, j, imageWidth, imageHeight);
                    int red = color.R;
                    int green = color.G;
                    int blue = color.B;
                    var ave = (red + green + blue) / 3;
                    ave = ave < 128 ? 0 : 255;

                    var rgbValue = RgbToInt(ave, ave, ave);
                    var averageBytes = BitConverter.GetBytes(rgbValue);

                    var bWColor = Color.FromArgb(0, averageBytes[0], averageBytes[1], averageBytes[2]);

                    setPixelBgra8(pixels, i, j, bWColor, imageWidth, imageHeight);
                }
            }

            var bWImage = new WriteableBitmap((int) imageWidth, (int) imageHeight);
            return bWImage;
        }

        private static Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        /// <summary>
        ///     Sets the pixel Color to the location of x and y
        /// </summary>
        /// <param name="pixels">The image pixels.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="color">The color to set.</param>     
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns></returns>
        public static void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        /// <summary>
        ///     Creates the copy of image palette.
        /// </summary>
        /// <param name="palette">The palette.</param>
        /// <returns></returns>
        public static List<byte[]> CreateCopyOfImagePalette(List<byte[]> palette)
        {
            var copyPalette = new List<byte[]>();
            foreach (var item in palette)
            {
                copyPalette.Add(item);
            }

            return copyPalette;
        }

        /// <summary>
        ///     Creates the copy of average colors.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <returns></returns>
        public static List<Color> CreateCopyOfAverageColors(List<Color> colors)
        {
            var copyColors = new List<Color>();
            foreach (var item in colors)
            {
                copyColors.Add(item);
            }

            return copyColors;
        }

        #endregion
    }
}