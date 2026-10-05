# ADR-004 — Foundation V2 Product Architecture

## Status
Accepted as the migration direction.

## Decision
Design Studio will evolve from the current cabinet-first prototype into a domain model where architecture, finishes, furniture/products, manufacturing, documentation, and pricing are independent concepts connected by stable IDs.

The existing cabinet implementation is retained as a compatibility/legacy subsystem during migration. New project-wide capabilities must not introduce additional `Cabinet*` dependencies.

## Foundation contracts

- `ProjectObject.Metadata` provides generic category, parent, layer, visibility, locking, and 2D placement metadata.
- `Building -> Floor -> Room` establishes the first explicit project hierarchy without invalidating existing room APIs.
- `ProductDefinition` represents a generic product independent of Cabinet.
- `ManufacturingDefinition` references its source design object and owns generic parts, hardware, operations, and assemblies.
- `DesignDocument` represents project-wide documentation independent of Cabinet production documents.
- `PricingItem` represents cost lines independently from Material and furniture types.
- Existing cabinet cut-list/nesting/documentation services remain until equivalent generalized services are implemented and regression-tested.

## Storage
The existing versioned JSON container remains readable. New metadata is optional when loading older documents, so schema 1/2 projects remain compatible. New Foundation V2 data is serialized only through explicit records; no runtime object graph serialization is permitted.

## Consequences

### Positive
- New product families can reuse manufacturing and documentation infrastructure.
- Costing is no longer coupled to cabinetry.
- Architectural and interior-design objects can share visibility, hierarchy, and placement concepts.
- iPad/cross-platform UI can consume application/domain contracts without inheriting WPF concepts.

### Temporary debt
- Legacy `Cabinet*` types remain in Domain/Application.
- The current JSON serializer still contains type switches and is therefore transitional.
- The current geometry and WPF presentation layers are not yet cross-platform.

## Exit criteria for the next migration stage

1. Generalized manufacturing service can produce a real definition for at least one non-cabinet product.
2. Generalized documentation can generate at least one non-cabinet drawing.
3. Pricing can aggregate material, labor, installation, and project-level lines.
4. Storage round-trip tests cover hierarchy and metadata.
5. Only then may legacy cabinet manufacturing types begin systematic replacement.
