# Integração da API principal com a IA

A API principal usa ASP.NET Core 10, Entity Framework Core 9 e PostgreSQL. A IA usa FastAPI, ONNX Runtime para o YOLO de detecção e um modelo joblib de regressão de oito metais. O frontend existente usa Next.js/React e não foi alterado.

O contrato atual e os detalhes de persistência estão em [CONTRATO_PREDICAO.md](CONTRATO_PREDICAO.md); ele usa a API de IA como referência.

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

Os dados utilizados pela classificação ficam em `ResultadoJson.riskInputs` e `history`, junto da entrada enviada. A migração adicional `ReferenciarDadosClassificacao` copia a entidade e o instante observado para as novas predições. As duas migrações precisam estar aplicadas no banco desejado. **Não foram executadas no PostgreSQL configurado nesta alteração**. Registros legados são preservados sem preenchimento presumido e ficam fora da classificação até uma reanálise verificável.

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

## Consultar risco atual de um corpo hídrico

`GET /api/corpohidrico/{id}/risco-atual` exige um JWT válido, seguindo a consulta por ID de corpo hídrico. O controller envia uma query pelo MediatR, o handler valida o resultado persistido e o repositório consulta o EF Core. A relação usada é `CorpoHidrico -> Coleta (CorpoHidricoId) -> PredicaoIA (ColetaId)`. O IQA de `Qualidade` não é um nível de risco e não é convertido em risco nesta consulta.

Não havia uma definição implementada de “risco atual”. Foi adotado o resultado válido da **coleta mais recente**, ordenando de forma decrescente por `PredicaoIA.DataColeta`, `PredicaoIA.ColetaId`, `PredicaoIA.CriadaEm` e `PredicaoIA.Id`. O instante e a entidade são cópias feitas na análise, sem depender de edições posteriores da coleta. Assim, reprocessar uma coleta antiga não substitui o resultado de uma coleta mais recente; os IDs tornam os empates determinísticos. Coletas sem resultado válido são puladas, sem limitar a busca à janela de cinco usada pela classificação do histórico.

Só entram predições `integrada` com entidade/instante copiados e contrato verificável pelo `IaClient`: amostra válida, `riskInputs` correspondentes aos valores enviados e às detecções, histórico compatível, `riskRuleVersion = waterpath-risk-v1`, níveis inteiros de 1 a 3, rótulo e motivos válidos. JSON inválido, outra versão, ausência de classificação, instantes futuros e análise apenas visual são ignorados. A resposta copia o nível final, o rótulo e os motivos salvos, após conferir sua consistência pela regra existente. A consulta não chama a IA, não gera predição nem altera dados; a projeção não carrega as imagens.

Exemplo de requisição (substitua o ID e o token):

```powershell
curl.exe 'http://localhost:5189/api/corpohidrico/1/risco-atual' -H 'Authorization: Bearer SEU_TOKEN'
```

Exemplo ilustrativo de resposta `200`:

```json
{
  "corpoHidricoId": 1,
  "nivelRisco": 2,
  "rotuloRisco": "moderado",
  "motivos": ["Sinal visual: Lixo."],
  "coletaId": 7,
  "dataColeta": "2026-10-05T12:00:00+00:00",
  "predicaoId": 12,
  "dataPredicao": "2026-10-05T12:05:00Z",
  "versaoRegraRisco": "waterpath-risk-v1"
}
```

| Situação | HTTP | Resposta |
| --- | --- | --- |
| Sem JWT válido | 401 | Desafio de autenticação existente |
| ID zero ou negativo | 400 | `Informe um ID de corpo hídrico válido.` |
| ID não numérico ou fora de `Int32` | 400 | Validação automática do ASP.NET Core (`ProblemDetails`) |
| Corpo hídrico inexistente | 404 | `Corpo hídrico não encontrado` |
| Corpo existente sem resultado válido | 404 | `Corpo hídrico sem resultado de risco válido` |

Os erros textuais seguem as ações existentes de corpo hídrico. Ausência de dados nunca produz nível 1.

### Verificação em 5 de outubro de 2026

As APIs locais estavam paradas e foram iniciadas com o perfil `http` da API principal e o ambiente Python `.venv-training` já existente. Nenhuma URL persistente ou credencial foi alterada.

| Serviço | Evidência HTTP | Resultado |
| --- | --- | --- |
| API principal, `http://localhost:5189` | GET de corpos hídricos e coletas: 200; nova rota: 401 sem JWT, 400 para ID inválido e 404 para ID inexistente | Rodando e acessando PostgreSQL |
| IA local, `http://127.0.0.1:8000` | GET `/` e `/predict/infos`: 200; inferência real pelo cliente C# e consulta posterior de risco | Rodando; integração local passou após correção do contrato |
| IA configurada, `https://watherpathia.onrender.com` | GET `/` e `/openapi.json`: 200; POST `/analyze`: 200 | Acessível, mas uma resposta recebida pelo cliente C# estava incompatível |

Há dois impedimentos no ambiente configurado:

- A tabela `waterPath.PredicoesIA` não existe no PostgreSQL (`42P01`). Tanto GET de risco para um corpo existente quanto POST de predição com uma coleta existente retornaram 500. O POST falhou ao consultar o histórico, antes de chamar a IA ou gravar dados. A migração existente `20261004181139_PersistirPredicoesIA` precisa ser aplicada nesse banco; nesta tarefa o banco não foi migrado.
- Na IA hospedada, o teste real via `IaClient` rejeitou uma resposta sem `riskRuleVersion` e com `history` como lista. Uma chamada independente também recebeu esses campos incompatíveis, embora outra chamada tenha retornado o contrato completo. É necessário publicar/verificar a versão corrigida da IA hospedada antes de considerar a integração configurada funcional.

Na IA local foi corrigida apenas a montagem da resposta: versão da regra, contagens do histórico, ajuste efetivo e amostras com os aliases do contrato. Os critérios e cálculos de classificação existentes foram preservados.

Validação atual: **58 testes C# passaram**, incluindo o fluxo HTTP com modelos reais da IA local, persistência em SQLite descartável e consulta autenticada de risco; **28 testes Python passaram** com os pesos existentes. Há cobertura de entradas estritas, ausências opcionais, amostras incompletas, respostas divergentes, histórico, identidade/instante copiados, legados/futuros, isolamento, empates e ausência de gravações parciais. A geração SQL da nova migração e o snapshot EF passaram sem acesso ao PostgreSQL configurado. A compilação mantém avisos de nulabilidade preexistentes. O SDK instalado 10.0.101 e um ambiente temporário Python 3.12 com scikit-learn 1.6.1 foram usados; no macOS, o OpenMP fornecido pelo scikit-learn foi disponibilizado apenas ao processo de teste. Nenhum pacote do projeto ou modelo foi alterado para contornar problemas do ambiente.
