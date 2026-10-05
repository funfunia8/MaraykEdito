# ADR-001 — Technical Foundation

## Status
Accepted.

## Decision
Use C#/.NET 10 LTS as the application foundation and WPF as the initial Windows desktop shell.

## Rationale
- Windows is the V1 target.
- .NET 10 is the current LTS release.
- WPF is mature, Windows-native, and supports the desktop interaction model required for the initial product.
- Core/domain projects remain UI-independent, preserving the option to replace the presentation layer later.

## Deferred
Geometry engine, 3D renderer, project serialization format, and advanced rendering remain Technical Spike decisions.
