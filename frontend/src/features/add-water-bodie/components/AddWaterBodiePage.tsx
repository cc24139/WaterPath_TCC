"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { LuCircleCheck, LuFileText, LuRuler } from "react-icons/lu";

import { useCadastro } from "@/api/hooks/useCorpoHidrico";
import { SideBar } from "@/components/layout/SideBar";
import { Button } from "@/components/ui/Button";
import { FormField } from "@/components/ui/FormField";
import { FormSectionCard } from "@/components/ui/FormSectionCard";
import { WaterBodySummary } from "@/features/add-water-bodie/components/WaterBodySummary";
import { waterBodyAccessOptions } from "@/features/add-water-bodie/constants/addWaterBodieOptions";
import { useAddWaterBodieForm } from "@/features/add-water-bodie/hooks/useAddWaterBodieForm";
import type { AddWaterBodieErrors } from "@/features/add-water-bodie/types/addWaterBodie";

type SubmissionFeedback =
  | { type: "error" | "success"; message: string }
  | null;

export function AddWaterBodiePage() {
  const router = useRouter();
  const [submissionFeedback, setSubmissionFeedback] =
    useState<SubmissionFeedback>(null);
  const { cadastrarCorpoHidrico, isLoading } = useCadastro();
  const {
    form,
    errors,
    updateField,
    updatePrivateStatus,
    validateForm,
    resetForm,
  } = useAddWaterBodieForm();

  function handleCancel() {
    resetForm();
    router.push("/water-bodies");
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmissionFeedback(null);

    const validationErrors = validateForm();
    const firstInvalidField = getFirstInvalidField(validationErrors);

    if (firstInvalidField) {
      setSubmissionFeedback({
        type: "error",
        message: "Revise os campos obrigatórios destacados antes de salvar.",
      });

      window.requestAnimationFrame(() => {
        document.getElementById(`water-body-${firstInvalidField}`)?.focus();
      });
      return;
    }

    const result = await cadastrarCorpoHidrico({
      nome: form.name.trim(),
      localizacao: form.location.trim(),
      tamanho: parseWaterBodySize(form.size),
      ehPrivado: form.ehPrivado,
    });

    if (!result.ok) {
      setSubmissionFeedback({
        type: "error",
        message: result.message,
      });

      return;
    }

    setSubmissionFeedback({
      type: "success",
      message: `${form.name.trim()} foi cadastrado com sucesso. Abrindo uma nova análise...`,
    });

    router.push("/add-analysis");
  }

  return (
    <div className="min-h-screen bg-background lg:flex">
      <SideBar
        variant="analysis-registration"
        subtitle="Área de cadastro"
        activeHref="/add-water-bodie"
      />

      <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
        <form
          onSubmit={handleSubmit}
          noValidate
          className="mx-auto flex w-full max-w-[1500px] flex-col gap-6 lg:gap-7"
        >
          <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-start">
            <div className="min-w-0">
              <h1 className="font-heading text-[28px] font-bold leading-tight text-text-primary sm:text-[32px]">
                Adicionar corpo hídrico
              </h1>
              <p className="mt-2 max-w-3xl font-heading text-[12px] font-medium leading-relaxed text-text-secondary sm:text-[13px]">
                Cadastre um novo rio, lago, córrego ou represa para uso nas
                análises futuras.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-3 sm:flex sm:justify-end">
              <Button
                variant="outline"
                onClick={handleCancel}
                disabled={isLoading}
                className="px-4 sm:min-w-32 sm:px-6"
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                isLoading={isLoading}
                className="px-4 sm:min-w-44 sm:px-6"
              >
                {isLoading ? "Salvando..." : "Salvar corpo hídrico"}
              </Button>
            </div>
          </div>

          {submissionFeedback ? (
            <div
              role={submissionFeedback.type === "error" ? "alert" : "status"}
              aria-live="polite"
              className={`flex items-start gap-2 rounded-lg border px-4 py-3 font-heading text-[12px] font-semibold ${
                submissionFeedback.type === "success"
                  ? "border-primary/25 bg-primary/10 text-primary"
                  : "border-contrast/25 bg-contrast/10 text-contrast"
              }`}
            >
              {submissionFeedback.type === "success" ? (
                <LuCircleCheck className="mt-0.5 h-4 w-4 shrink-0" />
              ) : null}
              {submissionFeedback.message}
            </div>
          ) : null}

          <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px] 2xl:grid-cols-[minmax(0,1fr)_390px]">
            <div className="flex min-w-0 flex-col gap-5">
              <FormSectionCard
                title="Dados principais"
                description="Informe os dados básicos do corpo hídrico."
                icon={LuFileText}
              >
                <div className="grid gap-x-6 gap-y-5 md:grid-cols-2">
                  <FormField
                    id="water-body-name"
                    label="Nome do corpo hídrico"
                    name="name"
                    value={form.name}
                    onChange={(value) => updateField("name", value)}
                    placeholder="Ex.: Rio Paraíba do Sul"
                    autoComplete="off"
                    required
                    error={errors.name}
                  />
                  <FormField
                    id="water-body-location"
                    label="Localização"
                    name="location"
                    value={form.location}
                    onChange={(value) => updateField("location", value)}
                    placeholder="Ex.: Niterói, Rio de Janeiro"
                    autoComplete="address-level2"
                    required
                    error={errors.location}
                  />
                </div>
              </FormSectionCard>

              <FormSectionCard
                title="Dimensão e acesso"
                description="Informe o tamanho do corpo hídrico e as condições de acesso ao local."
                icon={LuRuler}
              >
                <div className="grid gap-x-6 gap-y-5 md:grid-cols-2">
                  <FormField
                    id="water-body-size"
                    label="Tamanho do corpo hídrico (km)"
                    name="size"
                    value={form.size}
                    onChange={(value) => updateField("size", value)}
                    placeholder="Ex.: 120,5"
                    helper="Informe a extensão aproximada em quilômetros."
                    type="number"
                    inputMode="decimal"
                    min="0.01"
                    step="any"
                    required
                    error={errors.size}
                  />
                  <FormField
                    id="water-body-access"
                    label="Tipo de acesso"
                    name="ehPrivado"
                    value={String(form.ehPrivado)}
                    onChange={(value) => updatePrivateStatus(value === "true")}
                    options={waterBodyAccessOptions}
                    helper="Indique se o corpo hídrico está em uma área privada."
                  />
                </div>
              </FormSectionCard>
            </div>

            <aside className="flex min-w-0 flex-col gap-5 xl:sticky xl:top-6 xl:self-start">
              <WaterBodySummary form={form} />
            </aside>
          </section>
        </form>
      </main>
    </div>
  );
}

function getFirstInvalidField(errors: AddWaterBodieErrors) {
  return (Object.keys(errors) as Array<keyof AddWaterBodieErrors>)[0];
}

function parseWaterBodySize(size: string) {
  return Number(size.replace(",", "."));
}
