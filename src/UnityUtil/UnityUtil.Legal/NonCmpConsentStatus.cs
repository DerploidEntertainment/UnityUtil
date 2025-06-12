namespace UnityUtil.Legal;

/// <summary>
/// Consent status for data processors that are not registered with the IAB Transparency and Consent Framework (TCF),
/// i.e., that do not have their consent managed by a Consent Management Platform (CMP).
/// </summary>
public enum NonCmpConsentStatus
{
    // Originally there was a `NotRequired` state also for the Editor, but we decided that all platforms should use the same consent flow,
    // both to simplify the code and to allow more realistic testing in the Editor.
    // Developers (like end users) will only have to set consent once (unless they clear consent state during testing), so we're not slowing down the test loop at all.

    /// <summary>
    /// Consent has not been saved yet, or was never requested from the user.
    /// </summary>
    StillRequired = 1,  // 1 for backwards compatibility after we removed `NotRequired`

    /// <summary>
    /// Consent is explicitly granted by the user.
    /// </summary>
    Granted,

    /// <summary>
    /// Consent is explicitly denied by the user.
    /// </summary>
    Denied,
}
