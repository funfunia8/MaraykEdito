# Review 001

## Issue found and corrected

The Intelligent Room opening creation path could insert a valid Door/Window once during validation and then attempt to insert it a second time.

The corrected implementation uses a single insertion inside `EnsureValid`, and the regression test `ValidOpeningsAreAddedExactlyOnce` verifies the invariant.

## Status

Corrected before package promotion.
