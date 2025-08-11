using Fanex.Affiliate.Link.Services.Interface;
using Fanex.Caching;
using Fanex.Caching.Memory;
using Newtonsoft.Json;

namespace Fanex.Affiliate.Link.Services.Impl
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private const int TIMEOUT = 300; // 5 minutes.

        private static readonly ICache MemoryCacheInstance = new MemoryCache(new MemoryCacheOptions());

        public T Get<T>(string key)
        {
            MemoryCacheInstance.TryGet<T>(key, out T value);

            return DeepClone(value);
        }

        public void Set<T>(string key, T value, int seconds = TIMEOUT)
        {
            if (!Equals(value, default(T)))
            {
                var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds));
                MemoryCacheInstance.Set(key, DeepClone(value), cachePolicy);
            }
        }
        public void SetWithSilingExpire<T>(string key, T value, int seconds = TIMEOUT)
        {
            if (!Equals(value, default(T)))
            {
                var cachePolicy = new CacheItemOptions();
                cachePolicy.SetSlidingExpiration(TimeSpan.FromSeconds(seconds));
                MemoryCacheInstance.Set(key, DeepClone(value), cachePolicy);
            }
        }

        private static T DeepClone<T>(T source)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }
    }
}