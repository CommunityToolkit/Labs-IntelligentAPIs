<h1 align="center">
  Intelligent APIs
</h1>

## What are Intelligent APIs?

Intelligent APIs aim to make machine learning tasks easier for developers to leverage in their applications without needing ML expertise or creating a new model.
By just importing a nuget package and calling a function, we want developers to be able to build intelligent app experiences without needing to deal with the complexities of inferencing machine learning models on Windows. 

Each of these APIs employs WinML (Windows Machine Learning) to use the models on Windows. WinML helps abstract a lot of the model-specific code away and performs hardware optimizations to improve performance significantly on Windows. Learn more about WinML [here](https://docs.microsoft.com/en-us/windows/ai/windows-ml/). 

## Pre-requisites

[Visual Studio 2017 Version 15.7.4 or Newer](https://developer.microsoft.com/en-us/windows/downloads/)

[Windows 10 - Build 17763 (RS5) or Newer](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewiso)

[Windows SDK - Build 17763 (RS5) or Newer](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewSDK)

## Getting started with the nuget packages

We have 2 nuget packages ready for you to test and play around with. These nuget packages enable you to perform classic machine learning tasks like image classification and object detection in 1-2 lines of code. 

## Steps to import the nuget package into your project:

1. Add a new nuget source with the feed URL as `https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json`. If you have not done this before, follow the steps listed [here](https://docs.microsoft.com/en-us/azure/devops/artifacts/nuget/consume?view=azure-devops&tabs=windows#2-set-up-visual-studio) to add a new nuget package source. Name the source as "WCT Labs" and set the URL as `https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json`.
2. Open a blank UWP app or an existing app and add one or both packages from the newly created source. Follow step 3 [here](https://docs.microsoft.com/en-us/azure/devops/artifacts/nuget/consume?view=azure-devops&tabs=windows#3-consume-packages) to do so.
3. There should be two nuget packages listed, `CommunityToolkit.Labs.Intelligent.ImageClassification` and `CommunityToolkit.Labs.Intelligent.ObjectDetection`. 

## Using the packages

### Image Classification
To perform image classification, import the `CommunityToolkit.Labs.Intelligent.ImageClassification` nuget package. To classify an image, you will need to pass a StorageFile object which is the image file itself, and the number of top results that you want (optional). In the following example, we pass an image of a Rottweiler and we want the top 3 results.

```C#
   using CommunityToolkit.Labs.Intelligent.ImageClassification;  
   ...
   List<ClassificationResult> imageClasses = await SqueezeNetImageClassifier.ClassifyImage(selectedStorageFile, 3);
```
<div  align="center">
<img src="https://user-images.githubusercontent.com/22471775/125314778-5a977780-e2eb-11eb-983f-0dde00b34e18.png" alt="drawing" width="400"/>
</div>


This nuget package performs [SqueezeNet](https://github.com/onnx/models/tree/master/vision/classification/squeezenet) model inferencing using [WinML](https://github.com/microsoft/Windows-Machine-Learning). SqueezeNet can detect [1000 different classes](https://github.com/onnx/models/blob/master/vision/classification/synset.txt).

### Object Detection 
(Note: This currently only works with Windows 11. We are looking into this [issue](https://github.com/CommunityToolkit/Labs-IntelligentAPIs/issues/3))

To perform object detection on your images/video, import the `CommunityToolkit.Labs.Intelligent.ObjectDetection` nuget package. To detect objects in the image, you can either pass an image file as a StorageFile object, VideoFrame or SoftwareBitmap.

```C#
   using CommunityToolkit.Labs.Intelligent.ObjectDetection;
   ...
   List<DetectionResult> listOfObjects = await YOLOObjectDetector.DetectObjects(selectedStorageFile);    
```

This nuget package performs object detection using [YOLOv4](https://github.com/hunglc007/tensorflow-yolov4-tflite) model inference on WinML and also return the co-ordinates of the bounding boxes around the detected objects. YOLOv4 can detect objects of [80 different classes](https://github.com/hunglc007/tensorflow-yolov4-tflite/blob/9f16748aa3f45ff240608da4bd9b1216a29127f5/android/app/src/main/assets/coco.txt).
<div  align="center">
<img src="https://user-images.githubusercontent.com/22471775/127563873-886ca161-607b-4306-8651-afb6cf84245e.png" alt="drawing" width="300"/>
</div>

## NEW!!! Emotion Recognition
To perform emotion recognition, import the `CommunityToolkit.Labs.Intelligent.EmotionRecognition` nuget package. To detect emotion in an image, you can either pass a StorageFile object or a SoftwareBitmap of the image.

```C#
   using CommunityToolkit.Labs.Intelligent.EmotionRecognition;
   ...
   DetectedEmotion detectedEmotion = await EmotionRecognizer.DetectEmotion(selectedStorageFile);    
```
<div  align="center">
<img width="378" alt="image" src="https://user-images.githubusercontent.com/22471775/140475535-6b38f155-e333-45f3-a23a-9fd7302c2e9b.png">
</div>

## 🧪 This project is under Community Toolkit Labs. What does that mean?

[Community Toolkit Labs](https://aka.ms/toolkit/wiki/labs) is a place for rapidly prototyping ideas and gathering community feedback. It is an incubation space where the developer community can come together to work on new ideas before thinking about final quality gates and ship cycles. Developers can focus on the scenarios and usage of their features before finalizing docs, samples, and tests required to ship a complete idea within the Toolkit.


## Steps to clone the repo

1. `git clone` the repo 
3.  Open the .sln file in VS 2017 or newer
4.  Set `IntelligentLabsTest` as the startup project.
5.  Build the project and run!

Note: If you have the Windows machine learning code generator or [mlgen](https://marketplace.visualstudio.com/items?itemName=WinML.mlgen) extension on VS, please disable it for this project since all the code is already generated for you.

