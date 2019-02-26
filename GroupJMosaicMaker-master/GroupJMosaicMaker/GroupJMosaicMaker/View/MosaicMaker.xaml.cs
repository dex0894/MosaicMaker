using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupJMosaicMaker.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupJMosaicMaker.View
{
    /// <inheritdoc />
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        /// <summary>
        ///     The application height
        /// </summary>
        public const int ApplicationHeight = 525;

        /// <summary>
        ///     The application width
        /// </summary>
        public const int ApplicationWidth = 1424;

        private const ContentDialogResult FolderSelected = ContentDialogResult.Secondary;
        private const ContentDialogResult SourceImageSelected = ContentDialogResult.Primary;
        private const ContentDialogResult Replace = ContentDialogResult.Primary;
        private const ContentDialogResult Merge = ContentDialogResult.Secondary;
        private const int UpperBoundary = 50;
        private const int LowerBoundary = 5;
        private readonly MosaicMakerViewModel mosaicViewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size {Width = ApplicationWidth, Height = ApplicationHeight};
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(ApplicationWidth, ApplicationHeight));

            this.mosaicViewModel = new MosaicMakerViewModel();
            DataContext = this.mosaicViewModel;
        }

        #endregion

        #region Methods

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "Mosaic",
                DefaultFileExtension = this.mosaicViewModel.OriginalFileExt
            };

            this.setupFileSaverFileTypes(fileSavePicker);
            var saveFile = await fileSavePicker.PickSaveFileAsync();

            if (saveFile != null)
            {
                var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
                if (this.blackAndWhiteToggle.IsOn)
                {
                    this.mosaicViewModel.SaveFile(stream, this.mosaicViewModel.BlackAndWhiteImage);
                }
                else
                {
                    this.mosaicViewModel.SaveFile(stream, this.mosaicViewModel.MosaicImage);
                }
            }
        }

        private void setupFileSaverFileTypes(FileSavePicker fileSavePicker)
        {
            var originalFIleExt = this.mosaicViewModel.OriginalFileExt;

            switch (originalFIleExt)
            {
                case ".png":
                    fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
                    fileSavePicker.FileTypeChoices.Add("JPG files", new List<string> {".jpg"});
                    fileSavePicker.FileTypeChoices.Add("BMP files", new List<string> {".bmp"});
                    break;
                case ".jpg":
                    fileSavePicker.FileTypeChoices.Add("JPG files", new List<string> {".jpg"});
                    fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
                    fileSavePicker.FileTypeChoices.Add("BMP files", new List<string> {".bmp"});
                    break;
                case ".bmp":
                    fileSavePicker.FileTypeChoices.Add("BMP files", new List<string> {".bmp"});
                    fileSavePicker.FileTypeChoices.Add("JPG files", new List<string> {".jpg"});
                    fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
                    break;
            }
        }

        private async void loadButton_Click(object sender, RoutedEventArgs e)
        {
            var loadTypeDialog = new ContentDialog {
                Title = "Would you like to load a source image or a folder for the picture mosaic palette?",
                PrimaryButtonText = "Source Image",
                SecondaryButtonText = "Folder"
            };
            var result = await loadTypeDialog.ShowAsync();

            switch (result)
            {
                case SourceImageSelected:
                    var sourceImageFile = await this.selectImageFile();
                    if (sourceImageFile == null)
                    {
                        return;
                    }

                    await this.mosaicViewModel.LoadFile(sourceImageFile);
                    this.activateButtons();
                    this.activatePictureMosaicBtn();
                    break;

                case ContentDialogResult.None:
                    loadTypeDialog.Hide();
                    break;

                case FolderSelected:
                    await this.checkLoadChoice();
                    var folder = await this.selectStorageFolder();
                    if (folder == null)
                    {
                        return;
                    }

                    await this.mosaicViewModel.LoadFolder(folder);
                    if (this.sourceImageDisplay.Source != null)
                    {
                        this.activatePictureMosaicBtn();
                        this.activateButtons();
                    }

                    this.enableLoadedImagesFeatures();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.sourceImageDisplay.Source = this.mosaicViewModel.OriginalImage;
            await this.updateGridLines();
        }

        private void enableLoadedImagesFeatures()
        {
            this.selectImageOpt.IsEnabled = true;
            this.imageUsedOnceOpt.IsEnabled = true;
            this.preventJuxtaposed.IsEnabled = true;
            this.loadedImagesClear.IsEnabled = true;
            this.loadedImagesAdd.IsEnabled = true;
        }

        private async Task checkLoadChoice()
        {
            if (this.mosaicViewModel.ImagePalette.Count > 0)
            {
                var mergeDialogue = new ContentDialog {
                    Title = "Would you like to replace the existing Palette, or add to the images?",
                    PrimaryButtonText = "Replace",
                    SecondaryButtonText = "Add To"
                };
                var userResult = await mergeDialogue.ShowAsync();

                switch (userResult)
                {
                    case Replace:
                        this.mosaicViewModel.ImagePalette.Clear();
                        break;
                    case Merge:
                        break;
                }
            }
        }

        private void activateButtons()
        {
            this.solidMosaicBtn.IsEnabled = true;
            this.squareGridToggle.IsEnabled = true;
            this.triangleGridToggle.IsEnabled = true;
            this.mosaicOptions.IsEnabled = true;
            this.zoomSwitch.IsEnabled = true;
        }

        private async Task<StorageFile> selectImageFile()
        {
            var warningDialog = new ContentDialog {
                Title = "The selected file type was invalid, please select a file with the correct format.",
                PrimaryButtonText = "OK"
            };
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add("*");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                if (!openPicker.FileTypeFilter.Contains(file.FileType))
                {
                    await warningDialog.ShowAsync();
                    file = null;
                    file = await openPicker.PickSingleFileAsync();
                }
            }

            return file;
        }

        private async Task<StorageFolder> selectStorageFolder()
        {
            var openPicker = new FolderPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var folder = await openPicker.PickSingleFolderAsync();

            return folder;
        }

        private async void GridButton_Click(object sender, RoutedEventArgs e)
        {
            var gridDialog = new SetGridDialog();
            var invalidEntryDialog = new ContentDialog {
                Title = "Please enter a value between 5 and 50.", PrimaryButtonText = "OK"
            };
            var result = await gridDialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    var output = gridDialog.UserText;

                    if (int.TryParse(output, out var parsedInt))
                    {
                        var grid = parsedInt;
                        if (grid < LowerBoundary || grid > UpperBoundary)
                        {
                            await invalidEntryDialog.ShowAsync();
                            break;
                        }

                        this.gridValue.Text = string.Empty + grid;
                    }

                    await this.updateGridLines();
                    break;
                case ContentDialogResult.Secondary:
                    gridDialog.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.solidMosaicBtn.IsEnabled = true;
            this.activatePictureMosaicBtn();
        }

        private async Task updateGridLines()
        {
            if ((bool) this.squareGridToggle.IsChecked)
            {
                await this.createSquareGrid();
            }
            else if ((bool) this.triangleGridToggle.IsChecked)
            {
                await this.createTriangleGrid();
            }
        }

        private void activatePictureMosaicBtn()
        {
            if (this.mosaicViewModel.ImagePalette.Count > 0)
            {
                this.pictureMosaicBtn.IsEnabled = true;
            }
        }

        private async void SolidSquareMosaicBtn_Click(object sender, RoutedEventArgs e)
        {
            this.turnOffBlackAndWhiteToggle();
            await this.createSolidSquareMosaic();
            this.saveBtn.IsEnabled = true;
            this.solidMosaicBtn.IsEnabled = false;
            this.activatePictureMosaicBtn();
            this.blackAndWhiteToggle.IsEnabled = true;
        }

        private void turnOffBlackAndWhiteToggle()
        {
            if (this.blackAndWhiteToggle.IsOn)
            {
                this.blackAndWhiteToggle.IsOn = false;
            }
        }

        private async void SolidTriangleMosaicBtn_Click(object sender, RoutedEventArgs e)
        {
            this.turnOffBlackAndWhiteToggle();
            await this.createSolidTriangleMosaic();
            this.saveBtn.IsEnabled = true;
            this.solidMosaicBtn.IsEnabled = false;
            this.activatePictureMosaicBtn();
            this.blackAndWhiteToggle.IsEnabled = true;
        }

        private async Task createSolidSquareMosaic()
        {
            this.mosaicViewModel.CreateSolidSquareMosaic();
            this.createBothGridLines();

            using (var writeStream = this.mosaicViewModel.MosaicImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.mosaicViewModel.SolidMosaicPixels, 0,
                    this.mosaicViewModel.SolidMosaicPixels.Length);

                this.mosaicImageDisplay.Source = this.mosaicViewModel.MosaicImage;
                await writeStream.FlushAsync();
                writeStream.Dispose();
            }
        }

        private async Task createSolidTriangleMosaic()
        {
            this.mosaicViewModel.CreateSolidTriangleMosaic();
            this.createBothGridLines();

            using (var writeStream = this.mosaicViewModel.MosaicImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.mosaicViewModel.SolidMosaicPixels, 0,
                    this.mosaicViewModel.SolidMosaicPixels.Length);

                this.mosaicImageDisplay.Source = this.mosaicViewModel.MosaicImage;
                await writeStream.FlushAsync();
                writeStream.Dispose();
            }
        }

        private void createBothGridLines()
        {
            this.mosaicViewModel.CreateSquareGridLines();
            this.mosaicViewModel.CreateTriangleGridLines();
        }

        private async void SquareGridToggle_OnClick(object sender, RoutedEventArgs e)
        {
            await this.createSquareGrid();
        }

        private async void TriangleGridToggle_OnClick(object sender, RoutedEventArgs e)
        {
            await this.createTriangleGrid();
        }

        private async Task createSquareGrid()
        {
            if (this.squareGridToggle.IsChecked == true)
            {
                this.triangleGridToggle.IsChecked = false;
                this.mosaicViewModel.CreateSquareGridLines();
                using (var writeStream = this.mosaicViewModel.GridImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.mosaicViewModel.GridPixels, 0,
                        this.mosaicViewModel.GridPixels.Length);

                    this.sourceImageDisplay.Source = this.mosaicViewModel.GridImage;
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }
            }
            else
            {
                this.sourceImageDisplay.Source = this.mosaicViewModel.OriginalImage;
            }
        }

        private async Task createTriangleGrid()
        {
            if (this.triangleGridToggle.IsChecked == true)
            {
                this.squareGridToggle.IsChecked = false;
                this.mosaicViewModel.CreateTriangleGridLines();
                using (var writeStream = this.mosaicViewModel.GridImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.mosaicViewModel.GridPixels, 0,
                        this.mosaicViewModel.GridPixels.Length);

                    this.sourceImageDisplay.Source = this.mosaicViewModel.GridImage;
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }
            }
            else
            {
                this.sourceImageDisplay.Source = this.mosaicViewModel.OriginalImage;
            }
        }

        private async void PictureMosaicBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.turnOffBlackAndWhiteToggle();
            await this.createPictureMosaic();
            this.pictureMosaicBtn.IsEnabled = false;
            this.solidMosaicBtn.IsEnabled = true;
            this.saveBtn.IsEnabled = true;
            this.blackAndWhiteToggle.IsEnabled = true;
        }

        private async Task createSelectedPictureMosaic()
        {
            this.mosaicViewModel.CreatePictureMosaicFromSelectedImages();
            using (var writeStream = this.mosaicViewModel.MosaicImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.mosaicViewModel.PictureMosaicPixels, 0,
                    this.mosaicViewModel.PictureMosaicPixels.Length);

                this.mosaicImageDisplay.Source = this.mosaicViewModel.MosaicImage;
                await writeStream.FlushAsync();
                writeStream.Dispose();
            }
        }

        private async void SelectedPictureMosaicBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.turnOffBlackAndWhiteToggle();
            await this.createSelectedPictureMosaic();
            this.pictureMosaicBtn.IsEnabled = false;
            this.solidMosaicBtn.IsEnabled = true;
            this.saveBtn.IsEnabled = true;
        }

        private async Task createPictureMosaic()
        {
            this.mosaicViewModel.CreatePictureMosaic();
            using (var writeStream = this.mosaicViewModel.MosaicImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(this.mosaicViewModel.PictureMosaicPixels, 0,
                    this.mosaicViewModel.PictureMosaicPixels.Length);

                this.mosaicImageDisplay.Source = this.mosaicViewModel.MosaicImage;
                await writeStream.FlushAsync();
                writeStream.Dispose();
            }
        }

        private void LoadedImagesClear_Click(object sender, RoutedEventArgs e)
        {
            this.mosaicViewModel.ClearPalette();
            this.deactivateButtons();
        }

        private void deactivateButtons()
        {
            this.loadedImagesClear.IsEnabled = false;
            this.loadedImagesAdd.IsEnabled = false;
            this.pictureMosaicBtn.IsEnabled = false;
            this.selectImageOpt.IsEnabled = false;
            this.imageUsedOnceOpt.IsEnabled = false;
            this.preventJuxtaposed.IsEnabled = false;
            this.preventJuxtaposed.IsChecked = false;
            this.imageUsedOnceOpt.IsChecked = false;
        }

        private async void LoadedImagesAdd_Click(object sender, RoutedEventArgs e)
        {
            var imageFile = await this.selectImageFile();
            if (imageFile == null)
            {
                return;
            }

            await this.mosaicViewModel.AddImageToPalette(imageFile);
        }

        private void ZoomSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.zoomSwitch.IsOn)
            {
                this.enableZoomFeatures();
            }
            else
            {
                this.disableZoomFeatures();
            }
        }

        private void disableZoomFeatures()
        {
            this.mosaicImageZoom.ZoomMode = ZoomMode.Disabled;
            this.sourceImageZoom.ZoomMode = ZoomMode.Disabled;
            this.zoomIncrease.IsEnabled = false;
            this.zoomDecrease.IsEnabled = false;
            this.mosaicImageZoom.ChangeView(0, 0, 1);
            this.sourceImageZoom.ChangeView(0, 0, 1);
        }

        private void enableZoomFeatures()
        {
            this.mosaicImageZoom.ZoomMode = ZoomMode.Enabled;
            this.sourceImageZoom.ZoomMode = ZoomMode.Enabled;
            this.zoomIncrease.IsEnabled = true;
            this.zoomDecrease.IsEnabled = true;
        }

        private void ZoomDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (this.mosaicImageZoom.ZoomFactor > 1)
            {
                this.mosaicImageZoom.ChangeView(150, 150, this.mosaicImageZoom.ZoomFactor - 1);
            }

            if (this.sourceImageZoom.ZoomFactor > 1)
            {
                this.sourceImageZoom.ChangeView(150, 150, this.sourceImageZoom.ZoomFactor - 1);
            }
        }

        private void ZoomIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (this.mosaicImageZoom.ZoomFactor < 3)
            {
                this.mosaicImageZoom.ChangeView(150, 150, this.mosaicImageZoom.ZoomFactor + 1);
            }

            if (this.sourceImageZoom.ZoomFactor < 3)
            {
                this.sourceImageZoom.ChangeView(150, 150, this.sourceImageZoom.ZoomFactor + 1);
            }
        }

        private async void BlackAndWhiteToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.blackAndWhiteToggle.IsOn)
            {
                this.mosaicViewModel.ConvertToBlackAndWhite();

                using (var writeStream = this.mosaicViewModel.BlackAndWhiteImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(this.mosaicViewModel.BlackAndWhitePixels, 0,
                        this.mosaicViewModel.BlackAndWhitePixels.Length);

                    this.mosaicImageDisplay.Source = this.mosaicViewModel.BlackAndWhiteImage;
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }
            }
            else
            {
                this.mosaicImageDisplay.Source = this.mosaicViewModel.MosaicImage;
            }
        }

        private void LoadedImagePalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.mosaicViewModel.SelectedImages =
                this.loadedImagePalette.SelectedItems.Cast<WriteableBitmap>().ToList();
            if (this.mosaicViewModel.SelectedImages.Count > 0)
            {
                this.useSelectedImages.IsEnabled = true;
            }
            else
            {
                this.useSelectedImages.IsEnabled = false;
            }
        }

        private void SelectImagesOption_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.loadedImagePalette.SelectionMode == ListViewSelectionMode.Single)
            {
                this.loadedImagePalette.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                this.loadedImagePalette.SelectionMode = ListViewSelectionMode.Single;
            }
        }

        private void PreventJuxtaposed_OnClick(object sender, RoutedEventArgs e)
        {
            this.imageUsedOnceOpt.IsChecked = false;
            this.pictureMosaicBtn.IsEnabled = true;
        }

        private void ImageUsedOnceOpt_OnClick(object sender, RoutedEventArgs e)
        {
            this.preventJuxtaposed.IsChecked = false;
            this.pictureMosaicBtn.IsEnabled = true;
        }

        #endregion
    }
}