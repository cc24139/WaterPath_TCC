"use client";

import { type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LuDroplet, LuFileText, LuFlaskConical } from "react-icons/lu";
import { SideBar } from "@/components/layout/SideBar";
import { Button } from "@/components/ui/Button";
import { FormField } from "@/components/ui/FormField";
import { useAuthSession } from "@/features/auth/hooks/useAuthSession";
import { AddAnalysisField } from "./AddAnalysisField";
import { AddAnalysisFormSection } from "./AddAnalysisFormSection";
import { AddAnalysisSummary } from "./AddAnalysisSummary";
import { AnalysisImageCard } from "./AnalysisImageCard";
import { HeavyMetalsFields } from "./HeavyMetalsFields";
import { AnalysisResult } from "./AnalysisResult";
import { indicatorFields } from "../constants/addAnalysisOptions";
import { useAddAnalysisForm } from "../hooks/useAddAnalysisForm";
import { useWaterBodyOptions } from "../hooks/useWaterBodyOptions";

export function AddAnalysisPage() {
  const router = useRouter();
  const { user } = useAuthSession();
  const { options: waterBodyOptions, loading: loadingWaterBodies, error: waterBodiesError, retry: retryWaterBodies } = useWaterBodyOptions();
  const { form, errors, image, imagePreviewUrl, updateField, toggleMetal, updateMetal, updateImage, removeImage,
    resetForm, save, savedCollectionId, prediction, status, feedback, isBusy } = useAddAnalysisForm();
  const selectedWaterBody = waterBodyOptions.find((option) => option.value === form.waterBody);
  const cannotSubmit = isBusy || loadingWaterBodies || Boolean(waterBodiesError) || !user || !selectedWaterBody;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (cannotSubmit) return;
    const submitter = (event.nativeEvent as SubmitEvent).submitter as HTMLButtonElement | null;
    await save(submitter?.value === "analyze");
    router.refresh();
  }

  return (
    <div className="min-h-screen bg-background lg:flex">
      <SideBar variant="analysis-registration" activeHref="/add-analysis" />
      <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">
        <form onSubmit={handleSubmit} noValidate aria-busy={isBusy} className="mx-auto flex w-full max-w-[1500px] flex-col gap-6 lg:gap-7">
          <header className="flex flex-col gap-4">
            <div>
              <h1 className="font-heading text-[28px] font-bold leading-tight text-text-primary sm:text-[32px]">Adicionar análise</h1>
              <p className="mt-2 max-w-3xl font-heading text-xs font-medium leading-relaxed text-text-secondary sm:text-[13px]">Cadastre os indicadores e os metais medidos em uma coleta. Adicione uma imagem para analisar com IA.</p>
            </div>
            <div className="flex flex-wrap gap-3 lg:justify-end">
              <Button variant="ghost" disabled={isBusy} onClick={() => router.push("/water-bodies")}>{savedCollectionId ? "Voltar" : "Cancelar"}</Button>
              {savedCollectionId && <Button variant="outline" disabled={isBusy} onClick={resetForm}>Nova análise</Button>}
              {!savedCollectionId && <Button type="submit" name="action" value="save" variant="outline" disabled={cannotSubmit} isLoading={status === "saving"}>Salvar coleta</Button>}
              {!prediction && <Button type="submit" name="action" value="analyze" disabled={cannotSubmit || !image} isLoading={status === "analyzing"}>{status === "analyzing" ? "Analisando..." : savedCollectionId ? "Analisar com IA" : "Salvar e analisar com IA"}</Button>}
            </div>
          </header>

          {feedback && <div role={feedback.type === "error" ? "alert" : "status"} className={`rounded-lg border px-4 py-3 font-heading text-xs leading-relaxed ${feedback.type === "error" ? "border-contrast/35 bg-contrast/10 text-text-primary" : "border-success/30 bg-success/10 text-text-primary"}`}>
            <p>{feedback.message}</p>
            {savedCollectionId && <Link href={`/water-bodies/${form.waterBody}`} className="mt-2 inline-flex font-bold text-primary underline underline-offset-4">Ver monitoramento do corpo hídrico</Link>}
          </div>}

          <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px] 2xl:grid-cols-[minmax(0,1fr)_390px]">
            <div className="flex min-w-0 flex-col gap-5">
              {savedCollectionId && <p className="font-heading text-xs text-text-secondary">Os dados da coleta #{savedCollectionId} já foram salvos. Para cadastrar outra amostra, selecione “Nova análise”.</p>}
              <fieldset disabled={isBusy || Boolean(savedCollectionId)} className="flex min-w-0 flex-col gap-5">
                <legend className="sr-only">Dados da coleta e medições</legend>
                <AddAnalysisFormSection title="Dados gerais" description="Campos com * são obrigatórios." icon={LuFileText}>
                  <div className="grid gap-x-6 gap-y-5 md:grid-cols-2 xl:grid-cols-3">
                    <div className="min-w-0" aria-busy={loadingWaterBodies}>
                      <AddAnalysisField label="Corpo hídrico *" name="waterBody" value={selectedWaterBody?.value ?? ""} onChange={updateField}
                        placeholder={loadingWaterBodies ? "Carregando..." : "Selecione um corpo hídrico"} options={waterBodyOptions}
                        disabled={loadingWaterBodies || Boolean(waterBodiesError) || !waterBodyOptions.length} error={waterBodiesError ?? errors.waterBody} required />
                      {waterBodiesError && <button type="button" onClick={retryWaterBodies} className="mt-2 rounded text-xs font-semibold text-primary underline underline-offset-4">Tentar novamente</button>}
                      {!loadingWaterBodies && !waterBodiesError && !waterBodyOptions.length && <div className="mt-2 text-xs text-text-secondary"><p>Nenhum corpo hídrico cadastrado.</p><Link href="/add-water-bodie" className="font-semibold text-primary underline">Adicionar corpo hídrico</Link></div>}
                    </div>
                    <AddAnalysisField label="Data e hora da coleta *" name="analysisDate" value={form.analysisDate} onChange={updateField} type="datetime-local" helper="Informe o horário local da coleta." error={errors.analysisDate} required />
                    <FormField id="analysis-responsible" name="responsible" label="Responsável pelo cadastro" value={user?.name ?? ""} onChange={() => {}} disabled helper="Identificado pela sua conta." />
                  </div>
                </AddAnalysisFormSection>
                <AddAnalysisFormSection title="Indicadores da água" description="Informe os parâmetros medidos e confira as unidades." icon={LuDroplet}>
                  {[true, false].map((required) => <div key={String(required)} className={required ? "" : "mt-6 border-t border-placeholder pt-5"}>
                    {!required && <h3 className="mb-4 font-heading text-xs font-bold text-text-primary">Parâmetros complementares · opcionais</h3>}
                    <div className="grid gap-x-6 gap-y-5 sm:grid-cols-2 xl:grid-cols-3">
                      {indicatorFields.filter((field) => field.required === required).map((field) => <AddAnalysisField key={field.name}
                        label={`${field.label}${field.name === "ph" ? "" : ` (${field.unit})`}${field.required ? " *" : ""}`}
                        name={field.name} value={form[field.name]} onChange={updateField} placeholder={field.placeholder}
                        inputMode="decimal" required={field.required} error={errors[field.name]} />)}
                    </div>
                  </div>)}
                </AddAnalysisFormSection>
                <AddAnalysisFormSection title="Metais pesados" description="Opcional. Informe somente os metais para os quais você possui resultados de medição." icon={LuFlaskConical}>
                  <HeavyMetalsFields metals={form.metals} errors={errors} onToggle={toggleMetal} onChange={updateMetal} />
                </AddAnalysisFormSection>
              </fieldset>
              {prediction && <AnalysisResult prediction={prediction} />}
            </div>
            <aside className="flex min-w-0 flex-col gap-5 xl:sticky xl:top-6 xl:self-start">
              <AddAnalysisSummary form={form} waterBodyLabel={selectedWaterBody?.label ?? ""} responsible={user?.name ?? ""} imageName={image?.name ?? ""} savedCollectionId={savedCollectionId} />
              <fieldset disabled={isBusy || Boolean(prediction)} className="min-w-0">
                <legend className="sr-only">Imagem para análise com IA</legend>
                <AnalysisImageCard imageName={image?.name ?? ""} imagePreviewUrl={imagePreviewUrl} onImageChange={updateImage} onRemoveImage={removeImage} error={errors.image} />
              </fieldset>
              <p className="font-heading text-[11px] leading-relaxed text-text-secondary">A imagem é necessária para analisar com IA. Ao selecionar apenas “Salvar coleta”, a imagem não será enviada.</p>
            </aside>
          </section>
        </form>
      </main>
    </div>
  );
}
