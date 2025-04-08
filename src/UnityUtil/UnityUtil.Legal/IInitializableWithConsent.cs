using System.Threading.Tasks;

namespace UnityUtil.Legal;

public interface IInitializableWithConsent
{
    string ConsentPreferenceKey { get; }
    Task InitializeAsync(bool hasConsent);
    void OptOut();
}
