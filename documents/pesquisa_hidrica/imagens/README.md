# Imagens históricas de contexto

Imagens reais NASA GIBS, camadas `MODIS_Terra_CorrectedReflectance_TrueColor` e `MODIS_Aqua_CorrectedReflectance_TrueColor`, solicitadas com data explícita por WMS 1.1.1. Os JPEGs são recortes servidos pela NASA, sem edição local, sem geração artificial. Norte no topo. Coordenadas em EPSG:4326; o manifesto informa limites oeste,sul,leste,norte.


- **Lagos Honghu, Liangzi, Daye e East (Donghu), China:** imagens de 23 e 25/09/2021, dentro da campanha de 23–26/09/2021 documentada na seção 2.1 do artigo. A data individual de cada estação não está estabelecida; diferença máxima possível de 3 dias (23/09) e 2 dias (25/09). São vistas regionais que contêm os quatro lagos, não fotografias da amostragem. Fonte da campanha: https://pmc.ncbi.nlm.nih.gov/articles/PMC9963068/ .
- **Rio Doce, Linhares/ES:** imagens pedidas para 16/12/2025 e 21/07/2026, mesmas datas dos registros PMQQS já existentes em `../../dados_tcc/Rio_Doce/entrada/parametros.csv`; estação RDO-15 (-19.41011,-40.05896) dentro do recorte. Diferença de calendário: 0 dias. Horário de passagem do satélite não confirmado e não equivale ao horário de coleta. Os dados químicos desse conjunto são incompletos para inferência, conforme README original.

## Limitações de uso

São mosaicos diários de reflectância corrigida em cores naturais, destinados à visualização. A resolução nominal da camada é aproximadamente 250 m; aumentar as dimensões do JPEG não aumenta a resolução do sensor. Nuvens e névoa podem ocultar a água. A imagem de Rio Doce de 16/12/2025 é dominada por nuvens e foi preservada como registro de indisponibilidade visual nessa data.

**Não usar estes JPEGs automaticamente no classificador treinado para fotografias terrestres de água.** Não demonstram concentração de metais, não substituem análise laboratorial e não constituem validação química. O vínculo temporal é ao dia/campanha; não há confirmação de coincidência no instante ou pixel exato da coleta.

## Proveniência e reprodução

Consulte `manifesto_imagens.csv` para URLs completas, datas e hashes SHA-256. Downloads realizados em 28/10/2026. Fonte: NASA EOSDIS Global Imagery Browse Services (GIBS), Terra/MODIS e Aqua/MODIS. Atribuição recomendada: “NASA EOSDIS GIBS / Terra MODIS” ou “NASA EOSDIS GIBS / Aqua MODIS”, conforme o manifesto. A distribuição dos dados NASA é aberta, mas preserve os créditos e consulte as diretrizes de uso antes de redistribuir: https://www.nasa.gov/nasa-brand-center/images-and-media/ .

Documentação oficial do serviço: https://nasa-gibs.github.io/gibs-api-docs/python-usage/ e https://nasa-gibs.github.io/gibs-api-docs/access-basics/ . A URL no manifesto reproduz o recorte e sua data solicitada; a disponibilidade e conteúdo remoto podem mudar.

Inspeção visual dos seis arquivos: Hubei 25/09 é a vista regional preferida; Hubei 23/09 tem nuvens e faixa sem dados. Rio Doce 21/07 e Aqua 20/07 também têm nuvens dominantes. Terra 19/07 tem quase todo o recorte sem cobertura (preto). As tentativas próximas foram preservadas para rastreabilidade, mas nenhuma imagem do Rio Doce permite observação confiável da água. Aqua 20/07 está a 1 dia da coleta; Terra 19/07 a 2 dias. A camada Aqua segue a mesma família de reflectância corrigida, com crédito NASA EOSDIS GIBS / Aqua MODIS.

