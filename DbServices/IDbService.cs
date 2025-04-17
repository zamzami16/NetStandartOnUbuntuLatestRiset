using DbServices.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DbServices;

public interface IDbService
{
    Task<IList<User>> GetAllUserAsync(CancellationToken token = default);

    Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> SaveAsync(User user, CancellationToken cancellationToken = default);

    Task<Guid> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<Guid> DeleteAsync(User user, CancellationToken token = default);

    Task MigrateAsync(CancellationToken token = default);
}
