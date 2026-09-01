from fastapi import FastAPI, File, UploadFile
from ultralytics import YOLO
import os
import numpy as np
import joblib
import cv2
app = FastAPI(
    title="API de IA",
    description="API para predição com YOLO",
    version="1.0.0"
)
#Modelo de analise de imagem
trainVersion = "train";
servicesPath = os.path.join(os.getcwd(), "services")
modelComputerVison = None

#Modelo de predição (já treinado no google colab)
path_modelo = f'{os.getcwd()}/app/services/Predict/model/model.pkl'
modelPredicao = joblib.load(path_modelo)


def carregar_modelo():
    global modelComputerVison
    if modelComputerVison is None:
        modelComputerVison = YOLO(f"{servicesPath}/computerVision/runs/detect/{trainVersion}/weights/best.pt")
    return modelComputerVison

def predict_image(img):
    model = carregar_modelo()
    imgPredict = model.predict(img, conf=0.25, save=False,show=True, save_txt=False)
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


def test():
    # Retorna uma lista/array com os nomes das colunas de entrada
    print(modelPredicao.feature_names_in_)

# Retorna a quantidade de colunas que o modelo espera receber
    print(modelPredicao.n_features_in_)

test()
#Para rodar: uvicorn main:app --reload