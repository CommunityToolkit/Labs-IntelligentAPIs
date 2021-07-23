// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace IntelligentAPI.ObjectDetection
{
    /// <summary>
    /// YOLOObjectDetector is used to perform object detection using the YOLOv4 model.
    /// </summary>
    public class YOLOObjectDetector
    {
        /// <summary>
        /// LearningModel instance
        /// </summary>
        private LearningModel _model = null;

        /// <summary>
        /// LearningModelSession instance
        /// </summary>
        private LearningModelSession _session;

        /// <summary>
        /// LearningModelBinding instance
        /// </summary>
        private LearningModelBinding _binding;

        /// <summary>
        /// YOLOObjectDetector instance
        /// </summary>
        private static YOLOObjectDetector instance;

        /// <summary>
        /// List of labels  
        /// </summary>
        private readonly string[] _labels =
    {
                "person",
                "bicycle",
                "car",
                "motorbike",
                "aeroplane",
                "bus",
                "train",
                "truck",
                "boat",
                "traffic light",
                "fire hydrant",
                "stop sign",
                "parking meter",
                "bench",
                "bird",
                "cat",
                "dog",
                "horse",
                "sheep",
                "cow",
                "elephant",
                "bear",
                "zebra",
                "giraffe",
                "backpack",
                "umbrella",
                "handbag",
                "tie",
                "suitcase",
                "frisbee",
                "skis",
                "snowboard",
                "sports ball",
                "kite",
                "baseball bat",
                "baseball glove",
                "skateboard",
                "surfboard",
                "tennis racket",
                "bottle",
                "wine glass",
                "cup",
                "fork",
                "knife",
                "spoon",
                "bowl",
                "banana",
                "apple",
                "sandwich",
                "orange",
                "broccoli",
                "carrot",
                "hot dog",
                "pizza",
                "donut",
                "cake",
                "chair",
                "sofa",
                "pottedplant",
                "bed",
                "diningtable",
                "toilet",
                "tvmonitor",
                "laptop",
                "mouse",
                "remote",
                "keyboard",
                "cell phone",
                "microwave",
                "oven",
                "toaster",
                "sink",
                "refrigerator",
                "book",
                "clock",
                "vase",
                "scissors",
                "teddy bear",
                "hair drier",
                "toothbrush"
        };

        private YOLOObjectDetector()
        {

        }

        /// <summary>
        /// Detects objects in a StorageFile input 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<List<DetectionResult>> DetectObjects(StorageFile file)
        {
            SoftwareBitmap bitmap = await GenerateSoftwareBitmapFromStorageFile(file);
            VideoFrame videoFrame = await GenerateVideoFrameFromBitmap(bitmap);
            return await DetectObjects(videoFrame);
        }


        /// <summary>
        /// Detects Objects in a SoftwareBitmap input
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static async Task<List<DetectionResult>> DetectObjects(SoftwareBitmap bitmap)
        {
            VideoFrame videoFrame = await GenerateVideoFrameFromBitmap(bitmap);
            return await DetectObjects(videoFrame);
        }

        /// <summary>
        /// Detects objects in a VideoFrame input
        /// </summary>
        /// <param name="videoFrame"></param>
        /// <returns></returns>
        public static async Task<List<DetectionResult>> DetectObjects(VideoFrame videoFrame)
        {
            if (instance == null)
            {
                instance = new YOLOObjectDetector();
            }

            return await instance.EvaluateFrame(videoFrame);
        }

        /// <summary>
        /// Load YOLOv4 model, creates LearningModelSession and LearningModelBinding instances
        /// </summary>
        /// <returns></returns>
        private async Task InitModelAsync()
        {
            if(_model != null)
            {
                return;
            }
            try
            {
                var model_file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelligentAPI_ObjectDetector/Assets//Yolo.onnx"));
                _model = await LearningModel.LoadFromStorageFileAsync(model_file);
                var device = new LearningModelDevice(LearningModelDeviceKind.Default);
                _session = new LearningModelSession(_model, device);
                _binding = new LearningModelBinding(_session);
            }
            catch (Exception ex)
            {
                _model = null;
                throw ex;
            }
        }

        /// <summary>
        /// Evaluate YOLOv4 model to detect objects in a single VideoFrame
        /// </summary>
        /// <param name="inputImage"></param>
        /// <returns></returns>
        public async Task<List<DetectionResult>> EvaluateFrame(VideoFrame inputImage)
        {
            await InitModelAsync();
            SoftwareBitmap bitmap = inputImage.SoftwareBitmap;
            inputImage = await ResizeImage(inputImage, bitmap);
            _binding.Clear();
            _binding.Bind("input_1:0", inputImage);
            var results = await _session.EvaluateAsync(_binding, "");

            TensorFloat result = results.Outputs["Identity:0"] as TensorFloat;
            var data = result.GetAsVectorView();

            List<DetectionResult> detections = ParseResult(data.ToList<float>().ToArray());
            Comparer cp = new Comparer();
            detections.Sort(cp);
            List<DetectionResult> final_detections = NMS(detections);

            return final_detections;
        }

        /// <summary>
        /// Helper function to resize image
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static async Task<VideoFrame> ResizeImage(VideoFrame inputImage, SoftwareBitmap bitmap)
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);

                encoder.SetSoftwareBitmap(bitmap);

                encoder.BitmapTransform.ScaledWidth = 416;
                encoder.BitmapTransform.ScaledHeight = 416;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.NearestNeighbor;

                await encoder.FlushAsync();

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                SoftwareBitmap newBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, bitmap.BitmapAlphaMode);
                inputImage = VideoFrame.CreateWithSoftwareBitmap(newBitmap);
            }

            return inputImage;
        }

        /// <summary>
        /// Helper function to generate VideoFrame from SoftwareBitmap
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
        /// Helper function to generate SoftwareBitmap from StorageFile
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
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            return softwareBitmap;
        }

        /// <summary>
        /// Compare function 
        /// </summary>
        class Comparer : IComparer<DetectionResult>
        {
            public int Compare(DetectionResult x, DetectionResult y)
            {
                return y.prob.CompareTo(x.prob);
            }
        }
       
        /// <summary>
        /// Compute intersection over union
        /// </summary>
        /// <param name="DRa"></param>
        /// <param name="DRb"></param>
        /// <returns></returns>
        private float ComputeIOU(DetectionResult DRa, DetectionResult DRb)
        {
            float ay1 = DRa.bbox[0];
            float ax1 = DRa.bbox[1];
            float ay2 = DRa.bbox[2];
            float ax2 = DRa.bbox[3];
            float by1 = DRb.bbox[0];
            float bx1 = DRb.bbox[1];
            float by2 = DRb.bbox[2];
            float bx2 = DRb.bbox[3];

            Debug.Assert(ay1 < ay2);
            Debug.Assert(ax1 < ax2);
            Debug.Assert(by1 < by2);
            Debug.Assert(bx1 < bx2);

            // determine the coordinates of the intersection rectangle
            float x_left = Math.Max(ax1, bx1);
            float y_top = Math.Max(ay1, by1);
            float x_right = Math.Min(ax2, bx2);
            float y_bottom = Math.Min(ay2, by2);

            if (x_right < x_left || y_bottom < y_top)
                return 0;
            float intersection_area = (x_right - x_left) * (y_bottom - y_top);
            float bb1_area = (ax2 - ax1) * (ay2 - ay1);
            float bb2_area = (bx2 - bx1) * (by2 - by1);
            float iou = intersection_area / (bb1_area + bb2_area - intersection_area);

            Debug.Assert(iou >= 0 && iou <= 1);
            return iou;
        }

        // Non-maximum Suppression(NMS), a technique which filters the proposals 
        // based on Intersection over Union(IOU)
        private List<DetectionResult> NMS(IReadOnlyList<DetectionResult> detections,
            float IOU_threshold = 0.45f,
            float score_threshold = 0.3f)
        {
            List<DetectionResult> final_detections = new List<DetectionResult>();
            for (int i = 0; i < detections.Count; i++)
            {
                int j = 0;
                for (j = 0; j < final_detections.Count; j++)
                {
                    if (ComputeIOU(final_detections[j], detections[i]) > IOU_threshold)
                    {
                        break;
                    }
                }
                if (j == final_detections.Count)
                {
                    final_detections.Add(detections[i]);
                }
            }
            return final_detections;
        }

        /// <summary>
        /// Parse the result from WinML evaluation results to self defined object struct
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private List<DetectionResult> ParseResult(float[] results)
        {
            int c_values = 84;
            int c_boxes = results.Length / c_values;
            float confidence_threshold = 0.5f;
            List<DetectionResult> detections = new List<DetectionResult>();
            for (int i_box = 0; i_box < c_boxes; i_box++)
            {
                float max_prob = 0.0f;
                int label_index = -1;
                for (int j_confidence = 4; j_confidence < c_values; j_confidence++)
                {
                    int index = i_box * c_values + j_confidence;
                    if (results[index] > max_prob)
                    {
                        max_prob = results[index];
                        label_index = j_confidence - 4;
                    }
                }
                if (max_prob > confidence_threshold)
                {
                    List<float> bbox = new List<float>();
                    bbox.Add(results[i_box * c_values + 0]);
                    bbox.Add(results[i_box * c_values + 1]);
                    bbox.Add(results[i_box * c_values + 2]);
                    bbox.Add(results[i_box * c_values + 3]);

                    detections.Add(new DetectionResult()
                    {
                        label = _labels[label_index],
                        bbox = bbox,
                        prob = max_prob
                    });
                }
            }
            return detections;
        }

    }

    /// <summary>
    /// Detection result struct that contains the label, probability and bounding box co-ordinates of a detected object.
    /// </summary>
    public class DetectionResult
    {
        public string label;
        public List<float> bbox;
        public double prob;
    }
}
