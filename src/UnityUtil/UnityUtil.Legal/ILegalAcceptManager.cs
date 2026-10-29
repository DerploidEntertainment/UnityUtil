using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityUtil.Legal;

public readonly struct LegalDocumentState(string currentTag, string? acceptedTag)
{
    public string CurrentTag { get; } = currentTag;
    public string? AcceptedTag { get; } = acceptedTag;
}

/// <summary>
/// Manages the user flow for accepting legal documents at runtime.
/// </summary>
public interface ILegalAcceptManager
{
    /// <summary>
    /// Check the acceptance status of all provided <paramref name="legalDocuments"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="LegalDocument.TagHeader"/> values in the responses from the provided documents' <see cref="LegalDocument.LatestVersionUri"/>s
    /// will be checked against the tags stored at their <see cref="LegalDocument.PreferencesKey"/>s.
    /// </remarks>
    /// <param name="legalDocuments">The <see cref="LegalDocument"/>s for which to check acceptance.</param>
    /// <returns>
    /// A task representing the asynchronous status check operation,
    /// which wraps the <see cref="LegalDocumentState"/> for each of the provided <paramref name="legalDocuments"/>, in the same order.
    /// </returns>
    Task<IEnumerable<LegalDocumentState>> CheckStatusAsync(IEnumerable<LegalDocument> legalDocuments);

    /// <summary>
    /// Save that all <see cref="LegalDocument"/>s (provided in a previous call to <see cref="CheckStatusAsync(IEnumerable{LegalDocument})"/>)
    /// have been accepted by the user.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// There are no known <see cref="LegalDocument"/>s to accept (perhaps <see cref="CheckStatusAsync(IEnumerable{LegalDocument})"/> has not been called yet).
    /// </exception>
    void Accept();

    /// <summary>
    /// Have all <see cref="LegalDocument"/>s (provided in a previous call to <see cref="CheckStatusAsync(IEnumerable{LegalDocument})"/>) been accepted by the user?
    /// I.e., has <see cref="Accept()"/> been called yet?
    /// </summary>
    bool HasAccepted { get; }
}
