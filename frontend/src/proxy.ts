import { type MiddlewareConfig, NextRequest, NextResponse } from "next/server"

const publicRoutes = [
    {path:'/register', whenAuthenticated:'redirect'},
    {path:'/login', whenAuthenticated:'redirect'},
    {path:'/', whenAuthenticated:'next'},
    {path:'/about-us', whenAuthenticated:'next'},
    {path:'/methodology', whenAuthenticated:'next'},
    {path:'/water-bodies', whenAuthenticated:'next'},
] as const

const REDIRECT_WHEN_NOT_AUTHENTICATED_ROUTE = '/login'

export function proxy(request: NextRequest){
    const path = request.nextUrl.pathname
    const publicRoute = publicRoutes.find(route => route.path === path)
    const authToken = request.cookies.get('token')

    if(!authToken && publicRoute){
        return NextResponse.next()
    }
    
    if(!authToken && !publicRoute){
        const redirectUrl = request.nextUrl.clone()
        redirectUrl.pathname = REDIRECT_WHEN_NOT_AUTHENTICATED_ROUTE

        return NextResponse.redirect(redirectUrl)
    }

    if (authToken && publicRoute && publicRoute.whenAuthenticated === 'redirect'){
        const redirectUrl = request.nextUrl.clone()
        redirectUrl.pathname = '/'

        return NextResponse.redirect(redirectUrl)
    }

    if (authToken && !publicRoute){
        // Checar se o JWT está EXPIRADO
        // Se sim, remover o cookie e redirecionar o usuário pro login

        return NextResponse.next()
    }

    return NextResponse.next()
}

export const config: MiddlewareConfig = {
    matcher: [
        /*
        * Match all request paths except for the ones starting with:
        * - api (API routes)
        * - _next/static (static files)
        * - _next/image (image optimization files)
        * - favicon.ico (favicon file)
        */
        '/((?!api|_next/static|_next/image|favicon.ico).*)',
    ],
}