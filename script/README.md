# Importação pública do IQA-ES para WaterPath

O importador busca o CSV público cujo link é publicado na [página oficial IQA-ES da AGERH](https://servicos.agerh.es.gov.br/iqa/home.php). O nome e o endereço do arquivo podem mudar. Esta implementação está intencionalmente limitada ao **Espírito Santo**, conforme o escopo aprovado.

A AGERH informa que o sistema divulga dados históricos e atuais do Programa QualiRios ES e fornece download CSV em UTF-8. O esquema real do arquivo não é presumido pelo script. O importador não usa o webservice XML nem exige credencial de acesso da ANA.

## O que será enviado

- Corpos hídricos são cadastrados pela rota autenticada `POST /api/corpohidrico`. O tamanho obrigatório é interpretado em km e precisa estar previamente conferido na ANA BHO. `size_source` precisa citar BHO e uma URL do catálogo oficial `metadados.snirh.gov.br`; o campo ainda exige inspeção manual da feição/trecho que fundamenta a medida. O script não calcula nem inventa extensão. Sem isso, a validação para antes das gravações.
- Coletas e medições compatíveis com o enum da API são enviadas juntas por `POST /api/coleta`. O IQA publicado pela AGERH pode ser mapeado como `Iqa` e é gravado como medição da coleta, preservando a data e o vínculo ao ponto; valores fora da escala 0–100 são rejeitados. Isso armazena a nota publicada, sem recalculá-la a partir dos parâmetros. Códigos, unidades, localização e colunas dependem do mapeamento explícito revisado pelo operador.
- O mapeamento deve manter `source_name` e `source_url` HTTPS; a origem é gravada no estado local junto à coleta porque a entidade da API não tem campos de proveniência.
- Imagens só são enviadas quando apontadas por manifesto local, com origem, licença, atribuição e vínculo com ponto e coleta importados. O script não busca nem associa automaticamente imagens de satélite. A API recebe multipart em `POST /api/imagem`; os campos de proveniência adicionais não fazem parte do DTO da API e ficam registrados no estado local do importador. Guarde o manifesto junto desse estado para preservar a atribuição.
- Metais podem ser enviados no campo `metaisPesados` do cadastro da coleta, mas a API trata esses valores como concentrações na **água**. Os símbolos aceitos são `Fe` e `Mn` em `mg/L`, e `Cr`, `Ni`, `Cu`, `Zn`, `Cd` e `Pb` em `µg/L`. A configuração pode informar `source_unit` como `mg/L` ou `µg/L`; quando necessário o script converte por fator 1.000 e registra valor/unidade de origem e destino no estado local. Resultados censurados (`<LQ`) são recusados porque a API não tem campo para limite de quantificação. Arsênio, cobalto e alumínio também não são aceitos pelo contrato atual.

## Segurança e repetição

Sem `--apply`, a execução é somente de inspeção e prévia. `--apply` é explícito. A autenticação é feita no momento da execução com `WATERPATH_EMAIL` e `WATERPATH_PASSWORD`; não coloque credenciais em JSON, linha de comando, documentação ou repositório. `WATERPATH_API_URL` define a URL base da API.

Requer Python 3.10 ou superior. Em Windows, instale a base de fusos IANA usada pelo padrão `America/Sao_Paulo` antes de validar mapeamentos sem offset explícito: `python -m pip install tzdata`. Timestamps ISO-8601 com offset explícito também podem ser processados sem essa base.

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

   Para um CSV local que tenha concentrações de metais em água, adicione a lista `metals` e declare o compartimento. Os nomes de coluna abaixo também são exemplos, não cabeçalhos presumidos:

   ```json
   {
     "compartment": "water",
     "parameters": [],
     "metals": [
       { "column": "COLUNA_FERRO", "name": "Fe", "unit": "mg/L", "source_unit": "µg/L" },
       { "column": "COLUNA_CHUMBO", "name": "Pb", "unit": "µg/L" }
     ]
   }
   ```

   A unidade em `unit` é a unidade canônica exigida pela API. `source_unit` é opcional quando a unidade do arquivo já coincide; use-a quando o conversor precisar aplicar a conversão explícita.

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

## Metais publicados e imagens de referência

`research_metal_measurements.csv` registra medições extraídas de teses da UFES para os rios Santa Maria da Vitória e Jucu, incluindo URL, autoria, estação, mês/ano da campanha, compartimento, método, parâmetro, valor, unidade e incerteza publicada (SD ou RSD quando disponível). São dados publicados, não amostras sintéticas. As campanhas informam somente mês/ano, sem dia de coleta; por isso não podem ser gravadas como `Coleta` sem inventar horário. Os resultados de sedimento ficam separados e não são encaminhados ao campo de metais da API. Para uso futuro, também será necessário avaliar censura e unidades de cada observação. Esses dados podem ajudar a conferir valores e formatos, mas a comparação da saída da IA com poucos estudos, matrizes e campanhas não constitui validação científica.

O teste do importador cobre o formato de gravação, vínculo coleta/corpo hídrico, conversão de unidade, repetição e rejeição de sedimento/censura. Esses são testes de funcionamento. Não demonstram precisão, calibração ou validade científica das predições de IA.

Imagens candidatas encontradas para contextualização visual (não são evidência de metais):

| Rio/local | Origem e autoria indicada | Condições de uso encontradas |
|---|---|---|
| Foz do Rio Doce, Regência/ES | [ANA — Foz do Rio Doce](https://www.gov.br/ana/pt-br/imagens/imagens-das-regioes-hidrograficas/regiao-hidrografica-atlantico-sudeste/atsd_106-d-es1588-foz-do-rio-doce-regencia-es-zig-koch.png/view), foto Zig Koch / Banco de imagens ANA | Página identifica autoria, mas não encontrei licença aberta; a ANA oferece serviço próprio para solicitar uso/reprodução. Não incluída no manifesto. |
| Bacia do Rio Jucu | [AGERH — Bacias Hidrográficas](https://agerh.es.gov.br/bacias-hidrograficas), imagem creditada a Amanda Amaral | A página pública não explicita licença de reutilização; não incluída no manifesto até confirmação dos termos. |
| Rio Santa Maria da Vitória, Santa Leopoldina | [A Gazeta — imagens antes/depois da chuva](https://www.agazeta.com.br/es/cotidiano/imagens-mostram-rio-santa-maria-da-vitoria-antes-e-depois-da-chuva-1021), página atribuída à Redação de A Gazeta; fotógrafo individual não identificado no conteúdo consultado | Não encontrei licença aberta; não copiar nem enviar à API sem autorização/termos confirmados. |

As imagens acima são referências para futura curadoria e não foram baixadas nem enviadas à base.

Fontes tabulares e de parâmetros: [AGERH — IQA-ES](https://servicos.agerh.es.gov.br/iqa/home.php), [PMQQS — Monitoramento convencional e downloads do Rio Doce](https://monitoramentoriodoce.org/download-dos-dados/), [tese UFES — elementos traço no Rio Santa Maria da Vitória](https://repositorio.ufes.br/server/api/core/bitstreams/56e40747-d4f1-4333-a5df-acb03e1b0a95/content) e [tese UFES — água e sedimentos do Rio Jucu](https://repositorio.ufes.br/bitstreams/5fe2f66d-c0e5-4205-8d64-a29c6e26354b/download). O portal PMQQS publica dados convencionais separados para água e sedimento; verifique o compartimento e as unidades antes de montar o mapeamento. A [BHO6 da ANA](https://metadados.snirh.gov.br/geonetwork/srv/por/catalog.search#/metadata/32e309da-a8c1-443f-90ac-0cd79ce6a33d) fornece a base hidrográfica e requer conferência manual do trecho e da medida linear.
