from fastapi import FastAPI, File, UploadFile
from ultralytics import YOLO
import os
import numpy as np
import cv2
app = FastAPI(
    title="API de IA",
    description="API para predição com YOLO",
    version="1.0.0"
)

trainVersion = "train";
servicesPath = os.path.join(os.getcwd(), "services")
modelComputerVison = YOLO(f"{servicesPath}/computerVision/runs/detect/{trainVersion}/weights/best.pt")



def predict_image(img):
    imgPredict = modelComputerVison.predict(img, conf=0.25, save=False,show=True, save_txt=False)
    return imgPredict

@app.post("/vision/predict")
async def predict(file: UploadFile = File(...)):
    conteudo = await file.read()

    nparr = np.frombuffer(conteudo, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    imgPredict = predict_image(img)
    return {
        "msg": "Predição realizada com sucesso",
        "predictions": str(imgPredict)
    }
@app.get("/")
async def root():
    return {"msg": "API funcionando"}


#Para rodar: uvicorn main:app --reload