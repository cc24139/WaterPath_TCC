# Dados da tela water-bodies

A tela usa os GETs públicos existentes de `corpohidrico`, `coleta` e `qualidade`,
via os services compartilhados. Não há fallback para mocks nem requisição por card.
As três listas são consultadas em paralelo e requisições são canceladas ao desmontar.
Falha da lista principal mostra erro com nova tentativa; falha de coletas/IQA mantém
a lista com aviso de indisponibilidade. Uma resposta vazia é diferente de erro.

## Contrato e integração

- Corpos hídricos: `id`, `nome`, `localizacao`, `users[].id`.
- Coletas: `id`, `data`, `ph`, `turbidez`, `oxigenioDissolvido`, `corpoHidrico.id`.
- Qualidade: `id`, `iqa`, `corpoHidrico.id`. Não há data, unidade ou coleta associada.
- O adaptador também aceita campos em PascalCase, `IQA`, relações por
  `corpoHidricoId` e números como strings com ponto ou vírgula decimal. Não aceita
  objetos de paginação como se fossem listas; uma alteração desse contrato deve
  atualizar o carregador e os testes.
- Os GETs de coletas e qualidade foram ajustados no backend para carregar a
  relação com o corpo hídrico e projetar os campos escalares e IDs relacionados.
  Isso evita vínculos ausentes e a serialização de grafos de entidades cíclicos.
  Os endpoints individuais e de escrita continuam com seus contratos anteriores.
  As alterações do backend precisam ser publicadas para disponibilizar esses vínculos.
- Busca por nome/localização, seleção, contagem e filtros usam a lista real.
  "Meus corpos hídricos" usa `users[].id`, pois a coleta não informa seu autor.
  Esse filtro de interface não substitui autorização no servidor.

## Gráficos e valores ausentes

O antigo gráfico de IQA tinha números e meses fixos. Como `QualidadeEntity` não
possui data nem vínculo com uma coleta, um histórico temporal de IQA não pode ser
obtido desse contrato. A tela apresenta o histórico de pH, turbidez ou oxigênio
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
- A escala de pH é fixa em 0–14. Turbidez e oxigênio usam zero como base e máximo
  ajustado aos dados de cada card; alturas visuais entre cards não são comparáveis
  sem ler os eixos. As unidades não são inventadas: o contrato não as especifica.
- "Última coleta" usa a data, com ID como desempate determinístico, e não preenche
  seus valores ausentes com valores de outra coleta. A tabela permite ver empates.
- Um único IQA válido é exibido como registro sem data. Havendo mais de um registro,
  os valores são listados sem ordem cronológica e não se escolhe um "mais recente"
  pelo ID ou pela posição. As faixas visuais anteriores do projeto foram mantidas
  e explicitadas na tela; não constituem validação normativa de qualidade.
- Temperatura, foto ilustrativa e porcentagem de IQA foram retiradas. Os valores
  detalhados das coletas ficam na tabela expansível do próprio card.
- O link "Ver Análise" aponta para `/water-bodies/{id}`, preparado para a futura
  tela de análise completa. A página de destino ainda precisa ser implementada.

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

A compilação local do backend exige SDK .NET 10 (o ambiente de verificação tinha
somente SDK 8). Há também uma inconsistência preexistente a revisar antes de publicar:
`ColetaTableConfigure` ainda referencia `CloroResidual`, `Floretos`,
`ColiformesTotais` e `EscherichiaColi`, ausentes na `ColetaEntity` atual. A adequação
do modelo e das migrações do banco não foi incluída nesta integração de leitura.
