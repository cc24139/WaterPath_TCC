# Página Metodologias

A rota pública `/methodology` apresenta o processo de análise, os indicadores da água, os metais disponíveis e a leitura dos resultados. O visual parte do protótipo fornecido: introdução com imagem, fluxo de etapas e uma linha vertical de parâmetros. Foram acrescentados detalhes expansíveis, dúvidas frequentes, referências e acesso à listagem de corpos hídricos.

## Organização

- `components/MethodologyPage.tsx`: composição da página, com Header, Footer, Card e RiskBadge existentes. Usa a imagem de rio já presente no projeto.
- `constants/methodologyContent.ts`: explicações dos indicadores, etapas e dúvidas frequentes. Nomes, unidades e obrigatoriedade vêm de `indicatorFields`; os oito metais usam `metalOptions` no componente.
- `src/app/(public)/methodology/page.tsx`: rota e metadados.
- Os links “Metodologias” no cabeçalho, menu móvel e rodapé apontam para a nova rota. O proxy já permitia acesso público a esse endereço.

A página é renderizada no servidor. As explicações usam `details`/`summary` nativos, com navegação por teclado e abertura de um item por grupo. Não há nova dependência, consulta à API, inferência de IA ou alteração no banco para carregar o conteúdo.

## Conteúdo e manutenção

O texto distingue medições informadas, concentrações estimadas e detecções visuais. A seção de risco descreve os critérios implementados em `back-end/ia/app/services/risk/classification.py`: pH, oxigênio dissolvido, sinais visuais e recorrência no histórico. Metais não compõem diretamente o nível de risco atual. O risco não é apresentado como IQA ou certificação de potabilidade.

As definições dos parâmetros têm caráter explicativo, sem estabelecer novos limites ambientais. A página aponta para as referências de indicadores/IQA da ANA e de variáveis de qualidade da água da CETESB. Uma mudança na regra de risco ou no fluxo de análise deve atualizar também estas explicações. Novos indicadores exigem descrição em `indicatorExplanations`; o tipo `Record<IndicatorName, ...>` ajuda a identificar uma descrição ausente na compilação.

Previsões futuras e recomendações individualizadas não são descritas como recursos disponíveis. Se forem implementadas, acrescentar método, dados necessários e limitações antes de divulgá-las nesta página.

## Verificação

- TypeScript, ESLint nos arquivos alterados e build de produção.
- Testes existentes do proxy, incluindo acesso público a `/methodology` e proteção das áreas individuais.
- Navegador no servidor real de desenvolvimento: layout desktop/celular, navegação por âncoras, parâmetros e dúvidas expansíveis, teclado, links do menu e ausência de rolagem horizontal.

Comandos na pasta `frontend`: `npx tsc --noEmit`, `npm run test:proxy` e `npm run build`.
