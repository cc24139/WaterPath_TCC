-- Para bancos no esquema das migrations até relacionamentoCorpoHidricoUser.
-- Execute uma única vez, antes de CreateMedicoes.sql (se Medicoes ainda não existir).
-- Preserva os valores legados; não converte indicadores antigos em medições.
-- Script independente: não altera __EFMigrationsHistory.
BEGIN;

ALTER TABLE "waterPath"."Coletas" RENAME COLUMN "Data" TO "DataHora";
ALTER TABLE "waterPath"."Coletas"
    ADD COLUMN "Latitude" double precision NULL,
    ADD COLUMN "Longitude" double precision NULL,
    ADD COLUMN "ProfundidadeMetros" double precision NULL,
    ALTER COLUMN "CorpoHidricoId" SET NOT NULL,
    ALTER COLUMN "Nome" DROP NOT NULL,
    ALTER COLUMN "CloroResidual" DROP NOT NULL,
    ALTER COLUMN "ColiformesTotais" DROP NOT NULL,
    ALTER COLUMN "EscherichiaColi" DROP NOT NULL,
    ALTER COLUMN "Floretos" DROP NOT NULL,
    ALTER COLUMN "OxigenioDissolvido" DROP NOT NULL,
    ALTER COLUMN "Ph" DROP NOT NULL,
    ALTER COLUMN "Turbidez" DROP NOT NULL;

-- Reafirma o vínculo obrigatório e a exclusão em cascata.
ALTER TABLE "waterPath"."Coletas"
    DROP CONSTRAINT "FK_Coletas_CorposHidricos_CorpoHidricoId",
    ADD CONSTRAINT "FK_Coletas_CorposHidricos_CorpoHidricoId"
        FOREIGN KEY ("CorpoHidricoId") REFERENCES "waterPath"."CorposHidricos" ("Id")
        ON DELETE CASCADE;

COMMIT;
