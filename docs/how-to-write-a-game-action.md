# Writing a game action

Add `[Authorize]` and `[IdempotencyFilter]` to the action. Build a `GameCommandRequest` using the authenticated user ID, the GUID from `IdempotencyFilter.GetRequestId`, a stable operation name, the game ID, the expected version and the validated request DTO. The fingerprint sorts object keys and includes the operation, target and expected version. Use a fresh key after changing the request.

Pass a current access check to the runner. On replay it must confirm the caller may still see the entire saved response. A new command checks the expected version before calling the handler. Check the current phase, deadline and action rules with the server time supplied by the runner.

```csharp
var response = await runner.ExecuteGameAsync(command, access.RequireCurrentVisibilityAsync,
    async (game, now, nextVersion, token) =>
    {
        rules.RequireStatementPhase(game, now);
        var statement = await statements.StageAsync(game, input, now, token);
        return new GameCommandChange(
            new GameCommandResponse(201, JsonSerializer.SerializeToElement(new { statement.Id, nextVersion })),
            [events.StatementRecorded(statement)]);
    }, cancellationToken);
return new JsonResult(response.Body) { StatusCode = response.StatusCode };
```

The example service names stand for action-specific code. Stage writes through the same scoped context. The runner increments the version and saves the state, outbox and receipt together. Keep provider calls and other external side effects outside the handler. A write conflict rolls back, reloads and rechecks access and version before retrying, with at most two retries.

For start, use `ExecuteStartAsync` with the room ID in the payload. Its access check gets the saved game ID on replay. The new-start handler must lock the room, recheck its start rules and stage the game and generation job. The runner commits the start writes with the receipt.
