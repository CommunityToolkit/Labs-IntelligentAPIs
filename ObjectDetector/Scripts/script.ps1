$ProgressPreference = 'SilentlyContinue'

$yolofile = "./Assets/Yolo.onnx"


if (-not(Test-Path -Path $yolofile -PathType Leaf)) {
     try {
         Invoke-WebRequest -URI "https://github.com/microsoft/Windows-Machine-Learning/raw/master/Samples/Tutorial%20Samples/YOLOv4ObjectDetection/YOLOv4ObjectDetection/Assets/Yolo.onnx" -OutFile $yolofile 
         Write-Host "The file [$yolofile] has been created."
     }
     catch {
         throw $_.Exception.Message
     }
 }
 else {
    Write-Host "The file [$yolofile] exists."
}



