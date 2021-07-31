using CommunityToolkit.Labs.Intelligent.ImageClassification;
using CommunityToolkit.Labs.Intelligent.ObjectDetection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IntelligentLabsTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultsPage : Page
    {
        /// <summary>
        /// Transparent fill inside bounding box
        /// </summary>
        private readonly SolidColorBrush _fill_brush = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// Green brush for bounding box borders
        /// </summary>
        private readonly SolidColorBrush _line_brush = new SolidColorBrush(Colors.DarkGreen);

        /// <summary>
        /// Line thickness of bounding box
        /// </summary>
        private readonly double _line_thickness = 5.0;

        public ResultsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Calls Classification and Object Detection APIs when a file is selected.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (!(e.Parameter is StorageFile selectedStorageFile))
            {
                rootFrame.GoBack();
                return;
            }

            await DisplayImage(selectedStorageFile);

            //Use Squeezenet model to classify image
            List<ClassificationResult> imageClasses = await SqueezeNetImageClassifier.ClassifyImage(selectedStorageFile, 3);


            //Use YOLOv4 to detect objects
            List<DetectionResult> listOfObjects = await YOLOObjectDetector.DetectObjects(selectedStorageFile);

            UpdateTextBox(imageClasses, listOfObjects);
            ProgressRing.IsActive = false;
            Dimmer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the image uploaded
        /// </summary>
        /// <param name="selectedStorageFile"></param>
        /// <returns></returns>
        private async Task DisplayImage(StorageFile selectedStorageFile)
        {
            SoftwareBitmap softwareBitmap;
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

        /// <summary>
        /// Updates the text box with results from image classification and object detection
        /// </summary>
        /// <param name="imageClasses"></param>
        /// <param name="listOfObjects"></param>
        /// <returns></returns>
        private void UpdateTextBox(List<ClassificationResult> imageClasses, List<DetectionResult> listOfObjects)
        {
            ResultsBlock.Text = "";
            for (int i = 0; i < imageClasses.Count; ++i)
            {
                if (i == 0)
                {
                    ResultsBlock.Text = "Image classification Results: \n"; ;
                }

                ResultsBlock.Text += imageClasses[i].category + " (" + Math.Round(imageClasses[i].confidence, 2) + ")\n";

            }

            DrawBoxes(listOfObjects);
        }

        /// <summary>
        /// Draws bounding boxes on the output frame based on evaluation result
        /// </summary>
        private void DrawBoxes(List<DetectionResult> results)
        {
            OverlayCanvas.Height = UIPreviewImage.ActualHeight;
            OverlayCanvas.Width = UIPreviewImage.Width;

            for (int i = 0; i < results.Count; ++i)
            {
                int top = (int)(results[i].bbox[0] * UIPreviewImage.ActualHeight);
                int left = (int)(results[i].bbox[1] * UIPreviewImage.Width);
                int bottom = (int)(results[i].bbox[2] * UIPreviewImage.ActualHeight);
                int right = (int)(results[i].bbox[3] * UIPreviewImage.Width);

                var r = new Rectangle();
                r.Width = right - left;
                r.Height = bottom - top;
                r.Fill = this._fill_brush;
                r.Stroke = this._line_brush;
                r.StrokeThickness = this._line_thickness;
                r.Margin = new Thickness(left, top, 0, 0);
                r.Visibility = Visibility.Visible;
                this.OverlayCanvas.Children.Add(r);


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

                Canvas.SetLeft(border, results[i].bbox[1] * UIPreviewImage.Width + 2);
                Canvas.SetTop(border, results[i].bbox[0] * UIPreviewImage.ActualHeight);
                textBlock.Visibility = Visibility.Visible;
                // Add to canvas
                this.OverlayCanvas.Children.Add(border);
            }
        }

        /// <summary>
        /// Click event for back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }
    }
}
