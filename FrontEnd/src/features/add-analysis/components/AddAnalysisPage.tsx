"use client";

import { FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LuDroplet, LuFileText, LuMessageCircle } from "react-icons/lu";

import { SideBar } from "@/components/layout/SideBar";
import { AddAnalysisField } from "@/features/add-analysis/components/AddAnalysisField";
import { AddAnalysisFormSection } from "@/features/add-analysis/components/AddAnalysisFormSection";
import { AddAnalysisSummary } from "@/features/add-analysis/components/AddAnalysisSummary";
import { AnalysisImageCard } from "@/features/add-analysis/components/AnalysisImageCard";
import {
  indicatorFields,
  responsibleOptions,
  visualConditionOptions,
} from "@/features/add-analysis/constants/addAnalysisOptions";
import { useAddAnalysisForm } from "@/features/add-analysis/hooks/useAddAnalysisForm";
import { useWaterBodyOptions } from "@/features/add-analysis/hooks/useWaterBodyOptions";

export function AddAnalysisPage() {
  const router = useRouter();
  const {
    options: waterBodyOptions,
    loading: loadingWaterBodies,
    error: waterBodiesError,
    retry: retryWaterBodies,
  } = useWaterBodyOptions();
  const {
    form,
    updateField,
    updateImage,
    removeImage,
    resetForm,
    canSubmit,
  } = useAddAnalysisForm();
  const selectedWaterBody = waterBodyOptions.find((option) => option.value === form.waterBody);

  function handleCancel() {
    resetForm();
    router.push("/water-bodies");
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (loadingWaterBodies || waterBodiesError || !selectedWaterBody) return;

    if (!canSubmit()) {
      alert("Preencha os dados gerais obrigatórios antes de salvar.");
      return;
    }

    alert("Análise salva com sucesso.");
    router.push("/water-bodies");
  }

  function handleImageChange(file: File) {
    const accepted = updateImage(file);

    if (!accepted) {
      alert("Envie uma imagem nos formatos PNG, JPG ou JPEG.");
    }

    return accepted;
  }

  return (
    <div className="min-h-screen bg-background lg:flex">
      <SideBar variant="analysis-registration" activeHref="/add-analysis" />

      <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
        <form
          onSubmit={handleSubmit}
          className="mx-auto flex w-full max-w-[1500px] flex-col gap-6 lg:gap-7"
        >
          <header className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-start">
            <div className="min-w-0">
              <h1 className="font-heading text-[28px] font-bold leading-tight text-text-primary sm:text-[32px]">
                Adicionar análise
              </h1>
              <p className="mt-2 max-w-3xl font-heading text-[12px] font-medium leading-relaxed text-text-secondary sm:text-[13px]">
                Cadastre uma nova análise para um corpo hídrico e acompanhe os
                indicadores de monitoramento.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-3 sm:flex sm:justify-end">
              <button
                type="button"
                onClick={handleCancel}
                className="inline-flex h-12 items-center justify-center rounded-md border border-placeholder bg-white px-6 font-heading text-[12px] font-bold text-text-primary transition-colors hover:border-primary hover:text-primary sm:min-w-32"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={loadingWaterBodies || Boolean(waterBodiesError) || !selectedWaterBody}
                className="inline-flex h-12 items-center justify-center rounded-md bg-primary px-6 font-heading text-[12px] font-bold text-white shadow-[0_7px_18px_rgba(23,166,191,0.25)] transition-colors hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-60 sm:min-w-36"
              >
                Salvar análise
              </button>
            </div>
          </header>

          <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px] 2xl:grid-cols-[minmax(0,1fr)_390px]">
            <div className="flex min-w-0 flex-col gap-5">
              <AddAnalysisFormSection
                title="Dados gerais"
                description="Informe os dados básicos da análise."
                icon={LuFileText}
              >
                <div className="grid gap-x-6 gap-y-5 md:grid-cols-2 xl:grid-cols-3">
                  <div className="min-w-0" aria-busy={loadingWaterBodies}>
                    <AddAnalysisField
                      label="Corpo hídrico"
                      name="waterBody"
                      value={selectedWaterBody?.value ?? ""}
                      onChange={updateField}
                      placeholder={loadingWaterBodies ? "Carregando corpos hídricos..." : "Selecione um corpo hídrico"}
                      options={waterBodyOptions}
                      disabled={loadingWaterBodies || Boolean(waterBodiesError) || waterBodyOptions.length === 0}
                      error={waterBodiesError ?? undefined}
                      required
                    />
                    {waterBodiesError && (
                      <button type="button" onClick={retryWaterBodies} className="mt-2 rounded text-xs font-semibold text-primary underline underline-offset-4 focus-visible:outline-primary">
                        Tentar novamente
                      </button>
                    )}
                    {!loadingWaterBodies && !waterBodiesError && waterBodyOptions.length === 0 && (
                      <div className="mt-2 text-xs text-text-secondary">
                        <p role="status">Nenhum corpo hídrico cadastrado.</p>
                        <Link href="/add-water-bodie" className="mt-1 inline-flex rounded font-semibold text-primary underline underline-offset-4 focus-visible:outline-primary">
                          Adicionar corpo hídrico
                        </Link>
                      </div>
                    )}
                  </div>
                  <AddAnalysisField
                    label="Data da análise"
                    name="analysisDate"
                    value={form.analysisDate}
                    onChange={updateField}
                    type="date"
                  />
                  <AddAnalysisField
                    label="Responsável"
                    name="responsible"
                    value={form.responsible}
                    onChange={updateField}
                    placeholder="Selecione o responsável"
                    options={responsibleOptions}
                    className="xl:col-span-1"
                  />
                </div>
              </AddAnalysisFormSection>

              <AddAnalysisFormSection
                title="Indicadores da água"
                description="Informe os principais parâmetros medidos em campo."
                icon={LuDroplet}
              >
                <div className="grid gap-x-6 gap-y-5 sm:grid-cols-2 xl:grid-cols-3">
                  {indicatorFields.map((field) => (
                    <AddAnalysisField
                      key={field.name}
                      label={field.label}
                      name={field.name}
                      value={form[field.name]}
                      onChange={updateField}
                      placeholder={field.placeholder}
                      helper={field.helper}
                      inputMode={field.inputMode}
                    />
                  ))}
                </div>
              </AddAnalysisFormSection>

              <AddAnalysisFormSection
                title="Observações"
                description="Registre as condições observadas e o diagnóstico inicial."
                icon={LuMessageCircle}
              >
                <div className="grid gap-x-6 gap-y-5 lg:grid-cols-[minmax(220px,280px)_minmax(0,1fr)]">
                  <AddAnalysisField
                    label="Condição visual da água"
                    name="visualCondition"
                    value={form.visualCondition}
                    onChange={updateField}
                    placeholder="Selecione a condição"
                    options={visualConditionOptions}
                  />
                  <AddAnalysisField
                    label="Observações"
                    name="observations"
                    value={form.observations}
                    onChange={updateField}
                    placeholder="Descreva cor, odor, presença de matéria orgânica e demais observações relevantes."
                    isTextarea
                    maxLength={500}
                  />
                  <AddAnalysisField
                    label="Diagnóstico manual (opcional)"
                    name="manualDiagnosis"
                    value={form.manualDiagnosis}
                    onChange={updateField}
                    placeholder="Indique observações adicionais, atenção para turbidez e pH próximo do limite inferior, etc."
                    isTextarea
                    rows={3}
                    maxLength={500}
                    className="lg:col-span-2"
                  />
                </div>
              </AddAnalysisFormSection>
            </div>

            <aside className="flex min-w-0 flex-col gap-5 xl:sticky xl:top-6 xl:self-start">
              <AddAnalysisSummary form={form} waterBodyLabel={selectedWaterBody?.label ?? ""} />
              <AnalysisImageCard
                imageName={form.imageName}
                imagePreviewUrl={form.imagePreviewUrl}
                onImageChange={handleImageChange}
                onRemoveImage={removeImage}
              />
            </aside>
          </section>
        </form>
      </main>
    </div>
  );
}
