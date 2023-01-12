namespace Lisha.Infrastructure.Persistence.Initialization
{
    internal interface IDatabaseInitializer
    {
        Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    }
}
