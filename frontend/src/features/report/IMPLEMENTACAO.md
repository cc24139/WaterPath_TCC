# Relatório de monitoramento - etapas 1 a 4

Implementado e validado em 28/10/2026.

## Entrega

A rota `/water-bodies/{id}/report` recebe o ID validado e oferece configuração, geração, prévia paginada e download do mesmo PDF. A emissão consulta as coletas e análises existentes; não executa inferência nova nem grava registros de relatório. A identidade visual usa os cards, botões, sidebar e cores do projeto.

## Etapa 1 - configuração e validação

- Período analisado independente de previsão, no calendário de Brasília, com padrão de 30 datas. Datas reais, ordenadas, desde 1900, sem término futuro e até 366 dias.
- Detalhamento resumo/médio/técnico e extensão compacto/padrão/completo. Extensão controla quantidade de histórico, metais e imagens; não promete número fixo de páginas.
- Seções: síntese, indicadores atuais, evolução, metais, evidências visuais e metodologia detalhada. É necessário selecionar ao menos uma. Identificação, abrangência, motivos de atenção existentes e limitações básicas são obrigatórios.
- Opções adicionais enfatizam motivos de atenção e cores da classificação. A opção redundante de linguagem técnica foi incorporada ao detalhamento.
- Previsões, recomendações e contramedidas aparecem como próximas etapas, sem controles que prometam geração ainda não implementada.
- `types/reportConfig.ts`, `constants/reportOptions.ts` e `utils/reportConfig.ts` definem o contrato do frontend; `back-end/api/src/Relatorios/ReportContracts.cs` valida o mesmo contrato no servidor.
- Verificação executada antes da etapa seguinte: três testes do frontend e um do backend aprovados para datas, seções e configuração.

## Etapa 2 - dados autorizados

`ReportDataService` consulta um corpo hídrico por requisição e respeita `ResourceAuthorizationService`. Membros do corpo hídrico recebem suas coletas acessíveis; autores sem vínculo de membro recebem apenas suas próprias coletas, com nota explícita de escopo restrito. Resultados de outros usuários não são incorporados por associação indireta.

O período é convertido de Brasília para limites UTC, com fim exclusivo no início do dia seguinte. São aceitas até 200 coletas; acima disso, retorna 422 pedindo redução do intervalo. Não há truncamento silencioso nem relatório vazio: ausência de coletas também retorna 422.

São carregados medições, metais e metadados das predições em consultas de lote. Uma análise integrada por coleta, mais recente até a geração, com ID como desempate. A presença de predição é distinguida de risco válido e de ausência de análise. Uma versão anterior não substitui silenciosamente uma análise mais nova inválida.

A correspondência com as medições atuais reutiliza `AmostraDaColeta.Montar`, incluindo unidades, censura, metais e conversão de fósforo para a entrada da IA. Resultados anteriores a edições incompatíveis ficam indisponíveis. Valores apresentados nas tabelas preservam as unidades armazenadas.

Imagens só são consultadas quando a seção está habilitada, com até um/dois/três pares no modo compacto/padrão/completo. Notas distinguem a seleção reduzida de imagens ausentes. A cópia em memória dura apenas a geração; não constitui ainda um snapshot persistente de emissão.

Verificação antes da composição: quatro testes acumulados passaram. A conferência final acrescentou um teste de conversão de fósforo para preservar a correspondência entre banco e amostra normalizada.

## Etapa 3 - composição do conteúdo

`ReportComposer` produz blocos e tabelas a partir do `ReportData`, sem depender do renderizador PDF. Textos são derivados de resultados estruturados. Não há geração de recomendações por IA de linguagem.

- Abrangência: total de coletas, com risco válido, sem análise e com risco indisponível; distribuição de níveis sem média de categorias.
- Risco da última coleta, sem substituição pelo risco de uma coleta antiga.
- Indicadores: sete parâmetros previstos pelo cadastro, com origem, unidade, ausência e censura explícitas.
- Histórico: até 5/30/200 linhas, mantendo contagens e motivos de atenção de todo o período.
- Metais: última coleta / cinco últimas / todas, conforme extensão; origem medida/estimada, variação e referência devolvidas pela análise. Não são inventados limites regulatórios.
- Evidências: imagens originais/anotadas e classes detectadas.
- Metodologia: critérios de triagem, distinção de IQA, limitações, horários e rastreabilidade adicional no modo técnico.

Os gráficos usam datas reais e o contexto de ponto/profundidade da última coleta. Ausências, valores censurados e unidades diferentes quebram os segmentos. A legenda e a tabela permitem verificar os valores e datas.

Verificação antes do PDF: seis testes acumulados aprovados, incluindo seleção de seções, manutenção dos alertas e detalhamento técnico sem alterar resultados.

## Etapa 4 - PDF, prévia e download

- Dependência backend: QuestPDF **2026.9.1**, configurado com licença Community para o contexto acadêmico deste projeto. Rever a licença caso o uso mude. [Integração oficial](https://www.questpdf.com/examples/aspnet-integration.html) e [guia de licença](https://www.questpdf.com/license/guide.html).
- `ReportPdfDocument`: PDF A4, cores WaterPath, cabeçalho repetido, rodapé com páginas, tabelas paginadas, gráficos SVG e pares de imagens.
- `ControllerRelatorios`: `POST /api/relatorios/pdf`, com JWT, limites de requisição e `Cache-Control: no-store`. O nome do emissor vem da sessão.
- `api/services/reportServices.ts` envia a configuração por `apiFetch`; `utils/reportResponse.ts` valida status, tipo e assinatura PDF, sem mostrar HTML interno de erros.
- `useReportConfig`: bloqueio de solicitações simultâneas, timeout de 120 segundos, cancelamento ao sair/mudar usuário e limpeza das URLs de Blob. A prévia anterior é preservada em falha, identificada como desatualizada quando a configuração muda. Download fica disponível apenas para configuração correspondente.
- `ReportPreview` e `PdfPreviewPages`: renderização local com PDF.js **6.4.299**, worker incluído no build, paginação e adaptação à largura. O leitor nativo do navegador integrado não exibiu o PDF; PDF.js resolveu essa diferença. Prévia e download usam a mesma URL Blob, sem nova consulta ou geração ao baixar.
- Os botões de abrir e baixar mantêm acesso ao arquivo caso o leitor de prévia não possa renderizar uma página.

### Contrato de requisição

```json
{
  "corpoHidricoId": 12,
  "config": {
    "startDate": "2026-10-01",
    "endDate": "2026-10-28",
    "detailLevel": "medio",
    "documentLength": "medio",
    "includedSections": {
      "executiveSummary": true,
      "currentData": true,
      "indicatorEvolution": true,
      "heavyMetals": true,
      "visualEvidence": true,
      "detailedMethodology": false
    },
    "additionalSettings": {
      "highlightCriticalPoints": true,
      "includeVisualAlerts": true
    }
  }
}
```

Resposta: bytes `application/pdf` com nome para download. 400 para configuração inválida, 401 para sessão inválida, 403 para falta de permissão e 422 para ausência de dados ou volume acima do limite. Não há tabela ou migração nova.

## Verificação final

- **10 testes específicos do backend aprovados**: configuração, período, permissões, seleção das predições, edição de medição, conversão de unidades, composição, paginação PDF, gráficos e HTTP real do controller.
- **55 testes do frontend aprovados** na execução conjunta; cinco cobrem a nova configuração, resposta PDF, falhas HTTP e identificação de configuração alterada.
- TypeScript, ESLint nos arquivos novos/alterados e build de produção aprovados, incluindo o worker do PDF.js.
- PDF gerado e renderizado com Poppler, com inspeção das páginas, cabeçalhos, tabelas, gráficos, imagens e caracteres especiais. O exemplo usa dados sintéticos e uma imagem existente no dataset do projeto.
- Conferência no navegador: login e retorno à rota; geração; estados de carregamento; PDF renderizado em canvas; navegação entre páginas; aviso ao alterar seção; restauração da configuração; download real do arquivo. Conferência desktop e em viewport de 390 x 844, sem rolagem horizontal do documento HTML.
- Ambiente de navegador isolado: controller de relatórios real, autenticação JWT e banco SQLite descartável; login sintético somente no host auxiliar em `/tmp`. Não usa credenciais nem banco de produção.
- A suíte geral do backend registrou **119 aprovados, 2 ignorados e 1 falha** antes do último teste de conversão ser acrescentado. A falha está em `IaSharedContractTests.IaClientEnviaEConsomeExemplosDoContratoCompartilhado`: o exemplo do contrato não satisfaz os campos hoje exigidos por `IaClient`. Esse teste, o contrato compartilhado e o cliente de IA não foram alterados nesta entrega. Os dois ignorados dependem de serviço IA real, que não é necessário para emitir este PDF.

### Repetir os testes

Na pasta `frontend`: `npm run test:report`, `npm run test:proxy`, `npm run build`. Na raiz: `dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj --filter FullyQualifiedName~ReportTests`.

Para salvar um exemplo durante o teste de PDF, definir `WATERPATH_REPORT_SAMPLE` com caminho `.pdf` e, opcionalmente, `WATERPATH_REPORT_IMAGE` com imagem local de teste. Esses parâmetros só são lidos pelo projeto de testes.

## Publicação

Publicar a API atualizada antes do frontend, que depende de `/api/relatorios/pdf`. Instalar/restaurar as novas dependências nos dois projetos. A biblioteca inclui os recursos nativos de PDF; validar o runtime de destino no deploy. O frontend aceita `NEXT_PUBLIC_API_URL` como nas outras telas.

Não foi realizado deploy. O relatório é gerado sob demanda; o ID no cabeçalho identifica aquela geração, mas ainda não permite recuperar o documento no servidor.

## Atualização de interface — seleção de período (28/10/2026)

Os dois campos nativos foram substituídos pelo componente compartilhado `DateRangeField`, com calendário em português, digitação `DD/MM/AAAA` e atalhos de período. A alteração só é confirmada em “Aplicar período”; cancelar ou pressionar Escape mantém as datas anteriores. No celular, o calendário abre como painel inferior.

Correção da integração de estilos: o servidor local com Turbopack estava servindo os componentes novos sem as regras do calendário, deixando meses e dias sem formatação. Os estilos da biblioteca passaram a ser importados diretamente por `WaterCalendar`, seguidos das personalizações em `date-picker.css`. Ambos os calendários foram conferidos no servidor local em execução, incluindo cor, tamanho dos dias e aplicação de período. A rota temporária de conferência foi removida ao concluir.

Foram mantidos o contrato da API, o fuso de Brasília, o máximo de 366 dias e o bloqueio de datas futuras. A geração do relatório continua dependendo do botão de gerar. Os mesmos componentes de calendário também atendem à data da coleta, com horário separado. Detalhes de reutilização e verificações no [README dos componentes de data](../../components/ui/date/README.md).

## Etapa 5 - recursos adicionais e utilidade

Conectar previsões já calculadas de 7/15 dias, com data de referência/alvo, método e evidência; definir catálogo revisado de recomendações e contramedidas associado aos achados; ampliar o detalhamento metodológico das versões dos modelos. Isso transforma a consolidação atual em apoio ao acompanhamento, mantendo a distinção entre medição, estimativa e projeção. Opções devem ficar disponíveis somente quando seus critérios e dados existirem. Exportar um relatório continuará sem disparar inferência silenciosa.

## Etapa 6 - emissão persistente e utilidade

Criar entidade de relatório emitido, com usuário, configuração, período, versão do template, snapshot das fontes e referência do PDF em armazenamento persistente. Isso permite reabrir exatamente o documento entregue, mesmo depois de alterar coletas, reanalisar imagens ou atualizar modelos. Acrescentar consulta dos relatórios emitidos, política de acesso e validação em homologação com volume e dados representativos do ambiente de publicação. Uma geração nova passa a constituir outra emissão, sem sobrescrever a anterior.
