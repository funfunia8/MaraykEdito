# UI Shell Gate

The first real WPF shell is now connected to application-level state.

## Rules enforced

- UI does not construct Domain objects directly.
- Workspace creation is owned by `WorkspaceController`.
- Localization is a service, not hard-coded UI text.
- Arabic/English switching is event-driven.
- Domain remains independent of WPF.
- The center design view is intentionally a placeholder; no fake CAD behavior has been added.

## Next

The next UI gate is to connect the room commands:

Create Room
→ Application command
→ Domain model
→ Regeneration
→ Design View refresh

The UI will remain a consumer of state, not the owner of design logic.
