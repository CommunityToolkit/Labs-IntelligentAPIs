$ProgressPreference = 'SilentlyContinue'

$squeezenetfile = "./Assets/model.onnx"
if (-not(Test-Path -Path $squeezenetfile -PathType Leaf)) {
     try {
         Invoke-WebRequest -URI "https://github.com/microsoft/Windows-Machine-Learning/raw/master/Samples/SqueezeNetObjectDetection/UWP/cs/Assets/model.onnx" -OutFile $squeezenetfile
         Write-Host "The file [$squeezenetfile] has been created."
     }
     catch {
         throw $_.Exception.Message
     }
 }
else {
    Write-Host "The file [$squeezenetfile] exists."
}