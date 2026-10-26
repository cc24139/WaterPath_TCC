-- PROPOSTA NÃO EXECUTADA. Idempotente por Chave, para revisão após eventual
-- aprovação da DDL. Linhas demonstrativas associadas somente a coletas sintéticas.
INSERT INTO "waterPath"."ValidacoesSinteticas"
    ("Chave", "CorpoHidricoNome", "PontoCodigo", "DataHoraColeta", "Regra", "Resultado", "Evidencias")
VALUES
    ('SINTETICO-Rio Doce|RDO-15|2025-06-24T12:00:00-03:00|contrato', 'SINTETICO-Rio Doce', 'RDO-15', '2025-06-24T12:00:00-03:00', 'Formato e unidades conforme ColetaCadastroInput e contrato da API', 'PENDENTE', '{"sintetico":true,"medicoes":["Ph:pH","OxigenioDissolvido:mg/L","Turbidez:NTU","Iqa:pontos"],"metais":["Fe:mg/L","Pb:µg/L"],"registro_fonte":false,"validacao_executada":false}'::jsonb),
    ('SINTETICO-Rio Doce|RDO-15|2025-06-26T12:00:00-03:00|contrato', 'SINTETICO-Rio Doce', 'RDO-15', '2025-06-26T12:00:00-03:00', 'Formato e unidades conforme ColetaCadastroInput e contrato da API', 'PENDENTE', '{"sintetico":true,"medicoes":["Ph:pH","OxigenioDissolvido:mg/L","Turbidez:NTU","Iqa:pontos"],"metais":["Fe:mg/L","Pb:µg/L"],"registro_fonte":false,"validacao_executada":false}'::jsonb)
ON CONFLICT ("Chave") DO UPDATE SET
    "Regra" = EXCLUDED."Regra",
    "Resultado" = EXCLUDED."Resultado",
    "Evidencias" = EXCLUDED."Evidencias";
