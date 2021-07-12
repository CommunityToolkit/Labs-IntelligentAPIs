$ProgressPreference = 'SilentlyContinue'

$yolofile = "./Assets/Yolo.onnx"


if (-not(Test-Path -Path $yolofile -PathType Leaf)) {
     try {
         Invoke-WebRequest -URI "https://github.com/microsoft/Windows-Machine-Learning/blob/eb8b8be97d525f905f4f7559a0266699de7d40ea/Samples/Tutorial%20Samples/YOLOv4ObjectDetection/YOLOv4ObjectDetection/Assets/Yolo.onnx" -OutFile $yolofile 
         Write-Host "The file [$yolofile] has been created."
     }
     catch {
         throw $_.Exception.Message
     }
 }
 else {
    Write-Host "The file [$yolofile] exists."
}



