using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Lisha.Application.Common.Mappings
{
    public static class MappingExtensions
    {
        //public static Task<PaginationResponse<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable,
        //    int pageNumber, int pageSize)
        //    where TDestination : class
        //    => PaginationResponse<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);

        public static Task<List<TDestination>> ProjectToListAsync<TDestination>(this IQueryable queryable,
            IConfigurationProvider configuration)
            where TDestination : class
            => queryable.ProjectTo<TDestination>(configuration).AsNoTracking().ToListAsync();
    }
}
