using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface ICaseRepository
{
    /// <summary>Loads validated variants from active templates and published versions for internal game startup.</summary>
    Task<IReadOnlyList<CaseVariant>> GetPublishedCaseVariantsAsync(
        string language, byte playerCount, CancellationToken cancellationToken = default);

    /// <summary>Tracks the selected case snapshot for insertion.</summary>
    void AddGameCase(GameCase gameCase);
}
