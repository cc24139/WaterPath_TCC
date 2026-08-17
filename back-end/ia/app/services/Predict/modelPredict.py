import pandas as pd
import os
import numpy as np
from sklearn import model_selection
from sklearn import ensemble
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

class ModelPredict(ensemble.RandomForestRegressor):
    def __init__(self, n_estimators=300, random_state=42):
        super().__init__(n_estimators=n_estimators, random_state=random_state)
        self._rf = ensemble.RandomForestRegressor(n_estimators=n_estimators, random_state=random_state,job=-1)
        self._features = ["pH", "Turbidity (NTU)", "Temperature (°C)", "Dissolved oxygen (DO) (mg/L)"]
        self._result = ["Lead (mg/L)", "Mercury (mg/L)", "Arsenic (mg/L)"]
        self._XTest, self._YTest = None, None
        self._xTrain, self._yTrain = None, None
        
        
    def _trainModel(self):
        dataSet = pd.read_csv(os.getcwd()+"/station/Water_Quality_Dataset.csv",low_memory=True)
        df = pd.DataFrame(dataSet)
        X, y = df[self._features], df[self._result]
        self._xTrain, self._XTest, self._yTrain, self._YTest = model_selection.train_test_split(X, y, test_size=0.3, random_state=42)
        self._rf.fit(self._xTrain, self._yTrain)
    
    def predict(self,verbose = False,features=None):
        if features is not None:
            raise ValueError(f"features '{features}' is not a valid result. Valid results are: {self._features}")
        if self._XTest is None or self._YTest is None:
            self._trainModel()
        predictions = self._rf.predict(target=self._)
        if verbose:
            for target in self._result:
                mae = mean_absolute_error(self._YTest[target], predictions)
                rmse = np.sqrt(mean_squared_error(self._YTest[target], predictions))
                r2 = r2_score(self._YTest[target], predictions)
                print(f"Metrics for {target}:")
                print("MAE:", mae)
                print("RMSE:", rmse)
                print("R²:", r2)
        return predictions
        
    
    
        
    