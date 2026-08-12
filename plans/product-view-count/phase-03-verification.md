# Phase 03 — Cross-repository verification and handoff

**Depends on:** phases 01 and 02
**Covers:** spec success criteria and rollout safety

## Tasks

1. Review `git diff`, `git diff --check`, generated migration, EF model snapshot, and both repository build outputs.
2. Run backend build and focused/full tests according to available database fixtures.
3. Run frontend `npm run build` and any focused Vitest tests added for the feature.
4. Smoke-test the public flow: list → open first product → POST increment → detail count; repeat for second product.
5. Verify inactive/missing product behavior, failed increment non-blocking behavior, and one request per detail open in browser devtools.
6. Confirm no unrelated files or existing UI behavior changed.

## Exit Criteria

- All required build/tests pass or any environment-limited checks are explicitly recorded.
- Migration is safe to apply and initial values are verified.
- API route and frontend response parsing match in a real local run.
