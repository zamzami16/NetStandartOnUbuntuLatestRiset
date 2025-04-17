using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DbServices.Connection;

/// <summary>
/// Represents a data source for PostgreSQL database connections.
/// </summary>
public class PostgresDataSource : IAxataDataSource
{
    private NpgsqlDataSource _dataSource;
    private readonly string _connectionString;
    private readonly ILoggerFactory _loggerFactory;

    private const string DefaultConnectionString = @"Server=localhost;Port=5433;Database=AxataPOS;User Id=axata;Password=axataposkenari;Application Name=AxataPOS;";

    /// <summary>
    /// Initializes a new instance with NullLoggerFactory of the <see cref="PostgresDataSource"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use for the data source.</param>
    public PostgresDataSource(string connectionString)
        : this(new NullLoggerFactory(), connectionString)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresDataSource"/> class with the specified logger factory and connection string.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use for logging.</param>
    /// <param name="connectionString">The connection string to use for the data source.</param>
    public PostgresDataSource(ILoggerFactory loggerFactory, string connectionString)
    {
        _connectionString = connectionString;
        _loggerFactory = loggerFactory ?? new NullLoggerFactory();
        _dataSource = CreateDataSource();
    }

    /// <summary>
    /// Creates and opens a new PostgreSQL database connection.
    /// </summary>
    /// <returns>A new <see cref="NpgsqlConnection"/> object.</returns>
    public NpgsqlConnection CreateConnection()
    {
        var c = DataSource.CreateConnection();
        c.Open();
        return c;
    }

    /// <summary>
    /// Gets the PostgreSQL data source.
    /// </summary>
    public NpgsqlDataSource DataSource => _dataSource ??= CreateDataSource();

    /// <summary>
    /// Gets the logger factory used for logging.
    /// </summary>
    public ILoggerFactory LoggerFactory => _loggerFactory;

    /// <summary>
    /// Creates a new transaction for the current data source.
    /// </summary>
    /// <returns>A new <see cref="IAxTransaction"/> object.</returns>
    public IAxTransaction CreateTransaction() => new AxPosTransaction(DataSource);

    /// <summary>
    /// Asynchronously creates a new transaction for the current data source.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IAxTransaction"/> object.</returns>
    public async Task<IAxTransaction> CreateTransactionAsync()
    {
        var connection = DataSource.CreateConnection();
        await connection.OpenAsync();
        return new AxPosTransaction(connection, null);
    }

    /// <summary>
    /// Creates a new PostgreSQL data source.
    /// </summary>
    /// <returns>A new <see cref="NpgsqlDataSource"/> object.</returns>
    private NpgsqlDataSource CreateDataSource()
    {
        var DataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString ?? DefaultConnectionString);

        Debug.WriteIf(_loggerFactory is null, "logger factory is null");
        DataSourceBuilder.UseLoggerFactory(_loggerFactory);
        return DataSourceBuilder.Build();
    }
}
