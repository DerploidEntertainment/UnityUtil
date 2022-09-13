using System.Threading.Tasks;

namespace UnityEngine.Legal;

public interface IAnalyticsConsentManager
{
    bool ForceAnalyticsConsentBehavior { get; set; }
    DataConsentState CheckAnalyticsConsent();
    void RequestAnalyticsConsent();
    Task InitializeAnalyticsAsync();
}
