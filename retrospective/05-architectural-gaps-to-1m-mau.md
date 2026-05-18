# Architectural Gaps to 1M MAU: Scaling the Platform Beyond Successful Early Growth

## Abstract Summary
The current architecture can support meaningful enterprise usage, but 1M MAU requires deliberate redesign around read-heavy feed delivery, global media distribution, event reliability, and operational maturity. The existing system already contains the right building blocks, but not yet in a shape optimized for hyperscale product behavior.

## Current State in the Codebase
### In-depth current architecture profile
- Monolithic API handles diverse responsibilities (`Soundbite.Api`)
- EF Core + Azure SQL central data dependency (`SbDb`)
- Queue + Functions offload critical asynchronous workloads
- Durable function orchestration drives media encoding and sync flows
- Multi-tenant, role-based context and org/group scoping are embedded across service logic

### Gaps that matter most for 1M MAU
1. **Feed read path specialization**
   - Feed/session retrieval still heavily tied to primary transactional model
2. **Data partitioning strategy**
   - Need explicit high-scale partition/sharding and read-model strategy
3. **Media path optimization**
   - Must formalize edge-first playback architecture and tokenized origin controls
4. **Event reliability and replay**
   - Need stronger idempotency, dead-letter strategy, and replay/repair tooling
5. **Observability maturity**
   - Requires SLO-driven platform operations with complete distributed tracing
6. **Modern runtime/security baseline**
   - Out-of-support/runtime-age and dependency risk must be removed before scale

## Case Study
A large enterprise campaign publishes many short videos in a day and expects immediate, smooth playback for globally distributed employees. Today’s architecture can process media and enforce security, but at much higher MAU, feed fan-out, read amplification, and region performance variation would create latency and reliability pain without dedicated read and edge architecture.

## Future State if Rebuilt Today (TikTok for the Enterprise + Modern Microsoft)
- Split core bounded contexts:
  - identity/org, feed, media, notifications, analytics
- Introduce CQRS:
  - write model in SQL, read models in scalable stores/caches optimized for feed queries
- Use Service Bus + Event Grid with durable event contracts
- Global delivery via Azure Front Door + CDN + media tokenization + regional failover
- Platform SRE baseline:
  - SLOs, error budgets, autoscale policy tuning, incident runbooks, game days

## Miscellaneous Retrospective Aspects
- The current architecture proves real-market viability.
- Enterprise controls and integrations are already strong differentiators.
- Most 1M-MAU gaps are “scale architecture” and “operations architecture,” not feature absence.

## Quintessential Improvements for Next Time
- Architect read-heavy experiences separately from transactional writes
- Build event replay/idempotency in by default
- Design global media delivery from the first enterprise launch
- Operationalize reliability as a product requirement
