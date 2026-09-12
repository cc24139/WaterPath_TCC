from fastapi import FastAPI, File, Form, UploadFile
from ultralytics import YOLO
import os
import numpy as np
import joblib
import cv2
import pandas as pd
import json
import sklearn as sk
from DTOs.Amostra import Amostra
from services.Infos.objetos.Lixo import Lixo
app = FastAPI(
    title="API de IA",
    description="API para predição com YOLO",
    version="1.0.0"
)
#Modelo de analise de imagem
trainVersion = "train";
servicesPath = os.path.join(os.getcwd(), "services")
modelComputerVison = YOLO(f"{servicesPath}/computerVision/runs/detect/{trainVersion}/weights/best.pt")

#Modelo de predição (já treinado no google colab)
path_modelo = f'{os.getcwd()}/services/Predict/model/best_model.pkl'
modelPredicao = joblib.load(path_modelo)
# Dados testes 
dataTestPATH = f'{os.getcwd()}/services/Predict/data/Data_Lake Onego_V2.xlsx'
features = [
      "T, °C",
      "рН",
      "TSS, mg/L",
      "Color, mg Pt-Co/L",
      "TOC, mg/L",
      "CODMn, mg О/L",
      "CODCr, мгО/л",
      "BOD5, mg О2/L",
    "PO4-P, µg/L",
    "TP, µg/L",
    "NH4-N, mgN/L",
    "NO2-N, mgN/L",
    "NO3-N, mgN/L",
    "TN, mg/L",
    "EC, μS/сm at 25 °C",
    "Depth, m"
]


result = [
    "Fe, mg/L",
    "Mn, mg/L",
    "Cr, µg/L",
    "Ni, µg/L",
    "Cu, µg/L",
    "Zn, µg/L",
    "Cd, µg/L",
    "Pb, µg/L"
]




async def predict_image(img):
    imgPredict = modelComputerVison.predict(img, conf=0.25, save=False,show=True, save_txt=False)
    return imgPredict

async def _instaciateObjetcDeteced(box):
    conf = box.conf[0]
    name = box.cls[0]
    match(name):
        case 0:
            return Lixo(box, conf, "lixo")
    return Lixo(box, conf, "lixo")



async def _relatorioImg(predictedImg,predictedHeavyMetais):
    relatorio = []
    for resultsIMG in predictedImg:
        for box in resultsIMG.boxes:
            x1, y1, x2, y2 = box.xyxy[0]
            obj = await _instaciateObjetcDeteced(box)
            relatorio += [{obj.impact(predictedHeavyMetais)}]
    return relatorio
    


@app.get("/vision/infos")
async def getInfos():
    return {"msg:": "Informações do modelo de visão computacional", 
            "names": modelComputerVison.names, 
            "trainVersion": trainVersion}
    
    

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
    
async def _predict(data: Amostra):
    predicted = modelPredicao.predict(data.decode())
    response = {}
    for metal, value in zip(result, predicted[0]):
        response[metal] = f"{value.round(4)}"
    return response

@app.post("/predict")
async def predictQuality(data:str = Form(...),image:UploadFile = File(...)):
    dataAmostra = Amostra.parse_raw(data)
    predicted = await _predict(dataAmostra)
    conteudo = await image.read()
    nparr = np.frombuffer(conteudo, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    imgPredict = await predict_image(img)
    relatorioImg = await _relatorioImg(imgPredict,predicted)
    return {
        "msg": "Predição realizada com sucesso",
        "predictions": predicted,
        "predictedHeavyMetais": dataAmostra.dict(),
        "relatorioImg": relatorioImg
    }
#rota apena para dar dados para testar a predição
@app.get("/predict/test")
async def predict_test():
    dados = pd.read_excel(dataTestPATH)
    dados = pd.DataFrame(dados)
    dados.columns = dados.columns.str.strip()
    dados = dados.dropna()
    dados[features] = dados[features].map(tratar_censurado)
    dados[result] = dados[result].map(tratar_censurado)
    dados = dados.dropna()
    X = dados.loc[:, features]
    Y = dados.loc[:, result]
    TrainX, TestX, TrainY, TestY = sk.model_selection.train_test_split(
        X,
        Y,
        test_size=0.2,
        random_state=42
    )
    predicoes = modelPredicao.predict(TestX)
    PredY = pd.DataFrame(
        predicoes,
        columns=result,
        index=TestY.index
    ).round(4)
    TestY = TestY.round(4)
    resultado = []
    for i in TestX.index:
        resultado.append({
        "entrada": TestX.loc[i].to_dict(),
        "real": TestY.loc[i].to_dict(),
        "predito": PredY.loc[i].to_dict()
        })
    return resultado


@app.get("/")
async def root():
    return {"msg": "API funcionando"}



def tratar_censurado(valor):
    if isinstance(valor, str) and valor.startswith("<"):
        limite = float(valor[1:])
        return limite / 2

    return float(valor)


def modelInfos():
    print("Informações do modelo de predição:")
    print("Features:", modelPredicao.feature_names_in_)

    
modelInfos()
#Para rodar: uvicorn main:app --reload
#Para rodar mac:  python3 -m uvicorn main:app --reload
#Para rodar linux: python -m uvicorn main:app --reload