using System.Threading.Tasks;

namespace UnityUtil.Legal;

public enum LegalAcceptance
{
    Unprovided,
    Stale,
    Current,
}

public interface ILegalAcceptManager
{
    Task<LegalAcceptance> CheckAcceptanceAsync();
    void Accept();
}
