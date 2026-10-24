# Tela de análise do corpo hídrico

A rota `/water-bodies/{id}/analysis` usa o layout e a sidebar de monitoramento. A seleção mostra as análises integradas mais recentes disponíveis na primeira página de até 100 coletas. O risco e os motivos vêm do resumo persistido; o resultado completo e a imagem anotada são consultados pelo ID da predição. A imagem é carregada com autenticação e exibida a partir de um URL temporário do navegador.

Os cartões não misturam escalas: risco da IA (1 a 3), IQA medido na coleta (0 a 100) e quantidade de estimativas de metais são informações distintas. O gráfico apresenta pH, oxigênio dissolvido e condutividade elétrica dos últimos 90 dias em três linhas. O eixo vertical de 0 a 100 indica apenas a posição relativa de cada linha na sua própria escala: pH usa 0–14; os outros indicadores usam zero até 110% do maior valor válido no período (mínimo de 1). A legenda mostra o valor real com unidade e a descrição informa os limites de cada linha. A altura de linhas diferentes não compara qualidade nem concentração.

## Diferenças em relação ao Figma

- A API não fornece uma classificação geral da concentração de metais. O cartão informa quantas estimativas foram retornadas; os valores e unidades ficam nos detalhes. Uma classificação exigiria limites documentados, interpretação por metal e distinção entre estimativa e medição.
- O resultado de análise integrada não inclui previsão futura. O quarto cartão mostra a data da coleta analisada. Uma previsão poderá ser ligada aqui quando houver contrato que associe resultado, horizonte e coleta.
- A IA retorna motivos do risco e detecções visuais, mas não recomendações individualizadas. A tela mostra os motivos recebidos e ações de navegação para histórico e novo cadastro. Recomendações futuras exigem regras ou um contrato próprio com evidências.
- A imagem da IA contém as marcações produzidas pelo modelo. Os pinos sobre locais do rio no Figma dependeriam de coordenadas das detecções e de um vínculo geográfico que o contrato atual não oferece.

Sem predição ou imagem disponível, a tela apresenta o estado correspondente; não usa fotografia, número ou diagnóstico demonstrativo como se fossem dados reais.
