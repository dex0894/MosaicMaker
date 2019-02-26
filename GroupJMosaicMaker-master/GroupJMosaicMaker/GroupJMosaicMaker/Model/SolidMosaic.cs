using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupJMosaicMaker.Utility;

namespace GroupJMosaicMaker.Model
{
    /// <summary>
    ///     The solid mosaic class
    /// </summary>
    public class SolidMosaic
    {
        #region Properties

        /// <summary>
        ///     Gets the image pixels.
        /// </summary>
        /// <value>
        ///     The image pixels.
        /// </value>
        public byte[] ImagePixels { get; }

        /// <summary>
        ///     Gets or sets the mosaic.
        /// </summary>
        /// <value>
        ///     The mosaic.
        /// </value>
        public WriteableBitmap Mosaic { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SolidMosaic" /> class.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        public SolidMosaic(byte[] sourcePixels)
        {
            this.ImagePixels = sourcePixels;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the solid mosaic.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid</param>
        public void CreateSolidSquareMosaic(uint imageWidth, uint imageHeight, int grid)
        {
            var red = 0;
            var green = 0;
            var blue = 0;
            var total = 0;
            for (var i = 0; i < imageHeight; i += grid)
            {
                for (var j = 0; j < imageWidth; j += grid)
                {
                    var color = MosaicCalculations.FindAverageGridColor(this.ImagePixels, imageWidth, imageHeight, grid,
                        ref red, ref green,
                        ref blue, ref total, i, j);
                    this.setGridPixelsWithAverageColor(imageWidth, imageHeight, grid, i, j, color);
                }
            }

            this.Mosaic = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        /// <summary>
        ///     Creates the solid triangle mosaic.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreateSolidTriangleMosaic(uint imageWidth, uint imageHeight, int grid)
        {
            var red = 0;
            var green = 0;
            var blue = 0;
            var rTriangle = new List<Color>();
            var lTriangle = new List<Color>();
            var wShift = 0;
            var hShift = 0;

            for (var i = 0; i < imageHeight; i += grid)
            {
                for (var j = 0; j < imageWidth; j += grid)
                {
                    this.addPixelsToLeftAndRightTriangleLists(imageWidth, imageHeight, grid, rTriangle, lTriangle,
                        wShift, hShift, i, j);
                    this.setGridWithLeftAndRightTriangleColors(imageWidth, imageHeight, grid, ref red, ref green,
                        ref blue, rTriangle, lTriangle, wShift, hShift, i, j);
                    wShift += grid;
                }

                hShift += grid;
                wShift = 0;
            }

            this.Mosaic = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        private void addPixelsToLeftAndRightTriangleLists(uint imageWidth, uint imageHeight, int grid,
            List<Color> rTriangle, List<Color> lTriangle, int wShift, int hShift, int i, int j)
        {
            for (var hPixel = i; hPixel < i + grid && hPixel < imageHeight; hPixel++)
            {
                for (var wPixel = j; wPixel < j + grid && wPixel < imageWidth; wPixel++)
                {
                    if (wPixel - wShift >= hPixel - hShift)
                    {
                        var rPixelColor = MosaicCalculations.GetPixelBgra8(this.ImagePixels, hPixel, wPixel, imageWidth,
                            imageHeight);
                        rTriangle.Add(rPixelColor);
                    }
                    else
                    {
                        var lPixelColor = MosaicCalculations.GetPixelBgra8(this.ImagePixels, hPixel, wPixel, imageWidth,
                            imageHeight);
                        lTriangle.Add(lPixelColor);
                    }
                }
            }
        }

        private void setGridWithLeftAndRightTriangleColors(uint imageWidth, uint imageHeight, int grid, ref int red,
            ref int green, ref int blue, List<Color> rTriangle, List<Color> lTriangle, int wShift, int hShift, int i,
            int j)
        {
            var rightColor = this.calculateAverageTriangleColor(ref red, ref green, ref blue, rTriangle);
            var leftColor = this.calculateAverageTriangleColor(ref red, ref green, ref blue, lTriangle);

            for (var hPixel = i; hPixel < i + grid && hPixel < imageHeight; hPixel++)
            {
                for (var wPixel = j; wPixel < j + grid && wPixel < imageWidth; wPixel++)
                {
                    if (wPixel - wShift >= hPixel - hShift)
                    {
                        MosaicCalculations.SetPixelBgra8(this.ImagePixels, hPixel, wPixel, rightColor, imageWidth,
                            imageHeight);
                    }
                    else
                    {
                        MosaicCalculations.SetPixelBgra8(this.ImagePixels, hPixel, wPixel, leftColor, imageWidth,
                            imageHeight);
                    }
                }
            }
        }

        private Color calculateAverageTriangleColor(ref int red, ref int green, ref int blue, List<Color> colors)
        {
            foreach (var pixel in colors)
            {
                red += pixel.R;
                green += pixel.G;
                blue += pixel.B;
            }

            red /= colors.Count;
            green /= colors.Count;
            blue /= colors.Count;

            var rgbValue = MosaicCalculations.RgbToInt(red, green, blue);
            colors.Clear();
            red = 0;
            green = 0;
            blue = 0;
            var averageColor = BitConverter.GetBytes(rgbValue);
            var color = Color.FromArgb(0, averageColor[0], averageColor[1], averageColor[2]);
            return color;
        }

        private void setGridPixelsWithAverageColor(uint imageWidth, uint imageHeight, int grid, int i, int j,
            Color color)
        {
            for (var hPixel = i; hPixel < i + grid && hPixel < imageHeight; hPixel++)
            {
                for (var wPixel = j; wPixel < j + grid && wPixel < imageWidth; wPixel++)
                {
                    MosaicCalculations.SetPixelBgra8(this.ImagePixels, hPixel, wPixel, color, imageWidth, imageHeight);
                }
            }
        }

        #endregion
    }
}