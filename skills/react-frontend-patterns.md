# ⚛️ Skill: React Frontend Patterns — TypeScript + Vite

## Sobre
Esta skill define o padrão de desenvolvimento frontend do projeto GestaoNew. Stack: React 18 + TypeScript + Vite + React Router + CSS Modules. Todos os componentes devem seguir as convenções aqui definidas.

## Estrutura de Diretórios

```
Empresa.Web/src/
├── App.tsx                         # Entry point + rotas
├── main.tsx                        # ReactDOM.createRoot
├── components/
│   ├── layout/
│   │   ├── Header.tsx
│   │   ├── SideMenu.tsx
│   │   └── Layout.tsx
│   └── ui/                         # Componentes reutilizáveis
│       ├── Button.tsx
│       ├── Input.tsx
│       ├── Modal.tsx
│       ├── Table.tsx
│       ├── TreeView.tsx
│       └── Loading.tsx
├── modules/
│   ├── usuarios/
│   │   ├── components/
│   │   │   ├── UsuarioLista.tsx
│   │   │   ├── UsuarioForm.tsx
│   │   │   ├── PerfilSelect.tsx
│   │   │   ├── EstabelecimentoTree.tsx
│   │   │   └── PaginaTree.tsx
│   │   ├── hooks/
│   │   │   ├── useUsuarios.ts
│   │   │   └── usePerfis.ts
│   │   ├── services/
│   │   │   ├── usuarioService.ts
│   │   │   ├── perfilService.ts
│   │   │   ├── estabelecimentoService.ts
│   │   │   └── paginaService.ts
│   │   ├── types.ts
│   │   └── UsuarioPage.tsx
│   ├── compras/
│   ├── vendas/
│   └── financeiro/
├── services/
│   ├── api.ts                      # Axios instance + interceptors
│   └── authService.ts              # Login, refresh, logout
├── hooks/
│   └── useAuth.ts                  # Auth context/state
├── contexts/
│   └── AuthContext.tsx
└── utils/
    ├── format.ts                   # Formatação de data, moeda
    └── validation.ts               # Validação de formulários
```

## Configuração Axios (API Client)

```typescript
// src/services/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' }
});

// Interceptor: injeta JWT em toda requisição
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: trata 401 e tenta refresh automático
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken) {
        try {
          const { data } = await axios.post(
            `${import.meta.env.VITE_API_URL}/api/v1/auth/refresh`,
            { refreshToken }
          );
          localStorage.setItem('access_token', data.accessToken);
          originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
          return api(originalRequest);
        } catch {
          // Refresh falhou → logout
          localStorage.clear();
          window.location.href = '/login';
        }
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

## Pattern de Service (API Calls)

```typescript
// src/modules/usuarios/services/usuarioService.ts
import api from '@/services/api';
import type { Usuario, UsuarioRequest } from '../types';

const BASE = '/api/v1/usuarios';

export const usuarioService = {
  getAll: async (): Promise<Usuario[]> => {
    const { data } = await api.get<Usuario[]>(BASE);
    return data;
  },

  getById: async (id: number): Promise<Usuario> => {
    const { data } = await api.get<Usuario>(`${BASE}/${id}`);
    return data;
  },

  create: async (usuario: UsuarioRequest): Promise<Usuario> => {
    const { data } = await api.post<Usuario>(BASE, usuario);
    return data;
  },

  update: async (id: number, usuario: UsuarioRequest): Promise<Usuario> => {
    const { data } = await api.put<Usuario>(`${BASE}/${id}`, usuario);
    return data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  }
};
```

## Pattern de Hook Customizado

```typescript
// src/modules/usuarios/hooks/useUsuarios.ts
import { useState, useEffect, useCallback } from 'react';
import { usuarioService } from '../services/usuarioService';
import type { Usuario } from '../types';

export function useUsuarios() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const carregar = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const dados = await usuarioService.getAll();
      setUsuarios(dados);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar usuários');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carregar();
  }, [carregar]);

  return { usuarios, loading, error, recarregar: carregar };
}
```

## Pattern de Página (Module Page)

```typescript
// src/modules/usuarios/UsuarioPage.tsx
import { useState } from 'react';
import { useUsuarios } from './hooks/useUsuarios';
import { UsuarioLista } from './components/UsuarioLista';
import { UsuarioForm } from './components/UsuarioForm';
import type { Usuario } from './types';

export function UsuarioPage() {
  const [selected, setSelected] = useState<Usuario | null>(null);
  const [showForm, setShowForm] = useState(false);
  const { usuarios, loading, error, recarregar } = useUsuarios();

  if (loading) return <div>Carregando...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="page-container">
      <h1>Usuários</h1>

      <button onClick={() => { setSelected(null); setShowForm(true); }}>
        Novo Usuário
      </button>

      <UsuarioLista
        usuarios={usuarios}
        onEdit={(u) => { setSelected(u); setShowForm(true); }}
        onDelete={async (id) => {
          await usuarioService.delete(id);
          await recarregar();
        }}
      />

      {showForm && (
        <UsuarioForm
          usuario={selected}
          onClose={() => setShowForm(false)}
          onSaved={() => { setShowForm(false); recarregar(); }}
        />
      )}
    </div>
  );
}
```

## Pattern de Componente de Formulário

```typescript
// src/modules/usuarios/components/UsuarioForm.tsx
import { useState } from 'react';
import type { Usuario, UsuarioRequest } from '../types';
import { usuarioService } from '../services/usuarioService';

interface Props {
  usuario: Usuario | null;
  onClose: () => void;
  onSaved: () => void;
}

export function UsuarioForm({ usuario, onClose, onSaved }: Props) {
  const [form, setForm] = useState<UsuarioRequest>({
    nome: usuario?.nome ?? '',
    login: usuario?.login ?? '',
    senha: '',
    idPerfil: usuario?.idPerfil ?? 0,
    ativo: usuario?.ativo ?? 'S'
  });
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validar = (): boolean => {
    const erros: Record<string, string> = {};
    if (!form.nome.trim()) erros.nome = 'Nome é obrigatório';
    if (!form.login.trim()) erros.login = 'Login é obrigatório';
    if (!usuario && !form.senha) erros.senha = 'Senha é obrigatória';
    setErrors(erros);
    return Object.keys(erros).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validar()) return;

    setSaving(true);
    try {
      if (usuario) {
        await usuarioService.update(usuario.id, form);
      } else {
        await usuarioService.create(form);
      }
      onSaved();
    } catch (err) {
      setErrors({ form: 'Erro ao salvar. Verifique os dados.' });
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>{usuario ? 'Editar Usuário' : 'Novo Usuário'}</h2>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Nome</label>
            <input
              value={form.nome}
              onChange={(e) => setForm({ ...form, nome: e.target.value })}
            />
            {errors.nome && <span className="error">{errors.nome}</span>}
          </div>

          <div className="form-group">
            <label>Login</label>
            <input
              value={form.login}
              onChange={(e) => setForm({ ...form, login: e.target.value })}
            />
            {errors.login && <span className="error">{errors.login}</span>}
          </div>

          <div className="form-group">
            <label>Senha {usuario && '(deixe em branco para manter)'}</label>
            <input
              type="password"
              value={form.senha}
              onChange={(e) => setForm({ ...form, senha: e.target.value })}
            />
            {errors.senha && <span className="error">{errors.senha}</span>}
          </div>

          <div className="form-actions">
            <button type="button" onClick={onClose}>Cancelar</button>
            <button type="submit" disabled={saving}>
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </div>

          {errors.form && <div className="error">{errors.form}</div>}
        </form>
      </div>
    </div>
  );
}
```

## Definição de Tipos

```typescript
// src/modules/usuarios/types.ts
export interface Usuario {
  id: number;
  nome: string;
  login: string;
  ativo: 'S' | 'N';
  idPerfil: number;
  nomePerfil?: string;
}

export interface UsuarioRequest {
  nome: string;
  login: string;
  senha: string;
  idPerfil: number;
  ativo: 'S' | 'N';
}

export interface Perfil {
  id: number;
  nome: string;
  descricao: string;
  ativo: 'S' | 'N';
}

export interface Pagina {
  id: number;
  tituloMenu: string;
  url: string;
  idPaginaPai: number | null;
  ordem: number;
  filhos?: Pagina[];
}

export interface Estabelecimento {
  id: number;
  codigo: string;
  nome: string;
  idEstabelecimentoPai: number | null;
  filhos?: Estabelecimento[];
}
```

## Gerenciamento de Estado

```typescript
// src/contexts/AuthContext.tsx
import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { authService } from '@/services/authService';

interface AuthContextType {
  isAuthenticated: boolean;
  nomeUsuario: string;
  idPerfil: number;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  isAuthenticated: false,
  nomeUsuario: '',
  idPerfil: 0,
  logout: () => {}
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState({
    isAuthenticated: !!localStorage.getItem('access_token'),
    nomeUsuario: localStorage.getItem('nome_usuario') || '',
    idPerfil: Number(localStorage.getItem('id_perfil') || 0)
  });

  const logout = () => {
    localStorage.clear();
    setAuth({ isAuthenticated: false, nomeUsuario: '', idPerfil: 0 });
    window.location.href = '/login';
  };

  return (
    <AuthContext.Provider value={{ ...auth, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
```

## Regras Obrigatórias

1. **SEMPRE** usar TypeScript strict mode — nunca `any`
2. **SEMPRE** usar APIs via service layer (`services/`) — nunca axios direto nos componentes
3. **SEMPRE** usar hooks customizados (`hooks/`) para lógica de dados — nunca `useEffect` direto nos componentes
4. **SEMPRE** tratar estados: loading, error e vazio (`if (loading)`, `if (error)`, `if (!data.length)`)
5. **SEMPRE** usar `async/await` com try/catch — nunca `.then()` encadeado
6. **NUNCA** expor token JWT em logs ou console
7. **SEMPRE** limpar dados sensíveis do localStorage no logout
8. **SEMPRE** usar CSS Modules ou styled-components — nunca CSS inline
9. **NUNCA** usar `dangerouslySetInnerHTML` sem sanitização