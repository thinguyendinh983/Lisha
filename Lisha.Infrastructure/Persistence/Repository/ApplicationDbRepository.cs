using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lisha.Application.Common.Persistence;
using Lisha.Domain.Common.Contracts;
using Lisha.Infrastructure.Persistence.Context;

namespace Lisha.Infrastructure.Persistence.Repository
{
    // Inherited from Ardalis.Specification's RepositoryBase<T>
    public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
        where T : class, IAggregateRoot
    {
        private readonly IMapper _mapper;
        public ApplicationDbRepository(ApplicationDbContext dbContext, IMapper mapper)
            : base(dbContext)
        {
            _mapper = mapper;
        }

        // We override the default behavior when mapping to a dto.
        // We're using Mapster's ProjectToType here to immediately map the result from the database.
        protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        {
            return ApplySpecification(specification, false).ProjectTo<TResult>(_mapper.ConfigurationProvider);
        }
    }
}
