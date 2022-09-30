namespace UnityUtil.Legal
{
    public interface IConsentManager
    {
        bool ForceConsentBehavior { get; }

        void GiveConsent();
        void OptOut(IInitializableWithConsent initializableWithConsent);
    }
}
