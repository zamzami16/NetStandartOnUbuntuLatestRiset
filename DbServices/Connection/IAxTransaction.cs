using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbServices.Connection;

/// <summary>
/// Defines the contract for managing database transactions, including synchronous and asynchronous commit, rollback,
/// and command creation. Provides properties to access the underlying connection and transaction state.
/// </summary>
public interface IAxTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commits the current transaction, making all changes permanent.
    /// </summary>
    void Commit();

    /// <summary>
    /// Asynchronously commits the current transaction, making all changes permanent.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the current transaction, discarding all changes made during the transaction.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Asynchronously rolls back the current transaction, discarding all changes made during the transaction.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackAsync();

    /// <summary>
    /// Begins a new transaction for the current database connection.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if the connection is not initialized.</exception>
    void BeginTransaction();

    /// <summary>
    /// Begins a new transaction with a read-committed isolation level for the current database connection.
    /// </summary>
    void BeginReadTransaction();

    /// <summary>
    /// Gets the underlying database connection associated with the transaction.
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying database transaction object.
    /// </summary>
    DbTransaction Transaction { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction has been rolled back.
    /// </summary>
    bool IsRolledback { get; }

    /// <summary>
    /// Gets a value indicating whether the transaction has been committed.
    /// </summary>
    bool IsCommited { get; }

    /// <summary>
    /// Creates a new database command object associated with the current transaction.
    /// </summary>
    /// <returns>A <see cref="DbCommand"/> object configured for the transaction.</returns>
    /// <exception cref="AxataException">Thrown if the transaction is already committed or rolled back.</exception>
    DbCommand CreateCommand();

    /// <summary>
    /// Creates a new database command object with the specified SQL query text and associates it with the current transaction.
    /// </summary>
    /// <param name="sql">The SQL query text to set for the command.</param>
    /// <returns>A <see cref="DbCommand"/> object configured for the transaction.</returns>
    /// <exception cref="AxataException">Thrown if the transaction is already committed or rolled back.</exception>
    DbCommand CreateCommand(string sql);
}
