﻿using Dapper;
using Lisha.Application.Common.Exceptions;
using Lisha.Application.Common.Persistence;
using Lisha.Domain.Common.Contracts;
using Lisha.Infrastructure.Persistence.Context;
using System.Data;

namespace Lisha.Infrastructure.Persistence.Repository
{
    public class DapperRepository : IDapperRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DapperRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity =>
            (await _dbContext.Connection.QueryAsync<T>(sql, param, transaction))
                .AsList();

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
        {
            var entity = await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

            return entity ?? throw new NotFoundException(string.Empty);
        }

        public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
        {
            return _dbContext.Connection.QuerySingleAsync<T>(sql, param, transaction);
        }
    }
}
