# Frontend and Backend Architecture Retrospective: Multi-Surface Enterprise Media Platform

## Abstract Summary
Soundbite’s architecture combines a central .NET API with service-layer domain logic, EF-backed persistence, and Azure Function workers for asynchronous orchestration. The frontend spans a TypeScript workspace powering web, Teams, widgets, and SharePoint surfaces. This delivered broad enterprise capability, while introducing long-term complexity that a modern rebuild should simplify.

## Current State in the Codebase
### In-depth backend architecture
- API host:
  - `/home/runner/work/soundbite-ai/soundbite-ai/Soundbite.Api`
- Business/domain layer:
  - `/home/runner/work/soundbite-ai/soundbite-ai/Soundbite.Services`
- Data layer:
  - `/home/runner/work/soundbite-ai/soundbite-ai/Soundbite.Entity`
  - `/home/runner/work/soundbite-ai/soundbite-ai/Masticore.Entity`
- Background workers:
  - Azure Functions under `Soundbite.AzFunc.*`
  - Legacy WebJob under `Soundbite.WebJobs.Messaging`

### In-depth frontend architecture
- Workspace root:
  - `/home/runner/work/soundbite-ai/soundbite-ai/Soundbite.Client`
- Core app packages:
  - `packages/Soundbite.Spa`
  - `packages/Soundbite.Teams`
  - `packages/widgets.react*`
  - `packages/widgets.sharepoint.app`
- State management:
  - MobX stores in SPA and widget packages
- API interaction:
  - generated `@soundbite/api` contracts + axios adapter package

### Architectural strengths
- Reuse across multiple product surfaces
- Shared contract model between backend and frontend
- Separation of synchronous API concerns and asynchronous processing
- Broad enterprise identity and collaboration integration

### Architectural pain points
- Monolithic API responsibility concentration
- Hybrid worker model (WebJob + Functions) increases operational complexity
- Legacy compatibility endpoints create long-tail maintenance burden
- Frontend package sprawl and aging dependencies raise modernization cost

## Case Study
A company launching internal short-form executive updates can use the same core backend and identity context while exposing content in the secure web app, Teams experience, and embedded widget contexts. Engineering benefits because core business rules remain centralized while channel-specific UI packages adapt delivery to context.

## Future State if Rebuilt Today (TikTok for the Enterprise + Modern Microsoft)
- Domain-aligned service boundaries for feed, identity, media, notifications, analytics
- Read-optimized feed architecture separate from transactional write path
- Unified async model centered on modern Azure messaging and serverless/container orchestration
- Frontend platform strategy:
  - shared design system,
  - shared domain SDKs,
  - lighter host shells per surface (web/Teams/embedded)

## Miscellaneous Retrospective Aspects
- Architecture was successful for a growth-stage product with enterprise requirements.
- Cross-surface strategy was a meaningful differentiator.
- The next version should preserve cross-surface value while simplifying execution model and scaling read-heavy experiences.

## Quintessential Improvements for Next Time
- Minimize architecture modes (one modern worker model)
- Decouple feed/read scaling early
- Standardize frontend platform and ownership boundaries
- Keep enterprise integration depth while reducing platform complexity

