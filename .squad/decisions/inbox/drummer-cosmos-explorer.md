# Decision: Cosmos DB Data Explorer Configuration Pattern

**Date:** 2026-03-07  
**Decided by:** Drummer (DevOps)  
**Issue:** #39  
**Status:** IMPLEMENTED

## Decision
Enable Cosmos DB Data Explorer in Aspire preview emulator using lambda-based configuration pattern in `Program.cs`.

## Pattern
```csharp
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer();
    });
```

## Why This Pattern
- `RunAsPreviewEmulator()` accepts an optional lambda parameter for emulator-specific configuration
- `WithDataExplorer()` is an extension method on `IResourceBuilder<AzureCosmosDBEmulatorResource>`
- The lambda pattern ensures type safety and prevents compiler errors
- Alternative patterns (fluent chaining, separate assignment) cause type mismatch errors because the return type of `RunAsPreviewEmulator()` is narrower than the return type of `AddAzureCosmosDB()`

## Impact
- ✅ Developers can now inspect Cosmos DB collections and documents in the Aspire dashboard
- ✅ Simplifies local debugging and seed data validation
- ✅ No compilation errors or build breakage
- ✅ Compatible with existing API and Redis references to the cosmos resource

## Technical Notes
- Aspire SDK: 13.1.0 (AppHost)
- Cosmos DB Hosting: 13.1.2
- .NET: 10.0
- The lambda parameter is optional; if not provided, emulator starts without Data Explorer

## Team Learning
This pattern should be applied to any future Aspire emulator configuration requiring specialized setup. Always check if the resource supports lambda-based configuration before using fluent chaining.
