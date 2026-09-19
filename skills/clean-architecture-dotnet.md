# 📐 Skill: Clean Architecture para .NET

## Princípios

A GestãoNew segue Clean Architecture com 4 camadas:

```
Empresa.Api/       → Apresentação (endpoints, DTOs, services)
Empresa.Data/      → Infraestrutura (repositórios, DbSession, models)
Empresa.Util/      → Cross-cutting (utilitários, helpers)
Empresa.Worker/    → Background (workers, jobs agendados)
```

## Regras de Dependência

| Camada | Pode Referenciar |
|--------|-----------------|
| `Api` | `Data`, `Util` |
| `Data` | `Util` (apenas) |
| `Util` | Nenhuma |
| `Worker` | `Data`, `Util` |

Proibido:
- ❌ `Api` referenciar `Worker`
- ❌ `Data` referenciar `Api`
- ❌ Dependência circular entre camadas

## Padrões de Implementação

### Services (na camada Api)
- `IService` → interface com métodos async
- `Service` → classe concreta injetando repositórios
- Regras de negócio SEMPRE aqui, nunca nos endpoints

### Repositórios (na camada Data)
- `IRepository` → interface com métodos CRUD async
- `Repository` → classe concreta com Dapper
- Apenas operações de dados — sem regras de negócio

### DTOs (na camada Api)
- `Request` → dados de entrada (nunca expor model do banco)
- `Response` → dados de saída (nunca expor model do banco)
- Mapeamento manual ou com AutoMapper

### Endpoints (na camada Api)
- Minimal API (não Controllers)
- Agrupados por domínio em `Endpoints/`
- Registrados via extension methods em `Program.cs`

## Exemplo de Estrutura de Endpoint

```csharp
public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/usuarios");

        grupo.MapGet("/", async (IUsuarioService service) =>
        {
            var usuarios = await service.GetAllAsync();
            return Results.Ok(usuarios);
        });

        grupo.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
        {
            var usuario = await service.GetByIdAsync(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });
    }
}