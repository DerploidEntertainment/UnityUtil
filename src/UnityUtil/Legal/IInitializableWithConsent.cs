using System.Threading.Tasks;

namespace UnityEngine.Legal;

public interface IInitializableWithConsent
{
    string ConsentPreferenceKey { get; }
    Task InitializeAsync(bool hasConsent);
}
