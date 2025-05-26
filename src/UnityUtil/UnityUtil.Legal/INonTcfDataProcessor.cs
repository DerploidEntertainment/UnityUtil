namespace UnityUtil.Legal;

/// <summary>
/// Represents a data processor that is <em>not</em> certified with the IAB Transparency and Consent Framework (TCF)
/// and thus will not be mentioned by the application's Consent Management Platform (CMP).
/// Actions like consent, data deletion requests, etc. will thus have to be manually set up by the application.
/// </summary>
public interface INonTcfDataProcessor
{
    /// <summary>
    /// The user's consent (grant or deny) should be saved at this local preference key.
    /// </summary>
    string ConsentPreferenceKey { get; }

    /// <summary>
    /// Start collecting data for this data processor.
    /// Consumers should only call this method if they have a lawful basis (i.e., consent has been (re-)granted by the user).
    /// </summary>
    void StartDataCollection();

    /// <summary>
    /// Stop collecting data for this data processor.
    /// Consumers should call this method anytime a user revokes their consent at runtime.
    /// </summary>
    void StopDataCollection();

    /// <summary>
    /// Perform a Right to ERasure (RER) request, i.e., a user request to have their private data erased.
    /// </summary>
    void RequestDataDeletion();

    /// <summary>
    /// Perform a Data Subject Access Request (DSAR), i.e., a user request to view their private data that has been stored.
    /// </summary>
    void RequestData();
}
