using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository repository, ILogger<UsuarioService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync(string? search = null, string? ativo = null)
    {
        var usuarios = await _repository.GetAllAsync(search, ativo);
        return usuarios.Select(MapToResponse);
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        return usuario is null ? null : MapToResponse(usuario);
    }

    public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
            throw new ValidationException("Login é obrigatório");

        if (string.IsNullOrWhiteSpace(request.Senha))
            throw new ValidationException("Senha é obrigatória na criação");

        if (request.Senha.Length < 6)
            throw new ValidationException("Senha deve ter no mínimo 6 caracteres");

        if (await _repository.LoginExistsAsync(request.Login))
            throw new BusinessException("Já existe um usuário com este login");

        // ATENÇÃO: A coluna SENHA no banco legado é VARCHAR2(50) e armazena senha em texto puro.
        // O AuthService compara texto puro (usuario.Senha != request.Senha).
        // Quando migrar as senhas para BCrypt, será necessário alterar a coluna para VARCHAR2(200).
        var usuario = new Usuario
        {
            Nome = request.Nome,
            Login = request.Login,
            Senha = request.Senha,
            IDPerfil = request.IdPerfil,
            Ativo = request.Ativo ? "S" : "N",
            AtualizaSenha = request.AtualizaSenha ? "S" : "N"
        };

        var id = await _repository.CreateAsync(usuario);
        usuario.IDUsuario = id;

        _logger.LogInformation("Usuário {Login} (ID: {Id}) criado com sucesso", usuario.Login, id);
        return MapToResponse(usuario);
    }

    public async Task<bool> UpdateAsync(int id, UsuarioRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Usuário", id);

        existing.Nome = request.Nome;
        existing.Login = request.Login;
        existing.IDPerfil = request.IdPerfil;
        existing.Ativo = request.Ativo ? "S" : "N";
        existing.AtualizaSenha = request.AtualizaSenha ? "S" : "N";

        if (!string.IsNullOrWhiteSpace(request.Senha))
        {
            if (request.Senha.Length < 6)
                throw new ValidationException("Senha deve ter no mínimo 6 caracteres");
            // Mantém compatibilidade com legado: banco usa VARCHAR2(50) e senha em texto puro
            existing.Senha = request.Senha;
        }

        var result = await _repository.UpdateAsync(existing);
        if (result)
            _logger.LogInformation("Usuário ID {Id} atualizado com sucesso", id);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Usuário", id);

        var result = await _repository.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Usuário ID {Id} excluído logicamente", id);
        return result;
    }

    public async Task<IEnumerable<EstabelecimentoTreeResponse>> GetEstabelecimentosTreeAsync(int idUsuario)
    {
        var estabs = await _repository.GetEstabelecimentosTreeAsync(idUsuario);

        // Agrupar por empresa
        var grupos = estabs
            .GroupBy(e => e.CdEmpresa)
            .Select(grupo =>
            {
                // Identificar nome da empresa a partir do primeiro item
                var primeiro = grupo.First();
                var descritivo = primeiro.Descritivo ?? "";
                var dsEmpresa = descritivo.Split(' ').FirstOrDefault() ?? $"Empresa {grupo.Key}";

                return new EstabelecimentoTreeResponse
                {
                    CdEmpresa = grupo.Key,
                    DsEmpresa = dsEmpresa,
                    Estabelecimentos = grupo
                        .OrderBy(e => e.CdEstabelecimento)
                        .Select(e =>
                        {
                            var desc = e.Descritivo ?? "";
                            var dsEstabelecimento = desc.Contains(" - ")
                                ? desc[(desc.IndexOf(" - ") + 3)..].Trim()
                                : $"Estabelecimento {e.CdEstabelecimento}";

                            return new EstabelecimentoFilhoResponse
                            {
                                CdEstabelecimento = e.CdEstabelecimento,
                                DsEstabelecimento = dsEstabelecimento,
                                Vinculado = e.Vinculado == "S"
                            };
                        })
                        .ToList()
                };
            })
            .ToList();

        return grupos;
    }

    public async Task SincronizarEstabelecimentosAsync(int idUsuario, EstabelecimentoVinculoRequest request)
    {
        var existing = await _repository.GetByIdAsync(idUsuario);
        if (existing is null)
            throw new NotFoundException("Usuário", idUsuario);

        var vincular = request.VincularEstabelecimentos
            .Select(v => new VinculoEstabelecimento
            {
                CdEmpresa = v.CdEmpresa,
                CdEstabelecimento = v.CdEstabelecimento
            })
            .ToList();

        var desvincular = request.DesvincularEstabelecimentos
            .Select(v => new VinculoEstabelecimento
            {
                CdEmpresa = v.CdEmpresa,
                CdEstabelecimento = v.CdEstabelecimento
            })
            .ToList();

        await _repository.SincronizarEstabelecimentosAsync(idUsuario, vincular, desvincular);
        _logger.LogInformation("Vínculos de estabelecimentos do usuário ID {Id} atualizados", idUsuario);
    }

    private static UsuarioResponse MapToResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.IDUsuario,
            Nome = usuario.Nome,
            Login = usuario.Login,
            IdPerfil = usuario.IDPerfil,
            DescricaoPerfil = usuario.DescricaoPerfil,
            QuantidadeAcesso = usuario.QuantidadeAcesso,
            AtualizaSenha = usuario.AtualizaSenha == "S",
            Ativo = usuario.Ativo == "S",
            DataHoraUltimoAcesso = null // Será populado pelo repositório quando disponível
        };
    }
}