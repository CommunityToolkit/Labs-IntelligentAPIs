$ProgressPreference = 'SilentlyContinue'

Invoke-WebRequest -URI "https://github.com/onnx/models/raw/master/vision/classification/squeezenet/model/squeezenet1.1-7.onnx" -OutFile "../ProjectBangalore/Assets/model.onnx"
Invoke-WebRequest -URI "https://github.com/onnx/models/raw/master/vision/object_detection_segmentation/yolov4/model/yolov4.onnx" -OutFile "../ProjectYOLO/Assets/Yolo.onnx"