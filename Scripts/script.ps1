Set-Location ./models
git lfs pull --include="vision/classification/squeezenet/model/squeezenet1.1-7.onnx" --exclude=""

Copy-Item "./models/vision/classification/squeezenet/model/squeezenet1.1-7.onnx" "./ProjectBangalore/Assets"

Rename-Item "./ProjectBangalore/Assets/squeezenet1.1-7.onnx" "model.onnx"

