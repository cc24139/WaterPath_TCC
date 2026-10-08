
class Features:
    def __init__(self, amostra):
        self.features = self._getFeatures(amostra);
        
    def _getFeatures(self, amostra):
        features = []
        for value in amostra.decode()[0]:
            if value is None:
                features.append(float('nan'))
            else:
                features.append(value)
        return features