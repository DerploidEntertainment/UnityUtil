namespace UnityUtil;

public interface IAppVersion
{
    public string Version { get; }
    public string Description { get; }
    public int BuildNumber { get; }
}
