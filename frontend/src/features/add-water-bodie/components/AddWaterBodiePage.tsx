"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import {
  LuCircleCheck,
  LuFileText,
  LuMapPin,
} from "react-icons/lu";

import { SideBar } from "@/components/layout/SideBar";
import { Button } from "@/components/ui/Button";
import { FormField } from "@/components/ui/FormField";
import { FormSectionCard } from "@/components/ui/FormSectionCard";
import { ImageUploadCard } from "@/components/ui/ImageUploadCard";
import { CollectionPointsCard } from "@/features/add-water-bodie/components/CollectionPointsCard";
import { WaterBodySummary } from "@/features/add-water-bodie/components/WaterBodySummary";
import {
  brazilianStateOptions,
  waterBodyTypeOptions,
} from "@/features/add-water-bodie/constants/addWaterBodieOptions";
import { useAddWaterBodieForm } from "@/features/add-water-bodie/hooks/useAddWaterBodieForm";
import type {
  AddWaterBodieErrors,
  AddWaterBodieFormValues,
  CollectionPoint,
} from "@/features/add-water-bodie/types/addWaterBodie";

type SubmissionFeedback =
  | { type: "error" | "success"; message: string }
  | null;

export function AddWaterBodiePage() {
  const router = useRouter();
  const [isSaving, setIsSaving] = useState(false);
  const [submissionFeedback, setSubmissionFeedback] =
    useState<SubmissionFeedback>(null);
  const {
    form,
    errors,
    imageError,
    collectionPoints,
    updateField,
    validateForm,
    updateImage,
    removeImage,
    removeCollectionPoint,
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

    setIsSaving(true);

    const savedWaterBody = await simulateWaterBodySave({
      ...form,
      collectionPoints,
    });

    setSubmissionFeedback({
      type: "success",
      message: `${savedWaterBody.name} foi cadastrado com sucesso. Abrindo uma nova análise...`,
    });

    await wait(900);
    router.push("/add-analysis");
  }

  function handleAddPoint() {
    window.alert(
      "O formulário de criação de pontos será conectado nesta ação em uma próxima etapa."
    );
  }

  function handleEditPoint(point: CollectionPoint) {
    window.alert(
      `A edição de “${point.name}” será conectada nesta ação em uma próxima etapa.`
    );
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
                disabled={isSaving}
                className="px-4 sm:min-w-32 sm:px-6"
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                isLoading={isSaving}
                className="px-4 sm:min-w-44 sm:px-6"
              >
                {isSaving ? "Salvando..." : "Salvar corpo hídrico"}
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
                    id="water-body-type"
                    label="Tipo de corpo hídrico"
                    name="type"
                    value={form.type}
                    onChange={(value) => updateField("type", value)}
                    placeholder="Selecione o tipo"
                    options={waterBodyTypeOptions}
                    required
                    error={errors.type}
                  />
                  <FormField
                    id="water-body-city"
                    label="Cidade"
                    name="city"
                    value={form.city}
                    onChange={(value) => updateField("city", value)}
                    placeholder="Ex.: Niterói"
                    autoComplete="address-level2"
                    required
                    error={errors.city}
                  />
                  <FormField
                    id="water-body-state"
                    label="Estado"
                    name="state"
                    value={form.state}
                    onChange={(value) => updateField("state", value)}
                    placeholder="Selecione o estado"
                    options={brazilianStateOptions}
                    required
                    error={errors.state}
                  />
                </div>
              </FormSectionCard>

              <FormSectionCard
                title="Localização e contexto"
                description="Informe onde o corpo hídrico está localizado e forneça contexto adicional."
                icon={LuMapPin}
              >
                <div className="grid gap-x-6 gap-y-5 md:grid-cols-2">
                  <FormField
                    id="water-body-location"
                    label="Localização ou referência"
                    name="location"
                    value={form.location}
                    onChange={(value) => updateField("location", value)}
                    placeholder="Ex.: Próximo à Ponte Rio-Niterói, bairro Icaraí"
                  />
                  <FormField
                    id="water-body-basin"
                    label="Bacia hidrográfica (opcional)"
                    name="basin"
                    value={form.basin}
                    onChange={(value) => updateField("basin", value)}
                    placeholder="Ex.: Bacia do Rio Paraíba do Sul"
                  />
                  <FormField
                    id="water-body-description"
                    label="Descrição / observações (opcional)"
                    name="description"
                    value={form.description}
                    onChange={(value) => updateField("description", value)}
                    placeholder="Forneça informações relevantes sobre o corpo hídrico, como características, uso da água, pontos de interesse ou qualquer observação importante."
                    isTextarea
                    rows={4}
                    maxLength={500}
                    className="md:col-span-2"
                  />
                </div>
              </FormSectionCard>

              <CollectionPointsCard
                points={collectionPoints}
                onAddPoint={handleAddPoint}
                onEditPoint={handleEditPoint}
                onRemovePoint={removeCollectionPoint}
              />
            </div>

            <aside className="flex min-w-0 flex-col gap-5 xl:sticky xl:top-6 xl:self-start">
              <WaterBodySummary
                form={form}
                collectionPointCount={collectionPoints.length}
              />
              <ImageUploadCard
                inputId="water-body-image-upload"
                title="Imagem do corpo hídrico"
                description="Faça upload de uma imagem para identificar o corpo hídrico."
                imageName={form.imageName}
                imagePreviewUrl={form.imagePreviewUrl}
                onImageChange={updateImage}
                onRemoveImage={removeImage}
                error={imageError}
              />
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

async function simulateWaterBodySave(
  data: AddWaterBodieFormValues & { collectionPoints: CollectionPoint[] }
) {
  await wait(900);

  return {
    id: `water-body-${Date.now()}`,
    ...data,
  };
}

function wait(milliseconds: number) {
  return new Promise((resolve) => window.setTimeout(resolve, milliseconds));
}
