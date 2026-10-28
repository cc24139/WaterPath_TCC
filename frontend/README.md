This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Acesso pela rede local

O Next.js valida a origem das requisições aos recursos de desenvolvimento, incluindo a conexão de atualização automática (`/_next/webpack-hmr`). O IP `192.168.1.24` está permitido em `next.config.ts` para o ambiente informado pela equipe. Se o endereço da máquina mudar, atualize essa entrada usando apenas o IP ou hostname, sem protocolo ou porta, e reinicie `npm run dev`.

Um `GET / 404` deve ser investigado separadamente desse bloqueio. A home deste projeto fica em `src/app/(public)/(home)/page.tsx` e o proxy permite acesso público a `/`. Se o erro ocorrer na máquina de outro integrante, confirme que esse arquivo e a versão atual do projeto estão presentes e teste também `http://localhost:3000` após reiniciar o servidor.

Essa configuração libera os recursos de desenvolvimento do Next; não altera o endereço da API nem habilita APIs do navegador que exigem HTTPS ao acessar um IP da rede por HTTP.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
