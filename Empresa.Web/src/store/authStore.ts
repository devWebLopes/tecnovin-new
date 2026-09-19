import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { UsuarioInfo } from '@/types/api'
import api from '@/lib/api'

interface AuthState {
  token: string | null
  refreshToken: string | null
  expiresAt: string | null
  usuario: UsuarioInfo | null
  isAuthenticated: boolean

  // Ações
  login: (login: string, senha: string) => Promise<void>
  logout: () => void
  setTokens: (token: string, refreshToken: string, expiresAt: string) => void
}

/**
 * Store de autenticação global (Zustand + persist no localStorage).
 *
 * Persiste: token, refreshToken, expiresAt e dados do usuário.
 * O Axios interceptor lê diretamente do localStorage para injetar o Bearer.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      expiresAt: null,
      usuario: null,
      isAuthenticated: false,

      login: async (login: string, senha: string) => {
        const { data } = await api.post('/auth/login', { login, senha })
        // Backend retorna os campos no nível raiz (não em objeto "usuario")
        set({
          token: data.token,
          refreshToken: data.refreshToken,
          expiresAt: data.expiraEm,
          usuario: {
            id: data.idUsuario,
            nome: data.nome,
            login: data.login,
            email: data.login,
            perfilId: data.idPerfil,
            perfilNome: '',
            estabelecimentoId: 0,
          },
          isAuthenticated: true,
        })
      },

      logout: () => {
        set({
          token: null,
          refreshToken: null,
          expiresAt: null,
          usuario: null,
          isAuthenticated: false,
        })
      },

      setTokens: (token, refreshToken, expiresAt) => {
        set({ token, refreshToken, expiresAt })
      },
    }),
    {
      name: 'auth-storage',
      // Persiste apenas campos necessários (nunca expõe a senha)
      partialize: (state) => ({
        token: state.token,
        refreshToken: state.refreshToken,
        expiresAt: state.expiresAt,
        usuario: state.usuario,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
)
