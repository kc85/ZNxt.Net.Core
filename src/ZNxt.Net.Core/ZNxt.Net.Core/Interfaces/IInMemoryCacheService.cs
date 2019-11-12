namespace ZNxt.Net.Core.Interfaces
{
    public interface IInMemoryCacheService
    {
        void Put<T>(string key, T data, int duration = 10);
        T Get<T>(string key);
    }
}
