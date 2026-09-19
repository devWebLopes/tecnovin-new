import { useMemo } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useMenuStore } from '@/store/menuStore'
import { resolverRota } from '@/lib/routeMap'
import type { MenuItem } from '@/types/api'

interface MenuGuardProps {
  children: React.ReactNode
}

function collectLeafRoutes(items: MenuItem[], set: Set<string>) {
  for (const item of items) {
    const rota = resolverRota(item.chaveControle, item.url)
    if (item.filhos && item.filhos.length > 0) {
      collectLeafRoutes(item.filhos, set)
    } else {
      set.add(rota)
    }
  }
}

export function MenuGuard({ children }: MenuGuardProps) {
  const location = useLocation()
  const menu = useMenuStore((s) => s.menu)
  const ultimaCarga = useMenuStore((s) => s.ultimaCarga)

  const leafRoutes = useMemo(() => {
    const set = new Set<string>()
    collectLeafRoutes(menu, set)
    return set
  }, [menu])

  if (menu.length === 0 || ultimaCarga === null) {
    return <>{children}</>
  }

  const rotaAtual = location.pathname.replace(/\/+$/, '') || '/'
  if (!leafRoutes.has(rotaAtual)) {
    return <Navigate to="/sem-permissao" state={{ from: location }} replace />
  }

  return <>{children}</>
}
