$ProgressPreference = 'SilentlyContinue'

$squeezenetfile = "./Assets/model.onnx"
if (-not(Test-Path -Path $squeezenetfile -PathType Leaf)) {
     try {
         Invoke-WebRequest -URI "https://github.com/microsoft/Windows-Machine-Learning/blob/593575a3a6681891a8ef10f0d3ef432093bf9dce/Samples/SqueezeNetObjectDetection/UWP/cs/Assets/model.onnx" -OutFile $squeezenetfile
         Write-Host "The file [$squeezenetfile] has been created."
     }
     catch {
         throw $_.Exception.Message
     }
 }
else {
    Write-Host "The file [$squeezenetfile] exists."
}