# Phase 1 — Autosave & Recovery Gate

## Goal
Protect dirty project state from process interruption without making the recovery snapshot
another source of truth.

## Current behavior
- Dirty projects produce a recovery snapshot every 30 seconds while the desktop shell is open.
- Recovery writes are atomic and use the same project serializer as normal project files.
- Normal Save and Open clear the previous recovery snapshot.
- On startup, an available recovery snapshot is presented to the user for restore or discard.
- Restored projects are intentionally marked dirty and have no active file path until explicitly saved.
- Recovery failures are non-fatal to the UI timer.

## Architecture
`Design Model -> Project Serializer -> Recovery Store -> Recovery file`

The recovery file is never edited directly by the renderer or UI controls.

## Known limitation
The current recovery interval is fixed at 30 seconds in the desktop shell. It should become a
user/configuration setting after the project settings boundary exists.
