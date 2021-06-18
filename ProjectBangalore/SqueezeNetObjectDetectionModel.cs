using Newtonsoft.Json;
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

namespace ProjectBangalore
{

 
    public class SqueezeNetObjectDetectionModel
    {
        private const string _kModelFileName = "model.onnx";
        private const string _kLabelsFileName = "Labels.json";
        private LearningModel _model = null;
        private LearningModelSession _session;
        private List<string> _labels = new List<string>();
        private int _runCount = 0;
        private static SqueezeNetObjectDetectionModel instance;

        private SqueezeNetObjectDetectionModel()
        {
        }
        public static async Task<List<SqueezeNetResult>> ClassifyImage(StorageFile selectedStorageFile, int top)
        {
            if (instance == null)
            {
                instance = new SqueezeNetObjectDetectionModel();
            }
            return await instance.EvaluateModel(selectedStorageFile, top);
        }
        public async Task<List<SqueezeNetResult>> EvaluateModel(StorageFile selectedStorageFile, int top)
        {
            await LoadModelAsync();
            SoftwareBitmap softwareBitmap;
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

            // Encapsulate the image within a VideoFrame to be bound and evaluated
            VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

            return await EvaluateVideoFrameAsync(inputImage, top);
        }

        private async Task LoadModelAsync()
        {
            // just load the model one time.
            if (_model != null) return;

            try
            {
                // Parse labels from label json file.  We know the file's 
                // entries are already sorted in order.
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ProjectBangalore/Assets/Labels.json"));

                var fileString = await FileIO.ReadTextAsync(file);
         
                var fileDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileString);

                foreach (var kvp in fileDict)
                {
                    _labels.Add(kvp.Value);
                }


                var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ProjectBangalore/Assets/model.onnx"));
                _model = await LearningModel.LoadFromStorageFileAsync(modelFile);

                // Create the evaluation session with the model and device
                _session = new LearningModelSession(_model, new LearningModelDevice(GetDeviceKind()));
                
                
            }
            catch (Exception ex)
            {
                _model = null;
            }
        }

        LearningModelDeviceKind GetDeviceKind()
        {
 
            return LearningModelDeviceKind.Default;
        }


        private async Task<List<SqueezeNetResult>> EvaluateVideoFrameAsync(VideoFrame inputFrame, int top)
        {
            List<SqueezeNetResult> result = new List<SqueezeNetResult>();
            if (inputFrame != null)
            {
                try
                {
                    // create a binding object from the session
                    LearningModelBinding binding = new LearningModelBinding(_session);

                    // bind the input image
                    ImageFeatureValue imageTensor = ImageFeatureValue.CreateFromVideoFrame(inputFrame);
                    binding.Bind("data_0", imageTensor);

                    int ticks = Environment.TickCount;

                    // Process the frame with the model
                    var results = await _session.EvaluateAsync(binding, $"Run { ++_runCount } ");

                    ticks = Environment.TickCount - ticks;
                    string message = $"Run took { ticks } ticks";

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
                        top = 3;
                    }
                    for (int i = 0; i < top; i++)
                    {
                        message += $"\n\"{ _labels[indexedResults[i].index]}\" with confidence of { indexedResults[i].probability}";
                        result.Add(new SqueezeNetResult(_labels[indexedResults[i].index], indexedResults[i].probability));
                    }

                
                }
                catch (Exception ex)
                {
                  
                }            

            }

            return result;
        }
    }

    public class SqueezeNetResult
    {
        public string category;
        public float confidence;
        public SqueezeNetResult(string category, float confidence)
        {
            this.category = category;
            this.confidence = confidence;
        }
    }

}

    



