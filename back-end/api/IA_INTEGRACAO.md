# Integração da API principal com a IA

A API principal usa ASP.NET Core 10, Entity Framework Core 9 e PostgreSQL. A IA usa FastAPI, ONNX Runtime para o YOLO de detecção e um modelo joblib de regressão de oito metais. O frontend existente usa Next.js/React e não foi alterado.

## Fluxo

1. O cliente envia uma imagem e as medições da amostra para `POST /api/ia/predicoes`.
2. A API principal consulta o histórico do mesmo corpo hídrico e envia imagem, medições e histórico para `POST /analyze` da IA.
3. O YOLO existente detecta objetos uma única vez. O modelo de metais existente estima as concentrações usando as medições.
4. `services/risk/classification.py` combina as detecções reais, pH, oxigênio dissolvido e recorrência para calcular o nível de risco pela regra aprovada.
5. A API principal salva a imagem original, a anotada, a entrada e o resultado completo no mesmo registro de `waterPath.PredicoesIA`, vinculado à coleta. A gravação ocorre em um único `SaveChanges`, após uma resposta válida da IA.

Na IA, as concentrações são objetos de `Metal` e suas oito subclasses em `services/Infos/metais/*.py`. Cada detecção é um objeto de `Impact`, `Lixo`, `Urbano` ou `Drenagem`, com caixa, confiança, nome e identificador. As subclasses guardam as associações a metais e a indicação de sinal visual. `Turbidez` foi recuperada do histórico, onde era apenas um exemplo sem critérios; continua neutra.

`interpretation` é uma função de `services/integration.py`, não um arquivo separado. Ela chama `impact()` do objeto visual, que consulta os valores, unidades e limites dos objetos de metais. O nível de 1 a 3 continua em `services/risk/classification.py`, consultando `indica_risco()` dos mesmos objetos visuais e as medições e o histórico. Os arquivos-fonte também são incluídos na imagem Docker; arquivos `.pyc` não substituem essas definições.

Os modelos e seus pesos não foram alterados nem retreinados. A classificação de risco é uma regra de triagem aplicada à inferência real do YOLO e às medições; o YOLO atual não é um classificador treinado diretamente com rótulos de risco 1–3.

## Regra de risco aprovada

Sinal visual: presença de `Lixo`, `drainage connection` ou `urbano` detectada pelo YOLO, mantendo seu limiar atual de confiança acima de 0,25 e a supressão de caixas existente.

Sinal nas medições: pH menor que 6 ou maior que 9, ou oxigênio dissolvido menor que 5 mg/L. pH igual a 6/9 e OD igual a 5 não geram esse sinal. Os valores são referências de águas doces Classe 2 na [Resolução CONAMA 357/2005, artigos 14 e 15](https://conama.mma.gov.br/?id=450&option=com_sisconama&task=+arquivo.download). A combinação abaixo é uma regra do projeto, não uma classificação legal nem comprovação de potabilidade.

| Nível base | Significado na triagem |
| --- | --- |
| 1 — baixo | Nenhum sinal visual ou nas medições pelos critérios avaliados |
| 2 — moderado | Sinal visual ou sinal nas medições, em apenas um dos grupos |
| 3 — alto | Sinais nos dois grupos |

O histórico considera as **cinco coletas de data/hora estritamente anterior à coleta atual**, do mesmo corpo hídrico. Usa a análise integrada mais recente por coleta, desempata pela data de criação/ID da predição e conta cada coleta uma vez. Coletas sem resultado compatível com `waterpath-risk-v1` são ignoradas, sem buscar coletas além da janela de cinco. A coleta atual, coletas posteriores, outros corpos hídricos e análises somente visuais ficam fora da contagem.

Se três ou mais dessas coletas tiveram nível **base** 2 ou 3, soma-se um ao nível atual, limitado a 3. Caso contrário, mantém-se o nível base. Usar o nível base evita realimentar o peso do histórico. O resultado guarda `riskLevel`, `baseRiskLevel`, `riskLabel`, `riskReasons`, `riskRuleVersion` e `history`, incluindo a contagem, o ajuste efetivo e os IDs/níveis considerados. Os metais estimados permanecem complementares e não alteram o nível.

Esse uso dos dados é **consulta ao histórico durante a classificação**. Salvar uma predição não faz nenhum dos modelos aprender. Não foi implementado treinamento/retreinamento automático. As predições estimadas ficam separadas das medições reais e não devem ser tratadas como rótulos confirmados para treinamento.

## Banco e configuração

O armazenamento existente foi preservado: JSONs em `jsonb`, imagens em `bytea`, chave estrangeira para a coleta. Excluir uma coleta exclui suas predições. O contexto histórico fica copiado no resultado; seus IDs referenciam as predições usadas, que podem ser excluídas com as respectivas coletas.

Não foi necessário criar uma nova migração: os campos de risco entram no `ResultadoJson` existente. A migração `PersistirPredicoesIA` precisa estar aplicada no banco desejado. Ela **não foi executada no PostgreSQL configurado nesta alteração**.

A conexão e a chave JWT continuam nas variáveis existentes `ConnectionString` e `JWT_KEY`, carregadas pelo `.env` local ou pelo ambiente. Foi removida uma conexão com credenciais fixas do `appsettings.json`, que não era usada pelo registro atual do contexto. Não coloque credenciais no código ou neste documento.

Para testar localmente, configure na API principal:

```dotenv
IA__BaseUrl=http://localhost:8000
IA__TimeoutSeconds=120
```

A configuração existente em `appsettings.json` ainda aponta para o serviço hospedado. Para usar a nova integração nele, publique a versão atual da IA, que inclui `/analyze`, antes da API principal. Não há fallback para duas inferências ou resultados simulados se essa rota estiver ausente.

## Executar

Na raiz do projeto, com Python 3.13 e .NET 10 instalados, prepare o ambiente Python. O ambiente já disponível `.venv-training` também foi usado para a verificação desta alteração.

```powershell
python -m venv .venv-ia
& .venv-ia/Scripts/python.exe -m pip install -r back-end/ia/app/requirements.txt
```

Inicie a IA, mantendo este terminal aberto:

```powershell
Set-Location back-end/ia/app
& ../../../.venv-ia/Scripts/python.exe -m uvicorn main:app --host 127.0.0.1 --port 8000
```

Os caminhos padrão usam `services/ComputerVision/best.onnx` e `services/Predict/model/metals_v2/quality_model.joblib`, junto de `manifest.json`. `YOLO_MODEL_PATH` e `QUALITY_MODEL_PATH` permitem indicar outros caminhos locais. Os modelos são carregados uma vez por processo; reinicie a IA após trocar arquivos ou configuração. A validação de hash e do contrato do joblib foi preservada.

Em outro terminal, partindo da raiz:

```powershell
Set-Location back-end/api
$env:IA__BaseUrl = 'http://localhost:8000'
dotnet ef database update
dotnet run
```

`dotnet ef` requer a ferramenta EF instalada e a conexão PostgreSQL existente configurada. O endereço da API é mostrado no terminal; use-o no exemplo abaixo.

O Docker da IA agora copia o ONNX existente e os arquivos atuais de `metals_v2`, sem exportar os mesmos pesos novamente durante a construção. A imagem Docker não foi construída neste ambiente, que não dispõe de Docker.

## Enviar e consultar

`POST /api/ia/predicoes`, com `multipart/form-data`:

| Campo | Conteúdo |
| --- | --- |
| `coletaId` | ID de uma coleta existente, que identifica o corpo hídrico e a data do histórico |
| `image` | JPEG ou PNG válido, até 10 MB |
| `data` | Objeto JSON com as medições da mesma coleta, como texto |

As quatro medições obrigatórias são temperatura (°C), pH, condutividade elétrica (µS/cm) e oxigênio dissolvido (mg/L). Os campos opcionais são publicados pela IA em `/predict/infos`. A API não substitui automaticamente a amostra pelas medições salvas da coleta; envie valores correspondentes à imagem/coleta escolhida. Sem medições válidas, não há classificação combinada de risco.

Crie um arquivo `amostra.json` com:

```json
{
  "temperatura": 22,
  "ph": 7,
  "condutividade_eletrica": 100,
  "oxigenio_dissolvido": 6
}
```

Ajuste o endereço, o ID e os caminhos no exemplo:

```powershell
curl.exe -X POST 'http://localhost:5000/api/ia/predicoes' -F 'coletaId=1' -F 'image=@rio.jpg' -F 'data=<amostra.json'
```

A resposta `201` contém o ID, a coleta, o resultado com nível de risco e os links das duas imagens. O histórico é montado pela API principal, não enviado pelo cliente.

| Método | Rota | Retorno |
| --- | --- | --- |
| GET | `/api/ia/predicoes?coletaId=1&pagina=1&tamanhoPagina=20` | Histórico paginado; filtro opcional, máximo de 100 registros por página |
| GET | `/api/ia/predicoes/{id}` | Entrada, resultado com risco/contexto histórico e links das imagens |
| GET | `/api/ia/predicoes/{id}/imagem` | Imagem anotada salva no banco |
| GET | `/api/ia/predicoes/{id}/imagem/original` | Imagem original salva no banco |

`POST /api/vision`, com `file` e `coletaId`, mantém sua resposta JPEG e os cabeçalhos `X-Predicao-Id` e `Location`. Agora também salva o JSON das detecções. Essa rota não recebe medições e não atribui risco combinado. As rotas existentes da IA `/predict`, `/vision/predict`, `/predict/infos`, `/vision/infos` e a rota de teste opcional foram preservadas.

Erros: `400` para arquivo inválido ou JSON malformado; `404` para coleta/predição inexistente; `413` para arquivo grande; `422` para medições/histórico inválidos; `502` para IA indisponível ou resposta inválida; `504` para timeout. Falhas da IA não criam uma predição parcial. Os detalhes internos das falhas ficam no log da IA.

## Verificação automatizada

Na raiz, instale as dependências de teste Python e execute:

```powershell
& .venv-ia/Scripts/python.exe -m pip install -r back-end/ia/app/requirements-test.txt
Push-Location back-end/ia/app
& ../../../.venv-ia/Scripts/python.exe -B -m unittest discover -s tests -v
Pop-Location
dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj --artifacts-path "$env:TEMP/waterpath-tests"
```

Os testes Python usam os pesos e uma imagem reais do repositório; verificam detecção, preservação das coordenadas/confiança e das oito concentrações anteriores à reorganização, risco, limiares, histórico, imagem anotada, EXIF, arquivo inválido, limite de tamanho e falha do modelo. Os testes C# de erros isolam a IA e usam SQLite em memória para verificar persistência, associação, leitura das imagens, seleção de histórico, comunicação/timeout e ausência de registros parciais. Esses cenários de erro não substituem a classificação real da aplicação.

Para ativar o teste C# que faz chamadas à IA real, mantenha a IA local aberta e execute da raiz:

```powershell
$env:WATERPATH_IA_TEST_URL = 'http://127.0.0.1:8000/'
$env:WATERPATH_TEST_IMAGE = (Resolve-Path 'back-end/ia/app/services/ComputerVision/dataset/train/images/000058_jpg.rf.zumct7JlQ1EznlPftdka.jpg').Path
dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj --artifacts-path "$env:TEMP/waterpath-tests"
```

Esse teste cria coletas em um banco SQLite descartável, faz três análises reais anteriores e uma atual por HTTP entre as APIs, verifica o peso do histórico, lê as imagens salvas e rejeita um JPEG corrompido sem aumentar a quantidade de predições. O servidor HTTP C# de teste usa os mesmos controllers e serviço de persistência da API principal. Não acessa nem altera o PostgreSQL configurado.

Resultado desta alteração: **17 testes Python e 14 testes C# passaram**, incluindo o teste com IA real via HTTP. A compilação da API passou; permanecem avisos de nulabilidade preexistentes em outras áreas. PostgreSQL, construção Docker e atualização do serviço hospedado não foram verificados/executados. A integração está verificada localmente; o uso no ambiente hospedado depende dessas etapas de configuração e publicação.
