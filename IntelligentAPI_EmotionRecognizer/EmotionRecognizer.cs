using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CommunityToolkit.Labs.Intelligent.EmotionRecognition
{
    public class DetectedEmotion
    {
        public int emotionIndex;
        public string emotion;      
    }
    public class EmotionRecognizer
    {
        private LearningModel _model = null;
        private LearningModelSession _session = null;
        private LearningModelBinding _binding = null;
        private static EmotionRecognizer instance = null;

        private static List<string> labels;


        private async void InitModelAsync()
        {
            // load model file
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelligentAPI_EmotionRecognizer/Assets/model_emotion.onnx"));

            //Loads the mdoel from the file
            _model = await LearningModel.LoadFromStorageFileAsync(file);

            //Creating a session that binds the model to the device running the model
            _session = new LearningModelSession(_model, new LearningModelDevice(GetDeviceKind()));
        }

        private void LoadLabels()
        {
            labels = new List<string>()
            {
                "Neutral",
                "Happiness",
                "Surprise",
                "Sadness",
                "Anger",
                "Disgust",
                "Fear",
                "Contempt"
            };
        }

        LearningModelDeviceKind GetDeviceKind()
        {
            return LearningModelDeviceKind.Cpu;
        }

        private async static Task<IList<DetectedFace>> DetectFacesInImageAsync(SoftwareBitmap bitmap)
        {
            FaceDetector faceDetector = await FaceDetector.CreateAsync();
            var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            return await faceDetector.DetectFacesAsync(convertedBitmap);

        }

        public async static Task<DetectedEmotion> DetectEmotion(SoftwareBitmap bitmap)
        {
            if (instance == null)
            {
                instance = new EmotionRecognizer();
            }

            return await instance.EvaluateFrame(bitmap);
        }

        public async Task<DetectedEmotion> EvaluateFrame(SoftwareBitmap softwareBitmap)
        {
            InitModelAsync();
            LoadLabels();
            DetectedFace detectedFace = await DetectFace(softwareBitmap);
            if (detectedFace != null)
            {
                return await EvaluateEmotionInFace(detectedFace, softwareBitmap);
            }
            return null;
        }

        public async Task<DetectedEmotion> EvaluateEmotionInFace(DetectedFace detectedFace, SoftwareBitmap softwareBitmap)
        {

                var boundingBox = new Rect(detectedFace.FaceBox.X,
                                          detectedFace.FaceBox.Y,
                                          detectedFace.FaceBox.Width,
                                          detectedFace.FaceBox.Height);

                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8);

                var croppedFace = await Crop(softwareBitmap, boundingBox);
                LearningModelEvaluationResult emotionResults = await BindAndEvaluateModelAsync(croppedFace);

                // to get percentages, you'd need to run the output through a softmax function
                // we don't need percentages, we just need max value
                TensorFloat emotionIndexTensor = emotionResults.Outputs["Plus692_Output_0"] as TensorFloat;

                var emotionList = emotionIndexTensor.GetAsVectorView().ToList();
                var emotionIndex = emotionList.IndexOf(emotionList.Max());

                return new DetectedEmotion() { emotionIndex = emotionIndex, emotion = labels[emotionIndex] };
            

        }

        private static async Task<DetectedFace> DetectFace(SoftwareBitmap softwareBitmap)
        {
            var faces = await DetectFacesInImageAsync(softwareBitmap);

            // if there is a face in the frame, evaluate the emotion
            var detectedFace = faces.FirstOrDefault();
            return detectedFace;
        }

        public static async Task<SoftwareBitmap> Crop(SoftwareBitmap softwareBitmap, Rect bounds)
        {
            VideoFrame vid = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            vid = await Crop(vid, bounds);
            return vid.SoftwareBitmap;

        }
        public static async Task<VideoFrame> Crop(VideoFrame videoFrame, Rect bounds)
        {
            BitmapBounds cropBounds = new BitmapBounds()
            {
                Width = (uint)bounds.Width,
                Height = (uint)bounds.Height,
                X = (uint)bounds.X,
                Y = (uint)bounds.Y
            };
            VideoFrame result = new VideoFrame(BitmapPixelFormat.Bgra8,
                                               (int)cropBounds.Width,
                                               (int)cropBounds.Height,
                                               BitmapAlphaMode.Premultiplied);

            await videoFrame.CopyToAsync(result, cropBounds, null);

            return result;
        }


        private async Task<LearningModelEvaluationResult> BindAndEvaluateModelAsync(SoftwareBitmap croppedFace)
        {
            //Create Learning model binding which binds 
            _binding = new LearningModelBinding(_session);
            _binding.Bind("Input3", VideoFrame.CreateWithSoftwareBitmap(croppedFace));
            var emotionResults = await _session.EvaluateAsync(_binding, "id");
            return emotionResults;
        }

    }
}
