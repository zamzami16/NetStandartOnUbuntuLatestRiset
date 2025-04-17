using Npgsql;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbServices.Connection;

public class AxPosTransaction : IAxTransaction
{
    private readonly NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;
    private bool _commited = false;
    private bool _rolledback = false;

    public AxPosTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public AxPosTransaction(NpgsqlDataSource dataSource)
    {
        _connection = dataSource.CreateConnection();
        _connection.Open();
    }

    public void Commit()
    {
        if (_transaction is not null)
        {
            _transaction.Commit();
            _transaction = null;
        }
        _commited = true;
    }

    public async Task CommitAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            _transaction = null;
        }
        _commited = true;
    }

    public void Rollback()
    {
        if (_transaction is not null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
        _rolledback = true;
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        _rolledback = true;
    }

    public void BeginTransaction()
    {
        if (_connection == null) throw new NullReferenceException(nameof(_connection));
        _transaction = _connection.BeginTransaction();
    }

    public void BeginReadTransaction()
    {
        _transaction = _connection?.BeginTransaction(isolationLevel: IsolationLevel.ReadCommitted) as NpgsqlTransaction;
    }

    public DbConnection Connection => _connection;
    public DbTransaction Transaction => _transaction;

    public DbCommand CreateCommand() => CreateCommand("");

    public DbCommand CreateCommand(string sql)
    {
        if (_commited || _rolledback) throw new Exception("The AxTransaction instance already Commited or Rolledback, and no longer usable.");

        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return new NpgsqlCommand(sql, _connection, _transaction)
        {
            CommandType = CommandType.Text,
        };
    }

    public bool IsRolledback => _rolledback;

    public bool IsCommited => _commited;

    public void Dispose()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    Rollback();
                }
                _connection.Close();
            }
            finally
            {
                _connection.Dispose();
            }
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await RollbackAsync();
                }
                await _connection.CloseAsync();
            }
            finally
            {
                await _connection.DisposeAsync();
            }
        }

        GC.SuppressFinalize(this);
    }
}
