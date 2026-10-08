# Dados da tela water-bodies

A tela usa os GETs públicos existentes de `corpohidrico`, `coleta` e `qualidade`,
via os services compartilhados. Não há fallback para mocks nem requisição por card.
As três listas são consultadas em paralelo e requisições são canceladas ao desmontar.
Falha da lista principal mostra erro com nova tentativa; falha de coletas/IQA mantém
a lista com aviso de indisponibilidade. Uma resposta vazia é diferente de erro.

## Contrato e integração

- Corpos hídricos: `id`, `nome`, `localizacao`, `users[].id`.
- Coletas: `id`, `dataHora`, `corpoHidricoId` e `medicoes[]` com `codigoMedicao`, `valor`, `unidade` e `censurado`.
- Condutividade elétrica: código `4` ou `CondutividadeEletrica`, em `µS/cm` (também aceita as grafias `μS/cm` e `uS/cm`). O código `3` não é usado como substituto.
- Registros planos legados continuam aceitos com `data`, `ph`, `oxigenioDissolvido` e `corpoHidrico.id`. Para condutividade, exigem `condutividadeEletrica` e `condutividadeEletricaUnidade` explícitos.
- Qualidade: `id`, `iqa`, `corpoHidrico.id`. Não há data, unidade ou coleta associada.
- O adaptador também aceita campos em PascalCase, `IQA`, relações por
  `corpoHidricoId` e números como strings com ponto ou vírgula decimal. Não aceita
  objetos de paginação como se fossem listas; uma alteração desse contrato deve
  atualizar o carregador e os testes.
- A leitura usa o contrato atual de coletas sem misturar valores de outros parâmetros. A presença de `medicoes` tem prioridade sobre campos planos legados.
- Busca por nome/localização, seleção, contagem e filtros usam a lista real.
  "Meus corpos hídricos" usa `users[].id`, que relaciona usuários ao corpo hídrico.
  Esse filtro de interface não substitui autorização no servidor.

## Gráficos e valores ausentes

O antigo gráfico de IQA tinha números e meses fixos. Como `QualidadeEntity` não
possui data nem vínculo com uma coleta, um histórico temporal de IQA não pode ser
obtido desse contrato. A tela apresenta o histórico de pH, condutividade elétrica ou oxigênio
dissolvido das coletas. Para implementar IQA ao longo do tempo será necessário
persistir a data da avaliação ou a referência à coleta e expor isso na leitura.
A data de uma previsão de qualidade futura não é a data de uma medição de IQA.

- As coletas são ordenadas por data; os pontos no eixo X respeitam o tempo real
  entre registros. Não se presumem doze meses ou amostras mensais.
- Datas ISO sem fuso são interpretadas como UTC para manter a data estável entre
  navegadores. Datas com offset são convertidas para UTC. Os horários são
  identificados na interface; idealmente o backend deve sempre enviar o fuso.
- Datas inválidas, inclusive o DateTime padrão `0001-01-01`, são desconsideradas
  com aviso. IDs duplicados são desconsiderados. Não se associa medição por nome
  nem pela posição nas listas quando falta o ID do corpo hídrico.
- Valores ausentes, negativos ou não finitos viram `null`, nunca zero. pH fora de
  0–14 e IQA fora de 0–100 são tratados como inválidos. Zero válido é preservado.
- Não se converte automaticamente um IQA como `0.82` para `82`; a escala precisa
  ser definida pelo contrato, não inferida pelo valor.
- Um parâmetro ausente quebra a linha. Não há interpolação, suavização ou média
  mensal. Uma única medição é um ponto. Coletas no mesmo instante permanecem
  separadas na tabela e não são conectadas entre si; pontos iguais podem se sobrepor.
- A escala de pH é fixa em 0–14. Condutividade elétrica e oxigênio usam zero como base e máximo
  ajustado aos dados de cada card; alturas visuais entre cards não são comparáveis
  sem ler os eixos. Condutividade usa µS/cm; unidades ausentes/incompatíveis, medições censuradas ou códigos duplicados ficam indisponíveis para o gráfico. O contrato plano legado não informa unidade de oxigênio.
- "Última coleta" usa a data, com ID como desempate determinístico, e não preenche
  seus valores ausentes com valores de outra coleta. A tabela permite ver empates.
- Um único IQA válido é exibido como registro sem data. Havendo mais de um registro,
  os valores são listados sem ordem cronológica e não se escolhe um "mais recente"
  pelo ID ou pela posição. As faixas visuais anteriores do projeto foram mantidas
  e explicitadas na tela; não constituem validação normativa de qualidade.
- Temperatura, foto ilustrativa e porcentagem de IQA foram retiradas. Os valores
  detalhados das coletas ficam na tabela expansível do próprio card.
- O link "Ver Análise" aponta para o monitoramento em `/water-bodies/{id}`.

## Validação

### Prévia visual

Acesse `/water-bodies/demo` para explorar seis corpos hídricos fictícios com
históricos completos, lacunas, coleta única, múltiplos IQAs e ausência de dados.
A prévia reutiliza `WaterBodiesView`, a normalização e os gráficos da tela real.
Os exemplos ficam em `constants/demoRivers.ts` e só são importados pela rota de
demonstração. Essa rota não consulta nem grava medições na API; `/water-bodies`
continua usando as respostas reais.

`npm run test:water-bodies` executa testes de normalização e geometria com o runner
nativo do Node (Node 22.6+ com suporte a `--experimental-strip-types`). Os registros
de teste não são importados pela aplicação.

Na consulta de verificação, a API publicada retornou dois corpos hídricos e listas
vazias de coletas e qualidade. Nessa situação, a tela deve mostrar ausência de
medições, sem desenhar curvas de demonstração.


## Condutividade elétrica na interface

Cards, seleção de indicadores, tabelas e gráficos passaram a usar condutividade elétrica. O dashboard também exporta a medição no CSV e usa o ícone `LuZap`, da mesma família de ícones do projeto. Os dados demonstrativos foram atualizados para amostras sintéticas em µS/cm.

O DTO de qualidade futura mantém o campo legado `turbidez`, pois ele ainda faz parte do contrato desse endpoint no backend; o campo não alimenta as medições de condutividade nem aparece nesses componentes. Não houve alteração do banco ou da regra de risco nesta substituição.
