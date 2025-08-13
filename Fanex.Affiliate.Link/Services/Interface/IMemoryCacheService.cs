namespace Fanex.Affiliate.Link.Services.Interface
{
    public interface IMemoryCacheService
    {
        T Get<T>(string key);

        void Set<T>(string key, T value, int seconds = 300);
    }
}