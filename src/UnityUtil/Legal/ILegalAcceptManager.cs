using System;

namespace UnityEngine.Legal;

public enum LegalAcceptance
{
    Unprovided,
    Stale,
    Current,
}

public interface ILegalAcceptManager
{
    void CheckAcceptance(Action<LegalAcceptance> callback);
    void Accept();
}
