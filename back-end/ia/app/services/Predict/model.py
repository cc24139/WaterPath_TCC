import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, r2_score
from sklearn.ensemble import RandomForestRegressor, StackingRegressor
from sklearn.linear_model import RidgeCV
from xgboost import XGBRegressor
from catboost import CatBoostRegressor
from sklearn.multioutput import MultiOutputRegressor
import joblib
import os

print(os.getcwd())
dataTrain  = pd.read_csv(os.getcwd()+"/app/services/Predict/data/china_water_pollution_data.csv",low_memory=True)

features = [
    "Water_Temperature_C",
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
    "BOD_mg_L"
]

result = [
    "Heavy_Metals_Pb_ug_L",
    "Heavy_Metals_Cd_ug_L",
    "Heavy_Metals_Hg_ug_L"
]

class TESTModel:
    def __init__(self, data = dataTrain):
        self.data = pd.DataFrame(data)
        self.model = None
    
    def separeteData(self):
        df = self.data.copy()
        df.columns = df.columns.str.strip()  # Remove leading/trailing spaces from column names    
        df = df[features + result].copy().dropna()
        for columns in features + result:
            df[columns] = pd.to_numeric(df[columns], errors='coerce')
        self.data = df.dropna()
        X = self.data[features]
        y = self.data[result]
        return train_test_split(X, y, test_size=0.2, random_state=42)

    def train(self):
        rf = RandomForestRegressor(n_estimators=100, random_state=42)
        xgb = XGBRegressor(n_estimators=100, random_state=42)
        catboost = CatBoostRegressor(iterations=100, random_state=42, verbose=0)
        
        
        estimators = [
            ('rf', rf),
            ('xgb', xgb),
            ('catboost', catboost)
        ]
        
        stacking_model = StackingRegressor(estimators=estimators, final_estimator=RidgeCV(),cv =5)
        multi_output_model = MultiOutputRegressor(stacking_model)
        multi_output_model.fit(self.X_train, self.y_train)
        return multi_output_model

    def evaluate(self, model):
        y_pred = model.predict(self.X_test)
        mse = mean_squared_error(self.y_test, y_pred)
        r2 = r2_score(self.y_test, y_pred)
        return mse, r2
    
    def printModelResult(self, mse, r2):
        print(f"Mean Squared Error: {mse}")
        print(f"R-squared: {r2}")
        
    def verbose(self):
        self.X_train, self.X_test, self.y_train, self.y_test = self.separeteData()
        model = self.train()
        mse, r2 = self.evaluate(model)
        self.printModelResult(mse, r2)
        joblib.dump(model, f'{os.getcwd()}/app/services/Predict/model/model.pkl')


def main():
    model = TESTModel()
    model.verbose()

def test():
    model = TESTModel()
    
    # Carrega o modelo salvo
    caminho_modelo = f'{os.getcwd()}/app/services/Predict/model/model.pkl'
    modelcarregado = joblib.load(caminho_modelo)
    
    # Separa os dados usando a mesma semente (random_state=42) para garantir o mesmo X_Test
    X_Train, X_Test, y_Train, y_Test = model.separeteData()
    
    # Faz as predições
    y_pred = modelcarregado.predict(X_Test)
    
    # 1. CRIAR TABELA DE COMPARAÇÃO (Real vs Predito)
    df_comparacao = pd.DataFrame({
        'Pb_Real': y_Test['Heavy_Metals_Pb_ug_L'].values,
        'Pb_Predito': y_pred[:, 0],
        'Cd_Real': y_Test['Heavy_Metals_Cd_ug_L'].values,
        'Cd_Predito': y_pred[:, 1],
        'Hg_Real': y_Test['Heavy_Metals_Hg_ug_L'].values,
        'Hg_Predito': y_pred[:, 2]
    })
    
    print("\n--- Tabela de Valores (Real vs Predito) - Primeiras 15 amostras ---")
    print(df_comparacao.head(15).to_string())
    
    # Opcional: Salvar a tabela em CSV para usar no Word/Excel do seu TCC
    # df_comparacao.to_csv('comparacao_resultados.csv', index=False)

    # 2. GERAR GRÁFICOS DE DISPERSÃO PARA OS 3 METAIS
    fig, axes = plt.subplots(1, 3, figsize=(18, 5))
    
    metais = [
        ('Heavy_Metals_Pb_ug_L', 0, 'Chumbo (Pb)'),
        ('Heavy_Metals_Cd_ug_L', 1, 'Cádmio (Cd)'),
        ('Heavy_Metals_Hg_ug_L', 2, 'Mercúrio (Hg)')
    ]
    
    for ax, (coluna, idx, titulo) in zip(axes, metais):
        valores_reais = y_Test[coluna].values
        valores_preditos = y_pred[:, idx]
        
        # Plota os pontos
        ax.scatter(valores_reais, valores_preditos, alpha=0.5, color='#1f77b4', edgecolor='k')
        
        # Plota a linha ideal de predição (onde Real == Predito)
        limite_min = min(valores_reais.min(), valores_preditos.min())
        limite_max = max(valores_reais.max(), valores_preditos.max())
        ax.plot([limite_min, limite_max], [limite_min, limite_max], 'r--', lw=2, label='Predição Ideal')
        
        # Formatação do gráfico
        ax.set_title(f'Predito vs Real: {titulo}')
        ax.set_xlabel('Valor Real (ug/L)')
        ax.set_ylabel('Valor Predito (ug/L)')
        ax.legend()
        ax.grid(True, linestyle='--', alpha=0.7)
    
    # Ajusta o espaçamento para não sobrepor títulos e exibe
    plt.tight_layout()
    plt.show()
        
if __name__ == "__main__":
    test()