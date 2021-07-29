<h1 align="center">
  Intelligent APIs
</h1>

## What are Intelligent APIs?

Intelligent APIs aim to make machine learning tasks easier for developers to leverage in their applications without needing ML expertise or creating a new model.
By just importing a nuget package and calling a function, we want developers to be able to build intelligent app experiences without needing to deal with the complexities of inferencing machine learning models on Windows.

## Pre-requisites

[Visual Studio 2017 Version 15.7.4 or Newer](https://developer.microsoft.com/en-us/windows/downloads/)

[Windows 10 - Build 17763 (RS5) or Newer](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewiso)

[Windows SDK - Build 17763 (RS5) or Newer](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewSDK)

## How to get started

We have 2 nuget packages ready for you to test and play around with. These nuget packages enable you to perform classic machine learning tasks like image classification and object detection in 1-2 lines of code. They are available under the `packages` section of this repository.

1. Image classification: To perform image classification, import the `CommunityToolkit.Labs.Intelligent.ImageClassification` nuget package. To classify an image, you will need to pass a StorageFile object which is the image file itself, and the number of top results that you want. In the following example, we pass an image of a Rottweiler and we want the top 3 results.

```C#
   using IntelligentAPI.ImageClassification;  
   ...
   List<SqueezeNetResult> imageClasses = await SqueezeNetImageClassifier.ClassifyImage(selectedStorageFile, 3);
```
<div  align="center">
<img src="https://user-images.githubusercontent.com/22471775/125314778-5a977780-e2eb-11eb-983f-0dde00b34e18.png" alt="drawing" width="400"/>
</div>


This nuget package performs [SqueezeNet](https://github.com/onnx/models/tree/master/vision/classification/squeezenet) model inferencing using [WinML](https://github.com/microsoft/Windows-Machine-Learning). SqueezeNet can detect [1000 different classes](https://github.com/onnx/models/blob/master/vision/classification/synset.txt).

1. Object Detection: To perform object detection on your images/video, import the `CommunityToolkit.Labs.Intelligent.ObjectDetection` nuget package. To detect objects in the image, you can either pass an image file as a StorageFile object, or pass a VideoFrame.

```C#
   using IntelligentAPI.ObjectDetection;
   ...
   List<DetectionResult> listOfObjects = await YOLOObjectDetector.DetectObjects(selectedStorageFile);    
```

This nuget package performs object detection using [YOLOv4](https://github.com/hunglc007/tensorflow-yolov4-tflite) model inference on WinML and also return the co-ordinates of the bounding boxes around the detected objects. YOLOv4 can detect objects of [80 different classes](https://github.com/hunglc007/tensorflow-yolov4-tflite/blob/9f16748aa3f45ff240608da4bd9b1216a29127f5/android/app/src/main/assets/coco.txt).
<div  align="center">
<img src="![image](https://user-images.githubusercontent.com/22471775/127563873-886ca161-607b-4306-8651-afb6cf84245e.png)" alt="drawing" width="300"/>
</div>


## Steps to clone the repo

1. `git clone` the repo 
2.  Open the .sln file in VS 2017 or newer
3.  Set `IntelligentLabsTest` as the startup project.
4.  Build the project and run!

