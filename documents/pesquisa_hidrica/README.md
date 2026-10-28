# Dados pesquisados para teste do WaterPath

Pesquisa e preparação em 28/10/2026. **12 amostras de quatro lagos têm os quatro parâmetros obrigatórios do modelo.** Metais observados ficam separados das entradas. Nenhuma inferência, métrica ou alteração no banco foi executada.

## Arquivos e uso

- `entrada/parametros_lagos.csv`: parâmetros nas unidades do contrato `back-end/ia/app/services/Predict/schema.py`.
- `entrada/parametros_lagos.json`: lista com `amostra_id` e objeto `parametros`; usar o objeto de parâmetros na integração apropriada, não presumir que a lista inteira seja um corpo HTTP aceito.
- `validacao/metais_lagos.csv`: 96 observações dos oito metais-alvo; cruzar por `amostra_id`, metal e unidade. Só 68 valores positivos são candidatos a métricas quantitativas; 28 zeros publicados têm interpretação analítica incerta e foram marcados para exclusão.
- `amostras_lagos.csv`: corpo hídrico, ponto, camada e intervalo da campanha. O artigo informa 23–26/09/2021, não data exata por ponto; campos desconhecidos ficam vazios.
- `originais/jianghan_suplemento.zip` e PDF extraído: fonte preservada. As imagens `s1_conferencia.png` e `s5_conferencia.png` são páginas das tabelas para auditoria, não fotografias de coleta.
- `imagens/`: imagens históricas reais e manifesto preparados separadamente; conferir a cobertura espacial e temporal antes de uso.
- `preparar_lagos.py`: extração reproduzível a partir do PDF com pdfplumber; execute com Python. `validacao/conferencia.json` registra contagens e SHA-256 do PDF.

## Fonte dos lagos

Wang, C.; Wang, K.; Zhou, W.; Li, Y.; Zou, G.; Wang, Z. (2023). *Occurrence, Risk, and Source of Heavy Metals in Lake Water Columns and Sediment Cores in Jianghan Plain, Central China*. IJERPH 20(4), 3676. DOI [10.3390/ijerph20043676](https://doi.org/10.3390/ijerph20043676). [Artigo integral e métodos](https://pmc.ncbi.nlm.nih.gov/articles/PMC9963068/). [Suplemento original baixado](https://mdpi-res.com/d_attachment/ijerph/ijerph-20-03676/article_deploy/ijerph-20-03676-s001.zip). Licença CC BY 4.0; atribuir autores, artigo e informar que este pacote extraiu/converteu as tabelas.

Honghu (HH1/HH2), Donghu/East Lake em Wuhan (DH1/DH2), Liangzi (LZ) e Daye (DY), China. São seis pontos, cada um com água superficial e de fundo. Campanha 23–26 de setembro de 2021. A seção 2.1 informa coleta a 0,5 m da superfície e do fundo. Não foram inventadas coordenadas pontuais a partir do mapa do artigo. Datas de publicação não foram confundidas com datas de coleta.

Tabela S1 fornece temperatura °C, pH, condutividade µS/cm, oxigênio dissolvido mg/L, TN mg/L e TP mg/L. TP foi multiplicado por 1000 para µg/L. TSS e TOC não constam: vazios/nulos, sem imputação; turbidez e COD não os substituem. Tabela S5 fornece metais na água em µg/L. Fe e Mn foram divididos por 1000 para mg/L; demais metais-alvo mantidos em µg/L. `Top` em S5 corresponde a `Surface` em S1. **A ordem DY/LZ muda entre tabelas: o pareamento foi feito pelo ponto e camada, não pela posição global.** Sedimento em mg/kg (S6) não foi usado.

Os métodos (seção 2.2) descrevem digestão ácida da água. A fração não deve ser automaticamente tratada como metal dissolvido. Confirme compatibilidade com a definição dos alvos de treino antes de interpretar erro do modelo. Os valores `0.00` da publicação foram preservados como zeros reportados ambíguos: não significam ausência comprovada e não são referência quantitativa segura. O suplemento não fornece limites de detecção individuais suficientes para resolver esses zeros. Não se fez arredondamento de valores positivos para zero.

## Rio pesquisado e conjunto parcial existente

O [PMQQS / Monitoramento Rio Doce](https://monitoramentoriodoce.org/download-dos-dados/) foi consultado novamente em 28/10/2026. O portal disponibiliza monitoramento convencional e automático com filtro de até um ano. O repositório já contém oito coletas RDO-15, Linhares/ES, de 16/12/2025 a 21/07/2026, com pH e Fe dissolvido, em [../dados_tcc/Rio_Doce](../dados_tcc/Rio_Doce/README.md). Originais, entradas e metais já estão separados ali e não foram duplicados. Coordenadas existentes: -19.41011, -40.05896. **Não elegível para inferência atual:** faltam temperatura, condutividade e oxigênio. Não completar artificialmente. Condições de redistribuição e vínculo com dados já cadastrados estão no README original. A revisão da página pública não equivale a rebaixar/verificar novamente todos os registros ArcGIS.

Outra fonte localizada: [USGS, Chemical Composition of Water in Lakes, Ponds, Rivers, and Creeks in Northwest Alaska](https://www.usgs.gov/data/chemical-composition-water-lakes-ponds-rivers-and-creeks-northwest-alaska), DOI 10.5066/P13EV8UD, CC0. Catálogo descreve os quatro parâmetros e metais, mas o download ScienceBase apresentou bloqueio de acesso nesta sessão; nenhum dado numérico dessa fonte foi incluído nem alegado validado.

## Validação e limites

Extração das tabelas S1/S5 conferida visualmente. Verificados 12 IDs únicos, 96 alvos, correspondência de IDs, quatro entradas obrigatórias presentes, intervalo de pH e oxigênio não negativo. 68 referências positivas e 28 zeros ambíguos. Não houve teste de acurácia: faltam predições. Disponibilidade de entradas não prova adequação ao domínio de treino nem independência do conjunto; verificar sobreposição com treino antes de usar como teste externo. As 12 amostras de uma única campanha não sustentam conclusão geral de desempenho. Fotos/satélite fornecem contexto visual, não evidência de concentração de metais nem entrada validada para o módulo visual.
