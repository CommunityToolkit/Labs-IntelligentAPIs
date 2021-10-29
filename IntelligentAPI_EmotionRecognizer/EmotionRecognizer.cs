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

namespace CommunityToolkit.Labs.Intelligent.EmotionRecognition
{
    public class EmotionRecognizer
    {
        private LearningModel _model = null;
        private LearningModelSession _session = null;
        int happinessEmotionIndex;
        private LearningModelBinding _binding = null;
        FaceDetector faceDetector;

        List<string> labels;


        private async Task LoadModelAsync()
        {
            // load model file
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/model_emotion.onnx"));

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
            happinessEmotionIndex = 1; //happiness
        }

        LearningModelDeviceKind GetDeviceKind()
        {
            return LearningModelDeviceKind.Cpu;
        }

        private async Task<IList<DetectedFace>> DetectFacesInImageAsync(SoftwareBitmap bitmap)
        {
            faceDetector = await FaceDetector.CreateAsync();
            var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            return await faceDetector.DetectFacesAsync(convertedBitmap);

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
