// ─── Tipos para o Módulo de Acesso B.I. ───────────────────────────────
// Alinhados com os DTOs do backend (camelCase via System.Text.Json)

export interface UsuarioBi {
  idUsuario: number
  nome: string
  empresas: string
}

export interface EmpresaBi {
  idUsuarioEmpresa: number
  idUsuario: number
  codigoEmpresa: number
  nomeFantasia: string
}

export interface EmpresaDisponivel {
  codigoEmpresa: number
  nomeFantasia: string
}

export interface ConcederAcessoTotalRequest {
  idUsuario: number
}

export interface VincularEmpresaRequest {
  codigoEmpresa: number
}