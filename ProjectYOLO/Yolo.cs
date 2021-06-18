// This file was automatically generated by VS extension Windows Machine Learning Code Generator v3
// from model file Yolo.onnx
// Warning: This file may get overwritten if you add add an onnx file with the same name
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.AI.MachineLearning;
namespace ProjectYOLO
{
    
    public sealed class YoloInput
    {
        public ImageFeatureValue input_100; // BitmapPixelFormat: Bgra8, BitmapAlphaMode: Premultiplied, width: 416, height: 416
    }
    
    public sealed class YoloOutput
    {
        public TensorFloat Identity00; // shape(-1,-1,-1)
    }
    
    public sealed class YoloModel
    {
        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        public static async Task<YoloModel> CreateFromStreamAsync(IRandomAccessStreamReference stream)
        {
            YoloModel learningModel = new YoloModel();
            learningModel.model = await LearningModel.LoadFromStreamAsync(stream);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }
        public async Task<YoloOutput> EvaluateAsync(YoloInput input)
        {
            binding.Bind("input_1:0", input.input_100);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new YoloOutput();
            output.Identity00 = result.Outputs["Identity:0"] as TensorFloat;
            return output;
        }

    }
}

