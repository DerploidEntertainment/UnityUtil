namespace UnityUtil.Configuration;

public interface IConfigurator
{
    void Configure(object client, string cacheKey);
}
