# 🏛️ Skill: Oracle 23c — Boas Práticas

## Conexão

### Connection String
```
User Id=usuario;Password=senha;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=servicename)));
```

### Pooling (recomendado)
```
User Id=...;Password=...;Data Source=...;Min Pool Size=2;Max Pool Size=20;Connection Lifetime=300;
```

- `Min Pool Size=2` — mantém 2 conexões abertas mesmo em idle
- `Max Pool Size=20` — limite para evitar estouro no banco
- `Connection Lifetime=300` — recicla conexões a cada 5 minutos

## Modelagem

### Nomenclatura
- Tabelas em MAIÚSCULO: `USUARIO`, `PAGINA`, `PERFIL`
- Colunas com underscore: `ID_USUARIO`, `NOME`, `DT_CADASTRO`
- Chave primária: `ID_` + nome da tabela
- Chave estrangeira: `ID_` + nome da tabela referenciada

### Tipos Recomendados
| Dados | Tipo Oracle | Mapeamento C# |
|-------|-------------|---------------|
| ID numérico | `NUMBER(10)` | `int` |
| Texto curto | `VARCHAR2(100)` | `string` |
| Texto longo | `CLOB` | `string` |
| Flag sim/não | `CHAR(1)` | `string` (com validação 'S'/'N') |
| Data | `DATE` | `DateTime` |
| Moeda | `NUMBER(18,2)` | `decimal` |
| Ativo/Inativo | `CHAR(1)` | `string` (default 'S') |

### Boas Práticas de Modelagem
- Sempre ter coluna `ATIVO CHAR(1) DEFAULT 'S'` para exclusão lógica
- Usar `DATE` em vez de `TIMESTAMP` se não precisar de fração de segundos
- Evitar `VARCHAR2(4000)` — usar `CLOB` para textos grandes
- Sempre definir `NOT NULL` e `DEFAULT` nas colunas

## Queries

### Paginação (Oracle 12c+)
```sql
SELECT * FROM USUARIO
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
```

### Hierarquia (CONNECT BY)
```sql
SELECT ID_PAGINA, TITULO_MENU, ID_PAGINA_PAI, LEVEL
FROM PAGINA
START WITH ID_PAGINA_PAI IS NULL
CONNECT BY PRIOR ID_PAGINA = ID_PAGINA_PAI
ORDER BY LEVEL, ORDEM;
```

### Merge (UPSERT)
```sql
MERGE INTO USUARIO U
USING (SELECT :Id AS ID_USUARIO FROM DUAL) S
ON (U.ID_USUARIO = S.ID_USUARIO)
WHEN MATCHED THEN
    UPDATE SET NOME = :Nome, LOGIN = :Login
WHEN NOT MATCHED THEN
    INSERT (NOME, LOGIN, SENHA, ATIVO)
    VALUES (:Nome, :Login, :Senha, 'S');
```

## Performance

- **Índices:** criar para colunas usadas em `WHERE`, `JOIN`, `ORDER BY`
- **Evitar `SELECT *`:** sempre listar colunas necessárias
- **`EXPLAIN PLAN`:** usar para queries lentas
- **Evitar functions em colunas indexadas:** `WHERE UPPER(NOME) = :Nome` não usa índice
- **Batch inserts:** usar `INSERT ALL` ou `OracleBulkCopy` para grandes volumes

## Segurança

- Nunca usar `GRANT ALL` — conceder apenas privilégios necessários
- Usar bind variables (Dapper faz isso automaticamente)
- Evitar `EXECUTE IMMEDIATE` com concatenação de strings
- Para buscas sensíveis (CPF, email), considerar criptografia na aplicação (não no banco)