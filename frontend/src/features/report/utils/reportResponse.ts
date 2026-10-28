export async function readReportPdf(response: Response): Promise<Blob> {
  if (!response.ok) {
    if (response.status === 401) throw new Error("Sua sessão expirou. Entre novamente para gerar o relatório.");
    if (response.status === 403) throw new Error("Você não tem permissão para gerar este relatório.");
    if (response.status === 404) throw new Error("A geração de relatórios ainda não está disponível neste ambiente.");
    let detail: unknown;
    try { detail = (await response.json())?.detail; } catch { /* Respostas inesperadas não são exibidas como HTML. */ }
    throw new Error(typeof detail === "string" ? detail.slice(0, 400) : "Não foi possível gerar o relatório. Tente novamente.");
  }
  if (!response.headers.get("content-type")?.includes("application/pdf")) throw new Error("O servidor retornou um documento inválido.");
  const blob = await response.blob();
  if (await blob.slice(0, 5).text() !== "%PDF-") throw new Error("O PDF recebido está vazio ou inválido.");
  return blob;
}
