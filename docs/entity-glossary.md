# Domain entity glossary

| Entity | Purpose |
| --- | --- |
| Room | A lobby that can host multiple games. |
| RoomMember | A player's membership and readiness in a room. |
| Game | The authoritative state of one playthrough. |
| GamePlayer | A player's assigned role and participation in one game. |
| SettingsVersion | A version of the rules and progression settings. |
| PlayerConnection | An individual real-time connection for a game player. |
| CaseTemplate | The stable identity of a fallback case family. |
| CaseVersion | A reviewable revision of a fallback case family. |
| CaseVariant | A case revision adapted to one language and player count. |
| GameCase | The private case snapshot used for a game. |
| GameClue | A clue scheduled for release to a specific audience. |
| ClueRecipient | A player granted access to a particular clue. |
| GenerationJob | A durable request to prepare a case for a game. |
| Statement | A player's statement for one round. |
| Question | A detective's question and its target-owned answer. |
| AccusationVote | A detective's final choice of suspect for a round. |
| Arrest | The resolved arrest for one game round. |
| PendingScoreEvent | A score change held until game completion. |
| ScoreEntry | A finalized score change from a completed game. |
| RankDefinition | A rank and its minimum points in a settings version. |
| AchievementDefinition | A versioned achievement and its award criteria. |
| PlayerAchievement | A player's earned achievement tied to the game that awarded it. |
| AdminAudit | A record of an administrative action and its safe metadata. |
| ActionReceipt | The saved outcome of an idempotent request. |
| OutboxMessage | An event stored for delivery after its transaction commits. |

## Event delivery fields

| Column | SQL type | Purpose |
| --- | --- | --- |
| Rooms.StateVersion | bigint, required | Orders committed lobby updates. |
| OutboxMessages.RoomId | uniqueidentifier, nullable | References Rooms for lobby events, including events created before a game exists. |
| OutboxMessages.GameId | uniqueidentifier, nullable | References Games for game events. When UserId is present for a game event, (GameId, UserId) references GamePlayers. |

The database requires exactly one outbox target. Lobby events carry the room state version, and game events carry the game state version.
