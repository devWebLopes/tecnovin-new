using Dapper;
using Empresa.Api.Authorization;
using Empresa.Api.Endpoints;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data;
using Empresa.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// ─────────────────────────────────────────────
// CONFIGURAÇÃO DO BUILDER
// ─────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// Permite overrides locais não commitados (ex: appsettings.Development.local.json)
builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Dapper — mapeia colunas Oracle com underscore para propriedades C# (ID_USUARIO → IDUsuario)
DefaultTypeMap.MatchNamesWithUnderscores = true;

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// ─────────────────────────────────────────────
// SERVIÇOS
// ─────────────────────────────────────────────

// DbSession (Scoped)
builder.Services.AddScoped<DbSession>();

// Repositórios (Scoped)
builder.Services.AddScoped<IAcessoRepository, AcessoRepository>();
builder.Services.AddScoped<IAcessoBiRepository, AcessoBiRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPaginaRepository, PaginaRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<IEstabelecimentoRepository, EstabelecimentoRepository>();
builder.Services.AddScoped<IComprasRepository, ComprasRepository>();
builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
builder.Services.AddScoped<IPrazoMedioRepository, PrazoMedioRepository>();
builder.Services.AddScoped<IVendasRepository, VendasRepository>();
builder.Services.AddScoped<IMetaCompraRepository, MetaCompraRepository>();
builder.Services.AddScoped<IAgricolaRepository, AgricolaRepository>();
builder.Services.AddScoped<ICalendarioRepository, CalendarioRepository>();
builder.Services.AddScoped<IAgrupamentoRepository, AgrupamentoRepository>();
builder.Services.AddScoped<IDbaRepository, DbaRepository>();

// Services (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAcessoBiService, AcessoBiService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPaginaService, PaginaService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IEstabelecimentoService, EstabelecimentoService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<IPrazoMedioService, PrazoMedioService>();
builder.Services.AddScoped<IVendasService, VendasService>();
builder.Services.AddScoped<IMetaCompraService, MetaCompraService>();
builder.Services.AddScoped<IAgricolaService, AgricolaService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();
builder.Services.AddScoped<IAgrupamentoService, AgrupamentoService>();
builder.Services.AddScoped<IDbaService, DbaService>();
builder.Services.AddScoped<IRelatorioAuditoriaService, RelatorioAuditoriaService>();

// Menu (Fase 18 — PRD Menu Lateral)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthorizationHandler, PaginaAutorizacaoHandler>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "GestaoNew API",
        Version = "v1",
        Description = "API modernizada do sistema TreisTecnovin - Gestão Empresarial"
    });

    // Configuração JWT no Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header usando o esquema Bearer. " +
                      "Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret não configurado em appsettings.json");

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // RN-12 — policy "PaginaAcesso": autoriza por CHAVE_CONTROLE × perfil (PaginaAutorizacaoHandler)
    options.AddPolicy("PaginaAcesso", policy =>
        policy.Requirements.Add(new PaginaAutorizacaoRequirement()));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks();

// ─────────────────────────────────────────────
// PIPELINE HTTP
// ─────────────────────────────────────────────
var app = builder.Build();

// Middleware de Exception Handling (global)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (todos os ambientes por enquanto)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GestaoNew API v1");
    c.RoutePrefix = "swagger";
});

// CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ─────────────────────────────────────────────
// HEALTH CHECKS
// ─────────────────────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/api/health");

app.MapGet("/api/health/database", async (DbSession dbSession) =>
{
    try
    {
        using var conn = dbSession.CreateConnection();
        conn.Open();
        var result = await conn.QueryFirstOrDefaultAsync<string>("SELECT 'OK' FROM DUAL");
        return Results.Ok(new
        {
            status = "Healthy",
            database = "Oracle",
            timestamp = DateTime.UtcNow,
            detail = "Conexão com Oracle estabelecida com sucesso"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: "Database Unavailable"
        );
    }
})
.WithTags("Health")
.WithSummary("Verifica a conexão com o banco Oracle")
.WithDescription("Retorna 200 se a conexão com Oracle estiver OK, 503 se estiver indisponível")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status503ServiceUnavailable);

// ─────────────────────────────────────────────
// ENDPOINTS
// ─────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapAcessoBiEndpoints();
app.MapMenuEndpoints();
app.MapUsuarioEndpoints();
app.MapPaginaEndpoints();
app.MapPerfilEndpoints();
app.MapEstabelecimentoEndpoints();
app.MapComprasEndpoints();
app.MapFinanceiroEndpoints();
app.MapPrazoMedioEndpoints();
app.MapVendasEndpoints();
app.MapAgricolaEndpoints();
app.MapSecundariosEndpoints();

// ─────────────────────────────────────────────
// INICIALIZAÇÃO
// ─────────────────────────────────────────────
try
{
    Log.Information("=== GestaoNew API iniciando ===");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}