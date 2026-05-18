# Flow: Session Feed & Content Discovery

## Overview
The session feed is the primary consumption experience: authenticated users see a personalized, paginated list of published sessions relevant to them — scoped to their organization and the groups they belong to. The platform distinguishes between sessions that are still "Pending" (open for contributions) and those already "Published" (ready to watch). Feed queries enforce RBAC at the data layer, applying group-membership filters so users only see content they are authorized to access.

## Code Involved

| Layer | File | Role |
|-------|------|------|
| API Controller | [`Soundbite.Api/Controllers/SessionsController.cs`](../Soundbite.Api/Controllers/SessionsController.cs) | Exposes `/organizations/{orgRoute}/Sessions/Pending`, `/Past`, `/Feed`, and group-scoped variants |
| Feed Service | [`Soundbite.Services/Services/SessionFeedService.cs`](../Soundbite.Services/Services/SessionFeedService.cs) | `ReadPendingAsync`, `ReadPastAsync`, `ReadPublishedAsync` — paged RBAC-aware queries |
| Feed Extensions | [`Soundbite.Services/Services/SessionFeedExtensions.cs`](../Soundbite.Services/Services/SessionFeedExtensions.cs) | EF extension methods: `QueryPendingOrgSessions`, `QueryPastOrgSessions`, `QueryPendingGroupSessions` |
| Session Service | [`Soundbite.Services/Services/SessionService.cs`](../Soundbite.Services/Services/SessionService.cs) | Legacy `ReadFeedAsync_Obsolete`, `ReadPublicFeedAsync_Obsolete` (retained for compatibility) |
| RBAC | `Masticore/Security/IRbac` | `AssertCurrentUserInRole` and `UniversalIdForCurrentUser` — gate and identity source |
| Database | [`Soundbite.Entity/SbDb.cs`](../Soundbite.Entity/SbDb.cs) | EF Core DbContext; `Sessions`, `Prompts`, `Participants`, `Groups`, `Members` DbSets |
| DB Extensions | [`Soundbite.Entity/SbDbExtensions.cs`](../Soundbite.Entity/SbDbExtensions.cs) | `GroupIdsAsync`, `QueryPendingOrgSessions`, shared LINQ query helpers |
| Security Middleware | [`Soundbite.Api/Middleware/SecurityMiddleware.cs`](../Soundbite.Api/Middleware/SecurityMiddleware.cs) | Provides `ISecurityContext` with current user's universalId |
| Frontend Store | `Soundbite.Client/packages/widgets.react/src/store/` | MobX stores that call the feed endpoints and manage paging state on the client |

## User Perspective

When a viewer opens the Soundbite app they land on a feed of sessions. The "Pending" tab shows sessions they are expected to contribute to, while the "Past" / "Published" tab shows announcements they can watch. The feed is paginated — the app requests the first page and loads more as the user scrolls. Each item in the feed is a lightweight `SessionPreview` card (title, thumbnail, contributor count, deadline) to keep the payload small.

Crucially, the feed is personalized and secure: users only see sessions their org membership and group roles entitle them to. An employee in the Engineering group does not see sessions limited to the Marketing group, even if both groups are in the same organization.

## Data Flow Diagram

```mermaid
sequenceDiagram
    actor Viewer
    participant SPA as Frontend (SPA / Teams)
    participant MW as SecurityMiddleware
    participant SC as SessionsController
    participant SFS as SessionFeedService
    participant RBAC as IRbac
    participant DB as SbDb (SQL)
    participant FE as SessionFeedExtensions (LINQ)

    Viewer->>SPA: Open app / navigate to feed
    SPA->>MW: GET /organizations/{orgRoute}/Sessions/Past?page=1&pageSize=20
    MW->>MW: Validate token, populate ISecurityContext
    MW->>SC: Forward request

    SC->>SFS: ReadPastAsync(orgRoute, page)

    SFS->>RBAC: AssertCurrentUserInRole(orgRoute, Person)
    RBAC-->>SFS: OK
    SFS->>RBAC: UniversalIdForCurrentUser
    RBAC-->>SFS: userUniversalId

    SFS->>DB: Infrastructure.DbAsync()
    DB-->>SFS: SbDb instance

    SFS->>FE: db.QueryPastOrgSessions(orgRoute, userUniversalId, Mapper)
    FE->>DB: LINQ query:<br/>Sessions WHERE org=orgRoute<br/>AND State=Published<br/>AND (user is Participant OR session is public)<br/>AND user in group memberships
    DB-->>FE: IQueryable<SessionPreview>

    SFS->>SFS: ReadPageResponseAsync(query, page, filterPredicate)
    SFS->>DB: Execute paged SQL + COUNT
    DB-->>SFS: SessionPreview[] + totalCount

    SFS-->>SC: IndexPageResponse<SessionPreview>
    SC-->>SPA: 200 OK – { items: SessionPreview[], total, page }
    SPA-->>Viewer: Feed cards rendered (title, thumbnail, date)

    Note over Viewer,SPA: User scrolls → SPA requests page=2, repeating flow
```
