using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using ImageClassification;
using ObjectDetection;
using Windows.UI.Xaml.Shapes;
using Windows.UI;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ProjectBangaloreTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SoftwareBitmap softwareBitmap;

        private readonly SolidColorBrush _fill_brush = new SolidColorBrush(Colors.Transparent);
        private readonly SolidColorBrush _line_brush = new SolidColorBrush(Colors.DarkGreen);
        private readonly double _line_thickness = 5.0;

        public MainPage()
        {
            this.InitializeComponent();

        }
        private async void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();
            if (selectedStorageFile != null)
            {
                await DisplayImage(selectedStorageFile);


                //Use Squeezenet model to classify image
                var imageClasses = await ImageClassification.SqueezeNetImageClassifier.ClassifyImage(selectedStorageFile, 3);


                //Use YOLOv4 to detect objects
                var listOfObjects = await ObjectDetection.YOLOObjectDetector.DetectObjects(selectedStorageFile);

                UpdateTextBoxAsync(imageClasses, listOfObjects);
            }

        }

        private async Task UpdateTextBoxAsync(List<SqueezeNetResult> imageClasses, IReadOnlyList<YOLOObjectDetector.DetectionResult> listOfObjects)
        {
            StatusBlock.Text = "";
            for (int i = 0; i < imageClasses.Count; ++i)
            {
                if (i == 0)
                {
                    StatusBlock.Text = "Image classification Results: \n"; ;
                }

                StatusBlock.Text += imageClasses[i].category + " (" + imageClasses[i].confidence + ")\n";

            }

            for (int i = 0; i < listOfObjects.Count; ++i)
            {
                if (i == 0)
                {
                    StatusBlock.Text += "Object Detection Results : \n";
                }

                StatusBlock.Text += listOfObjects[i].label + "\n";

            }
            await DrawBoxes(listOfObjects);
        }


        // draw bounding boxes on the output frame based on evaluation result
        private async Task DrawBoxes(IReadOnlyList<YOLOObjectDetector.DetectionResult> results)
        {

            for (int i = 0; i < results.Count; ++i)
            {
                int top = (int)(results[i].bbox[0] * UIPreviewImage.Height);
                int left = (int)(results[i].bbox[1] * UIPreviewImage.Width);
                int bottom = (int)(results[i].bbox[2] * UIPreviewImage.Height);
                int right = (int)(results[i].bbox[3] * UIPreviewImage.Width);

                var brush = new ImageBrush();
                var bitmap_source = new SoftwareBitmapSource();
                await bitmap_source.SetBitmapAsync(softwareBitmap);

                brush.ImageSource = bitmap_source;
                brush.Stretch = Stretch.Fill;

                this.OverlayCanvas.Background = brush;

                var r = new Rectangle();
                r.Width = right - left;
                r.Height = bottom - top;
                r.Fill = this._fill_brush;
                r.Stroke = this._line_brush;
                r.StrokeThickness = this._line_thickness;
                r.Margin = new Thickness(left, top, 0, 0);

                this.OverlayCanvas.Children.Add(r);
                // Default configuration for border
                // Render text label


                var border = new Border();
                var backgroundColorBrush = new SolidColorBrush(Colors.Black);
                var foregroundColorBrush = new SolidColorBrush(Colors.SpringGreen);
                var textBlock = new TextBlock();
                textBlock.Foreground = foregroundColorBrush;
                textBlock.FontSize = 18;

                textBlock.Text = results[i].label;
                // Hide
                textBlock.Visibility = Visibility.Collapsed;
                border.Background = backgroundColorBrush;
                border.Child = textBlock;

                Canvas.SetLeft(border, results[i].bbox[1] * 416 + 2);
                Canvas.SetTop(border, results[i].bbox[0] * 416 + 2);
                textBlock.Visibility = Visibility.Visible;
                // Add to canvas
                this.OverlayCanvas.Children.Add(border);
            }
        }

        private async Task DisplayImage(StorageFile selectedStorageFile)
        {
            OverlayCanvas.Children.Clear();
            using (IRandomAccessStream stream = await selectedStorageFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            // Display the image
            SoftwareBitmapSource imageSource = new SoftwareBitmapSource();
            await imageSource.SetBitmapAsync(softwareBitmap);
            UIPreviewImage.Source = imageSource;
        }
    }
}
