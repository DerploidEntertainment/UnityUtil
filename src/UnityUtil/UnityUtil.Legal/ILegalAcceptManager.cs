using System.Threading.Tasks;

namespace UnityUtil.Legal;

public interface ILegalAcceptManager
{
    Task<LegalAcceptance> CheckAcceptanceAsync();
    void Accept();
}
