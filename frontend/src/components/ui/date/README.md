# Seleção de datas

Componentes compartilhados para manter calendário, digitação e validação com o visual do WaterPath.

- `DateField`: data única, digitação `DD/MM/AAAA`, calendário e atalhos Hoje/Ontem. Recebe e devolve `yyyy-MM-dd`. Uma digitação incompleta ou inválida limpa o valor enviado ao formulário, preservando o texto para correção.
- `DateTimeField`: data e horário separados na interface. Mantém o contrato `yyyy-MM-ddTHH:mm` do formulário de coleta, sem antecipar a conversão para UTC. Aceita `0930` como `09:30` e identifica horário inválido ou futuro.
- `DateRangeField`: intervalo com De/Até, seleção em dois cliques e atalhos Últimos 7 dias, Últimos 30 dias, Este mês e Mês anterior. Só chama `onChange` ao aplicar; cancelar, clicar fora ou pressionar Escape descarta o rascunho.
- `WaterCalendar`: adaptação do `@daypicker/react` 10.0.2 com idioma português, navegação por teclado e seletores de mês/ano.
- `DatePickerDialog`: diálogo nativo com foco contido e retorno ao botão de origem. Posicionado junto ao campo em desktop e na parte inferior no celular; permite rolagem em telas baixas.
- `dateUtils`: datas reais, formatação, máscara, atalhos e contagem inclusiva de dias.

Os estilos base da biblioteca e `date-picker.css` são importados diretamente por `WaterCalendar`, nessa ordem. Isso mantém a dependência de CSS no mesmo módulo do calendário. Não usar somente um `@import` no CSS processado pelo Tailwind: na verificação do servidor de desenvolvimento já em execução, esse caminho deixou os estilos fora do CSS servido, apesar de os componentes novos estarem presentes. A importação direta foi conferida também com o Turbopack do `npm run dev`, além do build de produção.

## Integração e fuso

`ReportSettingsPanel` usa o intervalo no lugar dos dois campos nativos. O período continua no calendário de Brasília, limitado a 366 dias e sem datas futuras. Alterar o intervalo não gera PDF automaticamente.

`AddAnalysisField` reutiliza `DateTimeField` quando o tipo solicitado é `datetime-local`. Assim, o cadastro geral e a inserção individual no rio recebem o mesmo controle. A data/hora continua local ao usuário; a conversão existente para UTC ocorre no envio da coleta. Os IDs usados para foco em erros foram preservados.

Datas de calendário são construídas ao meio-dia local e formatadas com componentes locais, sem passar por `toISOString()`. O cálculo do tamanho do período usa componentes civis em UTC para não depender da duração do dia em mudanças de horário de verão.

## Verificação

Na pasta `frontend`:

```sh
npm run test:date-fields
npm run test:report
npm run test:add-analysis
npm run build
```

Os testes de datas cobrem bissextos, entrada brasileira, fusos, viradas de mês/ano, atalhos, limites inclusivos e horário de 24 horas. A conferência manual no navegador cobre aplicação/cancelamento, Escape e retorno de foco, seleção inversa, teclado, datas futuras, digitação de horário e layouts desktop/celular. Essa conferência usa API simulada e dados descartáveis, sem salvar coletas.

Novos campos de data podem reutilizar os componentes informando limites e fuso conforme o contexto. Este ajuste não exige alterações na API ou no banco.
