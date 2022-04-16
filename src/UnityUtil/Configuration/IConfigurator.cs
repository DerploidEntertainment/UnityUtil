namespace UnityEngine;

public interface IConfigurator
{
    void Configure(object client, string cacheKey);
}
