import pandas as pd
import os
import numpy as np
from sklearn import model_selection
from sklearn import ensemble
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

constFeatures = ["Water_Temperature_C",
    "pH",
    "Dissolved_Oxygen_mg_L",
    "Conductivity_uS_cm",
    "Turbidity_NTU",
    "Nitrate_mg_L",
    "Nitrite_mg_L",
    "Ammonia_N_mg_L",
    "Total_Phosphorus_mg_L",
    "Total_Nitrogen_mg_L",
    "COD_mg_L",
    "BOD_mg_L"]
constResults = ["Heavy_Metals_Pb_ug_L",
    "Heavy_Metals_Cd_ug_L",
    "Heavy_Metals_Hg_ug_L"]

normalData = "data/china_water_pollution_data.csv"
class ModelPredict(ensemble.RandomForestRegressor):
    
    def __init__(self, data, n_estimators=300, random_state=42,features = constFeatures,results = constResults):
        super().__init__(n_estimators=n_estimators, random_state=random_state)
        self._rf = ensemble.RandomForestRegressor(n_estimators=n_estimators, random_state=random_state,n_jobs=-1)
        self._features = features
        self._result = results
        self._models = {}
        self._data = data if data !=None else pd.read_csv(os.getcwd()+"/"+normalData,low_memory=True)
        
    
    def _prepateData(self):
        formatData = pd.DataFrame(self._data)
        formatData.columns = formatData.columns.str.strip()
        required_cols = list(self._features) + list(self._result)
        formatData = formatData[required_cols].copy().dropna()
        for col in required_cols:
            formatData[col] = pd.to_numeric(formatData[col], errors='coerce')
        return formatData


    def _trainModel(self):
        df = self._prepateData()
        if self._models == {}:
            self._defineModels()
        X, Y = df[self._features], df[self._result]
        XTrain, XTest, YTrain, YTest = model_selection.train_test_split(X, Y, test_size=0.3, random_state=42)
        for metal, model_dict in self._models.items():
            rf = ensemble.RandomForestRegressor(n_estimators=300, random_state=42, n_jobs=-1)
            rf.fit(XTrain, YTrain[metal])
            pred = rf.predict(XTest)
            mae = mean_absolute_error(YTest[metal], pred)
            mse = mean_squared_error(YTest[metal], pred)
            r2 = r2_score(YTest[metal], pred)
            self._models[metal][f"{metal}_model"] = modelMetal(metal, rf, mae, mse, r2)

    def predict(self, X=None):
        if not self._models:
            self._trainModel()

        # se X não for fornecido, prediz sobre o dataset preparado
        if X is None:
            df = self._prepateData()
            X = df[self._features]

        predictions = {}
        for metal, model_dict in self._models.items():
            mdl = model_dict.get(f"{metal}_model")
            predictions[metal] = mdl.rf.predict(X)
        return predictions

    def _defineModels(self):
        for metal in self._result:
            self._models[metal] = {f"{metal}_model": None}

        

class modelMetal :
    def __init__(self, metal, rf, mae, mse, r2):
        self.metal = metal
        self.rf = rf
        self.mae = mae
        self.mse = mse
        self.r2 = r2


def test():
    ia = ModelPredict(None)
    df = ia._prepateData()
    X = df[ia._features]
    Y = df[ia._result]

    XTrain, XTest, YTrain, YTest = model_selection.train_test_split(
        X, Y, test_size=0.3, random_state=42
    )

    ia._defineModels()
    for metal in ia._result:
        rf = ensemble.RandomForestRegressor(n_estimators=300, random_state=42, n_jobs=-1)
        rf.fit(XTrain, YTrain[metal])
        ia._models[metal][f"{metal}_model"] = modelMetal(metal, rf, 0, 0, 0)

    sample_index = 0
    sample_row = XTrain.iloc[[sample_index]]
    sample_actual = YTrain.iloc[[sample_index]]

    print("Linha de treino selecionada:")
    print(sample_row.to_dict(orient="records")[0])
    print("\nComparação real vs previsto:")

    for metal in ia._result:
        real_value = float(sample_actual[metal].iloc[0])
        pred_value = float(ia._models[metal][f"{metal}_model"].rf.predict(sample_row)[0])
        print(f"{metal}: real={real_value:.6f} | previsto={pred_value:.6f} ")


test()