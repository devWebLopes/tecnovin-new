import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'

interface PrivateRouteProps {
  children: React.ReactNode
}

/**
 * Protege rotas autenticadas.
 * Redireciona para /login preservando a URL de destino (state.from).
 */
export function PrivateRoute({ children }: PrivateRouteProps) {
  const { token, expiresAt } = useAuthStore()
  const location = useLocation()

  const isValid =
    !!token && !!expiresAt && new Date(expiresAt) > new Date()

  if (!isValid) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <>{children}</>
}
