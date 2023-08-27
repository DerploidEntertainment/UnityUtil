using System;

namespace UnityUtil.Legal;

public interface IConsentManager
{
    bool ForceConsentBehavior { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initializableWithConsent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Consent has not yet been initialized</exception>
    /// <exception cref="ArgumentException"><paramref name="initializableWithConsent"/> is not managed by this consent manager</exception>
    bool HasConsent(IInitializableWithConsent initializableWithConsent);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initializableWithConsent"></param>
    /// <exception cref="InvalidOperationException">Consent has not yet been initialized</exception>
    /// <exception cref="ArgumentException"><paramref name="initializableWithConsent"/> is not managed by this consent manager</exception>
    void OptOut(IInitializableWithConsent initializableWithConsent);
}
