using DbServices.Domain;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DbServices;

public sealed class DbService(NpgsqlDataSource dataSource) : IDbService
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<Guid> DeleteAsync(User user, CancellationToken token = default)
    {
        if (user.Id == default)
        {
            throw new ArgumentNullException(nameof(user.Id));
        }

        _ = await FindByIdAsync(user.Id, token).ConfigureAwait(false)
            ?? throw new Exception("User not found!");

        using (var cmd = _dataSource.CreateCommand())
        {
            cmd.CommandText = """
                delete from t_users 
                where id = $1;
                """;

            cmd.Parameters.Add(new NpgsqlParameter<Guid> { Value = user.Id });

            await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        return user.Id;
    }

    public async Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User user = null;

        using (var cmd = _dataSource.CreateCommand())
        {
            cmd.CommandText = """
                select id, name
                from t_users
                where id = $1
                """;

            cmd.Parameters.Add(new NpgsqlParameter<Guid> { Value = id });

            using var rdr = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                user = new User
                {
                    Id = rdr.GetGuid(0),
                    Name = rdr.GetString(1),
                };
            }
        }

        return user;
    }

    public async Task<IList<User>> GetAllUserAsync(CancellationToken token = default)
    {
        List<User> users = [];

        using (var cmd = _dataSource.CreateCommand())
        {
            cmd.CommandText = """
                select id, name
                from t_users
                """;

            using var rdr = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
            while (await rdr.ReadAsync(token).ConfigureAwait(false))
            {
                users.Add(new User
                {
                    Id = rdr.GetGuid(0),
                    Name = rdr.GetString(1),
                });
            }
        }

        return users;
    }

    public async Task MigrateAsync(CancellationToken token = default)
    {
        using var cmd = _dataSource.CreateCommand();
        cmd.CommandText = """
                create table if not exists t_users (
                    id uuid default gen_random_uuid() primary key,
                    name text not null
                );
                """;

        await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    public async Task<Guid> SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id == default)
        {
            user.Id = Guid.NewGuid();
        }

        var existingUser = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(false);

        if (existingUser is not null)
        {
            throw new Exception("User already exists.");
        }

        using (var cmd = _dataSource.CreateCommand())
        {
            cmd.CommandText = """
                insert into t_users (id, name)
                values ($1, $2)
                """;

            cmd.Parameters.Add(new NpgsqlParameter<Guid> { Value = user.Id });
            cmd.Parameters.Add(new NpgsqlParameter<string> { Value = user.Name });

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return user.Id;
    }

    public async Task<Guid> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id == default)
        {
            throw new ArgumentNullException(nameof(user.Id));
        }

        _ = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception("User not found!");

        using (var cmd = _dataSource.CreateCommand())
        {
            cmd.CommandText = """
                update t_users 
                set name = $2
                where id = $1;
                """;

            cmd.Parameters.Add(new NpgsqlParameter<Guid> { Value = user.Id });
            cmd.Parameters.Add(new NpgsqlParameter<string> { Value = user.Name });

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return user.Id;
    }
}
