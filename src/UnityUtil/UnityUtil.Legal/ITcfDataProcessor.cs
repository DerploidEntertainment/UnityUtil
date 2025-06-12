using System.Threading.Tasks;

namespace UnityUtil.Legal;

/// <summary>
/// Represents a data processor certified with the IAB Transparency and Consent Framework (TCF).
/// <para>
/// Note that most user actions related to consent are handled by TCF-compliant Consent Management Platform (CMP) forms.
/// </para>
/// </summary>
public interface ITcfDataProcessor
{
    /// <summary>
    /// Initialize the data processor.
    /// </summary>
    /// <returns>A task representing the asynchronous initialize operation.</returns>
    Task InitializeAsync();
}
