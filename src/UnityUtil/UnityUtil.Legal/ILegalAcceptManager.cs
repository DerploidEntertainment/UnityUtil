using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityUtil.Legal;

/// <summary>
/// Manages the user flow for accepting legal documents at runtime.
/// </summary>
public interface ILegalAcceptManager
{
    /// <summary>
    /// Check the <see cref="LegalAcceptStatus"/> across all provided <paramref name="legalDocuments"/>.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous status check operation, containing the <see cref="LegalAcceptStatus"/> across all <paramref name="legalDocuments"/>.
    /// </returns>
    Task<LegalAcceptStatus> CheckStatusAsync(IEnumerable<LegalDocument> legalDocuments);

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
