# Relatório de correções da integração YOLO e predição

Data: 4 de outubro de 2026.

## Mudanças realizadas

| Área | Alteração | Resultado |
| --- | --- | --- |
| Cruzamento de metais | Em `Impact.py`, substituído `break` por `continue` quando um metal não é relevante ao objeto. | Ferro e manganês não interrompem a análise de zinco, chumbo, cádmio e demais metais relacionados. |
| Mensagens | Os excedentes são descritos como valores **previstos** acima do limite de referência; removida a impressão de valores no console. | O relatório distingue uma estimativa do modelo de uma medição confirmada. |
| Drenagem | Criada a classe `Drenagem`, com interpretação própria. | A classe `drainage connection` deixa de ser tratada como lixo e mantém sua confiança real. Não se atribuem metais específicos à drenagem sem uma associação validada. |
| Classes YOLO | O cruzamento usa os nomes nos metadados do modelo em vez de presumir que urbano sempre possui identificador 2. | Mudanças de ordem das classes não trocam as interpretações. Classes desconhecidas recebem mensagem neutra. |
| Resposta da API | Adicionados `detections`, `visualSummary`, `metalPredictions`, `sample`, `integrationMode` e `warnings`. | O cliente recebe dados estruturados além dos textos existentes. |
| Entrada tabular | Criado um DataFrame com os nomes e a ordem das 16 variáveis usadas na predição. | A inferência passa a identificar explicitamente as colunas. |
| Validação da amostra | `profundidade` tornou-se obrigatória e não negativa; valores não finitos são rejeitados nos campos numéricos. | Evita enviar `None`, infinito ou NaN como entrada do modelo. |
| Dataset visual | Criadas listas reproduzíveis para treino, validação e teste; `data.yaml` aponta para essas listas. | 74 imagens de treino, 16 de validação e 16 de teste, sem sobreposição de arquivos. |
| Treinamento | Atualizado `trainIa.ipynb` para gerar as listas, localizar os arquivos no repositório e avaliar os melhores pesos no teste reservado. | O notebook passa a usar a separação corrigida. Saídas antigas foram removidas para não apresentar métricas anteriores como resultados novos. |

As chaves e unidades do zinco já estavam corretas (`Zn, µg/L`) e foram preservadas. A indicação anterior de uma inconsistência nessa unidade foi incorreta.

## Contrato da resposta

Os campos anteriores `msg`, `predictions`, `predictedHeavyMetais` e `relatorioImg` foram mantidos. `predictions` continua contendo os valores textuais para compatibilidade. `predictedHeavyMetais` mantém o conteúdo antigo, apesar do nome impreciso; `sample` oferece a identificação correta para a amostra de entrada.

Os campos adicionais são:

- `detections`: identificador e nome da classe, confiança visual, caixa `[x1, y1, x2, y2]`, proporção da área da caixa e interpretação. As coordenadas se referem à imagem processada pela API, que pode ter sido redimensionada.
- `visualSummary`: contagem, confiança máxima e proporção média da área das caixas por classe. Classes sem detecção têm contagem zero e confiança máxima `null`. A área média das caixas não corresponde à área segmentada nem mede poluição.
- `metalPredictions`: lista com nome, valor numérico e unidade de cada metal.
- `sample`: dados da amostra recebida.
- `integrationMode`: `contextual`, pois o YOLO contextualiza o relatório sem alterar as concentrações.
- `warnings`: explica o papel das detecções e o significado da confiança e das áreas.

**Mudança de entrada:** clientes de `/predict` precisam enviar `profundidade` numérica, finita e maior ou igual a zero. Sua ausência é rejeitada na validação com HTTP 422.

## Verificação executada

Foram executados **8 testes automatizados**, todos aprovados:

1. Metais não relacionados não interrompem os alertas de zinco, chumbo e cádmio.
2. Drenagem conserva sua classe e confiança e não recebe alertas de metais por associação arbitrária.
3. A interpretação acompanha o nome da classe mesmo quando o identificador muda.
4. Ausência de detecções e classes desconhecidas são tratadas.
5. Valores iguais ao limite não geram excedente; contagens e médias são agregadas corretamente.
6. Metais estruturados conservam valores numéricos e unidades.
7. A divisão é reproduzível, mantém variantes Roboflow e arquivos idênticos juntos e preserva os originais.
8. Anotação ausente interrompe a geração antes de escrever listas.

Também foi verificada a sintaxe dos arquivos Python alterados, a sintaxe das células Python do notebook e a existência das 106 imagens referenciadas nas listas, sem duplicação entre conjuntos.

Para repetir os testes a partir da raiz do repositório:

```powershell
python -B -X utf8 -m unittest discover -s back-end/ia/app/tests -v
```

## Limites e próximos passos

- A API completa e os arquivos de modelo não foram executados: o Python disponível neste ambiente não possui NumPy, FastAPI, OpenCV, Pydantic e Joblib. Os testes verificam as regras de integração e divisão usando a biblioteca padrão; não confirmam a inferência real nem o contrato HTTP em execução.
- Os pesos existentes não foram retreinados. As novas listas corrigem futuros treinamentos; não tornam independentes as métricas dos pesos treinados com a divisão anterior. Execute o notebook atualizado para obter novos pesos e resultados.
- O agrupamento das imagens considera o nome original antes de `.rf.` e conteúdo idêntico. Ele não identifica automaticamente fotos distintas da mesma coleta ou ponto; uma divisão por local/data exige metadados de coleta.
- Os limites de metais já existentes foram preservados e não houve revisão normativa ou validação de associações ambientais.
- Não foi treinado um modelo multimodal. Isso exige imagens pareadas com medições do mesmo local e coleta, seguido de comparação com o modelo tabular em dados reservados.
- A alteração preexistente em `.vscode/settings.json` foi preservada.
