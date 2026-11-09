using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface IActionReceiptService
{
    /// <summary>Locks the user's request key inside the active transaction and verifies the saved fingerprint.</summary>
    Task<ActionReceipt?> TryGetAsync(string userId, Guid requestId, string payloadHash, CancellationToken cancellationToken);

    /// <summary>Stages a receipt for the command transaction to save.</summary>
    void Add(ActionReceipt receipt);
}
