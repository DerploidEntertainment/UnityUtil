namespace UnityEngine.Legal;

public interface IAdConsentManager
{
    bool ForceAdConsentBehavior { get; set; }
    DataConsentState CheckAdConsent();
    void RequestAdConsent();
    void InitializeAds();
}
