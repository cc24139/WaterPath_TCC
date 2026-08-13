import { type MiddlewareConfig, NextRequest, NextResponse } from "next/server";

type PublicRoute = {
  path: string;
  whenAuthenticated: "next" | "redirect";
  match?: "exact" | "prefix";
};

const publicRoutes: PublicRoute[] = [
  { path: "/register", whenAuthenticated: "redirect" },
  { path: "/login", whenAuthenticated: "redirect" },
  { path: "/", whenAuthenticated: "next" },
  { path: "/about-us", whenAuthenticated: "next" },
  { path: "/methodology", whenAuthenticated: "next" },
  { path: "/add-analysis", whenAuthenticated: "next" }, // Só pra fazer a tela, depois excluir
  { path: "/add-water-bodie", whenAuthenticated: "next" },
  { path: "/water-bodies", whenAuthenticated: "next", match: "prefix" },
];

const redirectWhenNotAuthenticatedRoute = "/login";
const redirectWhenAuthenticatedRoute = "/";
const authCookieName = "token";

export function proxy(request: NextRequest) {
  const path = request.nextUrl.pathname;
  const publicRoute = findPublicRoute(path);
  const authToken = request.cookies.get(authCookieName)?.value;

  if (authToken && isJwtExpired(authToken)) {
    return handleExpiredSession(request, publicRoute);
  }

  if (!authToken && publicRoute) {
    return NextResponse.next();
  }

  if (!authToken && !publicRoute) {
    return redirectTo(request, redirectWhenNotAuthenticatedRoute);
  }

  if (authToken && publicRoute?.whenAuthenticated === "redirect") {
    return redirectTo(request, redirectWhenAuthenticatedRoute);
  }

  return NextResponse.next();
}

function findPublicRoute(path: string) {
  return publicRoutes.find((route) => {
    if (route.match === "prefix") {
      return path === route.path || path.startsWith(`${route.path}/`);
    }

    return path === route.path;
  });
}

function handleExpiredSession(
  request: NextRequest,
  publicRoute: PublicRoute | undefined
) {
  const response = publicRoute
    ? NextResponse.next()
    : redirectTo(request, redirectWhenNotAuthenticatedRoute);

  response.cookies.delete(authCookieName);

  return response;
}

function redirectTo(request: NextRequest, pathname: string) {
  const redirectUrl = request.nextUrl.clone();
  redirectUrl.pathname = pathname;

  return NextResponse.redirect(redirectUrl);
}

function isJwtExpired(token: string) {
  const expiresAt = getJwtExpiresAt(token);

  return Boolean(expiresAt && expiresAt <= Date.now() / 1000);
}

function getJwtExpiresAt(token: string) {
  try {
    const [, payload] = token.split(".");

    if (!payload) {
      return null;
    }

    const normalizedPayload = normalizeBase64Url(payload);
    const decodedPayload = JSON.parse(atob(normalizedPayload)) as {
      exp?: unknown;
    };

    return typeof decodedPayload.exp === "number" ? decodedPayload.exp : null;
  } catch {
    return null;
  }
}

function normalizeBase64Url(value: string) {
  const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
  const padding = "=".repeat((4 - (base64.length % 4)) % 4);

  return `${base64}${padding}`;
}

export const config: MiddlewareConfig = {
  matcher: [
    /*
     * Match all request paths except for:
     * - api routes
     * - Next.js internals
     * - favicon
     * - public files with extensions, such as images and SVGs
     */
    "/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)",
  ],
};
