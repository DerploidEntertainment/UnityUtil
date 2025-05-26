using System;

namespace UnityUtil.Legal;

/// <summary>
/// Manages the flow for users to grant consent to non-TCF-registered data processors,
/// view the consent form provided by this application's Consent Management Platform (CMP) for TCF-registered data processors,
/// accept legal documents like the Privacy Policy and Terms of Service, etc.
/// </summary>
public interface IConsentManager
{
    /// <summary>
    /// Check if consent was already granted to <paramref name="nonTcfDataProcessor"/>.
    /// </summary>
    /// <param name="nonTcfDataProcessor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Consent has not yet been saved for <paramref name="nonTcfDataProcessor"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="nonTcfDataProcessor"/> is not managed by this <see cref="IConsentManager"/>.</exception>
    bool WasConsentGranted(INonTcfDataProcessor nonTcfDataProcessor);

    /// <summary>
    /// Revoke consent for <paramref name="nonTcfDataProcessor"/>.
    /// </summary>
    /// <param name="nonTcfDataProcessor"></param>
    /// <exception cref="InvalidOperationException">Consent has not yet been saved for <paramref name="nonTcfDataProcessor"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="nonTcfDataProcessor"/> is not managed by this <see cref="IConsentManager"/>.</exception>
    void RevokeConsent(INonTcfDataProcessor nonTcfDataProcessor);
}
