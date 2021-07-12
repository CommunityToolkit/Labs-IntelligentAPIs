Set-Location ./models
git lfs pull --include=["vision/classification/squeezenet/model/squeezenet1.1-7.onnx", "vision/object_detection_segmentation/yolov4/model/yolov4.onnx"] --exclude=""

Set-Location ..
Copy-Item "./models/vision/classification/squeezenet/model/squeezenet1.1-7.onnx" "./ProjectBangalore/Assets"
Copy-Item "./models/vision/object_detection_segmentation/yolov4/model/yolov4.onnx" "./ProjectYOLO/Assets"


Rename-Item "./ProjectBangalore/Assets/squeezenet1.1-7.onnx" "model.onnx"

Rename-Item "./ProjectYOLO/Assets/yolov4.onnx" "yolo.onnx"