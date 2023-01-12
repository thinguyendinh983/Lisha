using Lisha.Application.Common.Caching;

namespace Lisha.Infrastructure.Caching
{
    public class CacheKeyService : ICacheKeyService
    {
        public CacheKeyService()
        {
        }

        public string GetCacheKey(string name, object id)
        {
            return $"GLOBAL-{name}-{id}";
        }
    }
}
