"use client";

import { type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LuDroplet, LuFileText, LuFlaskConical, LuMapPin } from "react-icons/lu";
import { SideBar } from "@/components/layout/SideBar";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
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

export function AddAnalysisPage({ resumeCollectionId = null, waterBodyId = null }: { resumeCollectionId?: number | null; waterBodyId?: number | null }) {
  const router = useRouter();
  const { user, isReady } = useAuthSession();
  const { options: waterBodyOptions, loading: loadingWaterBodies, error: waterBodiesError, retry: retryWaterBodies } = useWaterBodyOptions();
  const { form, errors, image, imagePreviewUrl, updateField, toggleMetal, updateMetal, updateImage, removeImage,
    resetForm, save, savedCollectionId, prediction, status, feedback, isBusy } = useAddAnalysisForm(resumeCollectionId, waterBodyId);
  const selectedWaterBody = waterBodyOptions.find((option) => option.value === form.waterBody);
  const cannotSubmit = isBusy || loadingWaterBodies || Boolean(waterBodiesError) || !user || !selectedWaterBody;
  const monitoringPath = waterBodyId ? `/water-bodies/${waterBodyId}` : null;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (cannotSubmit) return;
    const submitter = (event.nativeEvent as SubmitEvent).submitter as HTMLButtonElement | null;
    await save(submitter?.value === "analyze");
    if (!waterBodyId) router.refresh();
  }

  if (waterBodyId && isReady && !user) return <Card className="space-y-3 !p-6">
    <h1 className="font-heading text-xl font-bold">Entre para inserir dados</h1>
    <p className="text-sm text-text-secondary">É necessário entrar na sua conta para registrar uma coleta neste corpo hídrico.</p>
    <Link href={`/login?next=${encodeURIComponent(`${monitoringPath}/insert`)}`} className="inline-block text-sm font-semibold text-primary underline">Entrar</Link>
  </Card>;
  if (waterBodyId && isReady && user && !loadingWaterBodies && !selectedWaterBody) return <Card className="space-y-3 !p-6">
    <h1 className="font-heading text-xl font-bold">Cadastro indisponível</h1>
    <p className="text-sm text-text-secondary">{waterBodiesError ?? "Este corpo hídrico não está disponível para cadastro de coletas nesta conta."}</p>
    {waterBodiesError && <Button variant="outline" onClick={retryWaterBodies}>Tentar novamente</Button>}
    <div><Link href={`${monitoringPath}/dashboard`} className="text-sm font-semibold text-primary underline">Voltar ao dashboard</Link></div>
  </Card>;

  const content = (
        <form onSubmit={handleSubmit} noValidate aria-busy={isBusy} className="mx-auto flex w-full max-w-[1500px] flex-col gap-6 lg:gap-7">
          <header className="flex flex-col gap-4">
            <div>
              {waterBodyId && <p className="mb-1 text-[11px] font-bold uppercase tracking-[0.16em] text-primary">Nova coleta · monitoramento</p>}
              <h1 className="font-heading text-[28px] font-bold leading-tight text-text-primary sm:text-[32px]">{waterBodyId ? "Inserir dados" : "Adicionar análise"}</h1>
              <p className="mt-2 max-w-3xl font-heading text-xs font-medium leading-relaxed text-text-secondary sm:text-[13px]">{waterBodyId
                ? "Registre as medições e escolha entre salvar a coleta ou anexar uma imagem para salvar e analisar com IA."
                : "Cadastre os indicadores e os metais medidos em uma coleta. Adicione uma imagem para analisar com IA."}</p>
              {waterBodyId && <p className="mt-4 inline-flex max-w-full items-center gap-2 rounded-lg border border-primary/15 bg-white px-3 py-2 text-xs font-semibold text-text-primary"><LuMapPin aria-hidden="true" className="shrink-0 text-primary" /><span className="break-words">{selectedWaterBody?.label ?? "Carregando corpo hídrico..."}</span></p>}
            </div>
            <div className="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:gap-3 lg:justify-end">
              <Button variant="ghost" className="w-full sm:w-auto" disabled={isBusy} onClick={() => router.push(monitoringPath ? `${monitoringPath}/dashboard` : "/water-bodies")}>{savedCollectionId ? waterBodyId ? "Voltar ao rio" : "Voltar" : "Cancelar"}</Button>
              {savedCollectionId && <Button variant="outline" className="w-full sm:w-auto" disabled={isBusy} onClick={() => { resetForm(); if (!waterBodyId) router.replace("/add-analysis"); }}>{waterBodyId ? "Nova coleta" : "Nova análise"}</Button>}
              {!savedCollectionId && <Button type="submit" name="action" value="save" variant={waterBodyId ? "primary" : "outline"} className="w-full sm:w-auto" disabled={cannotSubmit} isLoading={status === "saving"}>Salvar coleta</Button>}
              {!prediction && <Button type="submit" name="action" value="analyze" className="w-full sm:w-auto" disabled={cannotSubmit || !image} isLoading={status === "analyzing"}>{status === "analyzing" ? savedCollectionId ? "Analisando..." : "Salvando e analisando..." : savedCollectionId ? "Analisar com IA" : "Salvar e analisar com IA"}</Button>}
            </div>
          </header>

          {feedback && <div role={feedback.type === "error" ? "alert" : "status"} className={`rounded-lg border px-4 py-3 font-heading text-xs leading-relaxed ${feedback.type === "error" ? "border-contrast/35 bg-contrast/10 text-text-primary" : "border-success/30 bg-success/10 text-text-primary"}`}>
            <p>{feedback.message}</p>
            {savedCollectionId && <Link href={`/water-bodies/${form.waterBody}/${waterBodyId ? "history" : "dashboard"}`} className="mt-2 inline-flex font-bold text-primary underline underline-offset-4">{waterBodyId ? "Ver histórico do rio" : "Ver monitoramento do corpo hídrico"}</Link>}
          </div>}

          <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_360px] 2xl:grid-cols-[minmax(0,1fr)_390px]">
            <div className="flex min-w-0 flex-col gap-5">
              {savedCollectionId && <p className="rounded-lg border border-success/25 bg-success/5 px-4 py-3 font-heading text-xs leading-relaxed text-text-secondary">A coleta #{savedCollectionId} está salva. {waterBodyId ? "Ela já faz parte do histórico; a análise com IA é opcional." : "Para cadastrar outra amostra, selecione “Nova análise”."}</p>}
              <fieldset disabled={isBusy || Boolean(savedCollectionId)} className="flex min-w-0 flex-col gap-5">
                <legend className="sr-only">Dados da coleta e medições</legend>
                <AddAnalysisFormSection title="Dados gerais" description="Campos com * são obrigatórios." icon={LuFileText}>
                  <div className="grid gap-x-6 gap-y-5 md:grid-cols-2 xl:grid-cols-3">
                    <div className="min-w-0" aria-busy={loadingWaterBodies}>
                      {waterBodyId ? <FormField id="insert-water-body" name="waterBody" label="Corpo hídrico" value={selectedWaterBody?.label ?? "Carregando..."} onChange={() => {}} disabled helper="Definido pelo rio que você está monitorando." /> : <><AddAnalysisField label="Corpo hídrico *" name="waterBody" value={selectedWaterBody?.value ?? ""} onChange={updateField}
                        placeholder={loadingWaterBodies ? "Carregando..." : "Selecione um corpo hídrico"} options={waterBodyOptions}
                        disabled={loadingWaterBodies || Boolean(waterBodiesError) || !waterBodyOptions.length} error={waterBodiesError ?? errors.waterBody} required />
                      {waterBodiesError && <button type="button" onClick={retryWaterBodies} className="mt-2 rounded text-xs font-semibold text-primary underline underline-offset-4">Tentar novamente</button>}
                      {!loadingWaterBodies && !waterBodiesError && !waterBodyOptions.length && <div className="mt-2 text-xs text-text-secondary"><p>Nenhum rio público ou vinculado à sua conta disponível.</p><Link href="/add-water-bodie" className="font-semibold text-primary underline">Adicionar corpo hídrico</Link></div>}
                      </>}
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
              <p className="font-heading text-[11px] leading-relaxed text-text-secondary">A imagem é necessária somente para analisar com IA. Ao escolher “Salvar coleta”, ela não é enviada. Se a IA falhar, a coleta permanece salva.</p>
            </aside>
          </section>
        </form>
  );
  if (waterBodyId) return content;
  return <div className="min-h-screen bg-background lg:flex">
    <SideBar variant="analysis-registration" activeHref="/add-analysis" />
    <main className="min-w-0 flex-1 px-4 py-5 sm:px-6 lg:px-8 xl:px-10">{content}</main>
  </div>;
}
