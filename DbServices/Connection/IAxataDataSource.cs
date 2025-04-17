using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading.Tasks;

namespace DbServices.Connection;

public interface IAxataDataSource
{
    NpgsqlConnection CreateConnection();
    NpgsqlDataSource DataSource { get; }
    IAxTransaction CreateTransaction();
    ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Create <see cref="IAxTransaction"/> instance with opened connection
    /// </summary>
    /// <returns><see cref="IAxTransaction"/> instance</returns>
    Task<IAxTransaction> CreateTransactionAsync();
}
