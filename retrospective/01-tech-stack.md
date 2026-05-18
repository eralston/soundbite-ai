# Tech Stack Retrospective: From Secure Podcasting to Enterprise Short-Form Video

## Abstract Summary
Soundbite evolved into a broad Azure-hosted enterprise media platform with a C#/.NET backend, React/TypeScript frontend monorepo, Azure Functions for async processing, and a reusable internal foundation layer (`Masticore.*`). The stack successfully supported security-conscious enterprise workflows and multi-surface delivery (web, Teams, SharePoint), but now shows clear modernization pressure in runtime support, dependency age, and operational standardization.

## Current State in the Codebase
### In-depth architecture footprint
- Backend API: `Soundbite.Api` (`Soundbite.Api/`)
- Domain services: `Soundbite.Services` (`Soundbite.Services/`)
- Data model: `Soundbite.Entity` + `Masticore.Entity` with EF Core SQL model (`SbDb`)
- Async processing: Azure Queue + Functions projects:
  - `Soundbite.AzFunc.MediaProcessing`
  - `Soundbite.AzFunc.DirectorySync`
  - `Soundbite.AzFunc.Notifications`
  - `Soundbite.AzFunc.Scheduler`
- Legacy background worker retained: `Soundbite.WebJobs.Messaging`
- Client monorepo: `Soundbite.Client` npm workspaces with SPA, Teams web, widgets, and SPFx package

### Technology composition (as implemented)
- Language/runtime: .NET 6 targets across core projects
- Frontend: React 17 + TypeScript + MobX + React Router v5 + axios
- Storage/data: Azure SQL + Azure Blob/Queue Storage
- Identity: AAD (Entra) + Okta integrations
- Messaging/comms: ACS, Teams notifications, email/SMS abstractions
- Observability: Application Insights + custom performance middleware
- SDK strategy: .NET reflection-based TypeScript code generation (`Masticore.CodeGen`, `Soundbite.CodeGen`)

### What this stack did well
- Strong module decomposition for a long-lived product
- Enterprise integrations across multiple identity and collaboration channels
- Shared abstractions for portability and testability
- Contract synchronization between backend and TypeScript SDK consumers

### Constraints visible today
- Mixed generation dependencies with known vulnerable/outdated packages
- net6 and older ecosystem assumptions in build/test and runtime
- Tooling scripts tied to Windows commands (`copy`, `xcopy`) reduce cross-platform portability

## Case Study
An enterprise communications team can publish short-form announcements in a secure org context, distribute through Teams-compatible surfaces, and rely on integrated identity and permission checks. The same technical stack also supports engineering velocity by reusing generated API clients in frontend packages, reducing endpoint drift between server and client teams.

## Future State if Rebuilt Today (TikTok for the Enterprise + Modern Microsoft)
- .NET 8/9 LTS baseline with coordinated package lifecycle policy
- Azure Container Apps or AKS for API/workers where needed, with Functions for burst orchestration
- OpenAPI-first contracts + typed client generation (Kiota/NSwag/autorest-compatible workflow)
- Event backbone via Service Bus + Event Grid for cleaner fan-out and replay semantics
- Azure Front Door + CDN + WAF as codified infrastructure with hardened media delivery paths
- Identity hardening with managed identities, certificate-based auth, and centralized secret governance

## Miscellaneous Retrospective Aspects
- The stack reflects pragmatic business growth: fast feature expansion, broad integration coverage, and enterprise credibility.
- The same breadth increased cognitive load and maintenance surface area.
- Keeping both WebJobs and Functions illustrates a transitional architecture that worked commercially but should converge in a rebuild.

## Quintessential Improvements for Next Time
- Enforce supported runtime/dependency windows
- Design for cross-platform CI from day one
- Standardize deployment topology and background processing model early
- Keep generated contracts, but formalize governance and versioning policy
