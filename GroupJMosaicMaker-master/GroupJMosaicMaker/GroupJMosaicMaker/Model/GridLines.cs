using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using GroupJMosaicMaker.Utility;

namespace GroupJMosaicMaker.Model
{
    /// <summary>
    ///     The grid class
    /// </summary>
    public class GridLines
    {
        #region Data members

        /// <summary>
        ///     The grid
        /// </summary>
        public WriteableBitmap Grid;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the image pixels.
        /// </summary>
        /// <value>
        ///     The image pixels.
        /// </value>
        public byte[] ImagePixels { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GridLines" /> class.
        /// </summary>
        public GridLines(byte[] sourcePixels)
        {
            this.ImagePixels = sourcePixels;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the grid lines.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreateSquareGridLines(uint imageWidth, uint imageHeight, int grid)
        {
            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = grid; j < imageWidth; j += grid)
                {
                    var color = Color.FromArgb(0, 255, 255, 255);
                    MosaicCalculations.setPixelBgra8(this.ImagePixels, i, j, color, imageWidth, imageHeight);
                }
            }

            for (var i = grid; i < imageHeight; i += grid)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var color = Color.FromArgb(0, 255, 255, 255);
                    MosaicCalculations.setPixelBgra8(this.ImagePixels, i, j, color, imageWidth, imageHeight);
                }
            }

            this.Grid = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        /// <summary>
        ///     Creates the triangle grid lines.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="grid">The grid.</param>
        public void CreateTriangleGridLines(uint imageWidth, uint imageHeight, int grid)
        {
            var wCount = 0;

            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = wCount; j < imageWidth; j += grid)
                {
                    var color = Color.FromArgb(0, 255, 255, 255);
                    MosaicCalculations.setPixelBgra8(this.ImagePixels, i, j, color, imageWidth, imageHeight);
                }

                if (grid - wCount == 1)
                {
                    wCount = 0;
                }
                else
                {
                    wCount++;
                }
            }

            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = grid; j < imageWidth; j += grid)
                {
                    var color = Color.FromArgb(0, 255, 255, 255);
                    MosaicCalculations.setPixelBgra8(this.ImagePixels, i, j, color, imageWidth, imageHeight);
                }
            }

            for (var i = grid; i < imageHeight; i += grid)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var color = Color.FromArgb(0, 255, 255, 255);
                    MosaicCalculations.setPixelBgra8(this.ImagePixels, i, j, color, imageWidth, imageHeight);
                }
            }

            this.Grid = new WriteableBitmap((int) imageWidth, (int) imageHeight);
        }

        #endregion
    }
}