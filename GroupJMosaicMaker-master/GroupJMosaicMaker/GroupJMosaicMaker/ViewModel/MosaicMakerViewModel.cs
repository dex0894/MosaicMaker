using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GroupJMosaicMaker.Model;
using GroupJMosaicMaker.Utility;

namespace GroupJMosaicMaker.ViewModel
{
    /// <summary>
    ///     The Mosaic Maker View Model
    /// </summary>
    /// <seealso cref="GroupJMosaicMaker.Utility.BindableBase" />
    public class MosaicMakerViewModel : BindableBase
    {
        #region Data members

        private const string Jpg = ".jpg";
        private const string Png = ".png";
        private const string Bmp = ".bmp";

        /// <summary>
        ///     The source pixels
        /// </summary>
        public byte[] SourcePixels;

        /// <summary>
        ///     The solid mosaic pixels
        /// </summary>
        public byte[] SolidMosaicPixels;

        /// <summary>
        ///     The picture mosaic pixels
        /// </summary>
        public byte[] PictureMosaicPixels;

        /// <summary>
        ///     The picture mosaic pixels
        /// </summary>
        public byte[] BlackAndWhitePixels;

        /// <summary>
        ///     The grid pixels
        /// </summary>
        public byte[] GridPixels;

        private SolidMosaic solidMosaic;
        private GridLines gridLines;
        private PictureMosaic pictureMosaic;
        private double dpiX;
        private double dpiY;
        private readonly List<string> imageTypes = new List<string> {Jpg, Png, Bmp};
        private int gridValue;
        private BitmapDecoder decoder;
        private BitmapImage copyBitmapImage;
        private WriteableBitmap selectedImage;
        private bool isLoading;
        private bool useAtLeastOnce;
        private bool preventJuxtaposed;

        #endregion

        #region Properties

        /// <summary>
        ///     The modified image
        /// </summary>
        public WriteableBitmap MosaicImage { get; set; }

        /// <summary>
        ///     The modified image
        /// </summary>
        public WriteableBitmap BlackAndWhiteImage { get; set; }

        /// <summary>
        ///     Gets the solid mosaic.
        /// </summary>
        /// <value>
        ///     The solid mosaic.
        /// </value>
        public WriteableBitmap GridImage { get; set; }

        /// <summary>
        ///     Gets or sets the source image.
        /// </summary>
        /// <value>
        ///     The source image.
        /// </value>
        public BitmapImage OriginalImage { get; set; }

        /// <summary>
        ///     Gets or sets the remove command.
        /// </summary>
        /// <value>
        ///     The remove command.
        /// </value>
        public RelayCommand RemoveCommand { get; set; }

        /// <summary>
        ///     Gets or sets the image palette.
        /// </summary>
        /// <value>
        ///     The image palette.
        /// </value>
        public ObservableCollection<WriteableBitmap> ImagePalette { get; set; }

        /// <summary>
        ///     Gets or sets the selected images.
        /// </summary>
        /// <value>
        ///     The selected images.
        /// </value>
        public List<WriteableBitmap> SelectedImages { get; set; }

        /// <summary>
        ///     Gets or sets the grid value.
        /// </summary>
        /// <value>
        ///     The grid value.
        /// </value>
        public int GridValue
        {
            get => this.gridValue;
            set => SetProperty(ref this.gridValue, value);
        }

        /// <summary>
        ///     Gets or sets the selected image.
        /// </summary>
        /// <value>
        ///     The selected image.
        /// </value>
        public WriteableBitmap SelectedImage
        {
            get => this.selectedImage;
            set
            {
                this.selectedImage = value;
                OnPropertyChanged();
                this.RemoveCommand.OnCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets the original file ext.
        /// </summary>
        /// <value>
        ///     The original file ext.
        /// </value>
        public string OriginalFileExt { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is loading.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get => this.isLoading;
            set => SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [use at least once].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use at least once]; otherwise, <c>false</c>.
        /// </value>
        public bool UseAtLeastOnce
        {
            get => this.useAtLeastOnce;
            set => SetProperty(ref this.useAtLeastOnce, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [prevent juxtaposed].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [prevent juxtaposed]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventJuxtaposed
        {
            get => this.preventJuxtaposed;
            set => SetProperty(ref this.preventJuxtaposed, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MosaicMakerViewModel" /> class.
        /// </summary>
        public MosaicMakerViewModel()
        {
            this.initializeMembers();
            this.loadCommands();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the file.
        /// </summary>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <returns></returns>
        public async Task LoadFile(StorageFile sourceImageFile)
        {
            this.IsLoading = true;
            this.copyBitmapImage = await this.makeACopyOfTheFileToWorkOn(sourceImageFile);
            this.OriginalImage = await this.makeACopyOfTheFileToWorkOn(sourceImageFile);
            this.OriginalFileExt = sourceImageFile.FileType;

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                this.decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(this.copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(this.copyBitmapImage.PixelHeight)
                };

                this.dpiX = this.decoder.DpiX;
                this.dpiY = this.decoder.DpiY;

                var pixelData = await this.decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                this.SourcePixels = pixelData.DetachPixelData();
                this.IsLoading = false;
            }
        }

        /// <summary>
        ///     Adds the image.
        /// </summary>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <returns></returns>
        public async Task AddImageToPalette(StorageFile sourceImageFile)
        {
            using (var stream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var image = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                image.SetSource(stream);
                this.ImagePalette.Add(image);
            }
        }

        /// <summary>
        ///     Loads the folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <returns></returns>
        public async Task LoadFolder(StorageFolder sourceFolder)
        {
            this.IsLoading = true;
            var imageFiles = await sourceFolder.GetFilesAsync();

            foreach (var file in imageFiles)
            {
                if (!this.imageTypes.Contains(file.FileType))
                {
                    continue;
                }

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var image = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                    image.SetSource(stream);
                    this.ImagePalette.Add(image);
                }
            }

            this.IsLoading = false;
        }

        /// <summary>
        ///     Creates the solid mosaic.
        /// </summary>
        public void CreateSolidSquareMosaic()
        {
            this.IsLoading = true;
            this.SolidMosaicPixels = this.copySourcePixels();

            this.solidMosaic = new SolidMosaic(this.SolidMosaicPixels);

            this.solidMosaic.CreateSolidSquareMosaic(this.decoder.PixelWidth, this.decoder.PixelHeight, this.GridValue);

            this.MosaicImage =
                new WriteableBitmap(this.solidMosaic.Mosaic.PixelWidth, this.solidMosaic.Mosaic.PixelHeight);
            this.IsLoading = false;
        }

        /// <summary>
        ///     Creates the solid triangle mosaic.
        /// </summary>
        public void CreateSolidTriangleMosaic()
        {
            this.IsLoading = true;
            this.SolidMosaicPixels = this.copySourcePixels();

            this.solidMosaic = new SolidMosaic(this.SolidMosaicPixels);

            this.solidMosaic.CreateSolidTriangleMosaic(this.decoder.PixelWidth, this.decoder.PixelHeight,
                this.GridValue);

            this.MosaicImage =
                new WriteableBitmap(this.solidMosaic.Mosaic.PixelWidth, this.solidMosaic.Mosaic.PixelHeight);
            this.IsLoading = false;
        }

        /// <summary>
        ///     Creates the picture mosaic.
        /// </summary>
        public async void CreatePictureMosaic()
        {
            this.IsLoading = true;
            this.PictureMosaicPixels = this.copySourcePixels();
            var imageBytes = await this.convertWriteAbleCollectionToByte(this.ImagePalette);
            this.pictureMosaic = new PictureMosaic(this.PictureMosaicPixels, imageBytes);
            if (this.PreventJuxtaposed)
            {
                this.pictureMosaic.CreatePictureMosaicPreventJuxtaposed(this.decoder.PixelWidth,
                    this.decoder.PixelHeight, this.GridValue);
            }
            else if (this.UseAtLeastOnce)
            {
                this.pictureMosaic.CreatePictureMosaicUsingAllImagesBeforeRepeat(this.decoder.PixelWidth,
                    this.decoder.PixelHeight, this.GridValue);
            }
            else
            {
                this.pictureMosaic.CreatePictureMosaic(this.decoder.PixelWidth, this.decoder.PixelHeight,
                    this.GridValue);
            }

            this.MosaicImage =
                new WriteableBitmap(this.pictureMosaic.Mosaic.PixelWidth, this.pictureMosaic.Mosaic.PixelHeight);
            this.IsLoading = false;
        }

        /// <summary>
        ///     Creates the picture mosaic.
        /// </summary>
        public async void CreatePictureMosaicFromSelectedImages()
        {
            this.IsLoading = true;
            this.PictureMosaicPixels = this.copySourcePixels();

            var imageBytes = await this.convertWriteAbleCollectionToByte(this.SelectedImages);
            this.pictureMosaic = new PictureMosaic(this.PictureMosaicPixels, imageBytes);
            if (this.UseAtLeastOnce)
            {
                this.pictureMosaic.CreatePictureMosaicUsingAllImagesBeforeRepeat(this.decoder.PixelWidth,
                    this.decoder.PixelHeight, this.GridValue);
            }
            else if (this.PreventJuxtaposed)
            {
                this.pictureMosaic.CreatePictureMosaicPreventJuxtaposed(this.decoder.PixelWidth,
                    this.decoder.PixelHeight, this.GridValue);
            }
            else
            {
                this.pictureMosaic.CreatePictureMosaic(this.decoder.PixelWidth,
                    this.decoder.PixelHeight, this.GridValue);
            }

            this.MosaicImage =
                new WriteableBitmap(this.pictureMosaic.Mosaic.PixelWidth, this.pictureMosaic.Mosaic.PixelHeight);
            this.IsLoading = false;
        }

        /// <summary>
        ///     Creates the grid lines.
        /// </summary>
        public void CreateSquareGridLines()
        {
            this.GridPixels = this.copySourcePixels();
            this.gridLines = new GridLines(this.GridPixels);
            this.gridLines.CreateSquareGridLines(this.decoder.PixelWidth, this.decoder.PixelHeight, this.GridValue);
            this.GridImage = new WriteableBitmap(this.gridLines.Grid.PixelWidth, this.gridLines.Grid.PixelHeight);
        }

        /// <summary>
        ///     Creates the triangle grid lines.
        /// </summary>
        public void CreateTriangleGridLines()
        {
            this.GridPixels = this.copySourcePixels();
            this.gridLines = new GridLines(this.GridPixels);
            this.gridLines.CreateTriangleGridLines(this.decoder.PixelWidth, this.decoder.PixelHeight, this.GridValue);
            this.GridImage = new WriteableBitmap(this.gridLines.Grid.PixelWidth, this.gridLines.Grid.PixelHeight);
        }

        /// <summary>
        ///     Converts to black and white.
        /// </summary>
        public async void ConvertToBlackAndWhite()
        {
            this.IsLoading = true;
            this.BlackAndWhitePixels = await convertWriteAbleBitmapToByte(this.MosaicImage);

            var bWImage = MosaicCalculations.ConvertImageToBw(this.decoder.PixelWidth, this.decoder.PixelHeight,
                this.BlackAndWhitePixels);
            this.BlackAndWhiteImage = new WriteableBitmap(bWImage.PixelWidth, bWImage.PixelHeight);
            this.IsLoading = false;
        }

        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="image">The image.</param>
        public async void SaveFile(IRandomAccessStream stream, WriteableBitmap image)
        {
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            var pixelStream = image.PixelBuffer.AsStream();
            var pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                (uint) this.OriginalImage.PixelWidth,
                (uint) this.OriginalImage.PixelHeight, this.dpiX, this.dpiY, pixels);
            await encoder.FlushAsync();

            stream.Dispose();
        }

        /// <summary>
        ///     Clears the palette.
        /// </summary>
        public void ClearPalette()
        {
            this.ImagePalette.Clear();
        }

        private async Task<BitmapImage> makeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();

            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        private byte[] copySourcePixels()
        {
            var buffer = CryptographicBuffer.CreateFromByteArray(this.SourcePixels);
            CryptographicBuffer.CopyToByteArray(buffer, out var pixelCopy);
            return pixelCopy;
        }

        private static async Task<BitmapImage> resizeImage(StorageFile imageFile, int maxWidth, int maxHeight)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var sourceImage = new BitmapImage();
            sourceImage.SetSource(inputStream);
            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;
            var ratioX = maxWidth / (float) origWidth;
            var ratioY = maxHeight / (float) origHeight;
            var ratio = Math.Min(ratioX, ratioY);
            var newHeight = (int) (origHeight * ratio);
            var newWidth = (int) (origWidth * ratio);

            sourceImage.DecodePixelWidth = newWidth;
            sourceImage.DecodePixelHeight = newHeight;

            return sourceImage;
        }

        private static async Task<byte[]> convertWriteAbleBitmapToByte(WriteableBitmap copyOfImage)
        {
            byte[] pixels;
            using (var stream = copyOfImage.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint) stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);

                return pixels;
            }
        }

        private async Task<List<byte[]>> convertWriteAbleCollectionToByte(List<WriteableBitmap> collection)
        {
            var collectionOfImageBytes = new List<byte[]>();
            foreach (var image in collection)
            {
                var bytes = await convertWriteAbleBitmapToByte(image);
                collectionOfImageBytes.Add(bytes);
            }

            return collectionOfImageBytes;
        }

        private async Task<List<byte[]>> convertWriteAbleCollectionToByte(
            ObservableCollection<WriteableBitmap> collection)
        {
            var collectionOfImageBytes = new List<byte[]>();
            foreach (var image in collection)
            {
                var bytes = await convertWriteAbleBitmapToByte(image);
                collectionOfImageBytes.Add(bytes);
            }

            return collectionOfImageBytes;
        }

        private void initializeMembers()
        {
            this.OriginalImage = null;
            this.MosaicImage = null;
            this.ImagePalette = new ObservableCollection<WriteableBitmap>();
            this.dpiX = 0;
            this.dpiY = 0;
            this.GridValue = 25;
        }

        private void removeImage(object obj)
        {
            this.ImagePalette.Remove(this.SelectedImage);
        }

        private bool canDeleteImage(object obj)
        {
            return this.SelectedImage != null;
        }

        private void loadCommands()
        {
            this.RemoveCommand = new RelayCommand(this.removeImage, this.canDeleteImage);
        }

        #endregion
    }
}