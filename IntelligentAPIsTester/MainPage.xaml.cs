using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using CommunityToolkit.Labs.Intelligent.ImageClassification;
using CommunityToolkit.Labs.Intelligent.ObjectDetection;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.Media.Capture;
using Windows.Foundation;
namespace IntelligentLabsTest
{
    /// <summary>
    /// MainPage of the test project
    /// </summary>
    public sealed partial class MainPage : Page
    { 
        public MainPage()
        {
            this.InitializeComponent();

        }

        /// <summary>
        /// Called on click of "Pick Image" button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FilePickerButton.IsEnabled = false;
            FilePickerProgressRing.IsActive = true;
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();
            if (selectedStorageFile != null)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                Input input = new Input() { file = selectedStorageFile, typeOfInput = TypeOfInput.File };
                rootFrame.Navigate(typeof(ResultsPage), input);
            }
            else
            {
                FilePickerButton.IsEnabled = true;
                FilePickerProgressRing.IsActive = false;
            }
        }

        private async void CaptureImageButton_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI dialog = new CameraCaptureUI();
            Size aspectRatio = new Size(16, 9);
            dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

            StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if(file != null)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                Input input = new Input() { file = file, typeOfInput = TypeOfInput.Camera };
                rootFrame.Navigate(typeof(ResultsPage), input);
            }
        }

        public enum TypeOfInput
        {
            Camera,
            File
        }

        public class Input
        {
            public StorageFile file;
            public TypeOfInput typeOfInput;
        }
    }
}
