using System.Threading.Tasks;

namespace UnityUtil.Legal;

/// <summary>
/// Represents a Consent Management Platform (CMP) compliant with the IAB Transparency and Consent Framework (TCF).
/// </summary>
public interface ITcfCmpAdapter
{
    /// <summary>
    /// Checks for changes to an app's TCF consent status. Should be called only once during an app session, at launch.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous consent status check operation.
    /// </returns>
    Task UpdateConsentInfoAsync();

    /// <summary>
    /// Load and show the consent form if required.
    /// This should be called either on app startup or from an in-app menu allowing users to manage their consent options;
    /// This method may be called multiple times in an app session.
    /// <para>
    /// A consent form may not be required if the application's TC string has not changed since the last session,
    /// or if the current user is not in a region protected by privacy law
    /// (though, for fairness, users in <em>all</em> regions should really be given the same consent options),
    /// or for any other application-specific reasons.
    /// </para>
    /// </summary>
    /// <returns>A task representing the asynchronous operation of loading/showing the CMP's consent form.</returns>
    Task LoadAndShowConsentFormIfRequiredAsync();

    /// <summary>
    /// Reset TCF consent status.
    /// </summary>
    void RevokeConsent();
}
