# 📌 WaterPath

## 📖 Descrição
WaterPath é o Trabalho de Conclusão de Curso(TCC) focado em criar um sistema capaz de analísar a  qualidade de rios e lagos relacionando com as imagens do corpo hídrico para realizar uma predição de sua qualidade ao longo do tempo

## 🎯 Objetivo
O Projeto propôe-se cria

## 🧠 Problema que o Projeto Resolve

## 💡 Solução Proposta

## 🏗️ Arquitetura do Projeto

### Visão Geral

### Front-end

### Back-end / API

### Inteligência Artificial

## 🛠️ Tecnologias Utilizadas

## 📂 Estrutura do Repositório

## ⚙️ Como Executar o Projeto

### Pré-requisitos

### Instalação

### Execução

## 📊 Resultados Obtidos

Foi reorganizada a predição v2 em uma pasta própria, com preparação e treinamento inteiramente no [MultiOutputRegressor_v2.ipynb](back-end/ia/app/services/Predict/model/metals_v2_20261004/MultiOutputRegressor_v2.ipynb). O novo joblib usa MultiOutputRegressor com stacking de Random Forest, XGBoost e CatBoost, oito metais e sete entradas, incluindo oxigênio dissolvido. No mesmo teste da v2, o erro normalizado caiu 5.58% frente ao Random Forest anterior. Veja o [relatório da versão](back-end/ia/app/services/Predict/model/metals_v2_20261004/README.md) e a [pesquisa de datasets](back-end/ia/app/services/Predict/model/metals_v2_20261004/DATASETS_PESQUISADOS.md). O main.py e os pesos YOLO foram preservados nesta alteração.

Foi preparado um dataset de imagens pareadas com medições de qualidade para futuros testes do fluxo completo da IA. Veja a [documentação do Fishpond](back-end/ia/app/services/MultimodalDataset/README.md), o [notebook de preenchimento dos parâmetros ausentes](back-end/ia/app/services/MultimodalDataset/Preparar_Fishpond.ipynb) e a [pesquisa de parâmetros desejáveis](back-end/ia/app/services/MultimodalDataset/PARAMETROS_DESEJADOS.md). Nesta etapa foram realizadas aquisição e preparação dos dados, sem testes ou inferência.

## 📸 Demonstração / Screenshots

## 🚀 Funcionalidades

## 👨‍🏫 Orientador(a)

- Simone

## 👥 Integrantes do Grupo

- Luis Filipe Lima
- Daniel
