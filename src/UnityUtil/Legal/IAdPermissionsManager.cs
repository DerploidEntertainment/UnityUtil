namespace UnityEngine.Legal;

public enum AdPermissionsState
{
    NotRequired,
    StillRequired,
    Acknowledged,
}

public interface IAdPermissionsManager
{
    bool ForceAdPermissionsBehavior { get; set; }
    AdPermissionsState CheckAdPermissions();
    void RequestAdPermissions();
    void InitializeAds();
}
