# Phase 2 - Hardware & Manufacturing Rules Gate

## Goal
Make hardware a first-class catalog object and derive cabinet hardware quantities from explicit, replaceable manufacturing rules.

## Current policy defaults
- Up to 900 mm door height: 2 hinges per door.
- Up to 1800 mm door height: 3 hinges per door.
- Above 1800 mm: 4 hinges per door.
- 4 shelf pins per shelf.
- 1 handle per door.
- 8 carcass screws per cabinet.

These are policy defaults, not hidden geometry laws; the policy is replaceable.

## Output impact
Hardware plans flow into assembly documentation and production drawing output.
The model never stores a second manual quantity table.

## Catalog fallback
When a code is not present in the project HardwareItem catalog, a generic specification is emitted with a warning.

## Environment
The package has not been compiled here because the .NET SDK is unavailable in the current execution environment.
