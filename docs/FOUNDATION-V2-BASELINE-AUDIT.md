# Foundation V2 — Baseline Audit

## Scope
Audit of DesignStudio V01 Technical Foundation 27 before extending the product into a comprehensive interior-design platform.

## Verified baseline

- 7 existing projects before Foundation V2 additions.
- 93 C# files and approximately 5,783 lines across source/tests in the supplied package.
- .NET 10 target; WPF is currently the Windows presentation shell.
- Domain model is UI-independent.
- Geometry already has an `IGeometryEngine` boundary.
- Storage has versioned JSON and recovery/atomic-save infrastructure.
- Undo/redo is implemented through Application commands.
- Cabinet placement/clearance/collision has a useful deterministic foundation.

## Critical architectural findings

### 1. Cabinet coupling
Manufacturing, cut lists, nesting, hardware planning, and production documentation are predominantly represented by `Cabinet*` types. This is the main architectural blocker to supporting kitchens, wardrobes, bedrooms, tables, chairs, ceilings, finishes, and other project domains through shared infrastructure.

### 2. Flat project model
The existing `Project` stores objects in a flat dictionary. Room-to-wall and cabinet-to-room relationships exist, but the target product requires an explicit `Project -> Building -> Floor -> Room/Zone` hierarchy.

### 3. Placement duplication
Cabinet placement was previously stored in cabinet-specific properties. Foundation V2 moves placement into generic `ProjectObject.Metadata` while retaining the existing cabinet API as a compatibility façade.

### 4. Material model is too narrow
The existing Material model primarily represents sheet/edge-band manufacturing stock. Foundation V2 adds a generic material category/catalog metadata layer while preserving the old manufacturing fields.

### 5. Documentation scope is too narrow
Production documents currently model cabinet parts/cut lists/nesting. A project-wide documentation model is now introduced separately.

### 6. Pricing is absent as an independent domain
Costing must cover materials, furniture, hardware, glass, metal, stone, gypsum, flooring, finishes, lighting, labor, manufacturing, installation, transport, subcontracting, waste, markup/discount/tax. Foundation V2 introduces an independent Pricing model; detailed commercial rules remain a later gate.

### 7. Cross-platform boundary is incomplete
The core is not WPF-dependent, which is good. The actual UI remains WPF-specific. iPad support therefore remains a presentation/productization gate, not something to solve by adding WPF abstractions.

## Implementation performed in Foundation V2

- Generic object categories and metadata.
- Generic 2D placement.
- Explicit Building/Floor hierarchy primitives.
- Generic ProductDefinition.
- Generic ManufacturingDefinition contract and separate project boundary.
- Generic DesignDocument contract and separate project boundary.
- Independent PricingItem/PricingCalculator and separate project boundary.
- Backward-compatible storage metadata support.
- Regression tests for hierarchy, metadata round-trip, product independence, manufacturing references, documentation references, and pricing.

## Not claimed yet

- Production CNC nesting.
- Full 3D engine.
- Final geometry kernel.
- iPad UI.
- PDF/DWG production export.
- Full commercial costing engine.
- Full architectural/finish modeling.

## Build/test limitation
The supplied environment has no `dotnet`, `csc`, `msbuild`, or Mono compiler executable available. Therefore this stage has been statically inspected but cannot honestly be marked as compiled/passed. A Windows/.NET build gate is mandatory before accepting the changes.
