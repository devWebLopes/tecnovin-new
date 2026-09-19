# 🔐 Skill: JWT Authentication — .NET 9

## Sobre
Esta skill define o padrão completo de autenticação e autorização JWT para o projeto GestaoNew. Cobre login, geração de tokens, refresh tokens, hash de senhas e proteção de endpoints.

## Configuração — appsettings.json

```json
{
  "Jwt": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "GestaoNew",
    "Audience": "GestaoNew",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

> ⚠️ **NUNCA** commitar `Secret` no repositório. Usar User Secrets no desenvolvimento e variáveis de ambiente em produção.

## Registro no Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configuração JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Secret"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Sem tolerância de expiração
    };
});

builder.Services.AddAuthorization();
```

## Geração de Token — AuthService

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public string GerarAccessToken(Usuario usuario)
    {
        var jwtSection = _config.GetSection("Jwt");
        var secretKey = jwtSection["Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("id_usuario", usuario.IdUsuario.ToString()),
            new Claim("nome", usuario.Nome ?? string.Empty),
            new Claim("login", usuario.Login ?? string.Empty),
            new Claim("id_perfil", usuario.IdPerfil?.ToString() ?? "0"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwtSection["AccessTokenExpiryMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken()
    {
        // Refresh token é um GUID criptograficamente seguro
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
```

## Hash de Senha — BCrypt

```csharp
// Instalar: dotnet add package BCrypt.Net-Next

public static class SenhaHelper
{
    // Gera hash da senha para armazenamento
    public static string HashSenha(string senhaTextoPlano)
    {
        return BCrypt.Net.BCrypt.HashPassword(senhaTextoPlano, workFactor: 12);
    }

    // Verifica se a senha confere com o hash armazenado
    public static bool VerificarSenha(string senhaTextoPlano, string hashArmazenado)
    {
        return BCrypt.Net.BCrypt.Verify(senhaTextoPlano, hashArmazenado);
    }
}
```

> ⚠️ **WorkFactor=12**: equilibra segurança e performance (~300ms por hash). Ajustar para 14 em produção se o hardware suportar.

## Fluxo de Login — Endpoint

```csharp
// Em AuthEndpoints.cs
grupo.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var resultado = await authService.LoginAsync(request);
    if (resultado is null)
        return Results.Unauthorized();

    return Results.Ok(resultado);
})
.WithSummary("Autentica um usuário e retorna tokens JWT")
.AllowAnonymous() // ← Essencial: este endpoint é público
.Produces<LoginResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);
```

## DTOs de Login

```csharp
// LoginRequest.cs
public record LoginRequest(
    string Login,
    string Senha
);

// LoginResponse.cs
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    string NomeUsuario,
    int IdPerfil
);
```

## Extrair Claims do Token no Endpoint

```csharp
// Acessar informações do usuário autenticado
grupo.MapGet("/me", async (HttpContext http, IUsuarioService service) =>
{
    var idUsuarioClaim = http.User.FindFirst("id_usuario")?.Value;
    if (idUsuarioClaim is null || !int.TryParse(idUsuarioClaim, out var idUsuario))
        return Results.Unauthorized();

    var usuario = await service.GetByIdAsync(idUsuario);
    return usuario is null ? Results.NotFound() : Results.Ok(usuario);
})
.WithSummary("Retorna dados do usuário autenticado")
.RequireAuthorization();
```

## Autorização por Perfil (Role-Based)

```csharp
// Proteger endpoint por perfil específico
grupo.MapGet("/admin", () => Results.Ok("Acesso administrativo"))
    .RequireAuthorization(policy => policy.RequireClaim("id_perfil", "1"));

// Ou usando Policy baseada em claims
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("id_perfil", "1"));
});

grupo.MapGet("/admin", () => Results.Ok("Acesso administrativo"))
    .RequireAuthorization("AdminOnly");
```

## Refresh Token — Fluxo

```csharp
grupo.MapPost("/refresh", async (string refreshToken, IAuthService authService) =>
{
    var novoToken = await authService.RefreshTokenAsync(refreshToken);
    if (novoToken is null)
        return Results.Unauthorized();

    return Results.Ok(novoToken);
})
.WithSummary("Renova access token usando refresh token")
.AllowAnonymous()
.Produces<LoginResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);
```

## Rate Limiting no Login

```csharp
// Em Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Auth", config =>
    {
        config.PermitLimit = 5;           // 5 tentativas
        config.Window = TimeSpan.FromMinutes(1); // por minuto
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
});

// No endpoint de login
grupo.MapPost("/login", ...)
    .RequireRateLimiting("Auth");
```

## Regras de Segurança Obrigatórias

1. **NUNCA** armazenar senha em texto plano — sempre usar BCrypt (WorkFactor ≥ 12)
2. **NUNCA** retornar `SENHA` ou `SENHA_HASH` em nenhum DTO de Response
3. **SEMPRE** usar `RequireAuthorization()` em endpoints de negócio
4. **SEMPRE** usar `AllowAnonymous()` apenas em `/auth/login`, `/auth/refresh` e `/health`
5. **NUNCA** expor stack trace em respostas 401/403
6. **SEMPRE** usar claims padronizados: `id_usuario`, `nome`, `login`, `id_perfil`
7. **SEMPRE** invalidar token no servidor em caso de logout (blacklist ou redução de expiração)
8. **SEMPRE** validar `ATIVO = 'S'` no banco durante o login (exclusão lógica)