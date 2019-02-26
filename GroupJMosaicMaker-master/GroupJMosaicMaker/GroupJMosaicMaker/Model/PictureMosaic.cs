using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupJMosaicMaker.Utility;

namespace GroupJMosaicMaker.Model
{
    /// <summary>
    ///     The picture mosaic class
    /// </summary>
    public class PictureMosaic
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

        /// <summary>
        ///     Gets the mosaic palette.
        /// </summary>
        /// <value>
        ///     The mosaic palette.
        /// </value>
        public List<byte[]> MosaicPalette { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PictureMosaic" /> class.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="sourceImagePalette"></param>
        public PictureMosaic(byte[] sourcePixels, List<byte[]> sourceImagePalette)
        {
            this.ImagePixels = sourcePixels;
            this.MosaicPalette = new List<byte[]>(sourceImagePalette);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the picture mosaic.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreatePictureMosaic(uint imageWidth, uint imageHeight, int grid)
        {
            var averageColors = MosaicCalculations.CalculateAverageColorOfPalette(this.MosaicPalette);
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
                    var indexOfClosesImage = MosaicCalculations.FindIndexOfClosestColor(averageColors, color);
                    var closestImage = this.MosaicPalette[indexOfClosesImage];
                    var colorList = new List<Color>();

                    this.writeClosestImagePixelsToMosaic(imageWidth, imageHeight, grid, i, j, closestImage, colorList);
                }
            }

            this.Mosaic = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        /// <summary>
        ///     Creates the picture mosaic and prevents teh images from being juxtaposed against themselves.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreatePictureMosaicPreventJuxtaposed(uint imageWidth, uint imageHeight, int grid)
        {
            var averageColors = MosaicCalculations.CalculateAverageColorOfPalette(this.MosaicPalette);
            var copyOfPalette = MosaicCalculations.CreateCopyOfImagePalette(this.MosaicPalette);
            var copyOfColors = MosaicCalculations.CreateCopyOfAverageColors(averageColors);
            var threshold = new Random();
            var red = 0;
            var green = 0;
            var blue = 0;
            var total = 0;
            var usedImages = 0;
            for (var i = 0; i < imageHeight; i += grid)
            {
                for (var j = 0; j < imageWidth; j += grid)
                {
                    var color = MosaicCalculations.FindAverageGridColor(this.ImagePixels, imageWidth, imageHeight, grid,
                        ref red, ref green,
                        ref blue, ref total, i, j);

                    var indexOfClosesImage = MosaicCalculations.FindIndexOfClosestColor(copyOfColors, color);
                    var closestImage = copyOfPalette[indexOfClosesImage];
                    usedImages++;

                    if (usedImages >= threshold.Next(2, 7))
                    {
                        copyOfPalette = MosaicCalculations.CreateCopyOfImagePalette(this.MosaicPalette);
                        copyOfColors = MosaicCalculations.CreateCopyOfAverageColors(averageColors);
                        usedImages = 0;
                    }

                    copyOfPalette.Remove(closestImage);
                    copyOfColors.RemoveAt(indexOfClosesImage);

                    var colorList = new List<Color>();

                    this.writeClosestImagePixelsToMosaic(imageWidth, imageHeight, grid, i, j, closestImage, colorList);
                }
            }

            this.Mosaic = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        /// <summary>
        ///     Creates the picture mosaic using all images before repeat.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreatePictureMosaicUsingAllImagesBeforeRepeat(uint imageWidth, uint imageHeight, int grid)
        {
            var averageColors = MosaicCalculations.CalculateAverageColorOfPalette(this.MosaicPalette);
            var copyOfPalette = MosaicCalculations.CreateCopyOfImagePalette(this.MosaicPalette);
            var copyOfColors = MosaicCalculations.CreateCopyOfAverageColors(averageColors);

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

                    if (copyOfColors.Count == 0)
                    {
                        copyOfColors = MosaicCalculations.CreateCopyOfAverageColors(averageColors);
                        copyOfPalette = MosaicCalculations.CreateCopyOfImagePalette(this.MosaicPalette);
                    }

                    var indexOfClosesImage = MosaicCalculations.FindIndexOfClosestColor(copyOfColors, color);
                    var closestImage = copyOfPalette[indexOfClosesImage];
                    copyOfPalette.Remove(closestImage);
                    copyOfColors.RemoveAt(indexOfClosesImage);
                    var colorList = new List<Color>();

                    this.writeClosestImagePixelsToMosaic(imageWidth, imageHeight, grid, i, j, closestImage, colorList);
                }
            }

            this.Mosaic = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        private void writeClosestImagePixelsToMosaic(uint imageWidth, uint imageHeight, int grid, int i, int j,
            byte[] closestImage, List<Color> colorList)
        {
            for (var heightPixel = 0; heightPixel < 50; heightPixel++)
            {
                for (var widthPixel = 0; widthPixel < 50; widthPixel++)
                {
                    var pixelColor = MosaicCalculations.GetPixelBgra8(closestImage, heightPixel, widthPixel, 50, 50);
                    colorList.Add(pixelColor);
                }
            }

            var count = 0;

            for (var hPixel = i; hPixel < i + grid && hPixel < imageHeight; hPixel++)
            {
                for (var wPixel = j; wPixel < j + grid && wPixel < imageWidth; wPixel++)
                {
                    MosaicCalculations.SetPixelBgra8(this.ImagePixels, hPixel, wPixel, colorList[count], imageWidth,
                        imageHeight);
                    count++;
                }
            }
        }

        #endregion
    }
}