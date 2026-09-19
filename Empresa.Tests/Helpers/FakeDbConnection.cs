using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Empresa.Tests.Helpers;

/// <summary>
/// Conexão ADO.NET fake (deriva de <see cref="DbConnection"/> para o caminho async do Dapper)
/// para testes de repositórios sem Oracle. Captura o CommandText emitido e alimenta os leitores
/// com linhas roteirizadas por correspondência parcial do SQL. Permite validar o contrato SQL
/// (QA-01) e o mapeamento coluna→modelo sem banco real.
/// </summary>
public sealed class FakeDbConnection : DbConnection
{
    private readonly List<FakeDbCommand> _commands = new();
    private readonly Func<string, Func<FakeDbReader>?> _readerFactory;
    private ConnectionState _state = ConnectionState.Closed;

    public FakeDbConnection(Func<string, Func<FakeDbReader>?> readerFactory)
    {
        _readerFactory = readerFactory;
    }

    public IReadOnlyList<FakeDbCommand> Commands => _commands;

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override string Database => "FAKE";
    public override string DataSource => "FAKE";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => _state;

    protected override DbCommand CreateDbCommand()
    {
        var cmd = new FakeDbCommand(this, _readerFactory);
        _commands.Add(cmd);
        return cmd;
    }

    public override void Open() => _state = ConnectionState.Open;
    public override void Close() => _state = ConnectionState.Closed;
    public override void ChangeDatabase(string databaseName) { }
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException();
}

/// <summary>
/// Comando fake — captura CommandText e parâmetros; ExecuteReader devolve um leitor
/// roteirizado pela factory da conexão (primeira correspondência parcial do SQL).
/// </summary>
public sealed class FakeDbCommand : DbCommand
{
    private readonly FakeDbConnection _connection;
    private readonly Func<string, Func<FakeDbReader>?> _readerFactory;
    private readonly FakeDbParameterCollection _parameters = new();

    public FakeDbCommand(FakeDbConnection connection, Func<string, Func<FakeDbReader>?> readerFactory)
    {
        _connection = connection;
        DbConnection = connection;
        _readerFactory = readerFactory;
    }

    [AllowNull]
    public override string CommandText { get; set; } = string.Empty;

    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; } = CommandType.Text;
    protected override DbConnection DbConnection { get; set; } = null!;
    protected override DbParameterCollection DbParameterCollection => _parameters;
    protected override DbTransaction? DbTransaction { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    public override bool DesignTimeVisible { get; set; }

    protected override DbParameter CreateDbParameter() => new FakeDbParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => BuildReader();

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken ct) =>
        Task.FromResult<DbDataReader>(BuildReader());

    public override int ExecuteNonQuery() => 1;

    public override Task<int> ExecuteNonQueryAsync(CancellationToken ct) => Task.FromResult(1);

    public override object? ExecuteScalar() => null;

    public override Task<object?> ExecuteScalarAsync(CancellationToken ct) => Task.FromResult<object?>(null);

    public override void Prepare() { }
    public override void Cancel() { }

    private FakeDbReader BuildReader()
    {
        var factory = _readerFactory(CommandText);
        return factory is null
            ? new FakeDbReader(Array.Empty<string>(), Array.Empty<object?[]>())
            : factory();
    }
}

public sealed class FakeDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public override bool IsNullable { get; set; }

    [AllowNull]
    public override string ParameterName { get; set; } = string.Empty;

    [AllowNull]
    public override string SourceColumn { get; set; } = string.Empty;

    public override DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;
    public override object? Value { get; set; }
    public override byte Precision { get; set; }
    public override byte Scale { get; set; }
    public override int Size { get; set; }
    public override bool SourceColumnNullMapping { get; set; }
    public override void ResetDbType() { }
}

public sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<FakeDbParameter> _items = new();

    public override int Count => _items.Count;
    public override object SyncRoot => _items;

    public override int Add(object value)
    {
        _items.Add((FakeDbParameter)value);
        return _items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var v in values)
            _items.Add((FakeDbParameter)v!);
    }

    public override void Clear() => _items.Clear();
    public override bool Contains(object value) => _items.Contains((FakeDbParameter)value);
    public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);
    public override int IndexOf(object value) => _items.IndexOf((FakeDbParameter)value);
    public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);
    public override void Insert(int index, object value) => _items.Insert(index, (FakeDbParameter)value);
    public override void Remove(object value) => _items.Remove((FakeDbParameter)value);
    public override void RemoveAt(int index) => _items.RemoveAt(index);
    public override void RemoveAt(string parameterName) => _items.RemoveAll(p => p.ParameterName == parameterName);
    public override void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
    public override IEnumerator GetEnumerator() => _items.GetEnumerator();

    protected override DbParameter GetParameter(int index) => _items[index];
    protected override DbParameter GetParameter(string parameterName) =>
        _items.First(p => p.ParameterName == parameterName);
    protected override void SetParameter(int index, DbParameter value) => _items[index] = (FakeDbParameter)value;
    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var idx = _items.FindIndex(p => p.ParameterName == parameterName);
        if (idx >= 0) _items[idx] = (FakeDbParameter)value;
        else _items.Add((FakeDbParameter)value);
    }
}

/// <summary>
/// Leitor de dados roteirizado — colunas + linhas fixas com suporte a GetOrdinal/GetValue,
/// que é o caminho usado pelo mapeador do Dapper.
/// </summary>
public sealed class FakeDbReader : DbDataReader
{
    private readonly string[] _columns;
    private readonly object?[][] _rows;
    private int _index = -1;

    public FakeDbReader(string[] columns, object?[][] rows)
    {
        _columns = columns;
        _rows = rows;
    }

    public override int FieldCount => _columns.Length;
    public override bool HasRows => _rows.Length > 0;
    public override bool IsClosed => false;
    public override int Depth => 0;
    public override int RecordsAffected => 0;

    public override object this[int i] => _rows[_index][i] ?? DBNull.Value;
    public override object this[string name] => this[GetOrdinal(name)];

    public override bool Read()
    {
        _index++;
        return _index < _rows.Length;
    }

    public override bool NextResult() => false;
    public override void Close() { }

    public override int GetOrdinal(string name)
    {
        var idx = Array.FindIndex(_columns, c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
            throw new IndexOutOfRangeException($"Coluna '{name}' não existe no reader fake");
        return idx;
    }

    public override string GetName(int i) => _columns[i];
    public override Type GetFieldType(int i) =>
        _rows.Length > 0 && _rows[0][i] is not null ? _rows[0][i]!.GetType() : typeof(string);
    public override string GetDataTypeName(int i) => GetFieldType(i).Name;

    public override bool IsDBNull(int i) => _rows[_index][i] is null || _rows[_index][i] is DBNull;
    public override object GetValue(int i) => _rows[_index][i] ?? DBNull.Value;

    public override int GetValues(object[] values)
    {
        var n = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < n; i++) values[i] = GetValue(i);
        return n;
    }

    public override IEnumerator GetEnumerator() => _rows.GetEnumerator();

    public override DataTable GetSchemaTable() =>
        throw new NotSupportedException();

    public override int GetInt32(int i) => Convert.ToInt32(GetValue(i));
    public override long GetInt64(int i) => Convert.ToInt64(GetValue(i));
    public override short GetInt16(int i) => Convert.ToInt16(GetValue(i));
    public override string GetString(int i) => Convert.ToString(GetValue(i))!;
    public override bool GetBoolean(int i) => Convert.ToBoolean(GetValue(i));
    public override decimal GetDecimal(int i) => Convert.ToDecimal(GetValue(i));
    public override double GetDouble(int i) => Convert.ToDouble(GetValue(i));
    public override float GetFloat(int i) => Convert.ToSingle(GetValue(i));
    public override Guid GetGuid(int i) => (Guid)GetValue(i);
    public override byte GetByte(int i) => Convert.ToByte(GetValue(i));
    public override char GetChar(int i) => Convert.ToChar(GetValue(i));
    public override DateTime GetDateTime(int i) => Convert.ToDateTime(GetValue(i));

    public override long GetBytes(int i, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override long GetChars(int i, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
}
