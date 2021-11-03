$ProgressPreference = 'SilentlyContinue'

$emotionferplusfile = "./Assets/model_emotion.onnx"
if (-not(Test-Path -Path $emotionferplusfile -PathType Leaf)) {
     try {
         Invoke-WebRequest -URI "https://github.com/onnx/models/raw/master/vision/body_analysis/emotion_ferplus/model/emotion-ferplus-8.onnx" -OutFile $emotionferplusfile
         Write-Host "The file [$emotionferplusfile] has been created."
     }
     catch {
         throw $_.Exception.Message
     }
 }
else {
    Write-Host "The file [$emotionferplusfile] exists."
}