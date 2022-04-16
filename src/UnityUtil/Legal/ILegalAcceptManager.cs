namespace UnityEngine.Legal;

public interface ILegalAcceptManager
{
    bool HasAccepted { get; }
    void Accept();
}
