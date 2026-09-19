import { Suspense, lazy } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { Spin } from 'antd'
import { PrivateRoute } from '@/components/auth/PrivateRoute'
import { MenuGuard } from '@/components/auth/MenuGuard'
import AppLayout from '@/components/layout/AppLayout'

// ─── Lazy-loaded pages ────────────────────────────────────────────────────────
const LoginPage = lazy(() => import('@/pages/auth/LoginPage'))
const AlterarSenhaPage = lazy(() => import('@/pages/auth/AlterarSenhaPage'))
const SemPermissaoPage = lazy(() => import('@/pages/auth/SemPermissaoPage'))
const DashboardPage = lazy(() => import('@/pages/dashboard/DashboardPage'))
const UsuarioPage = lazy(() => import('@/modules/usuarios/UsuarioPage'))
const PerfilPage = lazy(() => import('@/modules/usuarios/PerfilPage'))
const AcessoBiPage = lazy(() => import('@/modules/acesso-bi/AcessoBiPage'))
const SafraMetaPage = lazy(() => import('@/modules/agricola/SafraMetaPage'))
const ComprasFrutasPage = lazy(() => import('@/modules/agricola/ComprasFrutasPage'))

// Placeholder para módulos futuros (Fases 14-19)
const ComingSoon = () => (
  <div style={{ textAlign: 'center', padding: '80px 0', color: '#9ca3af' }}>
    <div style={{ fontSize: 48 }}>🚧</div>
    <p style={{ marginTop: 12 }}>Módulo em desenvolvimento</p>
  </div>
)

// ─── Loading spinner global ───────────────────────────────────────────────────
const PageLoader = () => (
  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
    <Spin size="large" tip="Carregando...">
      <div style={{ padding: 32 }} />
    </Spin>
  </div>
)

/**
 * Roteador principal — React Router v6
 *
 * Padrão: rotas privadas envolvidas em PrivateRoute + AppLayout.
 * Todas as páginas são lazy-loaded para melhor performance.
 */
export default function App() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Rota pública */}
        <Route path="/login" element={<LoginPage />} />

        {/* Rotas privadas — exigem autenticação */}
        <Route
          path="/*"
          element={
            <PrivateRoute>
              <AppLayout>
                <Suspense fallback={<PageLoader />}>
                  <Routes>
                    <Route path="/" element={<DashboardPage />} />
                    <Route path="/alterar-senha" element={<AlterarSenhaPage />} />
                    <Route path="/sem-permissao" element={<SemPermissaoPage />} />

                    {/* RF04.6 — Módulos protegidos pelo MenuGuard (defesa em profundidade) */}
                    <Route
                      path="/compras/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/financeiro/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/prazo-medio/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/vendas/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/agricola/safra-meta"
                      element={
                        <MenuGuard>
                          <SafraMetaPage />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/agricola/compras-frutas"
                      element={
                        <MenuGuard>
                          <ComprasFrutasPage />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/agricola/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    <Route
                      path="/admin/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />
                    {/* Módulo Acesso (Fase 14) — backend é a autoridade na autorização (RN-12) */}
                    <Route path="/usuarios" element={<UsuarioPage />} />
                    <Route path="/perfis" element={<PerfilPage />} />
                    <Route path="/estabelecimentos" element={<MenuGuard><ComingSoon /></MenuGuard>} />
                    <Route path="/configuracoes/acesso-bi" element={<MenuGuard><AcessoBiPage /></MenuGuard>} />

                    <Route
                      path="/configuracoes/*"
                      element={
                        <MenuGuard>
                          <ComingSoon />
                        </MenuGuard>
                      }
                    />

                    {/* Fallback */}
                    <Route path="*" element={<Navigate to="/" replace />} />
                  </Routes>
                </Suspense>
              </AppLayout>
            </PrivateRoute>
          }
        />
      </Routes>
    </Suspense>
  )
}
