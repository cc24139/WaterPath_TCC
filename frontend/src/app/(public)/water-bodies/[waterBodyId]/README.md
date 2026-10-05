**Guia de implementação da área de um corpo hídrico**

**Estado da implementação — etapas 1 a 4 concluídas**

- O layout agora compartilha sidebar e área principal. `MonitoringSidebar` preserva o ID nos links; o relatório mantém seu conteúdo em `WaterBodyReport`, sem duplicar sidebar ou `<main>`.
- `loading.tsx` e `not-found.tsx` reutilizam componentes da feature. `error.tsx` mantém as ações de recuperação.
- `parseWaterBodyId` valida IDs positivos de 32 bits em todas as páginas desta área. A entrada redireciona para o dashboard.
- O dashboard consulta os detalhes no cliente com `useGetById`: trata carregamento, sucesso, 401, 403, 404 e falhas inesperadas, cancela requisições ao sair e ignora respostas antigas. A consulta de detalhes não redireciona automaticamente no 401; a interface oferece login com retorno ao dashboard.
- O 404 recebido no navegador mostra o componente de recurso não encontrado como estado local; ele não altera o status HTTP da página. IDs inválidos são tratados com `notFound()` no servidor.
- O dashboard de sucesso ainda contém apenas um título. Dados de apresentação, medições, gráficos, integração das outras telas e proteção da inserção permanecem nas próximas etapas.

Os exemplos abaixo documentam a proposta original e alternativas de integração; não representam todos os arquivos atuais. Validação desta entrega: lint sem erros (um aviso preexistente em `api/routes.tsx`), build concluído e 13 testes aprovados. A integração com a API em execução ainda precisa de conferência manual com sessões e registros reais.

Este documento orienta a implementação dos arquivos especiais desta pasta. Os exemplos são propostas para copiar e adaptar; a criação deste README não implementa esses comportamentos.

Base do projeto: Next.js 16.2.4, App Router, React 19 e Tailwind CSS. Caminhos iniciados com `src/` são relativos a `frontend/`.

**1. O papel de cada arquivo**

| Arquivo | Responsabilidade | Exemplo no Water Path |
| --- | --- | --- |
| `layout.tsx` | Estrutura compartilhada das páginas filhas | Sidebar, área principal e identificação do corpo hídrico |
| `loading.tsx` | Interface temporária enquanto o conteúdo da rota suspende | Esqueleto de cartões e gráficos |
| `error.tsx` | Recuperação de uma falha inesperada durante a renderização abaixo dele | Falha ao consultar a API ou processar a resposta |
| `not-found.tsx` | Interface para recurso não encontrado | ID inválido ou corpo hídrico removido |
| `page.tsx` | Entrada em `/water-bodies/{id}` | Encaminhamento para o dashboard |

Os quatro arquivos especiais não são endereços separados. Não se navega para `/loading` ou `/error` para usá-los.

Visão simplificada da árvore de renderização:

```text
layout.tsx                         ← sidebar e estrutura
└── limite de erro (error.tsx)
    └── Suspense (loading.tsx)
        └── conteúdo da rota
            ├── página escolhida
            └── not-found.tsx quando houver notFound()
```

O layout envolve os demais arquivos do segmento. Portanto, um erro no próprio layout precisa de um `error.tsx` em um nível acima; o `loading.tsx` desta pasta também não cobre uma busca aguardada diretamente nesse layout. [Referência de layout](https://nextjs.org/docs/app/api-reference/file-conventions/layout).

**2. `layout.tsx`: montar a estrutura compartilhada**

Comece com uma estrutura leve: sidebar, espaçamento e `{children}`. Deixe a primeira busca de dados nas páginas. Assim, a API não precisa responder antes de a estrutura principal ficar disponível.

Este layout pode continuar como Server Component. No Next.js 16, `params` é uma Promise; use `await params`. A sidebar interativa fica em um Client Component separado. [Parâmetros de layout](https://nextjs.org/docs/app/api-reference/file-conventions/layout).

Exemplo para substituir o conteúdo de `layout.tsx`, depois de criar `MonitoringSidebar`:

```tsx
import type { ReactNode } from "react";
import { MonitoringSidebar } from "@/features/river-analysis/components/MonitoringSidebar";

type WaterBodyLayoutProps = {
  children: ReactNode;
  params: Promise<{ waterBodyId: string }>;
};

export default async function WaterBodyLayout({
  children,
  params,
}: WaterBodyLayoutProps) {
  const { waterBodyId } = await params;

  return (
    <div className="min-h-dvh bg-background text-text-primary lg:flex">
      <MonitoringSidebar waterBodyId={waterBodyId} />

      <main id="monitoring-content" className="min-w-0 flex-1 px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto w-full max-w-[1440px]">{children}</div>
      </main>
    </div>
  );
}
```

Exemplo de novo arquivo `src/features/river-analysis/components/MonitoringSidebar.tsx`:

```tsx
"use client";

import {
  SideBar,
  monitoringSideBarSections,
} from "@/components/layout/SideBar";

export function MonitoringSidebar({ waterBodyId }: { waterBodyId: string }) {
  const basePath = `/water-bodies/${encodeURIComponent(waterBodyId)}`;

  const sections = monitoringSideBarSections.map((section) => ({
    ...section,
    items: section.items.map((item) => ({
      ...item,
      href:
        item.href && item.href !== "/water-bodies"
          ? `${basePath}${item.href}`
          : item.href,
    })),
  }));

  return <SideBar variant="monitoring" sections={sections} />;
}
```

Esse adaptador aproveita as opções e os ícones já existentes. Ele assume os links atuais da variante `monitoring`: `/dashboard`, `/history`, `/analysis`, `/report`, `/insert` e `/water-bodies`. Se a configuração mudar, revise o mapeamento; links externos, por exemplo, não devem receber esse prefixo.

Construa `sections` dentro do componente cliente: os itens contêm funções de ícones, e passar esse objeto diretamente de um Server Component para a sidebar criaria um problema de serialização. O layout só precisa passar a string `waterBodyId`.

Pontos específicos do projeto:

- `SideBar` já usa `usePathname()` para identificar o item ativo e já trata o menu móvel.
- O item atual “Sair” volta à listagem; ele não encerra a sessão. “Voltar aos corpos hídricos” é uma opção de rótulo mais explícita.
- `report/page.tsx` já possui sidebar, contêiner externo e `<main>`. Ao adotar o layout acima, remova essas estruturas da página de relatório e mantenha seu conteúdo interno, evitando duas sidebars e dois elementos `<main>` aninhados.
- Os títulos “Dashboard”, “Histórico” e “Relatório” pertencem às próprias páginas. Use um `<h1>` que descreva o conteúdo de cada uma.
- Um futuro `WaterBodyHeader` pode mostrar nome e localização. Se buscar dados no servidor dentro do layout, envolva esse componente em um `Suspense` próprio.
- Buscas de histórico, previsões e geração de relatório devem ficar nas respectivas funcionalidades, para não atrasar todas as telas.

O layout não injeta automaticamente os dados buscados nas páginas recebidas como `children`. Comece com uma função de consulta reutilizável chamada pelas páginas. Caso adote estado compartilhado no cliente, use um provider delimitado a esta área; ele atende componentes clientes. Filtros como período podem ficar na URL, por exemplo `history?from=2026-09-01&to=2026-09-12`. [Dados e limites dos layouts](https://nextjs.org/docs/app/api-reference/file-conventions/layout).

**3. `loading.tsx`: mostrar que a tela está carregando**

Use um esqueleto neutro que represente a área de conteúdo. A sidebar já pertence ao layout. Não apresente números fictícios como IQA ou valores de medições durante a espera.

Exemplo para `loading.tsx`:

```tsx
export default function WaterBodyLoading() {
  return (
    <section aria-label="Carregando monitoramento" className="space-y-6">
      <p role="status" className="text-sm text-text-secondary">
        Carregando dados do corpo hídrico...
      </p>

      <div aria-hidden="true" className="space-y-6 motion-safe:animate-pulse">
        <div className="h-8 w-52 rounded bg-primary/10" />
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {Array.from({ length: 4 }, (_, index) => (
            <div key={index} className="h-28 rounded-xl bg-primary/10" />
          ))}
        </div>
        <div className="h-72 rounded-xl bg-primary/10" />
      </div>
    </section>
  );
}
```

`loading.tsx` fornece um fallback de Suspense para o conteúdo abaixo do layout. Não recebe `params` ou outras props. Pode haver um `dashboard/loading.tsx` ou `history/loading.tsx` quando cada tela precisar de um esqueleto próprio. [Referência de loading](https://nextjs.org/docs/app/api-reference/file-conventions/loading).

No código atual, vários hooks fazem buscas no navegador. Uma requisição iniciada em `useEffect` não ativa automaticamente esse fallback. Nesse caminho, o componente da feature precisa renderizar seu estado de carregamento:

```tsx
// Trecho dentro de um componente cliente que usa um hook de consulta.
if (loading) {
  return <p role="status">Carregando medições...</p>;
}
```

Para reutilizar o esqueleto nos dois cenários, extraia-o para `features/river-analysis/components/MonitoringSkeleton.tsx` e importe-o no arquivo especial e na feature. Evite fazer a feature depender de um arquivo de rota.

Para salvar uma coleta ou gerar um relatório, use um estado local como `isSubmitting` ou `isGenerating`. O botão existente em `src/components/ui/Button.tsx` já oferece `isLoading`. Bloqueie envios duplicados e mantenha os valores do formulário durante a tentativa.

**4. `error.tsx`: permitir recuperação de falhas inesperadas**

Precisa de `"use client"`. Mostre uma mensagem compreensível e ações de recuperação. Evite apresentar diretamente `error.message`, stack trace ou o corpo bruto de uma resposta da API. O `digest`, quando disponível, pode ajudar a relacionar a ocorrência aos logs. [Referência de error](https://nextjs.org/docs/app/api-reference/file-conventions/error).

Exemplo para `error.tsx`:

```tsx
"use client";

import Link from "next/link";
import { Button } from "@/components/ui/Button";

type WaterBodyErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function WaterBodyError({ error, reset }: WaterBodyErrorProps) {
  return (
    <section aria-labelledby="monitoring-error-title" className="space-y-4 rounded-xl border border-placeholder p-6">
      <h1 id="monitoring-error-title" className="font-heading text-xl font-bold">
        Não foi possível exibir o monitoramento
      </h1>
      <p role="alert" className="text-sm text-text-secondary">
        Ocorreu uma falha ao carregar esta tela. Tente novamente ou recarregue a página.
      </p>
      {error.digest && (
        <p className="text-xs text-text-secondary">Referência: {error.digest}</p>
      )}
      <div className="flex flex-wrap gap-3">
        <Button onClick={reset}>Tentar novamente</Button>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Recarregar página
        </Button>
      </div>
      <Link href="/water-bodies" className="inline-block text-primary underline">
        Voltar aos corpos hídricos
      </Link>
    </section>
  );
}
```

Compatibilidade: os tipos instalados em `node_modules/next/dist/client/components/error-boundary.d.ts` expõem `reset` e `unstable_retry`. A documentação online mais recente já apresenta `retry`. O exemplo usa `reset`, disponível nesta instalação, sem depender da API experimental.

`reset()` limpa o estado de erro e tenta renderizar novamente; não garante uma nova consulta ao servidor. O botão de recarregar faz uma nova navegação completa, mas perde estado local não salvo. Se implementar recuperação sem recarga, valide a atualização dos dados junto ao reset na versão instalada. [Recuperação de erros](https://nextjs.org/docs/app/api-reference/file-conventions/error).

Onde tratar cada falha:

| Situação | Tratamento sugerido |
| --- | --- |
| Exceção durante a renderização de `dashboard/page.tsx` | `error.tsx` desta área |
| Erro na renderização do próprio `layout.tsx` desta pasta | Limite de erro em um segmento acima, como `water-bodies/error.tsx` |
| Erro em um clique para salvar uma coleta | `try/catch` na ação ou hook e mensagem no formulário |
| Falha assíncrona em uma busca iniciada por `useEffect` | Capturar e guardar um estado de erro no hook |
| API retorna HTTP 500 em um `fetch` | Conferir `response.ok`; o status HTTP sozinho não lança exceção |
| Falta de uma medição opcional | Mostrar indisponibilidade apenas nesse indicador |

Para um hook cliente, você pode exibir um aviso com botão de refazer a consulta. Se decidir que uma falha deve substituir toda a área, capture-a no hook e lance o erro durante a renderização do componente. Não deixe uma Promise rejeitada sem tratamento esperando que `error.tsx` a capture.

**5. `not-found.tsx`: tratar um corpo hídrico inexistente**

Uma sugestão de mensagem é “Corpo hídrico não encontrado”, com uma saída para a listagem. Não dependa do nome do rio nem faça outra busca para montar essa tela.

Exemplo para `not-found.tsx`:

```tsx
import Link from "next/link";

export default function WaterBodyNotFound() {
  return (
    <section aria-labelledby="water-body-not-found-title" className="space-y-4 rounded-xl border border-placeholder p-6">
      <h1 id="water-body-not-found-title" className="font-heading text-xl font-bold">
        Corpo hídrico não encontrado
      </h1>
      <p className="text-sm text-text-secondary">
        Não encontramos um corpo hídrico para este endereço. Ele pode ter sido removido.
      </p>
      <Link href="/water-bodies" className="inline-block text-primary underline">
        Consultar corpos hídricos
      </Link>
    </section>
  );
}
```

O arquivo não recebe props. Ele apresenta a interface quando uma consulta/renderização descendente chama `notFound()`. Uma URL totalmente sem rota, como `/water-bodies/12/qualquer-coisa`, não deve ser usada para comprovar esse tratamento local; a aplicação também pode ter `src/app/not-found.tsx` para endereços sem correspondência. [Referência de not-found](https://nextjs.org/docs/app/api-reference/file-conventions/not-found).

Exemplo mínimo para experimentar em `dashboard/page.tsx`, antes de integrar a API:

```tsx
import { notFound } from "next/navigation";

export default async function DashboardPage({
  params,
}: {
  params: Promise<{ waterBodyId: string }>;
}) {
  const { waterBodyId } = await params;
  const id = Number(waterBodyId);

  // O backend utiliza IDs inteiros positivos de 32 bits.
  if (!/^[1-9]\d*$/.test(waterBodyId) || !Number.isInteger(id) || id > 2147483647) {
    notFound();
  }

  return <h1 className="font-heading text-xl font-bold">Dashboard do corpo hídrico {id}</h1>;
}
```

Esse exemplo valida apenas o formato. Um ID como `999999` ainda depende da consulta à API para confirmar existência. Coloque essa validação em uma função reutilizável quando implementar as outras páginas, pois todas podem ser acessadas diretamente.

Na integração, chame `notFound()` somente para ID inválido ou resposta que confirme ausência. A função interrompe a renderização lançando uma exceção controlada: não a envolva em um `catch` que transforme tudo em um erro genérico. [Funcionamento de notFound](https://nextjs.org/docs/app/api-reference/functions/not-found).

Um corpo hídrico existente sem coletas é um estado vazio, com mensagem como “Nenhuma coleta registrada”. Não é um recurso inexistente. Se a resposta já começou a ser transmitida, a interface de não encontrado pode aparecer com HTTP 200; antes do streaming, pode responder 404. [Status e interface de not-found](https://nextjs.org/docs/app/api-reference/file-conventions/not-found).

**6. Conectar os arquivos à API do projeto**

Existe uma diferença que precisa ser considerada antes de copiar um exemplo de busca no servidor:

- `src/proxy.ts` libera o prefixo `/water-bodies` para visitantes.
- No backend, `GET /api/corpohidrico/{id}` possui `[Authorize]`; a consulta da listagem não possui esse atributo.
- `corpoHidricoServices.getById()` usa `apiFetch`, que busca o token por `getAuthSession()`.
- `getAuthSession()` depende do navegador e retorna `null` no servidor. Chamar esse serviço diretamente em um Server Component não encaminha automaticamente o token do visitante.

Para uma primeira implementação com os hooks existentes, faça a busca em uma feature cliente e trate explicitamente `loading`, erro, autenticação, ausência e sucesso. Revise `useGetById`: ele ainda precisa de tratamento de exceções, `finally` para encerrar o carregamento e distinção entre os status HTTP. Se a consulta estiver em `useEffect`, use cancelamento ou ignore respostas antigas ao trocar de ID.

Para usar busca no servidor, crie uma função separada, por exemplo `src/api/server/getWaterBody.ts`. Leia o cookie `token` com `await cookies()` de `next/headers` e envie `Authorization: Bearer ...` apenas para a API configurada. Use `cache: "no-store"` inicialmente para os dados autenticados. Não passe o token para componentes de apresentação.

Contrato sugerido para essa função, ainda não implementada:

```ts
import type { CorpoHidricoDTO } from "@/api/dtos/corpoHidricoDTO";

type WaterBodyResult =
  | { status: "success"; data: CorpoHidricoDTO }
  | { status: "not-found" }
  | { status: "unauthenticated" }
  | { status: "forbidden" };

// Assinatura proposta:
// getWaterBody(id: number): Promise<WaterBodyResult>
// Falhas inesperadas de rede, HTTP 5xx ou resposta inválida lançam Error.
```

Fluxo sugerido dentro de uma página no servidor, em pseudocódigo:

```text
aguardar params e validar waterBodyId
  ID inválido → notFound()

aguardar getWaterBody(id)
  not-found → notFound()
  unauthenticated → interface de autenticação / fluxo de login
  forbidden → mensagem de acesso negado
  success → renderizar a feature com os dados
  exceção inesperada → error.tsx
```

Não converta 401, 403 ou 500 em “corpo hídrico não encontrado”. Para tornar a consulta de detalhes pública, será necessário definir no backend quais registros e campos podem ser expostos. Trocar apenas a opção de autenticação no frontend não torna um endpoint público.

Antes de conectar a ação de inserir dados, proteja também `/water-bodies/{id}/insert`, hoje alcançada pela regra pública de prefixo. Verifique autorização na API para a operação sobre aquele corpo hídrico. Esconder o item da sidebar é apenas uma decisão de interface.

**7. A entrada `page.tsx` e o fluxo completo**

O botão “Ver Análise” já leva a `/water-bodies/{id}`. Quando o dashboard estiver preparado, a página de entrada pode encaminhar para ele:

```tsx
import { redirect } from "next/navigation";

export default async function WaterBodyPage({
  params,
}: {
  params: Promise<{ waterBodyId: string }>;
}) {
  const { waterBodyId } = await params;
  redirect(`/water-bodies/${encodeURIComponent(waterBodyId)}/dashboard`);
}
```

A página de destino continua responsável por validar o ID e consultar os dados. O redirecionamento não comprova que o registro existe.

**8. Ordem de implementação sugerida**

1. Criar `MonitoringSidebar`, adaptar o layout e retirar a estrutura duplicada do relatório.
2. Implementar as interfaces de `loading`, `error` e `not-found` com os exemplos acima.
3. Preparar o dashboard para validar o ID e adicionar o redirecionamento de entrada.
4. Escolher a busca no cliente ou no servidor e implementar o tratamento dos status da API.
5. Adicionar nome, localização e estado vazio antes dos gráficos reais.
6. Integrar histórico, análise e relatório com o mesmo ID da URL e suas próprias consultas.
7. Implementar inserção com autenticação, autorização e atualização dos dados exibidos após salvar.

**9. Cenários para conferir manualmente**

| Cenário | Como verificar | Resultado esperado |
| --- | --- | --- |
| Entrada pelo cartão | Clicar em “Ver Análise” | Abrir o dashboard do mesmo ID após implementar o redirecionamento |
| Navegação interna | Abrir todas as opções da sidebar | Manter o ID, destacar a opção correta e exibir uma única sidebar |
| Link direto | Recarregar `/water-bodies/12/history` | Consultar o ID da URL sem depender da seleção anterior |
| Carregamento no servidor | Introduzir temporariamente uma espera na página assíncrona em desenvolvimento | Mostrar o esqueleto e preservar a estrutura do layout |
| Carregamento no cliente | Reduzir a velocidade da rede nas ferramentas do navegador | Mostrar o estado local do hook |
| Falha de renderização | Lançar temporariamente `new Error(...)` na página | Mostrar a interface de erro; remover a simulação ao concluir |
| Recuperação | Retirar a causa da falha e usar os botões | Recuperar a renderização ou obter uma nova resposta ao recarregar |
| ID inválido | Abrir `/water-bodies/abc/dashboard` | Mostrar a interface de não encontrado após implementar a validação |
| ID ausente | Usar ID confirmado como inexistente, com consulta autorizada | Tratar o 404 da API com `notFound()` |
| Sem coletas | Usar corpo hídrico existente sem medições | Mostrar um estado vazio e manter as opções de navegação |
| Sem sessão / sem permissão | Consultar o endpoint protegido nas duas situações | Distinguir autenticação de acesso negado |
| Relatório | Abrir `report` depois de adaptar o layout | Preservar o formulário e evitar `<main>` aninhado |
| Acessibilidade | Navegar por teclado e ativar redução de movimento | Manter foco visível, ações acessíveis e esqueleto sem animação obrigatória |

Use as simulações apenas durante desenvolvimento. Depois de implementar, execute `npm run lint` e `npm run build` em `frontend/` para conferir regras do projeto e contratos das rotas. Este README foi escrito a partir do código atual e das referências vinculadas; seus exemplos ainda precisam ser integrados e validados na aplicação.
