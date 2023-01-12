using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Common.Caching
{
    public interface ICacheKeyService : IScopedService
    {
        public string GetCacheKey(string name, object id);
    }
}
