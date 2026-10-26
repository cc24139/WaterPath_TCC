-- PROPOSTA NÃO EXECUTADA.
-- O modelo atual não contém tabela de validações. Esta DDL criaria um log mínimo
-- para rastrear critérios e resultados dos dados sintéticos sem alegar ser parte
-- do esquema atual. PostgreSQL, esquema waterPath.
CREATE TABLE IF NOT EXISTS "waterPath"."ValidacoesSinteticas" (
    "Chave" text PRIMARY KEY,
    "CorpoHidricoNome" text NOT NULL,
    "PontoCodigo" text NOT NULL,
    "DataHoraColeta" timestamp with time zone NOT NULL,
    "Regra" text NOT NULL,
    "Resultado" text NOT NULL CHECK ("Resultado" IN ('APROVADO', 'REPROVADO', 'PENDENTE')),
    "Evidencias" jsonb NOT NULL,
    "CriadaEm" timestamp with time zone NOT NULL DEFAULT now()
);
