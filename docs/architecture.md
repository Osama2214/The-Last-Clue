# Architecture

## Layers

Application owns domain rules, service contracts and DTOs. It does not depend on Web, Infrastructure or Workers. Infrastructure implements persistence and external adapters. Workers contains hosted generation, timer and outbox services. Web is the composition root and owns transport and views. Both HTTP requests and workers will call the same application rules.

The current implementation contains the detective-count rule, a web shell and health probes. Database entities, Repository and Unit of Work implementations, Identity, SignalR and background workers remain in the task backlog.

## Consistency and visibility

SQL will own room and game state. Serialize mutations with database transactions and game/room write locks, increment the appropriate state version and persist outbox rows in the same transaction. An in-process semaphore may reduce contention but cannot replace the database lock.

Room events exist before a game exists. OutboxMessages therefore supports nullable RoomId and GameId, with exactly one target populated. Room events use Rooms.StateVersion; game events use Games.StateVersion. The updated data dictionary records this extension.

Build public, private, spectator and administrator views separately. A spectator retains the public board and receives new public clues; newly released private clues require an active eligible recipient. No client receives a full hidden case and hides it with CSS.

## Startup and providers

AI generation shares one 20-second deadline from accepted server time across queue delay and retries. Known outages and missing provider credentials select reviewed fallback cases. No AI call is required during rounds. Five reviewed families each require ten count/language variants, yielding fifty coverage checks before release.

## Existing design assets

The complete architecture, ERD, class, DFD, sequence, activity, state and deployment views are in [analysis and design](analysis-and-design/00_Complete_System_Analysis_and_Design.docx).
