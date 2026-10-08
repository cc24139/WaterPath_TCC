# Variação temporal de metais na análise de risco

## Entrada da IA e da API principal

`POST /analyze` recebe `image`, `data` (JSON como texto) e `history` em multipart. A API principal recebe `coletaId`, `image` e o mesmo `data` em `POST /api/ia/predicoes`; valida, preenche a data UTC da coleta e encaminha os metais sem alterar seus valores/unidades. O `history` de níveis de risco continua sendo montado pela API principal.

Os campos existentes e as quatro medições obrigatórias foram preservados. Acrescentam-se estes campos opcionais dentro do JSON da amostra:

| Campo | Formato |
| --- | --- |
| `metais_pesados` | Lista de observações atuais, ou null. Máximo de oito metais distintos. |
| `referencia_metais_pesados` | Objeto anterior, ou null: `{"data":"ISO 8601 com fuso", "metais_pesados":[...]}`. |
| `name` em cada observação | Símbolo exato: Fe, Mn, Cr, Ni, Cu, Zn, Cd ou Pb, como em `metalPredictions`. |
| `value` em cada observação | Número finito >= 0, null ou ausente. Zero é válido. Strings numéricas e booleanos são rejeitados. |
| `unit` em cada observação | String não vazia, null ou ausente. Não existe unidade presumida. |

A referência é uma amostra anterior do **mesmo corpo hídrico**, fornecida pelo cliente, com observações comparáveis (mesmo metal e tipo de medição). Como nos demais dados enviados pelo cliente, a API não certifica a origem laboratorial. Não consulta automaticamente `MetalPesado`, `QualidadeFutura` ou concentrações estimadas. O histórico existente guarda apenas níveis base/IDs e não oferece concentrações observadas; por isso a referência explícita é necessária. `data` da amostra atual é preenchido pela API principal. Em chamadas diretas à IA, informe-o para permitir a comparação.

Duplicação de símbolos/campos, campos desconhecidos, símbolos inválidos, valores negativos/não finitos ou tipos incorretos retornam 422. Omissão de valores, unidades ou instantes permite a análise dos demais parâmetros, com variação indeterminada para os metais afetados. Instantes precisam ser ISO 8601 com fuso, com a referência estritamente anterior; datas ausentes, inválidas, iguais ou posteriores não são comparadas.

## Comparação e saída

A feature de risco retorna `metalVariation`, separado de `metalPredictions`. Compara somente observações recebidas: estimativas do modelo não substituem medições ausentes. Inclui a união dos metais atuais e anteriores, ordenada pelo símbolo, para não ocultar um metal ausente em uma das amostras.

São compatíveis mg/L e µg/L, incluindo μg/L e ug/L. A conversão usa 1 mg/L = 1000 µg/L. O delta é atual menos anterior, na unidade atual registrada em `unit`. Unidades ausentes ou não suportadas (incluindo ppm ou mg/kg, mesmo quando aparecem nas duas amostras) não são presumidas compatíveis.

`variation` por metal é `aumento`, `reducao`, `estabilidade` (igualdade após conversão, sem tolerância arbitrária) ou `indeterminada`. Valores/unidades originais aparecem em `currentValue`, `currentUnit`, `previousValue` e `previousUnit`. Em casos indeterminados, `delta` é null e `reason` explica o impedimento:

| reason | Significado |
| --- | --- |
| `dados_ausentes` | Um dos valores/observações está ausente. |
| `referencia_temporal_ausente_ou_invalida` | Falta um instante válido com fuso. |
| `referencia_nao_anterior` | A referência tem instante igual ou posterior ao atual. |
| `unidades_ausentes` | Falta uma das unidades. |
| `unidades_incompativeis` | As unidades não pertencem às escalas suportadas. |
| `variacao_nao_representavel` | O delta não pode ser representado como número finito sem perder a variação. |

O `status` geral descreve a disponibilidade da comparação: `completa` quando todos são comparáveis, `parcial` quando apenas alguns são, `indeterminada` quando nenhum é. Ausência total de metais resulta em `metals: []`, `status: "indeterminada"` e mensagem explícita de impossibilidade; nunca afirma que não houve aumento.

Aumento de concentração **não** significa ultrapassagem de limite. Esta extensão não cria limites, alertas normativos ou pesos para `riskLevel`. A regra `waterpath-risk-v1`, `riskInputs`, `riskReasons`, recorrência e inferência dos modelos mantêm o comportamento existente. As interpretações preexistentes de valores estimados permanecem separadas da comparação temporal.

## Exemplo de requisição

Ajuste os endereços, o ID e o instante da coleta. Salve o JSON como `amostra.json`:

```json
{
  "temperatura": 22,
  "ph": 7,
  "condutividade_eletrica": 100,
  "oxigenio_dissolvido": 6,
  "data": "2026-10-07T12:00:00Z",
  "metais_pesados": [
    {"name": "Pb", "value": 10, "unit": "µg/L"},
    {"name": "Cd", "value": 1, "unit": "µg/L"},
    {"name": "Fe", "value": 0.1, "unit": "mg/L"},
    {"name": "Cu", "value": 10, "unit": "µg/L"},
    {"name": "Zn", "value": 4, "unit": "mg/kg"}
  ],
  "referencia_metais_pesados": {
    "data": "2026-10-06T12:00:00Z",
    "metais_pesados": [
      {"name": "Pb", "value": 5, "unit": "µg/L"},
      {"name": "Cd", "value": 2, "unit": "µg/L"},
      {"name": "Fe", "value": 100, "unit": "µg/L"},
      {"name": "Zn", "value": 5, "unit": "µg/L"}
    ]
  }
}
```

API principal (o campo `data` interno pode ser omitido: será preenchido a partir da coleta):

```powershell
curl.exe -X POST 'http://localhost:5189/api/ia/predicoes' -F 'coletaId=7' -F 'image=@rio.jpg' -F 'data=<amostra.json'
```

Chamada direta à IA, com o mesmo JSON:

```powershell
curl.exe -X POST 'http://localhost:8000/analyze' -F 'image=@rio.jpg' -F 'data=<amostra.json' -F 'history=[]'
```

## Exemplo de resposta

Objeto `metalVariation` da IA, também presente em `resultado.metalVariation` na resposta 201 da API principal e no detalhe da predição:

```json
{
  "status": "parcial",
  "currentDate": "2026-10-07T12:00:00Z",
  "referenceDate": "2026-10-06T12:00:00Z",
  "metals": [
    {"name":"Cd","currentValue":1,"currentUnit":"µg/L","previousValue":2,"previousUnit":"µg/L","variation":"reducao","delta":-1,"unit":"µg/L","reason":null},
    {"name":"Cu","currentValue":10,"currentUnit":"µg/L","previousValue":null,"previousUnit":null,"variation":"indeterminada","delta":null,"unit":"µg/L","reason":"dados_ausentes"},
    {"name":"Fe","currentValue":0.1,"currentUnit":"mg/L","previousValue":100,"previousUnit":"µg/L","variation":"estabilidade","delta":0,"unit":"mg/L","reason":null},
    {"name":"Pb","currentValue":10,"currentUnit":"µg/L","previousValue":5,"previousUnit":"µg/L","variation":"aumento","delta":5,"unit":"µg/L","reason":null},
    {"name":"Zn","currentValue":4,"currentUnit":"mg/kg","previousValue":5,"previousUnit":"µg/L","variation":"indeterminada","delta":null,"unit":"mg/kg","reason":"unidades_incompativeis"}
  ],
  "message": "Não é possível determinar a variação de todos os metais: faltam dados comparáveis."
}
```

`GET /api/corpohidrico/{id}/risco-atual` expõe o mesmo objeto em `variacaoMetais`, junto dos campos de risco existentes. Entrada e resultado ficam nos JSONs existentes; não há migração adicional. Predições legadas sem a extensão continuam consultáveis quando a entrada não contém metais/referência; nelas, `variacaoMetais` é null, pois a análise temporal não foi realizada. A API rejeita com 502 respostas que omitam a extensão quando metais forem enviados, ou que tragam retrato incompatível com a entrada.

Publicar a IA atualizada antes da API principal. Clientes antigos podem continuar enviando a amostra sem metais. `/analyze` então retorna explicitamente a variação indeterminada; os demais campos mantêm o comportamento existente.

## Verificações reproduzíveis

A partir da raiz:

```powershell
& .venv-training/Scripts/python.exe -B -X utf8 -m unittest discover -s back-end/ia/app/tests -v
dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj
```

Os testes Python verificam aumento, redução, estabilidade, conversão, zero, ausência parcial/total, instantes inválidos/não anteriores, unidades incompatíveis e preservação da regressão/risco. Os testes HTTP da IA usam os modelos reais. Os testes .NET verificam multipart, rejeição de entradas/respostas inválidas, persistência em SQLite descartável e consulta autenticada de risco, sem acessar o PostgreSQL configurado.

Verificação desta alteração: **41 testes Python e 69 testes C# aprovados**, incluindo os modelos reais da IA e a integração multipart/persistência/consulta da API principal. Um teste C# opcional contra a IA real foi ignorado porque `WATERPATH_IA_TEST_URL` e `WATERPATH_TEST_IMAGE` não estavam configurados. A compilação mantém avisos de nulabilidade preexistentes; o ambiente Python emite avisos de depreciação.
