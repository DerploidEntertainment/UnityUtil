namespace UnityUtil;

public interface IAppVersion
{
    string Version { get; }
    string Description { get; }
    int BuildNumber { get; }
}
