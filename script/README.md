# Importação pública do IQA-ES para WaterPath

O importador busca o CSV público cujo link é publicado na [página oficial IQA-ES da AGERH](https://servicos.agerh.es.gov.br/iqa/home.php). O nome e o endereço do arquivo podem mudar. Esta implementação está intencionalmente limitada ao **Espírito Santo**, conforme o escopo aprovado.

A AGERH informa que o sistema divulga dados históricos e atuais do Programa QualiRios ES e fornece download CSV em UTF-8. O esquema real do arquivo não é presumido pelo script. O importador não usa o webservice XML nem exige credencial de acesso da ANA.

## O que será enviado

- Corpos hídricos são cadastrados pela rota autenticada `POST /api/corpohidrico`. O tamanho obrigatório é interpretado em km e precisa estar previamente conferido na ANA BHO. `size_source` precisa citar BHO e uma URL do catálogo oficial `metadados.snirh.gov.br`; o campo ainda exige inspeção manual da feição/trecho que fundamenta a medida. O script não calcula nem inventa extensão. Sem isso, a validação para antes das gravações.
- Coletas e medições compatíveis com o enum da API são enviadas juntas por `POST /api/coleta`. O IQA publicado pela AGERH pode ser mapeado como `Iqa` e é gravado como medição da coleta, preservando a data e o vínculo ao ponto; valores fora da escala 0–100 são rejeitados. Isso armazena a nota publicada, sem recalculá-la a partir dos parâmetros. Códigos, unidades, localização e colunas dependem do mapeamento explícito revisado pelo operador.
- O mapeamento deve manter `source_name` e `source_url` HTTPS; a origem é gravada no estado local junto à coleta porque a entidade da API não tem campos de proveniência.
- Imagens só são enviadas quando apontadas por manifesto local, com origem, licença, atribuição e vínculo com ponto e coleta importados. O script não busca nem associa automaticamente imagens de satélite. A API recebe multipart em `POST /api/imagem`; os campos de proveniência adicionais não fazem parte do DTO da API e ficam registrados no estado local do importador. Guarde o manifesto junto desse estado para preservar a atribuição.

## Segurança e repetição

Sem `--apply`, a execução é somente de inspeção e prévia. `--apply` é explícito. A autenticação é feita no momento da execução com `WATERPATH_EMAIL` e `WATERPATH_PASSWORD`; não coloque credenciais em JSON, linha de comando, documentação ou repositório. `WATERPATH_API_URL` define a URL base da API.

O estado local (padrão `script/.waterpath-import-state.json`) registra a chave `ponto|data-hora` e os IDs retornados pela API. Uma coleta existente no mesmo corpo, horário e coordenadas é pulada. Se já houver coleta no mesmo corpo/horário, mas não for possível provar que pertence ao mesmo ponto, o importador para por colisão ambígua. Um conflito HTTP 409 também é tratado como colisão e pulado. Se a API criar uma coleta sem retornar ID, a execução para sem marcar a assinatura como concluída; revise antes de repetir. Preserve o arquivo de estado entre execuções e não o compartilhe publicamente.

O importador valida todas as linhas e o manifesto de imagens antes da primeira gravação. A API não oferece uma transação para toda uma importação; falhas de rede/API no meio da execução podem deixar um lote parcial. O estado é salvo após cada operação confirmada para permitir retomada.

## Uso

1. Inspecione os cabeçalhos e conteúdo da fonte oficial sem fazer gravações:

   ```powershell
   python script/import_agerh_iqa.py
   ```

   Ou inspecione um arquivo CSV baixado manualmente:

   ```powershell
   python script/import_agerh_iqa.py --csv .\dados-agerh.csv
   ```

2. Crie um JSON de mapeamento a partir deste formato ilustrativo. Os textos `SUBSTITUIR...` são marcadores, não cabeçalhos do CSV. Preencha-os somente depois da inspeção e confirme cada código/unidade:

   ```json
   {
     "source_name": "AGERH — Programa QualiRios ES / IQA-ES",
     "source_url": "https://servicos.agerh.es.gov.br/iqa/home.php",
     "timezone": "America/Sao_Paulo",
     "columns": {
       "station": "SUBSTITUIR_PELO_CABECALHO_DO_CODIGO_DO_PONTO",
       "datetime": "SUBSTITUIR_PELO_CABECALHO_DA_DATA",
       "latitude": "SUBSTITUIR_OU_REMOVER",
       "longitude": "SUBSTITUIR_OU_REMOVER"
     },
     "parameters": [
       {
         "column": "SUBSTITUIR_PELO_CABECALHO_DE_OD",
         "code": "OxigenioDissolvido",
         "unit": "mg/L"
     },
     {
       "column": "SUBSTITUIR_PELO_CABECALHO_DO_IQA",
       "code": "Iqa",
       "unit": "pontos"
       }
     ],
     "stations": {
       "CODIGO_OFICIAL_DO_PONTO": {
         "body_name": "Nome do rio conforme fonte oficial",
         "location": "Município, Espírito Santo",
         "state": "ES",
         "size_km": "SUBSTITUIR_POR_NUMERO_KM_VERIFICADO_NA_BHO",
         "size_source": "ANA BHO — preencher feição/trecho e evidência da medida; https://metadados.snirh.gov.br/geonetwork/srv/por/catalog.search#/metadata/32e309da-a8c1-443f-90ac-0cd79ce6a33d"
       }
     }
   }
   ```

3. Valide o mapeamento e reveja a prévia sem escrever:

   ```powershell
   python script/import_agerh_iqa.py --mapping .\agerh-mapping.json
   ```

4. Defina as variáveis de ambiente na sessão local (de preferência por um gerenciador de segredos ou sessão protegida) e execute contra a API pretendida:

   ```powershell
   $env:WATERPATH_API_URL = "https://URL-DA-API"
   $env:WATERPATH_EMAIL = "conta-existente@example.org"
   $securePassword = Read-Host "Senha da conta existente" -AsSecureString
   $env:WATERPATH_PASSWORD = [Net.NetworkCredential]::new("", $securePassword).Password
   python script/import_agerh_iqa.py --mapping .\agerh-mapping.json --apply
   ```

5. Para imagens licenciadas e baixadas publicamente, acrescente `--images-manifest .\imagens.json`. O [INPE informa que as imagens CBERS-4 são distribuídas sem custos e sem restrição de uso](https://www.gov.br/pt-br/servicos/obter-imagens-de-sensoriamento-remoto-da-terra-geradas-pelo-satelite-cbers-4?id=12267&origem=servico), mas requer cadastro e seleção da cena; o link é disponibilizado após solicitação. O script não baixa nem recorta cenas: envie apenas arquivos locais associados manualmente ao ponto e à coleta, com atribuição registrada. Exemplo de formato:

   ```json
   {
     "images": [
       {
         "file": ".\\imagens\\jucu-ponto-01.jpg",
         "source": "https://URL-PUBLICA-DA-IMAGEM",
         "license": "Licença ou termos de reutilização verificados",
         "attribution": "Crédito exigido pelo provedor",
         "station": "CODIGO_OFICIAL_DO_PONTO",
         "source_id": "identificador-estavel-da-imagem",
         "signature": "CODIGO_OFICIAL_DO_PONTO|2024-01-31T00:00:00Z"
       }
     ]
   }
   ```

   `signature` deve corresponder exatamente à chave ponto/data-hora da coleta validada, em UTC ISO-8601. A imagem só é enviada se essa coleta tiver ID confirmado no estado local. O manifesto não substitui a verificação dos termos da fonte.

## Limitações e validação

- O escopo desta implementação é somente Espírito Santo; linhas de outros estados são rejeitadas.
- O nome atual do CSV é descoberto lendo o link da página oficial. Se a AGERH alterar a marcação da página ou publicar mais de um CSV, a ferramenta interrompe e pede inspeção manual.
- O cabeçalho CSV e os campos disponíveis variam e não foram fixados nesta implementação. O operador precisa inspecionar a fonte e preencher o JSON; colunas ausentes impedem a carga. Cabeçalhos duplicados e linhas com número inconsistente de colunas interrompem a validação.
- Datas sem horário, futuras, duplicadas ou com hora local ambígua/inexistente são rejeitadas. Horários sem fuso usam o configurado (padrão `America/Sao_Paulo`) e são convertidos para UTC.
- Tamanho de corpo hídrico precisa de evidência verificável da ANA BHO inserida no mapeamento. Esta CLI não baixa nem resolve automaticamente o trecho BHO.
- A rota de imagem não grava os campos de origem/licença/atribuição adicionais enviados pelo cliente. O arquivo e o manifesto local devem ser preservados para manter essa informação auditável.
- Para testar com segurança, primeiro rode apenas sem `--apply` e compare contagens, amostras, ponto, data, unidade e tamanho. Use depois uma instância de desenvolvimento/vazia e um estado local separado. Nunca inclua tokens, senhas ou strings de conexão em logs ou arquivos de configuração versionados.

Fontes institucionais: [AGERH — IQA-ES](https://servicos.agerh.es.gov.br/iqa/home.php) e [catálogo oficial da ANA para BHO6](https://metadados.snirh.gov.br/geonetwork/srv/por/catalog.search#/metadata/32e309da-a8c1-443f-90ac-0cd79ce6a33d). Dados tabulares são publicados para download pela própria AGERH; a BHO6 fornece base hidrográfica geoespacial e requer que a pessoa responsável confira a feição/trecho e obtenha a medida linear apropriada. As condições de uso e atribuição de cada imagem precisam ser verificadas individualmente antes de incluí-la no manifesto.
