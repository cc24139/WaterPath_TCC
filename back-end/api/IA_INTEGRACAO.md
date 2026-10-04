# Integração da API com a IA

## Banco e configuração

A tabela `waterPath.PredicoesIA` guarda, por coleta, o JSON de entrada, o JSON completo de saída (metais e detecções), a imagem original e a imagem anotada. Os JSONs usam `jsonb` e as imagens usam `bytea` no PostgreSQL. As predições ficam separadas das medições reais da coleta. Excluir a coleta também exclui seu histórico de predições.

Antes de usar as rotas, aplique a migração no banco desejado, a partir de `back-end/api`:

```powershell
dotnet ef database update
```

A migração `PersistirPredicoesIA` foi criada; não foi executada no banco configurado. A conexão continua sendo definida pela variável `ConnectionString` existente.

Configure a IA em `appsettings.json` ou nas variáveis de ambiente:

```dotenv
IA__BaseUrl=http://localhost:8000
IA__TimeoutSeconds=120
```

## Criar uma predição integrada

`POST /api/ia/predicoes`, com `multipart/form-data`:

| Campo | Conteúdo |
| --- | --- |
| `coletaId` | ID de uma coleta existente |
| `image` | Arquivo JPEG ou PNG, até 10 MB |
| `data` | Objeto JSON da amostra, como texto |

Exemplo de `data` para o contrato atual da IA:

```json
{
  "temperatura": 22,
  "ph": 7,
  "condutividade_eletrica": 100,
  "oxigenio_dissolvido": 6,
  "solidos_suspensos_totais": null,
  "carbono_organico_total": null,
  "fosforo_total": null
}
```

As quatro primeiras medições são obrigatórias na IA. As unidades e os campos aceitos são publicados pelo serviço de IA em `/predict/infos`. O campo `data` enviado é a entrada da predição; a rota não o preenche automaticamente a partir das medições da coleta.

A API chama `/predict` com `data` e `image`, e `/vision/predict` com `file`. Só então grava um registro, em uma única transação. A resposta `201` inclui o ID, o resultado JSON com `detections`, `totalObjects` e `metalPredictions`, e os links das duas imagens. Os resultados correspondem a duas chamadas independentes ao modelo visual.

## Criar somente uma análise visual

`POST /api/vision`, com `multipart/form-data`: `file` e `coletaId`.

**Mudança no contrato existente:** `coletaId` agora é obrigatório para vincular e persistir a análise. A resposta continua sendo a imagem JPEG anotada. Os cabeçalhos `X-Predicao-Id` e `Location` identificam o registro salvo. Essa rota salva as duas imagens, com tipo `vision`; os campos de entrada e resultado JSON ficam nulos, pois `/vision/predict` retorna apenas uma imagem.

## Consultas

| Método | Rota | Retorno |
| --- | --- | --- |
| GET | `/api/ia/predicoes?coletaId=1&pagina=1&tamanhoPagina=20` | Histórico paginado; filtro de coleta opcional; máximo de 100 registros por página |
| GET | `/api/ia/predicoes/{id}` | Entrada, resultado e links das imagens |
| GET | `/api/ia/predicoes/{id}/imagem` | Imagem anotada salva no banco |
| GET | `/api/ia/predicoes/{id}/imagem/original` | Imagem original salva no banco |

A listagem e a consulta do resultado não carregam as imagens binárias. As novas rotas seguem o acesso público já utilizado pelas rotas de coleta e visão.

Erros: `400` para entrada inválida, `404` para coleta ou predição inexistente, `413` para imagem grande, `422` quando a IA rejeita a amostra, `502` para falhas ou respostas inválidas da IA e `504` para tempo excedido. Falhas antes da gravação não deixam uma predição parcial.

## Verificação

```powershell
dotnet test ../api.Tests/WaterPath.Api.Tests.csproj
```

Os testes usam um banco SQLite em memória e simulam a IA, verificando persistência, leitura das imagens, contrato multipart e ausência de gravação em falhas. Não fazem chamadas ao serviço de IA hospedado nem alterações no PostgreSQL configurado.
