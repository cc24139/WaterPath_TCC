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

Para popular rios sintéticos de forma idempotente e testar a API com uma coleção do Insomnia, consulte [população e testes de rios](back-end/api/testing/README.md). Os [resultados de verificação](back-end/api/testing/VERIFICACAO.md) distinguem testes locais das etapas pendentes no PostgreSQL configurado.

### Pré-requisitos

### Instalação

### Execução

## 📊 Resultados Obtidos

O ciclo de treino foi separado do serving em [`back-end/ia/training`](back-end/ia/training/README.md). A preparação e o treinamento da regressão v2 ficam no [MultiOutputRegressor_v2.ipynb](back-end/ia/training/notebooks/regression/MultiOutputRegressor_v2.ipynb), enquanto a API recebe somente os artefatos promovidos em `back-end/ia/app/models`. O joblib usa MultiOutputRegressor com stacking de Random Forest, XGBoost e CatBoost, oito metais e sete entradas, incluindo oxigênio dissolvido. No mesmo teste da v2, o erro normalizado caiu 5.58% frente ao Random Forest anterior. Veja também o [relatório da versão](back-end/ia/training/experiment_outputs/regression/metals_v2/README.md). Os pesos YOLO foram preservados.

A API principal envia imagens e medições à IA, classifica o risco de 1 a 3 pela regra de triagem aprovada e salva imagens e resultados associados à coleta. O histórico das cinco coletas anteriores do mesmo corpo hídrico pode aumentar o nível em um quando há pelo menos três ocorrências de alerta. Os pesos existentes são usados sem retreinamento automático. Consulte [o contrato da integração](back-end/contracts/ia/README.md) e [o registro da implementação](frontend/src/features/add-analysis/IMPLEMENTACAO.md).

## 📸 Demonstração / Screenshots

## 🚀 Funcionalidades

## 👨‍🏫 Orientador(a)

- Simone

## 👥 Integrantes do Grupo

- Luis Filipe Lima
- Daniel
