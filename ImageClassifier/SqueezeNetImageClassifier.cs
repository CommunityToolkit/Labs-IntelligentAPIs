// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.Json;


namespace CommunityToolkit.Labs.Intelligent.ImageClassification
{
    /// <summary>
    /// SqueezeNetImageClassifier is used to perform image classification using the SqueezeNet model.
    /// </summary>
    public class SqueezeNetImageClassifier
    {
        /// <summary>
        /// Model file name
        /// </summary>
        private const string _modelFileName = "model.onnx";

        /// <summary>
        /// Labels file name
        /// </summary>
        private const string _labelsFileName = "Labels.json";

        /// <summary>
        /// Learning model instance
        /// </summary>
        private LearningModel _model = null;

        /// <summary>
        /// LearningModelSession instance
        /// </summary>
        private LearningModelSession _session;

        /// <summary>
        /// list of labels that YOLOv4 can detect
        /// </summary>
        private List<string> _labels = new List<string>();

        /// <summary>
        /// Number of runs
        /// </summary>
        private int _runCount = 0;
        private static SqueezeNetImageClassifier instance;

        private SqueezeNetImageClassifier()
        {
        }

        /// <summary>
        /// Classifies image based on StorageFile input
        /// </summary>
        /// <param name="selectedStorageFile"></param>
        /// <param name="top">Top k results, accepts positive values up to 1000</param>
        /// <returns></returns>
        public static async Task<List<ClassificationResult>> ClassifyImage(StorageFile selectedStorageFile, uint top=3)
        {
            CreateInstanceIfNone();
            SoftwareBitmap softwareBitmap = await GenerateSoftwareBitmapFromStorageFile(selectedStorageFile);
            VideoFrame videoFrame = await GenerateVideoFrameFromBitmap(softwareBitmap);
            return await instance.EvaluateModel(videoFrame, top);
        }

        /// <summary>
        /// Classifies image based on SoftwareBitmap input 
        /// </summary>
        /// <param name="softwareBitmap"></param>
        /// <param name="top">Top k results, accepts positive values up to 1000</param>
        /// <returns></returns>
        public static async Task<List<ClassificationResult>> ClassifyImage(SoftwareBitmap softwareBitmap, uint top=3)
        {
            CreateInstanceIfNone();
            softwareBitmap = GetSoftwareBitmap(softwareBitmap);
            VideoFrame videoFrame = await GenerateVideoFrameFromBitmap(softwareBitmap);
            return await instance.EvaluateModel(videoFrame, top);
        }

        /// <summary>
        /// Classifies image based on VideoFrame input
        /// </summary>
        /// <param name="videoFrame"></param>
        /// <param name="top">Top k results, accepts positive values up to 1000</param>
        /// <returns></returns>
        public static async Task<List<ClassificationResult>> ClassifyImage(VideoFrame videoFrame, uint top=3)
        {
            CreateInstanceIfNone();
            return await instance.EvaluateModel(videoFrame, top);
        }


        /// <summary>
        /// Creates a new instance of SqueezeNetImageClassifier if it does not exist
        /// </summary>
        private static void CreateInstanceIfNone()
        {
            if (instance == null)
            {
                instance = new SqueezeNetImageClassifier();
            }
        }

        /// <summary>
        /// Evaluates the input image which is a VideoFrame instance
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public async Task<List<ClassificationResult>> EvaluateModel(VideoFrame inputImage, uint top)
        {
            await LoadModelAsync();
            return await EvaluateVideoFrameAsync(inputImage, top);
        }

        /// <summary>
        /// Converts object of type SoftwareBitmap to VideoFrame
        /// </summary>
        /// <param name="softwareBitmap"></param>
        /// <returns></returns>
        private static async Task<VideoFrame> GenerateVideoFrameFromBitmap(SoftwareBitmap softwareBitmap)
        {
            SoftwareBitmapSource imageSource = new SoftwareBitmapSource();
            await imageSource.SetBitmapAsync(softwareBitmap);

            // Encapsulate the image within a VideoFrame to be bound and evaluated
            VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            return videoFrame;
        }


        /// <summary>
        /// Converts object of type StorageFile to SoftwareBitmap
        /// </summary>
        /// <param name="selectedStorageFile"></param>
        /// <returns></returns>
        private static async Task<SoftwareBitmap> GenerateSoftwareBitmapFromStorageFile(StorageFile selectedStorageFile)
        {
            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await selectedStorageFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = GetSoftwareBitmap(softwareBitmap);
            }

            return softwareBitmap;
        }



        /// <summary>
        /// Get Software Bitmap
        /// </summary>
        /// <param name="softwareBitmap"></param>
        /// <returns></returns>
        private static SoftwareBitmap GetSoftwareBitmap(SoftwareBitmap softwareBitmap)
        {
            SoftwareBitmapSource imageSource = new SoftwareBitmapSource();
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || (softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore && softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied))
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            return softwareBitmap;
        }


        /// <summary>
        /// Loads an onnx model from file and deserializes the JSON file containing the labels
        /// </summary>
        /// <returns></returns>
        private async Task LoadModelAsync()
        {
            // just load the model one time.
            if (_model != null) return;

            try
            {
                // Parse labels from label json file.  We know the file's 
                // entries are already sorted in order.
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelligentAPI_ImageClassifier/Assets/" + _labelsFileName));

                var fileString = await FileIO.ReadTextAsync(file);
         
                var fileDict = JsonSerializer.Deserialize<Dictionary<string, string>>(fileString);

                foreach (var kvp in fileDict)
                {
                    _labels.Add(kvp.Value);
                }


                var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelligentAPI_ImageClassifier/Assets/" + _modelFileName));
                _model = await LearningModel.LoadFromStorageFileAsync(modelFile);

                // Create the evaluation session with the model and device
                _session = new LearningModelSession(_model, new LearningModelDevice(GetDeviceKind()));
                
                
            }
            catch (Exception ex)
            {
                _model = null;
                throw ex;
            }
        }

        /// <summary>
        /// Gets type of device to evaluate the model on 
        /// </summary>
        /// <returns></returns>
        LearningModelDeviceKind GetDeviceKind()
        {
 
            return LearningModelDeviceKind.Default;
        }

        /// <summary>
        /// Evaluate the SqueezeNet model 
        /// </summary>
        /// <param name="inputFrame"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        private async Task<List<ClassificationResult>> EvaluateVideoFrameAsync(VideoFrame inputFrame, uint top)
        {
            List<ClassificationResult> result = new List<ClassificationResult>();
            if (inputFrame != null)
            {
                try
                {
                    // create a binding object from the session
                    LearningModelBinding binding = new LearningModelBinding(_session);

                    // bind the input image
                    ImageFeatureValue imageTensor = ImageFeatureValue.CreateFromVideoFrame(inputFrame);
                    binding.Bind("data_0", imageTensor);

                    // Process the frame with the model
                    var results = await _session.EvaluateAsync(binding, $"Run { ++_runCount } ");

                    // retrieve results from evaluation
                    var resultTensor = results.Outputs["softmaxout_1"] as TensorFloat;
                    var resultVector = resultTensor.GetAsVectorView();

                    // Find the top 3 probabilities
                    List<(int index, float probability)> indexedResults = new List<(int, float)>();
                    for (int i = 0; i < resultVector.Count; i++)
                    {
                        indexedResults.Add((index: i, probability: resultVector.ElementAt(i)));
                    }
                    indexedResults.Sort((a, b) =>
                    {
                        if (a.probability < b.probability)
                        {
                            return 1;
                        }
                        else if (a.probability > b.probability)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    });

                    if(top < 0 || top > 1000)
                    {
                        throw new ArgumentOutOfRangeException("top", "Value is out of range, expected to be between 0 and 1000");
                    }
                    for (int i = 0; i < top; i++)
                    {
                        result.Add(new ClassificationResult(_labels[indexedResults[i].index], indexedResults[i].probability));
                    }

                
                }
                catch (Exception ex)
                {
                    throw ex; 
                }            

            }

            return result;
        }
    }

    /// <summary>
    /// Result of Image Classification model evaluation. 
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Category that the image belongs to
        /// </summary>
        public string category;

        /// <summary>
        /// Confidence value of the predicted category.
        /// </summary>
        public float confidence;
        public ClassificationResult(string category, float confidence)
        {
            this.category = category;
            this.confidence = confidence;
        }
    }

}

    



