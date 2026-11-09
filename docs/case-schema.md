# Case payload and authoring guide

Use the [JSON schema](cases/case.schema.json) for each language and player-count variant. The [Harbor Log example](cases/sample-harbor-log.json) is a four-player English variant for reviewing the format.

| Field | Contents | Visibility |
| --- | --- | --- |
| language, playerCount | Variant language and the total number of players | Public |
| briefing | Facts known to all players at the start | Public |
| characters | Suspect keys, names and public role descriptions | Public |
| criminalKey | The key of the one criminal in the suspect list | Server only until the game ends |
| scripts | One private script keyed by each suspect's character key | Assigned player |
| timeline, solution | The complete fictional timeline and answer | Server only until the game ends |
| clues | Clues with a unique key, a release round and an audience | Only the permitted audience after release |

Detectives are assigned by the game and have no suspect script. The available suspect counts are 3, 4, 4, 5 and 6 for 4, 5, 6, 7 and 8 players respectively. Use one detective with 4 or 5 players and two with 6–8 players. Exactly one suspect is identified by criminalKey.

Use the same character keys in characters, scripts, criminalKey and any character-specific clue. Every round needs at least one clue. Set each audience to Public, Detectives or Character. Include characterKey only for Character clues. Keep each clue body within 2,000 characters. Keep public descriptions separate from the private facts players use during questioning.

The schema checks the shape of the payload. The application validator will also check that character and clue keys are unique, the criminal is a known suspect, and the scripts and clue references resolve to existing characters.

## Review before publishing

- Check that names, places, times and evidence match across the briefing, scripts, timeline and clues.
- Follow the timeline in order and check each suspect's opportunity and alibi.
- Play through the available evidence and verify that the detectives can reasonably identify the criminal.
- Check that no early clue or public character description gives away the answer by itself.
- Compare English and Arabic variants for the same meaning and check that player-count changes preserve the case facts and deduction.
- Have a teammate review the format before drafting the fallback families. Record the reviewer and decision before a case version is published.
